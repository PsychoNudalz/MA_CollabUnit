using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BreakableObject : BreakablePart
{
    [Header("Destroy")]
    [SerializeField]
    private float destroyVelocity = 20f;

    [SerializeField]
    private UnityEvent destroyEvent;


    [SerializeField]
    private GameObject modelGameObject;

    [SerializeField]
    private bool destroyOnTelekinesisCollision = true;

    private List<BreakablePart> launchedParts;


    public override bool CanTelekinesis => breakableState is BreakableState.Free or BreakableState.Hold;

    // Start is called before the first frame update
    void Start()
    {
        fullBreakTime = .1f;
        if (meshSize == 0)
        {
            if (meshFilter)
            {
                meshSize = meshFilter.sharedMesh.bounds.size.magnitude * transform.lossyScale.x;
            }
            else
            {
                meshSize = 1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }


    protected override void CollisionEnterBehaviour(Collision collision)
    {
        BreakableComponent bc;
        switch (breakableState)
        {
            case BreakableState.Free:

                if (collision.gameObject.TryGetComponent(out bc))
                {
                    float v_diff = Mathf.Abs(bc.SelfRb.velocity.magnitude - selfRB.velocity.magnitude);
                    if (v_diff > destroyVelocity)
                    {
                        OnBreakableDestroy();
                        return;
                    }
                }

                else if (collision.gameObject.TryGetComponent(out Rigidbody rb))
                {
                    if (selfRB.velocity.magnitude > destroyVelocity)
                    {
                        {
                            float v_diff = Mathf.Abs(rb.velocity.magnitude - selfRB.velocity.magnitude);
                            OnBreakableDestroy();
                        }
                    }
                }
                else
                {
                    OnBreakableDestroy();
                }

                return;

                break;
            case BreakableState.Telekinesis_Shoot:


                if (collision.gameObject.TryGetComponent(out bc))
                {
                    if (bc is BreakablePart bp)
                    {
                        if (!launchedParts.Contains(bp))
                        {
                            if (selfRB.velocity.magnitude > destroyVelocity)
                            {
                                OnBreakableDestroy();
                            }
                        }
                    }
                    return;

                }

                if (destroyOnTelekinesisCollision)
                {
                    OnBreakableDestroy();
                    return;
                }

                if (selfRB.velocity.magnitude > destroyVelocity)
                {
                    OnBreakableDestroy();
                    return;
                }

                break;
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

    public override void Telekinesis()
    {
        if (breakableState == BreakableState.Hold)
        {
            Break(Vector3.zero, Vector3.zero);
        }

        base.Telekinesis();
    }

    public override void Launch(Vector3 accel, List<BreakablePart> parts = null)
    {
        base.Launch(accel, parts);
        launchedParts = parts;
    }
}