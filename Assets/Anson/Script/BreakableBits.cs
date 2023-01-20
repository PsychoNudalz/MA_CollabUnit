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
    


    [ContextMenu("FindAllColliders")]
    public void GetAllColliders()
    {
        allColliders = GetComponentsInChildren<Collider>();
    }

    public void AddRigidBodyToColliders()
    {
        Rigidbody rb;
        foreach (Collider c in allColliders)
        {
            ((MeshCollider) c).convex = true;
            if (!c.TryGetComponent(out rb))
            {
                rb = c.AddComponent<Rigidbody>();
            }
            
            rb.mass = mass;
            
        }
    }

    [ContextMenu("InitialiseBuilding")]
    public void Initialise()
    {
        GetAllColliders();
        AddRigidBodyToColliders();
    }
    
}
