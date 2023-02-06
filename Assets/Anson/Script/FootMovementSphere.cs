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
        Flying,
        Falling
    }

    [SerializeField]
    private FootState footState = FootState.Idle;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;


    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float launchCollisionIgnoreTime = 0.5f;


    [Header("Gravity")]
    [SerializeField]
    private float gravityMultiplier = 1f;

    private float gravityExtra = 0;
    private Vector3 gravityExtra_Vector;

    [Header("Anchor")]
    [SerializeField]
    private Transform anchorPoint;

    [SerializeField]
    private float footAnchorRange = 10f;


    [Header("GroundCheck")]
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundRadius = .5f;

    [SerializeField]
    private float groundCheckTime = 1f;

    private float groundCheckTime_Now = -1;
    private Vector3 collisionPoint = new Vector3();

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
    public bool IsFalling => footState == FootState.Falling;


    public Vector3 position
    {
        get => transform.position;
        set => worldPosition = value;
    }

    // Start is called before the first frame update
    void Awake()
    {
        AwakeBehaviour();
    }

    private void AwakeBehaviour()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void Start()
    {
        transform.position = anchorPoint.position;
        
        collisionPoint = transform.position;
        worldPosition = transform.position;
        ChangeState(FootState.Idle);
        if (isDebug)
        {
            ChangeState(FootState.Flying);
        }

        transform.parent = null;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(collisionPoint, groundRadius);
    }

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

                if (groundCheckTime_Now > 0)
                {
                    groundCheckTime_Now -= Time.deltaTime;
                }
                else
                {
                    groundCheckTime_Now = groundCheckTime;
                    if (!GroundCheck())
                    {
                        ChangeState(FootState.Falling);
                    }
                }

                break;
            case FootState.Flying:
                transform.position =
                    Vector3.ClampMagnitude(transform.position - anchorPoint.position, footAnchorRange) +
                    anchorPoint.position;

                break;
            case FootState.Falling:
                transform.position =
                    Vector3.ClampMagnitude(transform.position - anchorPoint.position, footAnchorRange) +
                    anchorPoint.position;

                break;
        }
    }

    private bool GroundCheck()
    {
        return Physics.CheckSphere(collisionPoint, groundRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        switch (footState)
        {
            case FootState.Idle:
                // rb.AddForce(new Vector3(0, -Physics.gravity.magnitude,0));

                break;
            case FootState.Flying:
                // if (Time.time - launchCollisionIgnoreTime_Set > 4 * launchCollisionIgnoreTime &&
                //     Vector3.Distance(lastPosition,position)>0.01f)
                // {
                //     SetFootIdle();
                // }
                // else
                // {
                //     lastPosition = position;
                // }

                if (Vector3.Distance(transform.position, anchorPoint.position) > footAnchorRange)
                {
                    // ChangeState(FootState.OutOfRange);
                }

                rb.AddForce(gravityExtra_Vector);

                break;

            case FootState.Falling:
                // if (Time.time - launchCollisionIgnoreTime_Set > 4 * launchCollisionIgnoreTime &&
                //     Vector3.Distance(lastPosition,position)>0.01f)
                // {
                //     SetFootIdle();
                // }
                // else
                // {
                //     lastPosition = position;
                // }
                rb.AddForce(new Vector3(0, -Physics.gravity.magnitude*rb.mass, 0));
                // rb.AddForce(gravityExtra_Vector);

                
                break;
        }
    }

    /// <summary>
    /// initialize foot
    /// </summary>
    /// <param name="qmc"></param>
    /// <param name="g"></param>
    public void Initialize(QuadrupedMovementController qmc, float g, Transform a, float anchorRange)
    {
        AwakeBehaviour();
        quadrupedMovementController = qmc;
        anchorPoint = a;
        footAnchorRange = anchorRange;
        UpdateGravity(g);
    }

    /// <summary>
    /// update gravity values
    /// </summary>
    /// <param name="g"></param>
    public void UpdateGravity(float g)
    {
        gravityMultiplier = g;

        gravityExtra = Physics.gravity.magnitude * gravityMultiplier;
        gravityExtra_Vector = new Vector3(0, -gravityExtra * rb.mass, 0);
    }

    public void Launch(Vector3 force)
    {
        if (footState is FootState.Idle or FootState.Falling)
        {
            ChangeState(FootState.Flying);
            rb.AddForce(force * rb.mass);
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    public void SetVelocity(Vector3 velocity)
    {
        if (footState is FootState.Idle or FootState.Falling)
        {
            ChangeState(FootState.Flying);
            rb.velocity = velocity;
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionPoint = collision.contacts[0].point;
        if (footState == FootState.Flying)
        {
            if (Time.time - launchCollisionIgnoreTime_Set > launchCollisionIgnoreTime)
            {
                // Debug.Log($"{this} collided {collision.collider.name}");
                SetFootIdle(collision);
                // ChangeState(FootState.Falling);
            }
        }
    }

    private void SetFootIdle(Collision collision = null)
    {
        if (collision==null)
        {
            collisionPoint = transform.position;
        }
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

            case FootState.Falling:
                rb.isKinematic = false;

                break;
        }

        footState = fs;
    }
}