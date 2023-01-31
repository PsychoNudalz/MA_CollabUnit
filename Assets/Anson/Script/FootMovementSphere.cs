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

    [SerializeField]
    private float launchCollisionIgnoreTime = 0.5f;
    private float launchCollisionIgnoreTime_Set =0f;
    
    
    private Vector3 worldPosition = new Vector3();
    private Vector3 lastPosition = new Vector3();
    public Rigidbody Rb => rb;

    public Vector3 position
    {
        get =>transform.position;
        set => worldPosition = value;
    }
        
    // Start is called before the first frame update
    void Awake()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        ChangeState(FootState.Idle);
        transform.parent = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(position,.3f);
    }

    // Update is called once per frame
    void Update()
    {
        switch (footState)
        {
            case FootState.Idle:
                transform.position = worldPosition;
                break;
            case FootState.Flying:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    private void FixedUpdate()
    {
        switch (footState)
        {
            case FootState.Idle:
                break;
            case FootState.Flying:
                if (lastPosition.Equals(position))
                {
                    SetFootIdle();
                }
                else
                {
                    lastPosition = position;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Initialize(QuadrupedMovementController qmc)
    {
        quadrupedMovementController = qmc;
    }

    public void Launch(Vector3 force)
    {
        ChangeState( FootState.Flying);
        rb.AddForce(force*rb.mass);
        launchCollisionIgnoreTime_Set = Time.time;
    }

    public void SetVelocity(Vector3 velocity)
    {
        ChangeState( FootState.Flying);
        rb.velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (footState == FootState.Flying)
        {
            if (Time.time - launchCollisionIgnoreTime_Set > launchCollisionIgnoreTime)
            {
                Debug.Log($"{this} collided {collision.collider.name}");
                SetFootIdle();
            }
        }
    }

    private void SetFootIdle()
    {
        ChangeState(FootState.Idle);
        worldPosition = position;
    }

    void ChangeState(FootState fs)
    {
        switch (fs)
        {
            case FootState.Idle:
                rb.isKinematic = true;
                rb.useGravity = false;
                break;
            case FootState.Flying:
                rb.isKinematic = false;
                rb.useGravity = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fs), fs, null);
        }
        
        footState = fs;
        
    }
}
