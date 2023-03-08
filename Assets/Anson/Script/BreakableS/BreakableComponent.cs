using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// [Serializable]
// public class BreakableData
// {
//     [SerializeField]
//     private BreakableComponent component;
//
//     [SerializeField]
//     private float distance;
//
//     // [SerializeField]
//     // [Tooltip("Min, Max")]
//     // private Vector2 breakForceLimit;
//
//
//     [SerializeField]
//     private Vector3 dir;
//
//     public BreakableComponent Component => component;
//     // public BreakableComponent Part => component as BreakableComponent;
//
//     public float Distance => distance;
//
//     // public Vector2 BreakForceLimit => breakForceLimit;
//
//     public Vector3 Dir => dir;
//
//     public BreakableData(BreakableComponent component, float distance, Vector2 breakForceLimit, Vector3 dir)
//     {
//         this.component = component;
//         this.distance = distance;
//         // this.breakForceLimit = breakForceLimit;
//         this.dir = dir;
//     }
//
//     public override bool Equals(object obj)
//     {
//         if (obj is BreakableComponent other)
//         {
//             other.Equals(component);
//             return true;
//         }
//
//         return base.Equals(obj);
//     }
//
//     public override int GetHashCode()
//     {
//         return base.GetHashCode();
//     }
// }

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
    protected PhysicMaterial physicMaterial;

    [SerializeField]
    protected BreakableStructureController breakableStructureController;

    [SerializeField]
    protected List<BreakableComponent> connectedParts;

    [SerializeField]
    protected List<BreakableComponent> otherConnectedParts;

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
    protected Vector2 breakDelay;

    [SerializeField]
    protected float minBottomAngle = 80f;

    [SerializeField]
    protected LayerMask groundLayer;

    [SerializeField]
    protected bool isGroundPiece = false;

    [SerializeField]
    protected bool originalNoBottom = false;

    public bool IsGroundPiece => isGroundPiece;

    public bool OriginalNoBottom => originalNoBottom;

    [Header("Connection")]
    [SerializeField]
    protected float affectiveRange = 10f;

    [SerializeField]
    protected LayerMask castLayer;

    [SerializeField]
    private bool ignoreParent = false;

    [Header("Break")]
    [SerializeField]
    protected UnityEvent breakEvent;

    protected bool isStoredExplosive = false;
    protected Vector3 storedExplosiveForce = new Vector3();
    protected Vector3 storedExplosiveVelocity = new Vector3();

    [Header("Despawning")]
    [SerializeField]
    protected UnityEvent despawnEvent;

    [SerializeField]
    protected float despawnTime = 5f;


    [Header("Debug")]
    protected bool isDebug = false;


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

    [SerializeField]
    protected BreakableCollective collectiveParent;

    protected Material rendererMaterial;

    public Rigidbody SelfRb => selfRB;

    public Vector3 Position => transform.position;

    public float BreakDelay => UnityEngine.Random.Range(breakDelay.x, breakDelay.y);

    public bool IsTooSmall => meshSize < minimumPartSize;

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
    }

    public BreakableState BreakableState => breakableState;

    public bool isTelekinesis => breakableState is BreakableState.Telekinesis or BreakableState.Telekinesis_Shoot;


    public virtual void ResetConnections()
    {
        parent = null;
        collectiveParent = null;
        connectedParts = new List<BreakableComponent>();
        otherConnectedParts = new List<BreakableComponent>();
    }

    public virtual void Initialise(GameObject p, BreakableStructureController bsc, float mass, float drag,
        float affectedRange, Vector2 breakForce,
        float forceTransfer, LayerMask bpLayer, AnimationCurve transferToDot, float minSize, Vector2 breakDelay,
        float bottomAngle, PhysicMaterial pm, UnityEvent breakEvent1, float despawnTime1, UnityEvent despawnEvent1,
        LayerMask groundLayer1, bool ignoreParent1)
    {
        if (parent)
        {
            bool hasCollective = parent.TryGetComponent(out BreakableCollective breakableCollective);
            if (hasCollective)
            {
                parent = breakableCollective.parent;
                collectiveParent = breakableCollective;
            }
            else
            {
                parent = p;
            }
        }
        else
        {
            parent = p;
        }


        Initialise();
        InitialiseValues(mass, bsc, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot, minSize,
            breakDelay, bottomAngle, pm, breakEvent1, despawnTime1, despawnEvent1, groundLayer1, ignoreParent1);
    }

    protected virtual void InitialiseValues(float mass, BreakableStructureController bsc, float drag,
        float affectedRange, Vector2 breakForce,
        float forceTransfer,
        LayerMask bpLayer, AnimationCurve transferToDot, float minSize, Vector2 breakDelay, float bottomAngle,
        PhysicMaterial pm, UnityEvent breakEvent1, float despawnTime1, UnityEvent despawnEvent1, LayerMask groundLayer1,
        bool ignoreParent1)
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
        float massSum = Mathf.RoundToInt(mass * meshSize);
        if (Math.Abs(mass - selfRB.mass) > .1f)
        {
            selfRB.mass = massSum;
        }

        selfRB.drag = drag;
        selfRB.angularDrag = drag;
        selfRB.isKinematic = true;
        selfRB.useGravity = false;
        selfRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        this.transferToDot = transferToDot;
        breakEvent = breakEvent1;
        despawnTime = despawnTime1;
        despawnEvent = despawnEvent1;
        groundLayer = groundLayer1;
        InitialiseGround();
        ignoreParent = ignoreParent1;
    }

    public virtual int InitialiseGround()
    {
        isGroundPiece = GroundCheck();
        originalNoBottom = !HasBottomPart();
        if (isGroundPiece)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    [ContextMenu("Initialise")]
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

        connectedParts = new List<BreakableComponent>();
        otherConnectedParts = new List<BreakableComponent>();
    }

    protected virtual bool GroundCheck()
    {
        bool b = false;
        if (breakableStructureController)
        {
            b = Physics.CheckSphere(transform.position + Vector3.down * meshSize * .4f, meshSize / 2f,
                groundLayer);
        }

        // b = b || !HasBottomPart();

        return b;
    }


    public virtual void Break(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f, bool forceBreak = false, Vector3 originPoint = default)
    {
        DebrisManager.Add(this);
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
            try
            {
                BreakableComponent current = connectedParts[i];
                if (current.Equals(part))
                {
                    // print($"{this} remove connection: {part}");
                    connectedParts.RemoveAt(i);
                    return;
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
        }

        for (int i = 0; i < otherConnectedParts.Count; i++)
        {
            try
            {
                BreakableComponent current = otherConnectedParts[i];

                if (current.Equals(part))
                {
                    otherConnectedParts.RemoveAt(i);
                    return;
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
            }
        }
    }

    protected void AddPart(BreakableComponent current)
    {
        // BreakableComponent item = CalculatePartDistance(current);

        if (!connectedParts.Contains(current))
        {
            connectedParts.Add(current);
        }
    }

    protected void AddOtherPart(BreakableComponent current)
    {
        // BreakableData item = CalculatePartDistance(current);

        if (!otherConnectedParts.Contains(current) && !connectedParts.Contains(current))
        {
            otherConnectedParts.Add(current);
        }
    }

    // protected BreakableData CalculatePartDistance(BreakableComponent bp)
    // {
    //     float d = Mathf.Abs((bp.transform.position - transform.position).magnitude);
    //     // float f = Mathf.Lerp(breakingForce, bp.breakingForce, d / affectiveRange);
    //     Vector2 f = bp.breakingForce;
    //     return new BreakableData(bp, d, f, (bp.transform.position - transform.position).normalized);
    // }

    public virtual List<BreakableComponent> InitialiseClosest(bool ignoreTooSmall = false)
    {
        float CastSizeMultiplier = .25f;
        float range = affectiveRange + meshSize * CastSizeMultiplier;
        Collider[] colliders = Physics.OverlapSphere(transform.position, range,
            castLayer);
        // List<PartDistance> tempParts = new List<PartDistance>();
        BreakableCollective bcTemp;
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out Rigidbody rigidbody))
            {
                if (!rigidbody.Equals(selfRB))
                {
                    if (collider.TryGetComponent(out BreakableComponent current))
                    {
                        if (current is BreakableCollective breakableCollective)
                        {
                            print("collective found");
                        }

                        if ((ignoreParent || current.parent.Equals(parent)) && !current.Equals(this))
                        {
                            if (!(ignoreTooSmall && current.IsTooSmall))
                            {
                                AddDetectedPart(current);
                            }
                        }
                    }
                }
            }
            else
            {
                bcTemp = collider.GetComponentInParent<BreakableCollective>();
                if (bcTemp)
                {
                    if (!bcTemp.parent.Equals(parent))
                    {
                        AddDetectedPart(bcTemp);
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

        if (isGroundPiece)
        {
            return true;
        }
        // double cos = Math.Cos(minBottomAngle*Mathf.Deg2Rad);

        foreach (BreakableComponent connectedPart in connectedParts)
        {
            if (!connectedPart.IsBroken())
            {
                //sht makes no sense but it's dotting the other way
                Vector3 dir = connectedPart.transform.position - transform.position;
                if (Vector3.Angle(Vector3.up, dir) > minBottomAngle)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public virtual void EvaluateBreak(BreakableComponent pd, Vector3 force, BreakableComponent originalPart,
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

    public virtual void Despawn(bool instant)
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
        if (breakableStructureController)
        {
            breakableStructureController.QueueBreakEffects(transform.position, meshFilter.mesh);
        }
    }

    protected virtual void AddScore()
    {
        if (breakableStructureController)
        {
            breakableStructureController.AddScore(meshSize);
        }
    }

    protected void EvaluateCollisionBreak(Collision collision)
    {
        if (breakableState is BreakableState.Despawn)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent(out BreakablePart bp))
        {
            try
            {
                if (!bp.Parent.Equals(parent) || bp.BreakableState == BreakableState.Free)
                {
                    CollisionBreak(bp.selfRB);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Null ref error from {this}");

                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
            }
        }
        else if (collision.gameObject.TryGetComponent(out MovableObject mo))
        {
            try
            {
                CollisionBreak(mo.Rb);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Null ref error from {this}");

                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
            }
        }
        else if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            try
            {
                CollisionBreak(rb);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError($"Null ref error from {this}");

                Debug.LogError(e);
                Debug.LogError(e.StackTrace);
            }
        }
    }

    public virtual void ExplodeBreak(Vector3 force, Vector3 point, Vector3 velocity = new Vector3())
    {
        switch (breakableState)
        {
            case BreakableState.Hold:
                if (force.magnitude > breakingForce.x)
                {
                    Break(force, force);
                }

                DelayAddForce(force, velocity);

                break;
            case BreakableState.InitialBreak:
                DelayAddForce(force, velocity);

                break;
            case BreakableState.Free:
                DelayAddForce(force, velocity);
                ApplyStoredForce();
                break;
        }
    }

    protected Vector3 Dir(BreakableComponent target)
    {
        return (target.transform.position - transform.position).normalized;
    }

    public void SetOriginalNoBottom(bool b)
    {
        originalNoBottom = b;
    }

    protected void DelayAddForce(Vector3 force, Vector3 velocity)
    {
        isStoredExplosive = true;
        storedExplosiveForce += force;
        storedExplosiveVelocity += velocity;
    }

    protected void ApplyStoredForce()
    {
        isStoredExplosive = false;
        selfRB.AddForce(storedExplosiveForce);
        selfRB.AddForce(storedExplosiveVelocity, ForceMode.VelocityChange);
        storedExplosiveForce = new Vector3();
        storedExplosiveVelocity = new Vector3();
    }
}