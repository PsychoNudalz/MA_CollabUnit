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

    public FootMovementSphere Foot => foot;

    public Transform RaycastPoint => raycastPoint;

    public Vector3 Position => foot.position;

    public FootCastPair(FootMovementSphere foot, Transform raycastPoint)
    {
        this.foot = foot;
        this.raycastPoint = raycastPoint;
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
    Transform body;

    [SerializeField]
    private float bodyHeight = 2f;

    [Space(10)]
    [Header("Movement Control")]
    [SerializeField]
    private MovementPattern movementPattern;

    [SerializeField]
    private Vector2 feetMoveAngle = new Vector2(30,15);


    [SerializeField]
    private float gravityMultiplier = 1f;

    [SerializeField]
    private float timeBetweenFoot = .5f;

    [SerializeField]
    private float footMoveTime = .1f;

    [SerializeField]
    private Vector2 inputDir;

    [Space(20)]
    [Header("Cat Model")]
    [SerializeField]
    private GameObject catModel;

    [SerializeField]
    private float modelLerpSpeed;

    // [SerializeField]
    // private Transform[] catModelFeet;


    private float lastMoveTime = 0f;


    private void Awake()
    {
        InitialiseAllFeet();
    }

    private void InitialiseAllFeet()
    {
        foreach (FootCastPair footCastPair in feet)
        {
            FootMovementSphere footMovementSphere = footCastPair.Foot;
            footMovementSphere.Initialize(this, gravityMultiplier);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{this} grav: {Physics.gravity.magnitude}");
        UpdateFeetToCastPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBody();
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
        LaunchCurrentFoot(frontFeet[oddIndex], 180f);
        LaunchCurrentFoot(backFeet[1 - oddIndex]);
    }

    private void FixedUpdate()
    {
    }

    void LaunchCurrentFoot(int index = -1, float castOffsetAngle = 0)
    {
        if (index < 0)
        {
            index = footIndex;
        }

        FootCastPair currentFoot = feet[index];

        Vector3 trajectory = FindTrajectoryToTarget(currentFoot.Foot, currentFoot.RaycastPoint, castOffsetAngle);
        currentFoot.Foot.SetVelocity(trajectory);
    }

    void LaunchCurrentFoot(FootCastPair footCastPair, float castOffsetAngle = 0)
    {
        Vector3 trajectory = FindTrajectoryToTarget(footCastPair.Foot, footCastPair.RaycastPoint, castOffsetAngle);
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

    Vector3 FindTrajectoryToTarget(FootMovementSphere currentFoot, Transform legCastPoint, float castOffsetAngle = 0)
    {
        Vector3 targetPos = FindLegTarget(legCastPoint, castOffsetAngle);
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

        Debug.Log($"Calculated Velocity: {velocity}, {velocity.magnitude}");


        return velocity;
    }


    Vector3 FindLegTarget(Transform legCastPoint, float offset = 0)
    {
        RaycastHit hit;
        Vector3 dir = legCastPoint.forward;
        // float moveAngle = Mathf.Atan(inputDir.x / inputDir.y) * Mathf.Rad2Deg;
        float moveAngle = Vector2.SignedAngle(Vector2.up,inputDir);

        moveAngle += (offset * inputDir.x);
        // if (inputDir.y < 0)
        // {
        //     moveAngle += 180f;
        // }

        Debug.Log($"{legCastPoint} angle: {moveAngle}");
        // dir = Quaternion.AngleAxis(moveAngle,transform.up)*dir;
        // if (inputDir.y < 0)
        // {
        //     moveAngle += 180f;
        // }
        // dir = Quaternion.AngleAxis(moveAngle, transform.up) *
        //       Quaternion.AngleAxis(-feetMoveAngle * inputDir.y, transform.right) *
        //       Quaternion.AngleAxis(-feetMoveAngle * inputDir.x, transform.forward) * dir;

        dir = Quaternion.AngleAxis(moveAngle, transform.up) * Quaternion.AngleAxis(-feetMoveAngle.x, transform.right) *
              dir;


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
        transform.position = AverageFeetPosition();
    }

    void UpdateTransformRotation()
    {
        if (feet.Length < 4)
        {
            Debug.LogError("Not enough legs to rotate");
            return;
        }

        Vector3 avg1 = (feet[0].Position + feet[1].Position) / 2f;
        Vector3 avg2 = (feet[2].Position + feet[3].Position) / 2f;
        Vector3 dir = avg1 - avg2;

        transform.forward = dir;
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

    void UpdateBody()
    {
        var position = body.position;
        position = new Vector3(position.x, AverageFeetPosition().y + bodyHeight, position.z);
        body.position = position;
    }

    void MoveModel()
    {
        catModel.transform.position =
            Vector3.Lerp(catModel.transform.position, body.position, modelLerpSpeed * Time.deltaTime);
    }
}