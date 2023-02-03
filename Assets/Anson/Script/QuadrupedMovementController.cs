using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private float anchorRange;


    public FootMovementSphere Foot => foot;

    public Transform RaycastPoint => raycastPoint;

    public Vector3 Position => foot.position;


    public Transform FootMesh => footMesh;

    public float AnchorRange => anchorRange;

    public FootCastPair(FootMovementSphere foot, Transform raycastPoint, Transform footMesh, float anchorRange = 4f)
    {
        this.foot = foot;
        this.raycastPoint = raycastPoint;
        this.footMesh = footMesh;
        this.anchorRange = anchorRange;
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

    [Header("Feet")]
    [SerializeField]
    private FootCastPair[] feet;

    [SerializeField]
    private FootCastPair[] frontFeet;

    [SerializeField]
    private FootCastPair[] backFeet;

    [SerializeField]
    private int footIndex = 0;


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

    [SerializeField]
    private Vector2 inputDir;

    [Space(20)]
    [Header("Main Transform")]
    [SerializeField]
    private Transform mainTransform;

    [SerializeField]
    private float transformLerpSpeed_Position = 5f;

    [SerializeField]
    private float transformLerpSpeed_Rotation = 5f;

    [Header("Cat Model")]
    [SerializeField]
    private GameObject catModel;

    [SerializeField]
    private float modelLerpSpeed_Position;

    [SerializeField]
    private float modelLerpSpeed_Rotation;

    private float lastMoveTime = 0f;


    private void Awake()
    {
        if (!mainTransform)
        {
            mainTransform = transform;
        }
    }

    private void InitialiseAllFeet()
    {
        foreach (FootCastPair footCastPair in feet)
        {
            FootMovementSphere footMovementSphere = footCastPair.Foot;
            footMovementSphere.Initialize(this, gravityMultiplier, footCastPair.FootMesh, footCastPair.AnchorRange);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitialiseAllFeet();

        UpdateFeetToCastPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBodyTarget();
        if (inputDir.magnitude > 0f)
        {
            if (Time.time - lastMoveTime > timeBetweenFoot)
            {
                MoveCatFeet();
            }
        }

        UpdateTransformPosition();
        UpdateTransformRotation();
        MoveModel();
    }

    private void MoveCatFeet()
    {
        lastMoveTime = Time.time;
        int oddIndex = footIndex % 2;

        switch (movementPattern)
        {
            case MovementPattern.OneAtATime:
                LaunchCurrentFoot();
                footIndex = (footIndex + 1) % feet.Length;
                break;
            case MovementPattern.EveryOtherOne:
                for (int i = oddIndex; i < feet.Length; i += 2)
                {
                    footIndex = i;
                    LaunchCurrentFoot();
                }

                footIndex = (footIndex + 1) % feet.Length;

                break;
            case MovementPattern.TwoInARow:
                for (int i = 0; i < feet.Length; i += 4)
                {
                    LaunchCurrentFoot((footIndex + i) % feet.Length);
                    LaunchCurrentFoot((footIndex + i + 1) % feet.Length);
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
                        LaunchCurrentFoot(footCastPair);
                    }
                }
                else
                {
                    foreach (FootCastPair footCastPair in backFeet)
                    {
                        LaunchCurrentFoot(footCastPair);
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
        LaunchCurrentFoot(frontFeet[oddIndex]);
        LaunchCurrentFoot(backFeet[1 - oddIndex], true);
    }

    private void FixedUpdate()
    {
    }

    void LaunchCurrentFoot(int index = -1, bool flipYAngles = false)
    {
        if (index < 0)
        {
            index = footIndex;
        }

        FootCastPair currentFoot = feet[index];

        Vector3 trajectory = FindTrajectoryToTarget(currentFoot.Foot, currentFoot.RaycastPoint, flipYAngles);
        currentFoot.Foot.SetVelocity(trajectory);
    }

    void LaunchCurrentFoot(FootCastPair footCastPair, bool flipYAngles = false)
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

    void OnMove(InputValue inputValue)
    {
        inputDir = inputValue.Get<Vector2>();
    }

    //SHT NO WORK AND IDK WHY

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
            Debug.DrawLine(legCastPoint.position, hit.point, Color.blue, 5f);
            return hit.point;
        }
        else
        {
            return new Vector3();
        }
    }

    // Vector3 GetFootLaunchTrajectory(FootMovementSphere currentFoot, Transform legCastPoint)
    // {
    //     
    //     if (Physics.Raycast(legCastPoint.position, legCastPoint.forward, out RaycastHit hit, castDistance, castLayer))
    //     {
    //         
    //     }
    //     return new Vector3();
    // }

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

    void UpdateBodyTarget()
    {
        var position = bodyTarget.position;
        Vector3 heightOffset = new Vector3(0, bodyHeight, 0);
        if (Physics.Raycast(transform.position + heightOffset, -transform.up, out RaycastHit hit,
                bodyHeight * 2f, castLayer))
        {
            position = hit.point + heightOffset;
        }
        else
        {
            position = new Vector3(position.x, AverageFeetPosition().y + bodyHeight, position.z);
        }

        bodyTarget.position = position;
    }

    void MoveModel()
    {
        catModel.transform.position =
            Vector3.Lerp(catModel.transform.position, bodyTarget.position, modelLerpSpeed_Position * Time.deltaTime);


        Vector3 avg1 = (frontFeet[0].Position + frontFeet[1].Position) / 2f;
        Vector3 avg2 = (backFeet[0].Position + backFeet[1].Position) / 2f;
        Vector3 dir = avg1 - avg2;


        catModel.transform.forward = Vector3.Lerp(catModel.transform.forward, dir.normalized,
            modelLerpSpeed_Rotation * Time.deltaTime);
    }
}