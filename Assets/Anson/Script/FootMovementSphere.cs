using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootMovementSphere : MonoBehaviour
{
    [Serializable]
    enum FootState
    {
        Idle,
        Flying
    }

    [SerializeField]
    private FootState footState = FootState.Idle;
    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    [SerializeField]
    private Rigidbody rb;
    private Vector3 worldPosition = new Vector3();
    public Rigidbody Rb => rb;

    public Vector3 position => transform.position;
    // Start is called before the first frame update
    void Awake()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(position,.3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (footState == FootState.Idle)
        {
            transform.position = worldPosition;
        }
    }

    public void Initialize(QuadrupedMovementController qmc)
    {
        quadrupedMovementController = qmc;
    }

    public void Launch(Vector3 force)
    {
        rb.AddForce(force*rb.mass);
        footState = FootState.Flying;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (footState == FootState.Flying)
        {
            footState = FootState.Idle;
            worldPosition = position;
        }
    }
}
