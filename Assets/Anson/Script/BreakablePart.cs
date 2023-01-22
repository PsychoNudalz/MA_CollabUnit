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
    private float breakForceLimit;

    [SerializeField]
    private Vector3 dir;

    public BreakablePart Part => part;

    public float Distance => distance;

    public float BreakForceLimit => breakForceLimit;

    public Vector3 Dir => dir;

    public PartDistance(BreakablePart part, float distance, float breakForceLimit, Vector3 dir)
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
    Broke
}

[Serializable]
public class BreakablePart : MonoBehaviour
{
    [SerializeField]
    private Rigidbody selfRB;

    [SerializeField]
    private List<PartDistance> connectedParts;

    [SerializeField]
    private BreakableState breakableState = BreakableState.Hold;

    [Header("Stats")]
    [SerializeField]
    private float breakingForce;

    [SerializeField]
    [Range(0f, 1f)]
    private float forceTransfer = .5f;

    [SerializeField]
    private AnimationCurve transferToDot;

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
    private float finalBrokeForce = 0f;

    [Space(10)]
    [SerializeField]
    private Renderer renderer;

    [SerializeField]
    private MeshFilter meshFilter;


    [SerializeField]
    private Transform parent;

    private Material rendererMaterial;


    public float BreakingForce
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


    public Transform Parent
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
                    Gizmos.DrawLine(transform.position, connectedPart.Part.transform.position);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            // if (!RBInConnected(rb))
            // {
            //     Collision(rb);
            // }
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

    public void Initialise(Transform p, float mass, float drag, float affectedRange, float breakForce,
        float forceTransfer, LayerMask bpLayer,AnimationCurve transferToDot)
    {
        parent = p;


        Initialise();
        this.AffectiveRange = affectedRange;
        this.BreakingForce = breakForce * meshSize;
        this.ForceTransfer = forceTransfer;
        this.CastLayer = bpLayer;
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

        meshSize = meshFilter.sharedMesh.bounds.size.magnitude * transform.lossyScale.x;
        // print($"{this} mesh size: {meshSize}");
        // var tempParts = InitialiseClosest();
        //
    }

    public List<PartDistance> InitialiseClosest()
    {
        float CastSizeMultiplier = .5f;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, affectiveRange + meshSize * CastSizeMultiplier,
            transform.up, castLayer);
        // List<PartDistance> tempParts = new List<PartDistance>();
        foreach (RaycastHit currentHit in hits)
        {
            if (currentHit.collider.TryGetComponent(out BreakablePart current))
            {
                if (current.parent.Equals(parent) && !current.Equals(this))
                {
                    AddPart(current);

                    //adding for other
                    current.AddPart(this);
                }
            }
        }

        return connectedParts;
    }

    public PartDistance CalculatePartDistance(BreakablePart bp)
    {
        float d = Mathf.Abs((bp.transform.position - transform.position).magnitude);
        // float f = Mathf.Lerp(breakingForce, bp.breakingForce, d / affectiveRange);
        float f = bp.breakingForce;
        return new PartDistance(bp, d, f, (bp.transform.position - transform.position).normalized);
    }


    /// <summary>
    /// Recurssively break
    ///
    /// No fking idea what the break will call back to the previous broken piece and cause a loop
    /// </summary>
    /// <param name="force"></param>
    /// <param name="originalForce"></param>
    /// <param name="???"></param>
    public void Break_Recursive(Vector3 force, Vector3 originalForce, List<BreakablePart> breakHistory = null)
    {
        if (breakableState == BreakableState.Broke)
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
            newForce = force * forceTransfer;

            float LerpForce = .5f;
            float dotValue = Mathf.Abs(Vector3.Dot(connectedPart.Dir, newForce.normalized));
            // print(dotValue);
            newForce = Vector3.Lerp(connectedPart.Dir, newForce.normalized, LerpForce) * newForce.magnitude *transferToDot.Evaluate(dotValue) ;
            connectedPart.Part.RemovePart(this);

            if (isDebug)
            {
                // Debug.DrawRay(transform.position,newForce,new Color(1-dotValue,dotValue,0),10f);
            }

            if (newForce.magnitude > connectedPart.BreakForceLimit)
            {
                print($"{this} recursive to: {connectedPart.Part}");
                connectedPart.Part.Break_Recursive(newForce, originalForce, breakHistory);
            }
            // else if(force.magnitude > connectedPart.BreakForceLimit)
            // {
            //     connectedPart.Part.Break_Single(newForce,originalForce);
            // }
        }

        ApplyForce(force);

        // print(string.Join(", ",breakHistory));
    }

    public void Break_Single(Vector3 force, Vector3 originalForce)
    {
        if (breakableState == BreakableState.Broke)
        {
            return;
        }
        if (isDebug)
        {
            // Debug.DrawRay(transform.position,force,Color.blue,5f);
        }

        finalBrokeForce = force.magnitude;
        if (finalBrokeForce < breakingForce)
        {
            Debug.LogWarning($"{this} break force {finalBrokeForce} not reaching limit.");
            return;
        }
        // print($"{this} Breaking with {finalBrokeForce}");

        breakableState = BreakableState.Broke;
        selfRB.isKinematic = false;
        selfRB.useGravity = true;

        if (isDebug)
        {
            rendererMaterial.color = Color.red;
        }
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
        if (breakableState == BreakableState.Broke)
        {
            return;
        }

        float force = 0f;
        Vector3 originalSpeed = new Vector3();
        Vector3 forceDir = new Vector3();
        if (rb.TryGetComponent(out MoveableObject moveableObject))
        {
            originalSpeed = moveableObject.Velocity;
            force = originalSpeed.magnitude * rb.mass;
            forceDir = originalSpeed.normalized;

        }
        else
        {
            originalSpeed = rb.velocity;
            force = rb.velocity.magnitude * rb.mass;
            forceDir = (transform.position-rb.transform.position).normalized;

        }

        
        

        
        if (isDebug)
        {
            // Debug.DrawRay(transform.position, forceDir * force, Color.cyan, 10f);
        }

        
        
        if (force > breakingForce * .7f)
        {
            print($"Collided with {rb} with force: {force}  Against: {breakingForce}");
        }

        if (force > breakingForce)
        {
            Break_Recursive(forceDir * force, forceDir * force);
        }

        //have original object to keep flying
        if (!RBInConnected(rb))
        {
            rb.velocity = originalSpeed*forceTransfer;
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
    }

    public void AddPart(BreakablePart current)
    {
        PartDistance item = CalculatePartDistance(current);

        if (!connectedParts.Contains(item))
        {
            connectedParts.Add(item);
        }
    }
}