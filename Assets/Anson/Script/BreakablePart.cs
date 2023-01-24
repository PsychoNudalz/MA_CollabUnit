using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public enum BreakableState
{
    Hold,
    InitialBreak,
    FullBreak
}

[Serializable]
public class BreakablePart : BreakableComponent
{
    private void Awake()
    {
        if (isDebug)
        {
            rendererMaterial = renderer?.material;
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
                    foreach (BreakableData connectedPart in otherConnectedParts)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(transform.position, connectedPart.Component.transform.position);
                    }
                }

                if (connectedParts?.Count > 0)
                {
                    foreach (BreakableData connectedPart in connectedParts)
                    {
                        Gizmos.color = Color.white;

                        Gizmos.DrawLine(transform.position, connectedPart.Component.transform.position);
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
        if (collision.gameObject.TryGetComponent(out BreakablePart bp))
        {
            if (!bp.Parent.Equals(parent) || bp.BreakableState == BreakableState.FullBreak)
            {
                CollisionBreak(bp.selfRB);
            }
        }
        else if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            CollisionBreak(rb);
        }
    }

    bool RBInConnected(Rigidbody rb)
    {
        foreach (BreakableData connectedPart in connectedParts)
        {
            if (connectedPart.Component.SelfRb.Equals(rb))
            {
                return true;
            }
        }

        return false;
    }

    
    public override void Break(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f, bool forceBreak = false)
    {
        if (breakDelay == 0f)
        {
            Break_Recursive(force, originalForce, breakHistory, breakDelay, forceBreak);
        }
        else
        {
            StartCoroutine(DelayBreak_Recursive(force, originalForce, breakHistory, breakDelay));
        }
    }

    /// <summary>
    /// Recurssively break
    ///
    /// No fking idea what the break will call back to the previous broken piece and cause a loop
    /// </summary>
    /// <param name="force"></param>
    /// <param name="originalForce"></param>
    /// <param name="???"></param>
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
        BreakableData[] tempPD = connectedParts.ToArray();
        foreach (BreakableData connectedPart in tempPD)
        {

            connectedPart.Component.EvaluateBreak(connectedPart, force, this, breakHistory);
        }

        tempPD = otherConnectedParts.ToArray();
        foreach (BreakableData partDistance in tempPD)
        {

            partDistance.Component.EvaluateFall();
        }

        ApplyForce(force);
    }

    private IEnumerator DelayBreak_Recursive(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory,
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
        foreach (BreakableData part in otherConnectedParts)
        {
            part.Component.RemovePart(this);
        }

        //if piece is too small
        if (meshSize < minimumPartSize)
        {
            gameObject.SetActive(false);

            breakableState = BreakableState.FullBreak;

            return;
        }
        else
        {
            // if (finalBrokeForce < breakingForce.x)
            // {
            //     Debug.LogWarning($"{this} break force {finalBrokeForce} not reaching limit.");
            //     return;
            // }
            // print($"{this} Breaking with {finalBrokeForce}");

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
    /// to determine what to do after the force is applied
    /// </summary>
    /// <param name="pd"></param>
    /// <param name="force"></param>
    public override void EvaluateBreak(BreakableData pd, Vector3 force, BreakableComponent originalPart,
        List<BreakableComponent> breakHistory)
    {
        if (!gameObject.activeSelf)
        {
            print($"{this} not active");
            return;
        }

        Vector3 newForce = force * forceTransfer;

        float LerpForce = .5f;
        float dotValue = Mathf.Abs(Vector3.Dot(pd.Dir, newForce.normalized));
        // print(dotValue);
        newForce = Vector3.Lerp(pd.Dir, newForce.normalized, LerpForce) *
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
            Break(newForce, force, breakHistory, this.breakDelay);
        }
        else
        {
            EvaluateFall();
        }
    }

    public override void EvaluateFall()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(DelayBreakBottom());
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

    public override void CollisionBreak(Rigidbody rb, Collision collision = null)

    {
        if (IsBroken())
        {
            return;
        }


        float force = 0f;
        Vector3 originalSpeed = new Vector3();
        Vector3 forceDir = new Vector3();
        if (rb.TryGetComponent(out MovableObject movableObject))
        {
            originalSpeed = movableObject.Velocity;
            force = originalSpeed.magnitude * rb.mass;
            forceDir = originalSpeed.normalized;
        }
        else
        {
            originalSpeed = rb.velocity;
            force = rb.velocity.magnitude * rb.mass;
            forceDir = (transform.position - rb.transform.position).normalized;
        }


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

    IEnumerator DelayBreakBottom()
    {
        // Debug.Log($"{this} bottom break start.");

        yield return new WaitForSeconds(breakDelay);

        if (!IsBroken() && !HasBottomPart())
        {
            // Debug.Log($"{this} break bottom.");
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
}