using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExplosiveController : MonoBehaviour
{
    [Header("Explosive states")]
    [SerializeField]
    private float range = 100;

    [SerializeField]
    private AnimationCurve rangeCurve;

    [SerializeField]
    private float maxForce = 3000f;

    [SerializeField]
    private float maxVelocity = 0f;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private UnityEvent explodeEffectEvent;

    [ContextMenu("Explode")]
    public void OnExplode()
    {
        BreakableCollective currentCollective;
        BreakablePart currentPart;
        Vector3 force;
        explodeEffectEvent.Invoke();

        //Break all collectives
        Collider[] overlapSphere1 = Physics.OverlapSphere(transform.position, range, layerMask);
        foreach (Collider collider in overlapSphere1)
        {
            currentCollective = collider.GetComponentInParent<BreakableCollective>();
            if (currentCollective)
            {
                // force = GetForce(currentCollective.Position);
                currentCollective.ExplodeBreak((currentCollective.Position - transform.position).normalized * maxForce,
                    transform.position);
            }
        }

        //Recast again so it will hit the activated parts
        Collider[] overlapSphere2 = Physics.OverlapSphere(transform.position, range, layerMask);

        int i = 0;
        foreach (Collider collider in overlapSphere2)
        {
            if (collider.TryGetComponent(out currentPart))
            {
                force = GetForce(currentPart.Position);
                currentPart.ExplodeBreak(force, transform.position,GetVelocity(currentPart.Position));
                i++;
            }
        }

    }

    Vector3 GetForce(Vector3 target)
    {
        Vector3 dir = target - transform.position;

        return dir.normalized * (rangeCurve.Evaluate(dir.magnitude / range) * maxForce);
        // return new Tuple<Vector3,float>(temp.normalized,temp.magnitude);
    }

    Vector3 GetVelocity(Vector3 target)
    {
        Vector3 dir = target - transform.position;

        return dir.normalized * (rangeCurve.Evaluate(dir.magnitude / range) * maxVelocity);
        // return new Tuple<Vector3,float>(temp.normalized,temp.magnitude);
    }
}