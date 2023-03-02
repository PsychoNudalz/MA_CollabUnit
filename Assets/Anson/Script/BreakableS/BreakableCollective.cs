using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
    private List<BreakableComponent> tooSmallComponents;

    [SerializeField]
    private float contactCastRadius = 0.5f;

    [SerializeField]
    private bool onlyConnectToCollectives = true;

    public BreakableComponent[] BreakableComponents => breakableComponents;


    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollisionBreak(collision);
    }


    public override void ResetConnections()
    {
        base.ResetConnections();
    }

    public override void Initialise(GameObject p, BreakableStructureController bsc, float mass, float drag,
        float affectedRange, Vector2 breakForce,
        float forceTransfer,
        LayerMask bpLayer, AnimationCurve transferToDot, float minSize, Vector2 breakDelay, float bottomAngle,
        PhysicMaterial pm, UnityEvent breakEvent1, float despawnTime1, UnityEvent despawnEvent1, LayerMask groundLayer1,
        bool ignoreParent1)
    {
        if (!shell || !fractureParent)
        {
            Debug.LogError($"{this} missing game object connections");
            return;
        }

        parent = p;
        Initialise();
        InitialiseValues(mass, bsc, drag, affectedRange, breakForce, forceTransfer, bpLayer, transferToDot, minSize,
            breakDelay, bottomAngle, pm, breakEvent1, despawnTime1, despawnEvent1, groundLayer1, ignoreParent1);

        // fractureParent.SetActive(true);


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


        List<BreakableComponent> foundComponents = new List<BreakableComponent>(fractureParent.GetComponentsInChildren<BreakableComponent>()) ;
        tooSmallComponents = new List<BreakableComponent>();

        foreach (BreakableComponent breakableComponent in foundComponents.ToArray())
        {
            breakableComponent.AddComponents(pm);
            breakableComponent.Initialise(gameObject, bsc, mass, drag, affectedRange, breakForce, forceTransfer,
                bpLayer,
                transferToDot,
                minimumPartSize, breakDelay, minBottomAngle, pm, breakEvent, despawnTime, despawnEvent,
                groundLayer, ignoreParent1);
            if (breakableComponent.IsTooSmall)
            {
                tooSmallComponents.Add(breakableComponent);
                foundComponents.Remove(breakableComponent);
            }
        }

        breakableComponents = foundComponents.ToArray();

        // fractureParent.SetActive(false);
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
        ChangeState(BreakableState.Free);
        FlipShell(true);
        foreach (BreakableComponent breakableComponent in tooSmallComponents)
        {
            breakableComponent.Despawn();
        }
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
        if (collision != null)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromCollision(collision))
            {
                breakableComponent.CollisionBreak(rb, collision, point);
            }
        }

        if (point != default)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromPoint(point))
            {
                breakableComponent.CollisionBreak(rb, collision, point);
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
        if (collision != null)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromCollision(collision))
            {
                breakableComponent.CollisionBreak(mo, collision, point);
            }
        }

        if (point != default)
        {
            foreach (BreakableComponent breakableComponent in FindBreakablesFromPoint(point))
            {
                breakableComponent.CollisionBreak(mo, collision, point);
            }
        }
    }

    public override void ExplodeBreak(Vector3 force, Vector3 point)
    {
        Break(force, force);
        selfRB.AddForce(force);
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

    public override List<BreakableComponent> InitialiseClosest(bool ignoreTooSmall = false)
    {
        // fractureParent.SetActive(true);
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.InitialiseClosest(true);
        }

        // fractureParent.SetActive(false);

        return base.InitialiseClosest(ignoreTooSmall);
    }

    protected override void AddDetectedPart(BreakableComponent current)
    {
        base.AddDetectedPart(current);
    }

    public override void EvaluateBreak(BreakableComponent pd, Vector3 force, BreakableComponent originalPart,
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

    public void FlipShell(bool b)
    {
        shell.SetActive(!b);
        fractureParent.SetActive(b);
    }

    public override void InitialiseGround()
    {
        base.InitialiseGround();
        foreach (BreakableComponent breakableComponent in breakableComponents)
        {
            breakableComponent.InitialiseGround();
        }
    }
}