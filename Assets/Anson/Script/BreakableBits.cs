using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableBits : MonoBehaviour
{

    [SerializeField]
    private Collider[] allColliders;

    [Header("RigidBody")]
    [SerializeField]
    private float mass = 10f;

    [SerializeField]
    private float drag = 0f;

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

    public void AddRigidBodyToColliders()
    {
        Rigidbody rb;
        BreakablePart bp;
        foreach (Collider c in allColliders)
        {
            ((MeshCollider) c).convex = true;
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

            bp.AffectiveRange = affectedRange;
            bp.BreakingForce = breakForce;
            bp.ForceTransfer = forceTransfer;
            bp.CastLayer = bpLayer;

        }

        foreach (Collider c in allColliders)
        {
            if (c.TryGetComponent(out bp))
            {
                bp.Initialise();
            }
        }
    }

    [ContextMenu("InitialiseBuilding")]
    public void Initialise()
    {
        GetAllColliders();
        AddRigidBodyToColliders();
    }
    
}
