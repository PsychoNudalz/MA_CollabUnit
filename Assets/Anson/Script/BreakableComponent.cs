using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BreakableData
{
    [SerializeField]
    private BreakableComponent component;

    [SerializeField]
    private float distance;

    // [SerializeField]
    // [Tooltip("Min, Max")]
    // private Vector2 breakForceLimit;


    [SerializeField]
    private Vector3 dir;

    public BreakableComponent Component => component;
    // public BreakableComponent Part => component as BreakableComponent;

    public float Distance => distance;

    // public Vector2 BreakForceLimit => breakForceLimit;

    public Vector3 Dir => dir;

    public BreakableData(BreakableComponent component, float distance, Vector2 breakForceLimit, Vector3 dir)
    {
        this.component = component;
        this.distance = distance;
        // this.breakForceLimit = breakForceLimit;
        this.dir = dir;
    }

    public override bool Equals(object obj)
    {
        if (obj is BreakableComponent other)
        {
            other.Equals(component);
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
    Free,
    Stay,
    Despawn,
    Telekinesis,
    Telekinesis_Shoot
}


public class BreakableComponent : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody selfRB;

    [SerializeField]
    protected Collider collider;

    [SerializeField]
    private PhysicMaterial physicMaterial;

    [SerializeField]
    private BreakableStructureController breakableStructureController;

    [SerializeField]
    protected List<BreakableData> connectedParts;

    [SerializeField]
    protected List<BreakableData> otherConnectedParts;

    [SerializeField]
    protected BreakableState breakableState = BreakableState.Hold;

    [Header("Stats")]
    [SerializeField]
    [Tooltip("Min, Max")]
    protected Vector2 breakingForce;

    [SerializeField]
    [Range(0f, 1f)]
    protected float forceTransfer = .5f;

    [SerializeField]
    protected AnimationCurve transferToDot;

    [SerializeField]
    protected float breakDelay = .5f;

    [SerializeField]
    protected float minBottomAngle = 80f;

    [Header("Connection")]
    [SerializeField]
    protected float affectiveRange = 10f;

    [SerializeField]
    protected LayerMask castLayer;

    [Header("Break")]
    [SerializeField]
    protected UnityEvent breakEvent;

    [Header("Despawning")]
    [SerializeField]
    protected UnityEvent despawnEvent;

    [SerializeField]
    protected float despawnTime = 5f;


    [Header("Debug")]
    protected bool isDebug = true;


    [SerializeField]
    protected bool forceShowConnection = false;

    [Space(10)]
    [SerializeField]
    protected float meshSize = 0f;

    [SerializeField]
    protected float minimumPartSize = 2f;

    [SerializeField]
    protected float finalBrokeForce = 0f;

    protected float fullBreakTime = .5f;

    [Space(10)]
    [SerializeField]
    protected Renderer renderer;

    [SerializeField]
    protected MeshFilter meshFilter;


    [SerializeField]
    protected GameObject parent;

    protected Material rendererMaterial;

    public Rigidbody SelfRb => selfRB;


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


    public virtual void ResetConnections()
    {
        connectedParts = new List<BreakableData>();
        otherConnectedParts = new List<BreakableData>();
    }

    public virtual void Initialise(GameObject p, BreakableStructureController bsc, float mass, float drag,
        float affectedRange, Vector2 breakForce,
        float forceTransfer, LayerMask bpLayer, AnimationCurve transferToDot, float minSize, float breakDelay,
        float bottomAngle, PhysicMaterial pm, UnityEvent breakEvent1, float despawnTime1, UnityEvent despawnEvent1)
    {
        if (parent && !parent.TryGetComponent(out BreakableCollective _))
        {
            parent = p;
        }
        else if (!parent)
        {
            parent = p;
        }


        Initialise();
        InitialiseValues(mass, bsc, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot, minSize,
            breakDelay, bottomAngle, pm, breakEvent1, despawnTime1, despawnEvent1);
    }

    protected virtual void InitialiseValues(float mass, BreakableStructureController bsc, float drag,
        float affectedRange, Vector2 breakForce,
        float forceTransfer,
        LayerMask bpLayer, AnimationCurve transferToDot, float minSize, float breakDelay, float bottomAngle,
        PhysicMaterial pm, UnityEvent breakEvent1, float despawnTime1, UnityEvent despawnEvent1)
    {
        breakableStructureController = bsc;
        physicMaterial = pm;
        meshSize = meshFilter.sharedMesh.bounds.size.magnitude * transform.lossyScale.x;
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
        breakEvent = breakEvent1;
        despawnTime = despawnTime1;
        despawnEvent = despawnEvent1;
    }

    public virtual void Initialise()
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

        if (!collider)
        {
            collider = GetComponent<Collider>();
        }
            
        connectedParts = new List<BreakableData>();
        otherConnectedParts = new List<BreakableData>();

    }


    public virtual void Break(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f, bool forceBreak = false, Vector3 originPoint = default)
    {
        BreakableManager.Add(this);
    }

    public virtual void CollisionBreak(Rigidbody rb, Collision collision = null, Vector3 point = default)
    {
    }
    public virtual void CollisionBreak(MovableObject mo, Collision collision = null, Vector3 point = default)
    {
    }


    protected bool IsBroken()
    {
        return breakableState is not BreakableState.Hold;
    }

    public virtual void RemovePart(BreakableComponent part)
    {
        for (int i = 0; i < connectedParts.Count; i++)
        {
            BreakableData current = connectedParts[i];
            if (current.Component.Equals(part))
            {
                // print($"{this} remove connection: {part}");
                connectedParts.RemoveAt(i);
                return;
            }
        }

        for (int i = 0; i < otherConnectedParts.Count; i++)
        {
            BreakableData current = otherConnectedParts[i];
            if (current.Component.Equals(part))
            {
                // print($"{this} remove connection: {part}");
                otherConnectedParts.RemoveAt(i);
                return;
            }
        }
    }

    protected void AddPart(BreakableComponent current)
    {
        BreakableData item = CalculatePartDistance(current);

        if (!connectedParts.Contains(item))
        {
            connectedParts.Add(item);
        }
    }

    protected void AddOtherPart(BreakableComponent current)
    {
        BreakableData item = CalculatePartDistance(current);

        if (!otherConnectedParts.Contains(item) && !connectedParts.Contains(item))
        {
            otherConnectedParts.Add(item);
        }
    }

    protected BreakableData CalculatePartDistance(BreakableComponent bp)
    {
        float d = Mathf.Abs((bp.transform.position - transform.position).magnitude);
        // float f = Mathf.Lerp(breakingForce, bp.breakingForce, d / affectiveRange);
        Vector2 f = bp.breakingForce;
        return new BreakableData(bp, d, f, (bp.transform.position - transform.position).normalized);
    }

    public virtual List<BreakableData> InitialiseClosest()
    {
        float CastSizeMultiplier = .25f;
        float range = affectiveRange + meshSize * CastSizeMultiplier;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, range,
            Vector3.up, 0, castLayer);
        // List<PartDistance> tempParts = new List<PartDistance>();
        foreach (RaycastHit currentHit in hits)
        {
            if (currentHit.collider.TryGetComponent(out Rigidbody rigidbody))
            {
                if (!rigidbody.Equals(selfRB))
                {
                    if (currentHit.collider.TryGetComponent(out BreakableComponent current))
                    {
                        if (current.parent.Equals(parent) && !current.Equals(this))
                        {
                            AddDetectedPart(current);
                        }
                    }
                }
            }
        }

        return connectedParts;
    }

    protected virtual void AddDetectedPart(BreakableComponent current)
    {
        AddPart(current);
        //adding for other
        current.AddOtherPart(this);
    }

    protected IEnumerator DelayToFullBreak()
    {
        yield return new WaitForSeconds(fullBreakTime);
        breakableState = BreakableState.Free;
    }

    protected bool HasBottomPart()
    {
        if (connectedParts.Count == 0)
        {
            return false;
        }
        // double cos = Math.Cos(minBottomAngle*Mathf.Deg2Rad);

        foreach (BreakableData connectedPart in connectedParts)
        {
            // float dot = Vector3.Dot(Vector3.up, connectedPart.Dir);
            //
            // if(dot>cos)
            // {
            //     return true;
            // }

            //sht makes no sense but it's dotting the other way
            if (Vector3.Angle(Vector3.up, connectedPart.Dir) > minBottomAngle)
            {
                return true;
            }
        }

        return false;
    }

    public virtual void EvaluateBreak(BreakableData pd, Vector3 force, BreakableComponent originalPart,
        List<BreakableComponent> breakHistory)
    {
    }

    public virtual void EvaluateFall()
    {
    }

    protected virtual float CalculateForce(Rigidbody rb, out Vector3 originalSpeed, out Vector3 forceDir)
    {
        float force = 0f;
        originalSpeed = new Vector3();
        forceDir = new Vector3();
        originalSpeed = rb.velocity;
        force = rb.velocity.magnitude * rb.mass;
        forceDir = (transform.position - rb.transform.position).normalized;

        return force;
    }

    protected virtual float CalculateForce(MovableObject movableObject, out Vector3 originalSpeed, out Vector3 forceDir)
    {
        float force = 0f;
        originalSpeed = new Vector3();
        forceDir = new Vector3();
        originalSpeed = movableObject.Velocity;
        force = originalSpeed.magnitude * movableObject.Rb.mass;
        forceDir = originalSpeed.normalized;

        return force;
    }

    public virtual BreakableComponent AddComponents(PhysicMaterial physicMaterial)
    {
        Rigidbody rb;
        BreakablePart bp;
        BreakableComponent bc;
        if (TryGetComponent(out Collider c))
        {
            if (c is MeshCollider mc)
            {
                mc.convex = true;
            }

            c.material = physicMaterial;
        }

        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        if (!TryGetComponent(out MovableObject mo))
        {
            mo = gameObject.AddComponent<MovableObject>();
        }

        if (!TryGetComponent(out bc))
        {
            bp = gameObject.AddComponent<BreakablePart>();
            bc = bp;
        }

        return bc;
    }

    public virtual void Despawn()
    {
        breakableState = BreakableState.Despawn;
        if (gameObject)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void ChangeState(BreakableState bs)
    {
        switch (breakableState)
        {
            case BreakableState.Hold:
                break;
            case BreakableState.InitialBreak:
                break;
            case BreakableState.Free:
                break;
            case BreakableState.Stay:
                break;

            
            
        }
        breakableState = bs;
        switch (bs)
        {
            case BreakableState.Hold:
                break;
            case BreakableState.InitialBreak:
                break;
            case BreakableState.Free:
                break;
            case BreakableState.Stay:
                break;

            
        }
    }

    protected virtual void PlayBreakEffects()
    {
        breakEvent.Invoke();
        if (breakableStructureController)
        {
            breakableStructureController.PlayBreakEffects(transform.position,meshFilter.mesh);
        }
    }
}