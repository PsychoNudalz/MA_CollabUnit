using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : BreakablePart
{
    [Header("Destroy")]
    [SerializeField]
    private float destroyForce = 100f;

    [SerializeField]
    private float destroyVelocity = 20f;

    [SerializeField]
    private UnityEvent destroyEvent;


    [SerializeField]
    private GameObject modelGameObject;

    // Start is called before the first frame update
    void Start()
    {
        fullBreakTime = .1f;
        meshSize = meshFilter.sharedMesh.bounds.size.magnitude * transform.lossyScale.x;

    }

    // Update is called once per frame
    void Update()
    {
    }


    protected override void CollisionEnterBehaviour(Collision collision)
    {
        switch (breakableState)
        {
            case BreakableState.Hold:
                break;
            case BreakableState.InitialBreak:
                break;
            case BreakableState.Free:
                if (selfRB.velocity.magnitude > destroyVelocity)
                {
                    OnBreakableDestroy();
                    return;
                }

                break;
            case BreakableState.Stay:
                break;
            case BreakableState.Despawn:
                break;
            case BreakableState.Telekinesis:
                break;
            case BreakableState.Telekinesis_Shoot:
                if (selfRB.velocity.magnitude > destroyVelocity)
                {
                    OnBreakableDestroy();
                    return;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (collision.gameObject.TryGetComponent(out Rigidbody rb))
        {
            float v_diff = Mathf.Abs(rb.velocity.magnitude - selfRB.velocity.magnitude);
            switch (breakableState)
            {
                case BreakableState.Hold:
                    break;
                case BreakableState.InitialBreak:
                    break;
                case BreakableState.Free:
                    if (v_diff > destroyVelocity)
                    {
                        OnBreakableDestroy();
                        return;
                    }

                    break;
                case BreakableState.Stay:
                    break;
                case BreakableState.Despawn:
                    break;
                case BreakableState.Telekinesis:
                    break;
                case BreakableState.Telekinesis_Shoot:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    void OnBreakableDestroy()
    {
        destroyEvent.Invoke();
        Despawn();
    }

    public override void Despawn()
    {
        if (!gameObject || breakableState == BreakableState.Despawn)
        {
            return;
        }

        selfRB.isKinematic = true;
        breakableState = BreakableState.Despawn;
        despawnEvent.Invoke();
        modelGameObject.SetActive(false);
        Destroy(gameObject, despawnTime + 1f);
    }
}