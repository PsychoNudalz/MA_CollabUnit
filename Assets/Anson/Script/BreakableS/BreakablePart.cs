using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = Unity.Mathematics.Random;


[Serializable]
public class BreakablePart : BreakableComponent
{
    // [Header("Auto Free time")]
    // [SerializeField]

    protected float freeToStayTime = 5f;
    protected float freeToStayTime_now = 0f;

    protected float CheckBottomTime = 5f;
    protected float CheckBottomTime_now = 0f;

    protected float moveDistance = 10f;
    protected Vector3 lastPosition;
    protected LayerMask partLayer;

    public virtual bool CanTelekinesis => breakableState == BreakableState.Free;

    private void Awake()
    {
        if (isDebug)
        {
            rendererMaterial = renderer?.material;
        }

        partLayer = LayerMask.NameToLayer("Breakable_Part");
    }

    private void FixedUpdate()
    {

        switch (breakableState)
        {
            case BreakableState.Hold:
                if (!originalNoBottom)
                {
                    CheckBottomTime_now -= Time.deltaTime;
                    if (CheckBottomTime_now < 0)
                    {
                        if (!HasBottomPart())
                        {
                            Break(new Vector3(), new Vector3());
                        }

                        CheckBottomTime_now = CheckBottomTime;
                    }
                }
                break;
            case BreakableState.Free:
                if (isStoredExplosive)
                {
                    ApplyStoredForce();
                }
                break;


        }
    }

    private void OnDrawGizmosSelected()
    {
        try
        {
            if (breakableState == BreakableState.Hold || forceShowConnection)
            {
                if (otherConnectedParts?.Count > 0)
                {
                    foreach (BreakableComponent connectedPart in otherConnectedParts)
                    {
                        if (connectedPart)
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawLine(transform.position, connectedPart.transform.position);
                        }
                    }
                }

                if (connectedParts?.Count > 0)
                {
                    foreach (BreakableComponent connectedPart in connectedParts)
                    {
                        if (connectedPart)
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(transform.position, connectedPart.transform.position);
                        }
                    }
                }
            }
        }
        catch (NullReferenceException e)
        {
            // Debug.LogWarning("Debug line is null");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnterBehaviour(collision);
    }

    protected virtual void CollisionEnterBehaviour(Collision collision)
    {
        if (breakableState == BreakableState.Telekinesis_Shoot)
        {
            ChangeState(BreakableState.Free);
        }
        else
        {
            if (!IsBroken())
            {
                EvaluateCollisionBreak(collision);
            }
        }
    }


    bool RBInConnected(Rigidbody rb)
    {
        foreach (BreakableComponent connectedPart in connectedParts)
        {
            if (connectedPart.SelfRb.Equals(rb))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Breaks the Part and will break connected Parts
    /// </summary>
    /// <param name="force"></param>
    /// <param name="originalForce"></param>
    /// <param name="breakHistory"> list of pass breaks to avoid loop</param>
    /// <param name="breakDelay"></param>
    /// <param name="forceBreak"></param>
    /// <param name="originPoint"></param>
    public override void Break(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f, bool forceBreak = false, Vector3 originPoint = default)
    {
        if (collectiveParent && collectiveParent.BreakableState == BreakableState.Hold)
        {
            collectiveParent.Break(Vector3.zero, Vector3.zero);
        }

        if (breakDelay == 0f)
        {
            Break_Recursive(force, originalForce, breakHistory, breakDelay, forceBreak);
        }
        else
        {
            StartCoroutine(DelayBreak_Recursive(force, originalForce, breakHistory, breakDelay));
        }

        AddScore();
        PlayBreakEffects();
        breakEvent.Invoke();
        gameObject.layer = partLayer;
        if (isGroundPiece)
        {
            breakableStructureController.RemoveGroundPiece();
        }

        if (breakableState != BreakableState.Despawn)
        {
            BreakableManager.Add(this);
        }
    }


    /// <summary>
    /// Recursively break connected componenets
    ///
    /// No fking idea what the break will call back to the previous broken piece
    /// and cause a loop
    ///
    /// </summary>
    /// <param name="force"></param>
    /// <param name="originalForce"></param>
    /// <param name="breakHistory"></param>
    /// <param name="breakDelay"></param>
    /// <param name="forceBreak"></param>
    public void Break_Recursive(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f, bool forceBreak = false)
    {
        if (IsBroken())
        {
            return;
        }

        if (breakHistory == null)
        {
            breakHistory = new List<BreakableComponent>();
        }

        if (breakHistory.Contains(this))
        {
            Debug.LogWarning($"Loop back error: {this}");
            return;
        }

        breakHistory.Add(this);

        Break_Single(force, originalForce);

        // print($"{this} Force: {force}");
        Vector3 newForce = force * forceTransfer;
        BreakableComponent[] tempPD = connectedParts.ToArray();
        foreach (BreakableComponent connectedPart in tempPD)
        {
            if (connectedPart)
            {
                connectedPart.EvaluateBreak(connectedPart, force, this, breakHistory);
            }
        }

        tempPD = otherConnectedParts.ToArray();
        foreach (BreakableComponent partDistance in tempPD)
        {
            if (partDistance)
            {
                partDistance.EvaluateFall();
            }
        }

        ApplyForce(force);
    }

    private IEnumerator DelayBreak_Recursive(Vector3 force, Vector3 originalForce,
        List<BreakableComponent> breakHistory,
        float breakDelay = 0f)
    {
        yield return new WaitForSeconds(breakDelay);

        Break_Recursive(force, originalForce, breakHistory, breakDelay);
        // print(string.Join(", ",breakHistory));
    }


    public void Break_Single(Vector3 force, Vector3 originalForce)
    {
        if (IsBroken())
        {
            return;
        }

        if (isDebug)
        {
            // Debug.DrawRay(transform.position,force,Color.blue,5f);
        }

        finalBrokeForce = force.magnitude;
        foreach (BreakableComponent part in otherConnectedParts)
        {
            part.RemovePart(this);
        }


        //if piece is too small
        if (meshSize < minimumPartSize)
        {
            gameObject.SetActive(false);
            Despawn();
            return;
        }
        else
        {
            breakableState = BreakableState.InitialBreak;
            StartCoroutine(DelayToFullBreak());
            selfRB.isKinematic = false;
            selfRB.useGravity = true;

            if (isDebug)
            {
                rendererMaterial.color = Color.red;
            }
        }
    }

    /// <summary>
    /// to determine what to do after the force from other breakable is applied
    /// </summary>
    /// <param name="pd"></param>
    /// <param name="force"></param>
    public override void EvaluateBreak(BreakableComponent pd, Vector3 force, BreakableComponent originalPart,
        List<BreakableComponent> breakHistory)
    {
        if (gameObject || !gameObject.activeSelf)
        {
            
            if (collectiveParent&&collectiveParent.BreakableState == BreakableState.Hold)
            {
                collectiveParent.Break(Vector3.zero,Vector3.zero);
            }
            else
            {
                return;
            }
        }

        Vector3 newForce = force * forceTransfer;

        float LerpForce = .5f;
        float dotValue = Mathf.Abs(Vector3.Dot(Dir(pd), newForce.normalized));
        // print(dotValue);
        newForce = Vector3.Lerp(Dir(pd), newForce.normalized, LerpForce) *
                   (newForce.magnitude * transferToDot.Evaluate(dotValue));
        RemovePart(originalPart);

        if (isDebug)
        {
            // Debug.DrawRay(transform.position,newForce,new Color(1-dotValue,dotValue,0),10f);
        }

        if (newForce.magnitude > breakingForce.y)
        {
            // print($"{this} recursive to: {connectedPart.Part}");
            Break(newForce, force, breakHistory);
        }
        else if (newForce.magnitude > breakingForce.x)
        {
            // print($"{this} recursive to: {connectedPart.Part}");
            Break(newForce, force, breakHistory, BreakDelay);
        }
        else
        {
            EvaluateFall();
        }
    }

    public override void EvaluateFall()
    {
        if (breakableState != BreakableState.Despawn)
        {
            if (collectiveParent && collectiveParent.BreakableState == BreakableState.Hold)
            {
                collectiveParent.Break(Vector3.zero, Vector3.zero);
            }

            try
            {
                StartCoroutine(DelayBreakBottom());
            }
            catch (Exception e)
            {
                return;
            }
        }
    }


    private void ApplyForce(Vector3 f)
    {
        float multiplier = 1f;
        Vector3 addForce = f * multiplier * forceTransfer;
        if (!float.IsNaN(addForce.x))
        {
            selfRB.AddForce(addForce);
        }
    }

    public override void CollisionBreak(Rigidbody rb, Collision collision = null, Vector3 point = default)

    {
        if (IsBroken())
        {
            return;
        }


        var force = CalculateForce(rb, out var originalSpeed, out var forceDir);


        if (isDebug)
        {
            // Debug.DrawRay(transform.position, forceDir * force, Color.cyan, 10f);
        }


        // if (force > breakingForce.x * .7f)
        // {
        //     print($"Collided with {rb} with force: {force}  Against: {breakingForce}");
        // }

        if (force > breakingForce.x)
        {
            Break(forceDir * force, forceDir * force);
        }
        else
        {
            breakingForce -= new Vector2(force, force);
        }

        //have original object to keep flying
        if (!RBInConnected(rb))
        {
            rb.velocity = originalSpeed * forceTransfer;
        }
    }

    public override void CollisionBreak(MovableObject mo, Collision collision = null, Vector3 point = default)
    {
        if (IsBroken())
        {
            return;
        }


        var force = CalculateForce(mo, out var originalSpeed, out var forceDir);


        if (isDebug)
        {
            // Debug.DrawRay(transform.position, forceDir * force, Color.cyan, 10f);
        }


        // if (force > breakingForce.x * .7f)
        // {
        //     print($"Collided with {rb} with force: {force}  Against: {breakingForce}");
        // }

        if (force > breakingForce.x)
        {
            Break(forceDir * force, forceDir * force);
        }
        else
        {
            breakingForce -= new Vector2(force, force);
        }

        //have original object to keep flying
        if (!RBInConnected(mo.Rb))
        {
            mo.CarryOnVelocity(originalSpeed * forceTransfer);
        }
    }


    IEnumerator DelayBreakBottom()
    {
        // Debug.Log($"{this} bottom break start.");

        yield return new WaitForSeconds(BreakDelay);
        if (!IsBroken() && !HasBottomPart())
        {
            Break(new Vector3(), new Vector3());
            if (isDebug)
            {
                rendererMaterial.color = Color.blue;
            }
        }
    }

    protected override void AddDetectedPart(BreakableComponent current)
    {
        if (current is BreakablePart bp)
        {
            AddPart(bp);
            bp.AddOtherPart(this);
        }
        else
        {
            base.AddDetectedPart(current);
        }
    }


    //******Despawning
    public override void Despawn()
    {
        try
        {
            if (!gameObject || breakableState == BreakableState.Despawn)
            {
                return;
            }

            selfRB.isKinematic = true;
            breakableState = BreakableState.Despawn;
            despawnEvent.Invoke();
            LeanTween.scale(gameObject, Vector3.zero, despawnTime);
            Destroy(gameObject, despawnTime + 1f);
            if (isDebug)
            {
                rendererMaterial.color = Color.yellow;
            }
        }
        catch (MissingReferenceException e)
        {
            // Console.WriteLine(e);
            return;
        }
        
    }

    //************Launching Part

    public virtual void Telekinesis()
    {
        ChangeState(BreakableState.Telekinesis);
        collider.enabled = false;
    }

    public virtual void Launch(Vector3 accel, List<BreakablePart> parts = null)
    {
        ChangeState(BreakableState.Telekinesis_Shoot);
        collider.enabled = true;
        SelfRb.AddForce(accel, ForceMode.Acceleration);
    }
    
    
}