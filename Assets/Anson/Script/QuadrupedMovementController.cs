using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[Serializable]
public struct FootCastPair
{
    [SerializeField]
    private FootMovementSphere foot;

    [SerializeField]
    private Transform raycastPoint;

    [SerializeField]
    private Transform footMesh;

    [SerializeField]


    public FootMovementSphere Foot => foot;

    public Transform RaycastPoint => raycastPoint;

    public Vector3 Position => foot.position;


    public Transform FootMesh => footMesh;


    public FootCastPair(FootMovementSphere foot, Transform raycastPoint, Transform footMesh)
    {
        this.foot = foot;
        this.raycastPoint = raycastPoint;
        this.footMesh = footMesh;
    }
}


public class QuadrupedMovementController : MonoBehaviour
{
    enum MovementPattern
    {
        OneAtATime,
        EveryOtherOne,
        TwoInARow,
        OppositeCorners,
        FrontThenBack,
    }

    enum QuadState
    {
        Upright,
        Ragdoll
    }

    [SerializeField]
    private QuadState quadState = QuadState.Upright;

    [Header("Feet")]
    [SerializeField]
    private FootCastPair[] feet;

    [SerializeField]
    private FootCastPair[] frontFeet;

    [SerializeField]
    private FootCastPair[] backFeet;

    [SerializeField]
    private int footIndex = 0;

    [SerializeField]
    private float anchorRange = 3f;


    [Header("Cast Points")]
    [SerializeField]
    private float castDistance = 7f;

    [SerializeField]
    private LayerMask castLayer;

    [Header("Body")]
    [SerializeField]
    Transform bodyTarget;


    [SerializeField]
    private float bodyHeight = 2f;

    [SerializeField]
    [Range(0f, 1f)]
    private float bodyToFeetLerp = .2f;

    [Space(10)]
    [Header("Movement Control")]
    [SerializeField]
    private MovementPattern movementPattern;

    [SerializeField]
    private Vector2 feetMoveAngle = new Vector2(30, 15);


    [SerializeField]
    private float gravityMultiplier = 1f;

    [SerializeField]
    private float timeBetweenFoot = .5f;

    [SerializeField]
    private float footMoveTime = .1f;

    private Vector2 inputDir;

    [SerializeField]
    private float jumpForce_Y = 100f;

    [SerializeField]
    private Vector2 jumpForce_XZ = new Vector2(100f, 100f);

    [SerializeField]
    private Vector2 jumpForceRandom_XZ = new Vector2(100f, 100f);

    [Space(10)]
    [Header("Ragdoll")]
    [SerializeField]
    private ConfigurableJoint[] joints;

    [SerializeField]
    private Rigidbody[] rigidbodies;

    [SerializeField]
    private float ragdollGravityMultiplier = 4f;

    [Space(10)]
    [Header("Main Transform")]
    [SerializeField]
    private Transform mainTransform;

    [Header("Lerp")]
    [SerializeField]
    private float transformLerpSpeed_Position = 5f;

    [SerializeField]
    private float transformLerpSpeed_Rotation = 5f;


    [Space(10)]
    [Header("Cat Model")]
    [SerializeField]
    private GameObject catModel;

    [SerializeField]
    private float modelLerpSpeed_Position;

    [SerializeField]
    private float modelLerpSpeed_Rotation;

    [Header("Force")]
    [SerializeField]
    private Rigidbody catRigidbody;

    [SerializeField]
    private float modelMoveMaxDist_Position = 100f;
    [SerializeField]

    private float modelMoveVelocityPerUnit_Position = 20f;


    [SerializeField]
    private float modelMoveTorgue_Rotation = 5f;
    [SerializeField]
    private float modelMoveTorgue_Rotation_Max = 10f;

    private float lastMoveTime = 0f;


    private void Awake()
    {
        if (!mainTransform)
        {
            mainTransform = transform;
        }
    }

    [ContextMenu("Initialise Feet")]
    public void InitialiseAllFeet()
    {
        foreach (FootCastPair footCastPair in feet)
        {
            FootMovementSphere footMovementSphere = footCastPair.Foot;
            footMovementSphere.Initialize(this, gravityMultiplier, footCastPair.FootMesh, anchorRange);
        }
    }

    [ContextMenu("Get RB and Joins from Model")]
    public void GetRBFromModel()
    {
        if (catModel)
        {
            rigidbodies = catModel.GetComponentsInChildren<Rigidbody>();
            joints = catModel.GetComponentsInChildren<ConfigurableJoint>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (inputDir.magnitude > 0.1f)
        {
            Vector2 dir = inputDir * 10f;
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(dir.x, 0, dir.y));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeQuadState(quadState);
        InitialiseAllFeet();

        UpdateFeetToCastPosition();
    }

    void Update()
    {

    }

    private void FixedUpdate()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                Update_Upright();
                break;
            case QuadState.Ragdoll:
                Update_Ragdoll();
    
                break;
        }
    }

    private void Update_Upright()
    {
        // UpdateBodyTargetToFloor();
        
        MoveCat();

        MoveModel();
    }

    private void MoveCat()
    {
        if (inputDir.magnitude > 0f)
        {
            if (Time.time - lastMoveTime > timeBetweenFoot)
            {
                MoveCatFeet();
            }
        }
    }

    private void Update_Ragdoll()
    {
        UpdateBodyToFeet();

        MoveCat();

        MoveModel();
    }


    public void OnMove(Vector2 moveDir)
    {
        if (!moveDir.Equals(inputDir))
        {
            inputDir = moveDir;
        }
    }

    [ContextMenu("Upright")]
    public void SwitchQuad_Upright()
    {
        ChangeQuadState(QuadState.Upright);
    }

    [ContextMenu("Ragdoll")]
    public void SwitchQuad_Ragdoll()
    {
        ChangeQuadState(QuadState.Ragdoll);
    }

    void ChangeQuadState(QuadState qs)
    {
        switch (qs)
        {
            case QuadState.Upright:
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                mainTransform.rotation = Quaternion.Euler(0, mainTransform.eulerAngles.y, 0);

                // catModel.transform.localPosition = new Vector3();
                // catModel.transform.rotation = mainTransform.rotation;

                foreach (FootCastPair footCastPair in feet)
                {
                    footCastPair.Foot.SetFootIdle();
                }

                UpdateFeetToCastPosition();

                break;
            case QuadState.Ragdoll:
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }

                break;
        }

        quadState = qs;
    }


    //****************CAT FEET MOVEMENT
    private void MoveCatFeet()
    {
        lastMoveTime = Time.time;
        int oddIndex = footIndex % 2;

        switch (movementPattern)
        {
            case MovementPattern.OneAtATime:
                LaunchFoot();
                footIndex = (footIndex + 1) % feet.Length;
                break;
            case MovementPattern.EveryOtherOne:
                for (int i = oddIndex; i < feet.Length; i += 2)
                {
                    footIndex = i;
                    LaunchFoot();
                }

                footIndex = (footIndex + 1) % feet.Length;

                break;
            case MovementPattern.TwoInARow:
                for (int i = 0; i < feet.Length; i += 4)
                {
                    LaunchFoot((footIndex + i) % feet.Length);
                    LaunchFoot((footIndex + i + 1) % feet.Length);
                }

                footIndex = (footIndex + 2) % feet.Length;

                break;


            case MovementPattern.OppositeCorners:
                MoveCatFeet_OppositeCorners(oddIndex);
                footIndex = (footIndex + 1) % feet.Length;

                break;
            case MovementPattern.FrontThenBack:
                if (oddIndex == 0)
                {
                    foreach (FootCastPair footCastPair in frontFeet)
                    {
                        LaunchFoot(footCastPair);
                    }
                }
                else
                {
                    foreach (FootCastPair footCastPair in backFeet)
                    {
                        LaunchFoot(footCastPair);
                    }
                }

                footIndex = (footIndex + 1) % feet.Length;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MoveCatFeet_OppositeCorners(int oddIndex)
    {
        LaunchFoot(frontFeet[oddIndex]);
        LaunchFoot(backFeet[1 - oddIndex], true);
    }

    void LaunchFoot(int index = -1, bool flipYAngles = false)
    {
        if (index < 0)
        {
            index = footIndex;
        }

        FootCastPair currentFoot = feet[index];

        Vector3 trajectory = FindTrajectoryToTarget(currentFoot.Foot, currentFoot.RaycastPoint, flipYAngles);
        currentFoot.Foot.Move_SetVelocity(trajectory);
    }

    void LaunchFoot(FootCastPair footCastPair, bool flipYAngles = false)
    {
        Vector3 trajectory = FindTrajectoryToTarget(footCastPair.Foot, footCastPair.RaycastPoint, flipYAngles);
        footCastPair.Foot.Move_SetVelocity(trajectory);
    }

    [ContextMenu("Force Update Feet")]
    private void UpdateFeetToCastPosition()
    {
        Transform currentCast;
        FootMovementSphere currentFoot;
        RaycastHit hit;
        for (int i = 0; i < feet.Length; i++)
        {
            currentCast = feet[i].RaycastPoint;
            currentFoot = feet[i].Foot;
            if (Physics.Raycast(currentCast.position, currentCast.forward, out hit, castDistance, castLayer))
            {
                currentFoot.position = hit.point + new Vector3(0, 1f, 0);
            }
        }
    }


    Vector3 FindTrajectoryToTarget(FootMovementSphere currentFoot, Transform legCastPoint, bool flipYAngle = false)
    {
        Vector3 targetPos = FindLegTarget(legCastPoint, flipYAngle);
        if (targetPos.Equals(new Vector3()))
        {
            return targetPos;
        }

        Vector3 moveAmount = targetPos - currentFoot.position;
        float t = footMoveTime;
        float horizontal = (moveAmount.magnitude / t);
        float gravityMagnitude = currentFoot.GravityExtra;
        float y = gravityMagnitude * t / 2f + moveAmount.y;
        Vector3 velocity = new Vector3(horizontal * moveAmount.normalized.x, y, horizontal * moveAmount.normalized.z);

        // Debug.Log($"Calculated Velocity: {velocity}, {velocity.magnitude}");


        return velocity;
    }


    Vector3 FindLegTarget(Transform legCastPoint, bool flipYAngle = false)
    {
        RaycastHit hit;
        Vector3 dir = legCastPoint.forward;
        float yAngle = feetMoveAngle.y;
        if (flipYAngle)
        {
            yAngle *= -1f;
        }

        Quaternion angleAxis = Quaternion.AngleAxis(yAngle * inputDir.x, transform.forward) *
                               Quaternion.AngleAxis(-feetMoveAngle.x * inputDir.y, transform.right);
        dir = angleAxis * dir;
        // Debug.Log($"{legCastPoint} angle: {angleAxis.eulerAngles}");


        if (Physics.Raycast(legCastPoint.position, dir, out hit, castDistance, castLayer))
        {
            // Debug.DrawLine(legCastPoint.position, hit.point, Color.blue, 5f);
            return hit.point;
        }
        else
        {
            return new Vector3();
        }
    }




    //****************CAT BODY MOVE
    
    Vector3 AverageFeetPosition()
    {
        Vector3 avg = new Vector3();
        int i = 0;
        foreach (FootCastPair footCastPair in feet)
        {
            avg += footCastPair.Position;
        }

        return avg / feet.Length;
    }

    void UpdateBodyTargetToFloor(bool useFlat = true)
    {
        Vector3 heightOffset = new Vector3(0, bodyHeight, 0);

        Vector3 avg = AverageFeetPosition();
        var position = avg;
        if (Physics.Raycast(avg + heightOffset, -Vector3.up, out RaycastHit hit,
                bodyHeight * 2f, castLayer))
        {
            position.y = hit.point.y;
        }
        else
        {
            position.y = avg.y;
        }

        position.y += bodyHeight;

        float numberOfFalling = 0;
        foreach (FootCastPair foot in feet)
        {
            if (foot.Foot.IsFalling)
            {
                numberOfFalling++;
            }
        }

        float bodyToFeetLerp = (numberOfFalling / feet.Length) * this.bodyToFeetLerp;
        position.y = Mathf.Lerp(position.y, avg.y, bodyToFeetLerp);

        bodyTarget.position = position;

        if (useFlat)
        {
            bodyTarget.up = Vector3.up;
        }
    }

    void UpdateBodyToFeet()
    {
        Vector3 position = bodyTarget.position;
        position.y = AverageFeetPosition().y;
        bodyTarget.position = position;
    }

    void MoveModel()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                MoveModel_Velocity();
                MoveModel_Torque();


                // MoveModel_Position();
                // MoveModel_Rotation_Forward();
                break;
            case QuadState.Ragdoll:
                // MoveModel_Position();
                // MoveModel_Rotation_Forward();

                // MoveModel_Force();
                // MoveModel_Torque();

                MoveModel_Gravity();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void MoveModel_Gravity()
    {
        catRigidbody.AddForce(new Vector3(0, -Physics.gravity.magnitude * catRigidbody.mass * ragdollGravityMultiplier,
            0));
    }

    private void MoveModel_Position()
    {
        catModel.transform.position =
            Vector3.Lerp(catModel.transform.position, bodyTarget.position, modelLerpSpeed_Position * Time.deltaTime);
    }

    private void MoveModel_Rotation()
    {
        Vector3 avg1 = (frontFeet[0].Position + frontFeet[1].Position) / 2f;
        Vector3 avg2 = (backFeet[0].Position + backFeet[1].Position) / 2f;
        Vector3 forwardDir = avg1 - avg2;

        avg1 = (frontFeet[0].Position + backFeet[0].Position) / 2f;
        avg2 = (frontFeet[1].Position + backFeet[1].Position) / 2f;
        Vector3 sideDir = avg2 - avg1;

        Transform ct = catModel.transform;
        float sideAngle = Vector3.SignedAngle(ct.right, sideDir, mainTransform.forward);
        float forwardAngle = Vector3.SignedAngle(ct.forward, forwardDir, mainTransform.right);

        Quaternion targetRotation = Quaternion.Euler(forwardAngle, 0f, sideAngle) * catModel.transform.rotation;

        catModel.transform.rotation = Quaternion.Lerp(catModel.transform.rotation, targetRotation,
            modelLerpSpeed_Rotation * Time.deltaTime);
    }

    private void MoveModel_Rotation_Forward()
    {
        Vector3 avg1 = (frontFeet[0].Position + frontFeet[1].Position) / 2f;
        Vector3 avg2 = (backFeet[0].Position + backFeet[1].Position) / 2f;
        Vector3 dir = avg1 - avg2;


        catModel.transform.forward = Vector3.Lerp(catModel.transform.forward, dir.normalized,
            modelLerpSpeed_Rotation * Time.deltaTime);
    }

    void MoveModel_Velocity()
    {

        Vector3 dir = bodyTarget.position - catRigidbody.position;

        Vector3 velocity = dir.normalized * (Mathf.Min(dir.magnitude,modelMoveMaxDist_Position) * modelMoveVelocityPerUnit_Position);
        Debug.DrawRay(catRigidbody.position, velocity, Color.blue);
        catRigidbody.velocity=velocity;
        // print($"Move velocity: {velocity}, {velocity.magnitude}  Compared to {catRigidbody.velocity.magnitude}");
    }

    void MoveModel_Torque()
    {
        //Testing 
        
        bodyTarget.up = Vector3.up;
        
        //
        
        
         Quaternion dif = bodyTarget.rotation * Quaternion.Inverse(catRigidbody.rotation) ;
        
        
        // Vector3 torque = -bodyTarget.localEulerAngles;
        Vector3 torque = -dif.eulerAngles;
        torque = Vector3.ClampMagnitude(torque * modelMoveTorgue_Rotation, modelMoveTorgue_Rotation_Max);
        print($"{dif.eulerAngles}, {torque}");
        catRigidbody.AddTorque(torque,ForceMode.VelocityChange);
    }

    //********************JUMP
    public void Jump()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                ChangeQuadState(QuadState.Ragdoll);
                Vector3 baseVelocity =
                    (new Vector3(inputDir.x * jumpForce_XZ.x, jumpForce_Y, inputDir.y * jumpForce_XZ.y));
                Vector3 velocity = baseVelocity;
                Vector3 totalVelocity = new Vector3();
                foreach (FootCastPair footCastPair in feet)
                {
                    // Vector3 velocity = (new Vector3(inputDir.x, 1, inputDir.y)).normalized;
                    // velocity *= jumpForce_Y;
                    velocity = baseVelocity;
                    velocity += new Vector3(Random.Range(-jumpForceRandom_XZ.x, jumpForceRandom_XZ.x), 0,
                        Random.Range(-jumpForceRandom_XZ.y, jumpForceRandom_XZ.y));
                    velocity = transform.rotation * velocity;
                    footCastPair.Foot.SetJump(velocity);
                    totalVelocity += footCastPair.Foot.Rb.velocity;
                }

                catRigidbody.velocity = totalVelocity / 5f;

                break;
            case QuadState.Ragdoll:
                ChangeQuadState(QuadState.Upright);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}