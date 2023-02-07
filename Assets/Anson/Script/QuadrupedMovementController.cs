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
    private Vector2 jumpForceRandom_XZ = new Vector2(100f, 100f);

    [Space(10)]
    [Header("Ragdoll")]
    [SerializeField]
    private ConfigurableJoint[] joints;

    [SerializeField]
    private Rigidbody[] rigidbodies;

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
    [Tooltip("Per unit")]
    private float modelMoveForce_Position = 100f;

    [SerializeField]
    [Tooltip("Per unit")]
    private float modelMoveTorgue_Rotation = 5f;

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
            Vector2 dir = inputDir*10f;
            Gizmos.DrawLine(transform.position,transform.position+new Vector3(dir.x,0,dir.y));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeQuadState(quadState);
        InitialiseAllFeet();

        UpdateFeetToCastPosition();
    }

    // Update is called once per frame
    void Update()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                Update_Upright();
                break;
            case QuadState.Ragdoll:

                break;
        }
    }

    private void FixedUpdate()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                break;
            case QuadState.Ragdoll:
                Update_Ragdoll();

                break;
        }
    }

    private void Update_Upright()
    {
        UpdateBodyTargetToFloor();
        MoveCat();

        UpdateTransformPosition();
        UpdateTransformRotation();
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

        UpdateTransformPosition();
        UpdateTransformRotation();

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

                catModel.transform.localPosition = new Vector3();
                catModel.transform.rotation = mainTransform.rotation;
                
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
        currentFoot.Foot.SetVelocity(trajectory);
    }

    void LaunchFoot(FootCastPair footCastPair, bool flipYAngles = false)
    {
        Vector3 trajectory = FindTrajectoryToTarget(footCastPair.Foot, footCastPair.RaycastPoint, flipYAngles);
        footCastPair.Foot.SetVelocity(trajectory);
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


    //****************CAT RAGDOLL


    //****************CAT BODY UPDATE

    void UpdateTransformPosition()
    {
        Vector3 average = AverageFeetPosition();
        mainTransform.position =
            Vector3.Lerp(mainTransform.position, average, transformLerpSpeed_Position * Time.deltaTime);
    }

    void UpdateTransformRotation()
    {
        // if (feet.Length < 4)
        // {
        //     Debug.LogError("Not enough legs to rotate");
        //     return;
        // }
        //
        // Vector3 avg1 = (feet[0].Position + feet[1].Position) / 2f;
        // Vector3 avg2 = (feet[2].Position + feet[3].Position) / 2f;
        // Vector3 dir = avg1 - avg2;
        //
        // dir.y = 0;
        //
        // transform.forward = dir.normalized;


        //Probably dont need this step when the camera is implemented
        if (inputDir.magnitude > 0)
        {
            Vector3 targetRotation =
                Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, transform.forward, Vector3.up), Vector3.up) *
                new Vector3(inputDir.x, 0, inputDir.y);

            mainTransform.forward = Vector3.Lerp(mainTransform.forward, targetRotation,
                transformLerpSpeed_Rotation * Time.deltaTime);
        }
    }

    Vector3 AverageFeetPosition()
    {
        Vector3 avg = new Vector3();
        int i = 0;
        // foreach (FootMovementSphere footMovementSphere in feet)
        // {
        //     if (footMovementSphere.IsIdle)
        //     {
        //         avg += footMovementSphere.position;
        //         i++;
        //     }
        // }
        //
        // if (i > 0)
        // {
        //     avg /= i;
        //     return avg;
        //
        // }
        // else
        // {
        //     return body.position;
        // }

        foreach (FootCastPair footCastPair in feet)
        {
            avg += footCastPair.Position;
        }

        return avg / feet.Length;
    }

    void UpdateBodyTargetToFloor()
    {
        Vector3 heightOffset = new Vector3(0, bodyHeight, 0);

        var position = mainTransform.position;
        if (Physics.Raycast(mainTransform.position + heightOffset, -Vector3.up, out RaycastHit hit,
                bodyHeight * 2f, castLayer))
        {
            position.y = hit.point.y;
        }
        else
        {
            position.y = AverageFeetPosition().y;
        }

        // float yMean = position.y;
        // float i = 1;
        //
        // foreach (FootCastPair footCastPair in feet)
        // {
        //     if (Physics.Raycast(footCastPair.RaycastPoint.position, -transform.up, out hit,
        //             bodyHeight, castLayer))
        //     {
        //         yMean += hit.point.y;
        //         i++;
        //
        //     }
        // }
        //
        // position.y = yMean / i;

        float numberOfFalling = 0;
        foreach (FootCastPair foot in feet)
        {
            if (foot.Foot.IsFalling)
            {
                numberOfFalling++;
            }
        }

        bodyToFeetLerp = numberOfFalling / feet.Length;
        position.y = Mathf.Lerp(position.y, AverageFeetPosition().y, bodyToFeetLerp);
            position.y += bodyHeight;

        bodyTarget.position = position;
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
                MoveModel_Position();
                MoveModel_Rotation_Forward();
                break;
            case QuadState.Ragdoll:
                MoveModel_Force();
                // MoveModel_Torque();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


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

    void MoveModel_Force()
    {
        Vector3 dir = bodyTarget.position - catRigidbody.transform.position;

        float dot = Vector3.Dot(dir.normalized, catRigidbody.velocity.normalized);

        catRigidbody.AddForce(dir.normalized * (modelMoveForce_Position * dot));
    }

    void MoveModel_Torque()
    {
        Vector3 dif = bodyTarget.eulerAngles - catRigidbody.transform.eulerAngles;
        catRigidbody.AddTorque(dif*modelMoveTorgue_Rotation);
    }

    //********************JUMP
    public void Jump()
    {
        switch (quadState)
        {
            case QuadState.Upright:
                ChangeQuadState(QuadState.Ragdoll);
                foreach (FootCastPair footCastPair in feet)
                {
                    Vector3 velocity = (new Vector3(inputDir.x*jumpForce_XZ.x, jumpForce_Y, inputDir.y*jumpForce_XZ.y));
                    // Vector3 velocity = (new Vector3(inputDir.x, 1, inputDir.y)).normalized;
                    // velocity *= jumpForce_Y;

                    velocity += new Vector3(inputDir.x * Random.Range(0,jumpForceRandom_XZ.x) , 0, inputDir.y * Random.Range(0,jumpForceRandom_XZ.y));
                    velocity = transform.rotation * velocity;
                    footCastPair.Foot.SetJump(velocity);
                }
                break;
            case QuadState.Ragdoll:
                ChangeQuadState(QuadState.Upright);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        
    }
}