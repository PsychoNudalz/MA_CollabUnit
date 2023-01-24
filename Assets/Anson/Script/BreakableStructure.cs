using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BreakableStructure : MonoBehaviour
{
    [SerializeField]
    private Collider[] allColliders;

    [SerializeField]
    private BreakableComponent[] breakableComponents;

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
    private Vector2 breakForce = new Vector2(100f,150f);

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
    private float breakDelay;

    [SerializeField]
    private float minBottomAngle;

    public BreakableComponent[] BreakableComponents => breakableComponents;

    [ContextMenu("FindAllColliders")]
    public void GetAllColliders()
    {
        allColliders = GetComponentsInChildren<Collider>();
    }

    private void Awake()
    {
        // foreach (BreakableComponent BreakableComponent in BreakableComponents)
        // {
        //     SetBP(BreakableComponent);
        // }
    }

    public void AddRigidBodyToColliders()
    {
        Rigidbody rb;
        BreakablePart bp;
        BreakableComponent bc;
        List<BreakableComponent> tempBPs = new List<BreakableComponent>();
        foreach (Collider c in allColliders)
        {
            if (c is MeshCollider mc)
            {
                mc.convex = true;
            }

            c.material = physicMaterial;
            if (!c.TryGetComponent(out rb))
            {
                rb = c.AddComponent<Rigidbody>();
            }

            if (!c.TryGetComponent(out MovableObject mo))
            {
                mo = c.AddComponent<MovableObject>();
            }
            // if (c.TryGetComponent(out bc))
            // {
            //     c.Componen
            // }

            if (!c.TryGetComponent(out bp))
            {
                bp = c.AddComponent<BreakablePart>();
            }

            tempBPs.Add(bp);
        }

        breakableComponents = tempBPs.ToArray();
    }

    private void SetBreakConnections()
    {
        foreach (BreakableComponent BreakableComponent in breakableComponents)
        {
            BreakableComponent.InitialiseClosest();
        }
    }

    private void InitialiseBreakComponents()
    {
        foreach (BreakableComponent BreakableComponent in breakableComponents)
        {
            SetBP(BreakableComponent);
        }
    }

    private void SetBP(BreakableComponent bp)
    {
        bp.Initialise(gameObject, mass, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot,
            minimumPartSize, breakDelay,minBottomAngle);
    }

    public void ResetConnections()
    {
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.ResetConnections();
        }
    }

    [ContextMenu("initialise")]
    public void Initialise()
    {
        GetAllColliders();
        ResetColliderNames();
        AddRigidBodyToColliders();
        
    }

    [ContextMenu("Update Building")]
    public void UpdateValues()
    {
        InitialiseBreakComponents();
        SetBreakConnections();
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
}