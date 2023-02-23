using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private LayerMask layerMask;

    [ContextMenu("Explode")]
    public void OnExplode()
    {
        BreakableCollective currentCollective;
        BreakablePart currentPart;
        Vector3 force;
        //Break all collectives
        foreach (Collider collider in Physics.OverlapSphere(transform.position, range, layerMask))
        {
            currentCollective = collider.GetComponentInParent<BreakableCollective>();
            if (currentCollective)
            {
                // force = GetForce(currentCollective.Position);
                currentCollective.ExplodeBreak((currentCollective.Position - transform.position).normalized * maxForce,
                    transform.position);
            }
        }

        int i = 0;
        foreach (Collider collider in Physics.OverlapSphere(transform.position, range, layerMask))
        {
            if (collider.TryGetComponent(out currentPart))
            {
                force = GetForce(currentPart.Position);
                currentPart.ExplodeBreak(force, transform.position);
                i++;
            }
        }

        print($"{this} exploded {i} parts");
    }

    Vector3 GetForce(Vector3 target)
    {
        Vector3 dir = target - transform.position;

        return dir.normalized * (rangeCurve.Evaluate(dir.magnitude / range) * maxForce);
        // return new Tuple<Vector3,float>(temp.normalized,temp.magnitude);
    }
}