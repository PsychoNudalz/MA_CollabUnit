using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Random = UnityEngine.Random;


/// <summary>
/// Controls collection of breakable components
/// </summary>
public class BreakableStructureController : MonoBehaviour
{
    private Collider[] allColliders;


    [SerializeField]
    private BreakableComponent[] allBreakableComponents;

    [SerializeField]
    private BreakablePart[] breakableParts;

    [SerializeField]
    private BreakableCollective[] breakableCollectives;

    [Header("RigidBody")]
    [SerializeField]
    private float mass = 10f;

    [SerializeField]
    private float drag = 0f;

    [SerializeField]
    private PhysicMaterial physicMaterial;

    [Header("Break Part")]
    [SerializeField]
    private bool ignoreParent = false;

    [SerializeField]
    private float affectedRange = 20f;

    [SerializeField]
    private Vector2 breakForce = new Vector2(100f, 150f);

    [SerializeField]
    [Range(0f, 1f)]
    private float forceTransfer = .5f;

    [SerializeField]
    private AnimationCurve transferToDot;

    [SerializeField]
    private LayerMask bpLayer;

    [SerializeField]
    private float minimumPartSize = 2f;

    [SerializeField]
    private Vector2 breakDelayRange = new Vector2(0.5f, 1.5f);

    public float BreakDelay => Random.Range(breakDelayRange.x, breakDelayRange.y);

    [SerializeField]
    private float minBottomAngle;

    [SerializeField]
    private LayerMask groundLayerMask;

    public LayerMask GroundLayerMask => groundLayerMask;

    [Header("Score")]
    private float scorePerMass = 10f;

    [Space(20f)]
    [Header("Effects")]
    [SerializeField]
    private BreakableStructureEffectsController effectsController;

    [SerializeField]
    private UnityEvent breakEvent;


    [Space(10)]
    [SerializeField]
    private float despawnTime = 1f;

    [SerializeField]
    private UnityEvent despawnEvent;

    public BreakableComponent[] AllBreakableComponents => allBreakableComponents;


    public BreakablePart[] BreakableParts => breakableParts;

    public BreakableCollective[] BreakableCollectives => breakableCollectives;

    private void Awake()
    {
        if (!effectsController)
        {
            effectsController = GetComponentInChildren<BreakableStructureEffectsController>();
        }
    }

    private void FixedUpdate()
    {
    }


    [ContextMenu("FindAllColliders")]
    public void GetAllColliders()
    {
        allColliders = GetComponentsInChildren<Collider>(true);
    }


    public void AddComponentsToColliders()
    {
        Rigidbody rb;
        BreakablePart bp;
        BreakableComponent bc;
        List<BreakableComponent> tempBPs = new List<BreakableComponent>();
        foreach (Collider c in allColliders)
        {
            if (ignoreParent || c.transform.parent.Equals(transform))
            {
                if (c is MeshCollider mc)
                {
                    mc.convex = true;
                }

                c.material = physicMaterial;


                bc = c.GetComponentInParent<BreakableComponent>();
                if (!bc)
                {
                    if (!c.TryGetComponent(out rb))
                    {
                        rb = c.AddComponent<Rigidbody>();
                    }

                    if (!c.TryGetComponent(out MovableObject mo))
                    {
                        mo = c.AddComponent<MovableObject>();
                    }

                    if (!c.TryGetComponent(out bc))
                    {
                        bp = c.AddComponent<BreakablePart>();
                    }
                }
            }

            c.gameObject.layer = gameObject.layer;
        }

        InitialiseComponents(GetComponentsInChildren<BreakableComponent>());
    }

    private void InitialiseComponents(BreakableComponent[] bcs)
    {
        List<BreakablePart> tempBPs = new List<BreakablePart>();
        List<BreakableCollective> tempBCs = new List<BreakableCollective>();

        foreach (BreakableComponent component in bcs)
        {
            if (ignoreParent || component.transform.parent.Equals(transform))
            {
                if (component is BreakablePart bp)
                {
                    tempBPs.Add(bp);
                }
                else if (component is BreakableCollective bc)
                {
                    tempBCs.Add(bc);
                }
            }
        }

        allBreakableComponents = bcs;
        breakableParts = tempBPs.ToArray();
        breakableCollectives = tempBCs.ToArray();
    }

    private void SetBreakConnections_Parts()
    {
        foreach (BreakablePart part in breakableParts)
        {
            part.InitialiseClosest();
        }
    }


    private void SetBreakConnections_Collectives()
    {
        foreach (BreakableCollective collective in breakableCollectives)
        {
            collective.InitialiseClosest();
        }
    }

    private void Initialise_BreakParts()
    {
        foreach (BreakablePart part in breakableParts)
        {
            SetBP(part);
        }
    }


    private void Initialise_BreakCollective()
    {
        List<BreakablePart> tempBP = new List<BreakablePart>(breakableParts);
        foreach (BreakableCollective collective in breakableCollectives)
        {
            SetBC(collective);
            if (breakableParts.Length > 0)
            {
                foreach (BreakableComponent breakableComponent in collective.BreakableComponents)
                {
                    if (breakableComponent is BreakablePart part)
                    {
                        if (tempBP.Contains(part))
                        {
                            tempBP.Remove(part);
                        }
                    }
                }
            }
        }
    }

    private void SetBP(BreakablePart bp)
    {
        bp.Initialise(gameObject, this, mass, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot,
            minimumPartSize, BreakDelay, minBottomAngle, physicMaterial, breakEvent, despawnTime, despawnEvent,
            groundLayerMask, ignoreParent);
    }

    private void SetBC(BreakableCollective breakableCollective)
    {
        breakableCollective.Initialise(gameObject, this, mass, drag, affectedRange, breakForce, forceTransfer, bpLayer,
            transferToDot,
            minimumPartSize, BreakDelay, minBottomAngle, physicMaterial, breakEvent, despawnTime, despawnEvent,
            groundLayerMask, ignoreParent);
    }

    public void ResetConnections()
    {
        foreach (BreakableComponent breakableComponent in allBreakableComponents)
        {
            breakableComponent.ResetConnections();
        }
    }

    void InitialiseGround()
    {
        foreach (BreakableCollective breakableCollective in breakableCollectives)
        {
            breakableCollective.InitialiseGround();
        }

        foreach (BreakablePart breakablePart in breakableParts)
        {
            breakablePart.InitialiseGround();
        }
    }

    [ContextMenu("initialise")]
    public void Initialise()
    {
        Awake();
        GetAllColliders();
        ResetColliderNames();
        AddComponentsToColliders();
    }

    [ContextMenu("Update Building")]
    public void UpdateValues()
    {

        Initialise_BreakCollective();
        Initialise_BreakParts();
        SetBreakConnections_Collectives();
        SetBreakConnections_Parts();
        InitialiseGround();

    }

    [ContextMenu("Reset collider names")]
    public void ResetColliderNames()
    {
        int i = 0;
        foreach (Collider c in allColliders)
        {
            c.name = name + "_Cell_" + i;
            i++;
        }
    }

    public void QueueBreakEffects(Vector3 position, Mesh mesh)
    {
        effectsController.QueueBreakEffects(position, mesh);
    }

    public void PlayBreakEffects(Vector3 position, Mesh mesh)
    {
        effectsController.PlayBreakEffects(position, mesh);
        breakEvent.Invoke();
    }

    public void AddScore(float meshSize)
    {
        ScoreManager.AddScore_BySize(scorePerMass, meshSize);
    }
}