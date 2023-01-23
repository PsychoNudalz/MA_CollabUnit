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
    private AnimationCurve transferToDot;
    [SerializeField]
    private LayerMask bpLayer;

    [SerializeField]
    private float minimumPartSize = 2f;


    [ContextMenu("FindAllColliders")]
    public void GetAllColliders()
    {
        allColliders = GetComponentsInChildren<Collider>();
    }

    private void Awake()
    {
        // foreach (BreakablePart breakablePart in breakableParts)
        // {
        //     SetBP(breakablePart);
        // }
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
            if (!c.TryGetComponent(out MoveableObject mo))
            {
                mo = c.AddComponent<MoveableObject>();
            }


            if (!c.TryGetComponent(out bp))
            {
                bp = c.AddComponent<BreakablePart>();
            }
            tempBPs.Add(bp);

        }

        breakableParts = tempBPs.ToArray();


    }

    private void SetBreakPoints()
    {
        foreach (BreakablePart breakablePart in breakableParts)
        {
            breakablePart.InitialiseClosest();
        }
    }

    private void InitialiseBreakPoints()
    {
        foreach (BreakablePart breakablePart in breakableParts)
        {
            SetBP(breakablePart);
        }
    }

    private void SetBP(BreakablePart bp)
    {

        bp.Initialise(gameObject, mass, drag, affectedRange, breakForce, forceTransfer, bpLayer,transferToDot,minimumPartSize);

    }

    [ContextMenu("initialise")]
    public void Initialise()
    {
        AddRigidBodyToColliders();
        GetAllColliders();
        AddRigidBodyToColliders();
        InitialiseBreakPoints();
        SetBreakPoints();

    }

    [ContextMenu("Update Building")]
    public void UpdateValues()
    {
        GetAllColliders();
        AddRigidBodyToColliders();
        InitialiseBreakPoints();
        SetBreakPoints();

    }
    
}
