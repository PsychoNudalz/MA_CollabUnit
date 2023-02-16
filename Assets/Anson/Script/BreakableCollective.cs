using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class BreakableCollective : BreakableComponent
{
    [Header("Collective")]
    [SerializeField]
    private GameObject shell;

    [SerializeField]
    private GameObject fractureParent;

    [SerializeField]
    private BreakableComponent[] breakableComponents;

    [SerializeField]
    private float contactCastRadius = 0.5f;

    [SerializeField]
    private bool onlyConnectToCollectives = true;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out BreakablePart bp))
        {
            if (!bp.Parent.Equals(parent) || bp.BreakableState == BreakableState.Free)
            {
                CollisionBreak(bp.SelfRb, collision);
            }
        }
        else if (collision.gameObject.TryGetComponent(out MovableObject mo))
        {
            CollisionBreak(mo, collision);
        }
        else if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            CollisionBreak(rb, collision);
        }
    }


    public override void ResetConnections()
    {
        base.ResetConnections();
    }

    public override void Initialise(GameObject p, float mass, float drag, float affectedRange, Vector2 breakForce,
        float forceTransfer,
        LayerMask bpLayer, AnimationCurve transferToDot, float minSize, float breakDelay, float bottomAngle,
        PhysicMaterial pm)
    {
        if (!shell || !fractureParent)
        {
            Debug.LogError($"{this} missing game object connections");
            return;
        }

        parent = p;
        Initialise();
        InitialiseValues(mass, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot, minSize,
            breakDelay, bottomAngle, pm);

        fractureParent.SetActive(true);


        Rigidbody rb;
        BreakablePart bp;
        BreakableComponent bc;
        foreach (Collider c in fractureParent.GetComponentsInChildren<Collider>())
        {
            if (c is MeshCollider mc)
            {
                mc.convex = true;
            }

            c.material = pm;
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


        breakableComponents = fractureParent.GetComponentsInChildren<BreakableComponent>();
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.AddComponents(pm);
            breakableComponent.Initialise(gameObject, mass, drag, affectedRange, breakForce, forceTransfer, bpLayer,
                transferToDot,
                minimumPartSize, breakDelay, minBottomAngle, pm);
        }

        fractureParent.SetActive(false);
    }

    protected override void InitialiseValues(float mass, float drag, float affectedRange, Vector2 breakForce,
        float forceTransfer,
        LayerMask bpLayer, AnimationCurve transferToDot, float minSize, float breakDelay, float bottomAngle,
        PhysicMaterial pm)
    {
        base.InitialiseValues(mass, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot, minSize,
            breakDelay, bottomAngle, pm);
    }

    public override void Initialise()
    {
        base.Initialise();
        if (!meshFilter)
        {
            meshFilter = shell.GetComponentInChildren<MeshFilter>();
        }
    }

    public override void Break(Vector3 force, Vector3 originalForce, List<BreakableComponent> breakHistory = null,
        float breakDelay = 0f,
        bool forceBreak = false, Vector3 originPoint = default)
    {
        // base.Break(force, originalForce, breakHistory, breakDelay, forceBreak);
        shell.SetActive(false);
        fractureParent.SetActive(true);
    }

    public override void CollisionBreak(Rigidbody rb, Collision collision = null, Vector3 point = default)
    {
        // base.CollisionBreak(rb, collision);
        if (IsBroken())
        {
            return;
        }

        var force = CalculateForce(rb, out var originalSpeed, out var forceDir);

        if (force <= breakingForce.x)
        {
            return;
        }

        Break(new Vector3(), new Vector3());
        if (collision!=null)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromCollision(collision))
            {
                breakableComponent.CollisionBreak(rb, collision,point);
            }
        }

        if (point != default)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromPoint(point))
            {
                breakableComponent.CollisionBreak(rb, collision,point);
            }
        }
    }

    public override void CollisionBreak(MovableObject mo, Collision collision = null, Vector3 point = default)
    {
        if (IsBroken())
        {
            return;
        }

        var force = CalculateForce(mo, out var originalSpeed, out var forceDir);

        if (force <= breakingForce.x)
        {
            return;
        }

        Break(new Vector3(), new Vector3());
        if (collision!=null)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromCollision(collision))
            {
                breakableComponent.CollisionBreak(mo, collision,point);
            }
        }

        if (point != default)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromPoint(point))
            {
                breakableComponent.CollisionBreak(mo, collision,point);
            }
        }
        
        
    }

    BreakableComponent[] FindBreakablesFromCollision(Collision collision)
    {
        List<ContactPoint> temp = new List<ContactPoint>();
        collision.GetContacts(temp);
        // for (int i = 0; i < collision.GetContacts(temp); i++)
        // {
        //     temp.Add(collision.GetContact(i));
        // }
        List<BreakableComponent> breakableComponents = new List<BreakableComponent>();
        RaycastHit[] hits;
        foreach (ContactPoint contactPoint in temp)
        {
            hits = Physics.SphereCastAll(contactPoint.point, contactCastRadius,
                Vector3.up, 0, castLayer);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.TryGetComponent(out BreakableComponent breakableComponent))
                {
                    if (!breakableComponents.Contains(breakableComponent))
                    {
                        breakableComponents.Add(breakableComponent);
                    }
                }
            }
        }

        if (isDebug)
        {
            // Debug.Log($"{this} contacts found {breakableComponents.Count} breakable");
        }

        return breakableComponents.ToArray();
    }

    BreakableComponent[] FindBreakablesFromPoint(Vector3 point)
    {
        List<BreakableComponent> breakableComponents = new List<BreakableComponent>();
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(point, contactCastRadius,
            Vector3.up, 0, castLayer);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out BreakableComponent breakableComponent))
            {
                if (!breakableComponents.Contains(breakableComponent))
                {
                    breakableComponents.Add(breakableComponent);
                }
            }
        }

        if (isDebug)
        {
            // Debug.Log($"{this} contacts found {breakableComponents.Count} breakable");
        }

        return breakableComponents.ToArray();
    }

    public override void RemovePart(BreakableComponent part)
    {
        base.RemovePart(part);
    }

    public override List<BreakableData> InitialiseClosest()
    {
        fractureParent.SetActive(true);
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.InitialiseClosest();
        }

        fractureParent.SetActive(false);

        return base.InitialiseClosest();
    }

    protected override void AddDetectedPart(BreakableComponent current)
    {
        base.AddDetectedPart(current);
    }

    public override void EvaluateBreak(BreakableData pd, Vector3 force, BreakableComponent originalPart,
        List<BreakableComponent> breakHistory)
    {
        base.EvaluateBreak(pd, force, originalPart, breakHistory);
    }

    public override void EvaluateFall()
    {
        base.EvaluateFall();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        return base.Equals(other);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void Despawn()
    {
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.Despawn();
        }
    }
}