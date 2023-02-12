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
        Move,
        Falling,
        Free //ONLY FOR RAGDOLL
    }

    [SerializeField]
    private FootState footState = FootState.Idle;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;


    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float launchCollisionIgnoreTime = 0.05f;

    [SerializeField]
    private Transform feetParent;

    [SerializeField]
    private bool setToWorld = true;


    [Header("Gravity")]
    [SerializeField]
    private float gravityMultiplier_Move = 1f;

    [SerializeField]
    private float gravityMultiplier_Fall = 5f;

    private float gravityExtra = 0;
    private Vector3 gravityExtra_Vector;

    [Header("Anchor")]
    [SerializeField]
    private bool useAnchor = true;

    [SerializeField]
    private Transform anchorPoint;

    [SerializeField]
    private float footAnchorRange = 3;


    [Header("GroundCheck")]
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundRange = .5f;

    [SerializeField]
    private float groundCheckTime = .2f;

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
            ChangeState(FootState.Move);
        }

        if (setToWorld)
        {
            if (!feetParent)
            {
                feetParent = transform;
            }

            feetParent.parent = null;
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(collisionPoint, groundRange);
    }

    // Update is called once per frame
    void Update()
    {
        switch (footState)
        {
            case FootState.Idle:
                if (Vector3.Distance(worldPosition, transform.position) > 0.001f)
                {
                    // transform.position = worldPosition;
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
            case FootState.Move:
                if (Time.time - launchCollisionIgnoreTime_Set > launchCollisionIgnoreTime * 10f)
                {
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
                }

                break;
            case FootState.Falling:
                if (groundCheckTime_Now > 0)
                {
                    groundCheckTime_Now -= Time.deltaTime;
                }
                else
                {
                    groundCheckTime_Now = groundCheckTime;
                    if (GroundCheck())
                    {
                        ChangeState(FootState.Idle);
                    }
                }


                break;
            case FootState.Free:

                break;
        }
    }

    private void FixedUpdate()
    {
        float gravityFall = -Physics.gravity.magnitude* rb.mass* gravityMultiplier_Fall;
        switch (footState)
        {
            case FootState.Idle:
                rb.AddForce(new Vector3(0, gravityFall,0));

                break;
            case FootState.Move:
                if (Vector3.Distance(transform.position, anchorPoint.position) > footAnchorRange)
                {
                    // ChangeState(FootState.OutOfRange);
                }

                rb.AddForce(gravityExtra_Vector);

                break;

            case FootState.Falling:

                rb.AddForce(new Vector3(0, gravityFall, 0));
                break;

            case FootState.Free:
                rb.AddForce(new Vector3(0, gravityFall, 0));

                break;
        }
    }

    private void AnchorFeet()
    {
        if (!useAnchor)
        {
            return;
        }

        transform.position =
            Vector3.ClampMagnitude(transform.position - anchorPoint.position, footAnchorRange) +
            anchorPoint.position;
    }

    private bool GroundCheck()
    {
        Debug.DrawRay(transform.position, Vector3.down * groundRange, Color.magenta, 2f);

        return Physics.Raycast(transform.position, Vector3.down, groundRange, groundLayer);
    }

    private bool GroundCheck(Vector3 point)
    {
        Debug.DrawRay(point, Vector3.down * groundRange, Color.cyan, 2f);
        return Physics.Raycast(point, Vector3.down, groundRange, groundLayer);
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
        gravityMultiplier_Move = g;

        gravityExtra = Physics.gravity.magnitude * gravityMultiplier_Move;
        gravityExtra_Vector = new Vector3(0, -gravityExtra * rb.mass, 0);
    }

    public void Move_Launch(Vector3 force)
    {
        if (footState is not FootState.Move)

        {
            ChangeState(FootState.Move);
            rb.AddForce(force * rb.mass);
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    public void Move_SetVelocity(Vector3 velocity)
    {
        if (footState is not FootState.Move)
        {
            ChangeState(FootState.Move);
            rb.velocity = velocity;
            // rb.angularVelocity = new Vector3();
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    public void SetJump(Vector3 velocity)
    {
        if (footState is not FootState.Free)
        {
            SetFootFree();
            rb.velocity = velocity;
            launchCollisionIgnoreTime_Set = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (footState == FootState.Move)
        {
            if (Time.time - launchCollisionIgnoreTime_Set > launchCollisionIgnoreTime)
            {
                // Debug.Log($"{this} collided {collision.collider.name}");
                if (GroundCheck(transform.position))
                {
                    SetFootIdle(collision);
                }
                else
                {
                    SetFootFall(collision);
                }
                // ChangeState(FootState.Falling);
            }
        }

        if (footState == FootState.Falling)
        {
            if (GroundCheck(transform.position))
            {
                SetFootIdle(collision);
            }
            else
            {
                SetFootFall(collision);
            }
        }
    }

    public void SetFootIdle(Collision collision = null)
    {
        if (collision == null)
        {
            collisionPoint = transform.position;
        }

        ChangeState(FootState.Idle);
        worldPosition = position;
    }

    public void SetFootFall(Collision collision = null)
    {
        if (collision == null)
        {
            collisionPoint = transform.position;
        }

        ChangeState(FootState.Falling);
    }

    public void SetFootFree(Collision collision = null)
    {
        if (collision == null)
        {
            collisionPoint = transform.position;
        }

        ChangeState(FootState.Free);
    }


    void ChangeState(FootState fs)
    {
        rb.drag = 0f;
        rb.constraints = RigidbodyConstraints.None;

        switch (fs)
        {
            case FootState.Idle:
                // rb.constraints = RigidbodyConstraints.FreezePosition;
                rb.drag = 1f;
                // rb.useGravity = false;
                break;
            case FootState.Move:
                break;

            case FootState.Falling:
                break;
            case FootState.Free:
                break;
                ;
        }

        footState = fs;
    }
}