using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{

    [SerializeField]
    private Collider[] allColliders;

    [SerializeField]
    private BreakablePart[] breakableParts;

    [Header("RigidBody")]
    [SerializeField]
    private float mass = 10f;

    [SerializeField]
    private float drag = 0f;

    [SerializeField]
    private PhysicMaterial physicMaterial;

    [Header("Break Part")]
    [SerializeField]
    private float affectedRange = 20f;
    [SerializeField]
    private float breakForce = 200f;

    [SerializeField]
    [Range(0f,1f)]
    private float forceTransfer = .5f;

    [SerializeField]
    private LayerMask bpLayer;
    


    [ContextMenu("FindAllColliders")]
    public void GetAllColliders()
    {
        allColliders = GetComponentsInChildren<Collider>();
    }

    private void Awake()
    {
        foreach (BreakablePart breakablePart in breakableParts)
        {
            SetBP(breakablePart);
        }
    }

    public void AddRigidBodyToColliders()
    {
        Rigidbody rb;
        BreakablePart bp;
        List<BreakablePart> tempBPs =new List<BreakablePart>();
        foreach (Collider c in allColliders)
        {
            ((MeshCollider) c).convex = true;
            c.material = physicMaterial;
            if (!c.TryGetComponent(out rb))
            {
                rb = c.AddComponent<Rigidbody>();
            }
            
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = drag;
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            if (!c.TryGetComponent(out bp))
            {
                bp = c.AddComponent<BreakablePart>();
            }
            tempBPs.Add(bp);

        }

        breakableParts = tempBPs.ToArray();
        foreach (BreakablePart breakablePart in breakableParts)
        {
            SetBP(breakablePart);
        }

        foreach (BreakablePart breakablePart in breakableParts)
        {
            breakablePart.InitialiseClosest();
        }

    }

    private void SetBP(BreakablePart bp)
    {
        bp.AffectiveRange = affectedRange;
        bp.BreakingForce = breakForce;
        bp.ForceTransfer = forceTransfer;
        bp.CastLayer = bpLayer;
        bp.Initialise(transform);

    }
    
    

    [ContextMenu("InitialiseBuilding")]
    public void Initialise()
    {
        GetAllColliders();
        AddRigidBodyToColliders();
    }
    
}
