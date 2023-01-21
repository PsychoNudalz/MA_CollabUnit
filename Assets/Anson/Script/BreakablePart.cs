using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    private PartDistance[] connectedParts;

    [SerializeField]
    private BreakableState breakableState = BreakableState.Hold;
    [Header("Stats")]
    [SerializeField]
    private float breakingForce;
    [SerializeField]
    [Range(0f,1f)]
    private float forceTransfer = .5f;
    [SerializeField]
    private float affectiveRange = 10f;




    [Header("Debug")]


    [SerializeField]
    private LayerMask castLayer;

    [SerializeField]
    private Transform parent;

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

    private void OnDrawGizmosSelected()
    {
        if (connectedParts?.Length > 0)
        {
            foreach (PartDistance connectedPart in connectedParts)
            {
                Gizmos.DrawLine(transform.position, connectedPart.Part.transform.position);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            // if (!RBInConnected(rb))
            // {
                Collision(rb);
            // }
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

    public void Initialise(Transform p)
    {
        parent = p;
        Initialise();
    }

    [ContextMenu("Initialise")]
    public void Initialise()
    {
        if (!selfRB)
        {
            selfRB = GetComponent<Rigidbody>();
        }

        connectedParts = new PartDistance[]{};
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, affectiveRange, transform.up, castLayer);
        List<PartDistance> tempParts = new List<PartDistance>();
        foreach (RaycastHit currentHit in hits)
        {
            BreakablePart current;
            if (currentHit.collider.TryGetComponent(out current))
            {
                if (current.parent.Equals(parent)&&!current.Equals(this))
                {
                    tempParts.Add(CalculatePartDistance(current));
                }
            }
        }

        connectedParts = tempParts.ToArray();
    }

    public PartDistance CalculatePartDistance(BreakablePart bp)
    {
        float d = Mathf.Abs((bp.transform.position - transform.position).magnitude);
        // float f = Mathf.Lerp(breakingForce, bp.breakingForce, d / affectiveRange);
        float f = bp.breakingForce;
        return new PartDistance(bp, d, f,(bp.transform.position-transform.position).normalized);
    }

    public void Break_Recursive(Vector3 force,Vector3 originalForce)
    {
        Break_Single(force,originalForce);
        force *= forceTransfer;
        // print($"{this} Force: {force}");
        Vector3 newForce = force;
        foreach (PartDistance connectedPart in connectedParts)
        {
            float recursiveThreshold = 2f;
            newForce = (connectedPart.Dir + force).normalized * force.magnitude;
            if (force.magnitude > connectedPart.BreakForceLimit/(1-forceTransfer))
            {
                
                connectedPart.Part.Break_Recursive(newForce,originalForce);
            }else if(force.magnitude > connectedPart.BreakForceLimit)
            {
                connectedPart.Part.Break_Single(newForce,originalForce);
            }
        }
        
    }

    public void Break_Single(Vector3 force,Vector3 originalForce)
    {
        if (breakableState == BreakableState.Broke)
        {
            return;
        }

        breakableState = BreakableState.Broke;
        selfRB.isKinematic = false;
        selfRB.useGravity = true;
        selfRB.AddForce(force.normalized*originalForce.magnitude*selfRB.mass);
    }

    public void Collision(Rigidbody rb)
    
    {
        if (breakableState == BreakableState.Broke)
        {
            return;
        }
        float force = rb.velocity.magnitude * rb.mass;
        if (force > breakingForce * .7f)
        {
            print($"Collided with {rb} with force: {force}  Against: {breakingForce}");
        }

        if (force > breakingForce)
        {
            Break_Recursive(rb.velocity.normalized * force,rb.velocity.normalized * force);
        }
    }
}