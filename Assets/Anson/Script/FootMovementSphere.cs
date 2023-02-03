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
    private float gravityMultiplier = 1f;

    private float gravityExtra = 0;
    private Vector3 gravityExtra_Vector;

    [SerializeField]
    private float footAnchorRange = 10f;

    [SerializeField]
    private float launchCollisionIgnoreTime = 0.5f;


    [Space(10)]
    [SerializeField]
    private bool isDebug = false;

    private float launchCollisionIgnoreTime_Set = 0f;


    private Vector3 worldPosition = new Vector3();
    private Vector3 lastPosition = new Vector3();
    public Rigidbody Rb => rb;

    public float GravityExtra => gravityExtra;

    // public float GravityAccel => gravityExtra / rb.mass;
    public bool IsIdle => footState == FootState.Idle;


    public Vector3 position
    {
        get => transform.position;
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
        if (isDebug)
        {
            ChangeState(FootState.Flying);
        }

        transform.parent = null;
        gravityExtra = Physics.gravity.magnitude * gravityMultiplier;
        gravityExtra_Vector = new Vector3(0, -gravityExtra * rb.mass, 0);
    }

    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.DrawSphere(position, .3f);
    // }

    // Update is called once per frame
    void Update()
    {
        switch (footState)
        {
            case FootState.Idle:
                if (Vector3.Distance(worldPosition, transform.position) > 0.001f)
                {
                    transform.position = worldPosition;
                }

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
                // rb.AddForce(new Vector3(0, -Physics.gravity.magnitude,0));

                break;
            case FootState.Flying:
                if (Time.time - launchCollisionIgnoreTime_Set > 4 * launchCollisionIgnoreTime &&
                    lastPosition.Equals(position))
                {
                    SetFootIdle();
                }
                else
                {
                    lastPosition = position;
                }

                rb.AddForce(gravityExtra_Vector);


                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Initialize(QuadrupedMovementController qmc, float g)
    {
        quadrupedMovementController = qmc;
        gravityMultiplier = g;
    }

    public void Launch(Vector3 force)
    {
        if (footState == FootState.Idle)
        {
            ChangeState(FootState.Flying);
            rb.AddForce(force * rb.mass);
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        if (footState == FootState.Idle)
        {
            ChangeState(FootState.Flying);
            rb.velocity = velocity;
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (footState == FootState.Flying)
        {
            if (Time.time - launchCollisionIgnoreTime_Set > launchCollisionIgnoreTime)
            {
                // Debug.Log($"{this} collided {collision.collider.name}");
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
                // rb.useGravity = false;
                break;
            case FootState.Flying:
                rb.isKinematic = false;
                // rb.useGravity = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fs), fs, null);
        }

        footState = fs;
    }
}