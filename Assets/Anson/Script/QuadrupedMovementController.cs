using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuadrupedMovementController : MonoBehaviour
{
    enum MovementPattern
    {
        OneAtATime,
        EveryOtherOne,
        TwoInARow
    }

    [Header("Feet")]
    [SerializeField]
    private FootMovementSphere[] feet;

    [SerializeField]
    private int footIndex = 0;

    [SerializeField]
    private float feetMoveAngle = 15f;

    [SerializeField]
    private float feetMoveHeight = 2f;

    [Header("Cast Points")]
    [SerializeField]
    private Transform[] feetCastPoints;

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

    // Start is called before the first frame update
    void Start()
    {
        foreach (FootMovementSphere footMovementSphere in feet)
        {
            footMovementSphere.Initialize(this);
        }

        if (feet.Length != feetCastPoints.Length)
        {
            Debug.LogError($"{this} feet array length mismatch.");
        }

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
        }
    }

    private void FixedUpdate()
    {
    }

    void LaunchCurrentFoot(int index = -1)
    {
        if (index < 0)
        {
            index = footIndex;
        }

        FootMovementSphere currentFoot = feet[index];

        Vector3 trajectory = FindTrajectoryToTarget(currentFoot, feetCastPoints[index]);
        // Vector3 trajectory = GetFootLaunchTrajectory(currentFoot,feetCastPoints[footIndex]);

        // currentFoot.Launch(transform.up * 100f);
        currentFoot.SetVelocity(trajectory);
    }

    [ContextMenu("Force Update Feet")]
    private void UpdateFeetToCastPosition()
    {
        Transform currentCast;
        FootMovementSphere currentFoot;
        RaycastHit hit;
        for (int i = 0; i < Mathf.Min(feet.Length, feetCastPoints.Length); i++)
        {
            currentCast = feetCastPoints[i];
            currentFoot = feet[i];
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

    Vector3 FindTrajectoryToTarget(FootMovementSphere currentFoot, Transform legCastPoint)
    {
        Vector3 targetPos = FindLegTarget(legCastPoint);
        Vector3 moveAmount = targetPos - currentFoot.position;
        float t = footMoveTime;
        // float signedAngle = Vector3.SignedAngle(transform.forward, moveAmount.normalized, transform.up) * Mathf.Deg2Rad;
        float signedAngle = Mathf.Atan(feetMoveHeight / moveAmount.magnitude);
        float horizontal = (moveAmount.magnitude / t);
        // float y = 0;
        // float y = (feetMoveHeight - Physics.gravity.magnitude * -.5f * Mathf.Pow(t,2)) / (t);
        float y = Physics.gravity.magnitude * t / 2f + moveAmount.y;
        Vector3 velocity = new Vector3(horizontal * moveAmount.normalized.x, y, horizontal * moveAmount.normalized.z);
        // Vector3 velocity = new Vector3(0, y, 0);
        // force /= Time.deltaTime;
        // velocity.x *= velocity.x;
        // velocity.y *= velocity.y;
        // velocity.z *= velocity.z;
        Debug.Log($"Calculated Velocity: {velocity}, {velocity.magnitude}");


        return velocity;
    }

    Vector3 FindLegTarget(Transform legCastPoint)
    {
        RaycastHit hit;
        Vector3 dir = legCastPoint.forward;
        float moveAngle = Mathf.Atan(inputDir.x / inputDir.y) * Mathf.Rad2Deg;
        // dir = Quaternion.AngleAxis(moveAngle,transform.up)*dir;
        // if (inputDir.y < 0)
        // {
        //     moveAngle += 180f;
        // }
        dir = Quaternion.AngleAxis(moveAngle, transform.up) *
              Quaternion.AngleAxis(-feetMoveAngle * inputDir.y, transform.right) *
              Quaternion.AngleAxis(-feetMoveAngle * inputDir.x, transform.forward) * dir;

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

        Vector3 avg1 = (feet[0].position + feet[1].position) / 2f;
        Vector3 avg2 = (feet[2].position + feet[3].position) / 2f;
        Vector3 dir = avg1 - avg2;

        transform.forward = dir;
    }

    Vector3 AverageFeetPosition()
    {
        Vector3 avg = new Vector3();
        foreach (FootMovementSphere footMovementSphere in feet)
        {
            avg += footMovementSphere.position;
        }

        avg /= feet.Length;
        return avg;
    }

    void UpdateBody()
    {
        body.position = AverageFeetPosition() + new Vector3(0, bodyHeight, 0);
    }

    void MoveModel()
    {
        catModel.transform.position =
            Vector3.Lerp(catModel.transform.position, body.position, modelLerpSpeed * Time.deltaTime);
    }
}