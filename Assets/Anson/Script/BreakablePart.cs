using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct PartDistance
{
    [SerializeField]
    private BreakablePart part;

    [SerializeField]
    private float distance;

    [SerializeField]
    [Tooltip("Min, Max")]
    private Vector2 breakForceLimit;


    [SerializeField]
    private Vector3 dir;

    public BreakablePart Part => part;

    public float Distance => distance;

    public Vector2 BreakForceLimit => breakForceLimit;

    public Vector3 Dir => dir;

    public PartDistance(BreakablePart part, float distance, Vector2 breakForceLimit, Vector3 dir)
    {
        this.part = part;
        this.distance = distance;
        this.breakForceLimit = breakForceLimit;
        this.dir = dir;
    }

    public override bool Equals(object obj)
    {
        if (obj is BreakablePart other)
        {
            other.Equals(part);
            return true;
        }

        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

[Serializable]
public enum BreakableState
{
    Hold,
    InitialBreak,
    FullBreak
}

[Serializable]
public class BreakablePart : MonoBehaviour
{
    [SerializeField]
    private Rigidbody selfRB;

    [SerializeField]
    private List<PartDistance> connectedParts;

    [SerializeField]
    private List<PartDistance> otherConnectedParts;

    [SerializeField]
    private BreakableState breakableState = BreakableState.Hold;

    [Header("Stats")]
    [SerializeField]
    [Tooltip("Min, Max")]
    private Vector2 breakingForce;

    [SerializeField]
    [Range(0f, 1f)]
    private float forceTransfer = .5f;

    [SerializeField]
    private AnimationCurve transferToDot;

    [SerializeField]
    private float breakDelay = .5f;

    [SerializeField]
    private float minBottomAngle = 80f;
    
    [Header("Connection")]
    [SerializeField]
    private float affectiveRange = 10f;

    [SerializeField]
    private LayerMask castLayer;


    [Header("Debug")]
    [SerializeField]
    private bool isDebug = true;


    [SerializeField]
    private bool forceShowConnection = false;

    [Space(10)]
    [SerializeField]
    private float meshSize = 0f;

    [SerializeField]
    private float minimumPartSize = 2f;

    [SerializeField]
    private float finalBrokeForce = 0f;

    private float fullBreakTime = .5f;

    [Space(10)]
    [SerializeField]
    private Renderer renderer;

    [SerializeField]
    private MeshFilter meshFilter;


    [SerializeField]
    private GameObject parent;

    private Material rendererMaterial;


    public Vector2 BreakingForce
    {
        get => breakingForce;
        set => breakingForce = value;
    }

    public float AffectiveRange
    {
        get => affectiveRange;
        set => affectiveRange = value;
    }

    public LayerMask CastLayer
    {
        get => castLayer;
        set => castLayer = value;
    }

    public float ForceTransfer
    {
        get => forceTransfer;
        set => forceTransfer = value;
    }


    public GameObject Parent
    {
        get => parent;
        set => parent = value;
    }

    public BreakableState BreakableState => breakableState;


    private void Awake()
    {
        if (isDebug)
        {
            rendererMaterial = renderer?.material;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (breakableState == BreakableState.Hold || forceShowConnection)
        {
            if (connectedParts?.Count > 0)
            {
                foreach (PartDistance connectedPart in connectedParts)
                {
                    Gizmos.color = Color.white;

                    Gizmos.DrawLine(transform.position, connectedPart.Part.transform.position);
                }
            }

            if (otherConnectedParts?.Count > 0)
            {
                foreach (PartDistance connectedPart in otherConnectedParts)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, connectedPart.Part.transform.position);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out BreakablePart bp))
        {
            if (!bp.Parent.Equals(parent) || bp.BreakableState == BreakableState.FullBreak)
            {
                Collision(bp.selfRB);
            }
        }
        else if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            Collision(rb);
        }
    }

    bool RBInConnected(Rigidbody rb)
    {
        foreach (PartDistance connectedPart in connectedParts)
        {
            if (connectedPart.Part.selfRB.Equals(rb))
            {
                return true;
            }
        }

        return false;
    }

    public void Initialise(GameObject p, float mass, float drag, float affectedRange, Vector2 breakForce,
        float forceTransfer, LayerMask bpLayer, AnimationCurve transferToDot, float minSize, float breakDelay,float bottomAngle)
    {
        parent = p;


        Initialise();
        this.AffectiveRange = affectedRange;
        this.BreakingForce = breakForce * meshSize;
        this.ForceTransfer = forceTransfer;
        this.CastLayer = bpLayer;
        minimumPartSize = minSize;
        this.breakDelay = breakDelay;
        minBottomAngle = bottomAngle;
        selfRB.mass = mass * meshSize;
        selfRB.drag = drag;
        selfRB.angularDrag = drag;
        selfRB.isKinematic = true;
        selfRB.useGravity = false;
        selfRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        this.transferToDot = transferToDot;
    }

    // [ContextMenu("Reset")]

    [ContextMenu("Initialise")]
    public void Initialise()
    {
        if (!selfRB)
        {
            selfRB = GetComponent<Rigidbody>();
        }

        if (!meshFilter)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (isDebug)
        {
            if (!renderer)
            {
                renderer = GetComponent<Renderer>();
            }
        }

        connectedParts = new List<PartDistance>();
        otherConnectedParts = new List<PartDistance>();

        meshSize = meshFilter.sharedMesh.bounds.size.magnitude * transform.lossyScale.x;
        // print($"{this} mesh size: {meshSize}");
        // var tempParts = InitialiseClosest();
        //
    }

    public List<PartDistance> InitialiseClosest()
    {
        float CastSizeMultiplier = .25f;
        float range = affectiveRange + meshSize * CastSizeMultiplier;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, range,
            Vector3.up, 0, castLayer);
        // List<PartDistance> tempParts = new List<PartDistance>();
        foreach (RaycastHit currentHit in hits)
        {
            if (currentHit.collider.TryGetComponent(out BreakablePart current))
            {
                if (current.parent.Equals(parent) && !current.Equals(this))
                {
                    AddPart(current);

                    //adding for other
                    current.AddOtherPart(this);
                }
            }
        }

        return connectedParts;
    }

    public PartDistance CalculatePartDistance(BreakablePart bp)
    {
        float d = Mathf.Abs((bp.transform.position - transform.position).magnitude);
        // float f = Mathf.Lerp(breakingForce, bp.breakingForce, d / affectiveRange);
        Vector2 f = bp.breakingForce;
        return new PartDistance(bp, d, f, (bp.transform.position - transform.position).normalized);
    }


    public void Break(Vector3 force, Vector3 originalForce, List<BreakablePart> breakHistory = null,
        float breakDelay = 0f)
    {
        if (breakDelay == 0f)
        {
            Break_Recursive(force, originalForce, breakHistory, breakDelay);
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
    public void Break_Recursive(Vector3 force, Vector3 originalForce, List<BreakablePart> breakHistory = null,
        float breakDelay = 0f)
    {
        if (IsBroken())
        {
            return;
        }

        if (breakHistory == null)
        {
            breakHistory = new List<BreakablePart>();
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
        PartDistance[] tempPD = connectedParts.ToArray();
        foreach (PartDistance connectedPart in tempPD)
        {
            connectedPart.Part.EvaluateBreak(connectedPart, force, this, breakHistory);
        }

        ApplyForce(force);
    }

    private IEnumerator DelayBreak_Recursive(Vector3 force, Vector3 originalForce, List<BreakablePart> breakHistory,
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
        foreach (PartDistance part in otherConnectedParts)
        {
            part.Part.RemovePart(this);
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
            if (finalBrokeForce < breakingForce.x)
            {
                Debug.LogWarning($"{this} break force {finalBrokeForce} not reaching limit.");
                return;
            }
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
    public void EvaluateBreak(PartDistance pd, Vector3 force, BreakablePart originalPart,
        List<BreakablePart> breakHistory)
    {
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
            StartCoroutine(DelayBreakBottom());
        }
        // else if(force.magnitude > connectedPart.BreakForceLimit)
        // {
        //     connectedPart.Part.Break_Single(newForce,originalForce);
        // }
    }

    private bool IsBroken()
    {
        return breakableState is BreakableState.InitialBreak or BreakableState.FullBreak;
    }

    private void ApplyForce(Vector3 f)
    {
        float multiplier = 1f;
        Vector3 addForce = f * selfRB.mass * multiplier * forceTransfer;
        if (!float.IsNaN(addForce.x))
        {
            selfRB.AddForce(addForce);
        }
    }

    public void Collision(Rigidbody rb)

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


        if (force > breakingForce.x * .7f)
        {
            print($"Collided with {rb} with force: {force}  Against: {breakingForce}");
        }

        if (force > breakingForce.x)
        {
            Break(forceDir * force, forceDir * force);
        }

        //have original object to keep flying
        if (!RBInConnected(rb))
        {
            rb.velocity = originalSpeed * forceTransfer;
        }
    }

    public void RemovePart(BreakablePart part)
    {
        for (int i = 0; i < connectedParts.Count; i++)
        {
            PartDistance current = connectedParts[i];
            if (current.Part.Equals(part))
            {
                // print($"{this} remove connection: {part}");
                connectedParts.RemoveAt(i);
                return;
            }
        }

        for (int i = 0; i < otherConnectedParts.Count; i++)
        {
            PartDistance current = otherConnectedParts[i];
            if (current.Part.Equals(part))
            {
                // print($"{this} remove connection: {part}");
                otherConnectedParts.RemoveAt(i);
                return;
            }
        }
    }

    public void AddPart(BreakablePart current)
    {
        PartDistance item = CalculatePartDistance(current);

        if (!connectedParts.Contains(item))
        {
            connectedParts.Add(item);
        }
    }

    public void AddOtherPart(BreakablePart current)
    {
        PartDistance item = CalculatePartDistance(current);

        if (!otherConnectedParts.Contains(item) && !connectedParts.Contains(item))
        {
            otherConnectedParts.Add(item);
        }
    }

    IEnumerator DelayToFullBreak()
    {
        yield return new WaitForSeconds(fullBreakTime);
        breakableState = BreakableState.FullBreak;
    }

    IEnumerator DelayBreakBottom()
    {
        yield return new WaitForSeconds(breakDelay);

        if (!HasBottomPart())
        {
        
            Debug.Log($"{this} break bottom.");
            Break(Vector3.down*breakingForce.y/forceTransfer*100f, Vector3.down*breakingForce.y*1.1f);
        }
    }

    bool HasBottomPart()
    {
        double cos = Math.Cos(minBottomAngle*Mathf.Deg2Rad);

        foreach (PartDistance connectedPart in connectedParts)
        {
            float dot = Vector3.Dot(Vector3.down, connectedPart.Dir);
            
            if(dot<cos)
            {
                return true;
            }
        }

        return false;
    }
}