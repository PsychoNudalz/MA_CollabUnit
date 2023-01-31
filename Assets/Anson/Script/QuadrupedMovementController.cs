using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuadrupedMovementController : MonoBehaviour
{
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
    private float timeBetweenFoot = .5f;

    [SerializeField]
    private Vector2 inputDir;


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

        UpdateFeetToCastPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputDir.magnitude > 0f)
        {
            if (Time.time - lastMoveTime > timeBetweenFoot)
            {
                lastMoveTime = Time.time;
                LaunchCurrentFoot();
            }
        }

        UpdateTransformPosition();
    }

    private void FixedUpdate()
    {
    }

    void LaunchCurrentFoot()
    {
        FootMovementSphere currentFoot = feet[footIndex];

        Vector3 trajectory = FindTrajectoryToTarget(currentFoot, feetCastPoints[footIndex]);

        currentFoot.Launch(transform.up * 100f);
        footIndex = (footIndex + 1) % feet.Length;
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
                currentFoot.position = hit.point;
            }
        }
    }

    void OnMove(InputValue inputValue)
    {
        inputDir = inputValue.Get<Vector2>();
    }

    Vector3 FindTrajectoryToTarget(FootMovementSphere currentFoot, Transform legCastPoint)
    {
        Vector3 targetPos = FindLegTarget(legCastPoint);
        Vector3 moveAmount = targetPos - currentFoot.position;
        float t = timeBetweenFoot /
                  2f;
        // float signedAngle = Vector3.SignedAngle(transform.forward, moveAmount.normalized, transform.up) * Mathf.Deg2Rad;
        float signedAngle = Mathf.Atan(feetMoveHeight/moveAmount.magnitude);
        float horizontal = (moveAmount.magnitude / Mathf.Cos(signedAngle) /t);
        // float y = 0;
        float y = feetMoveHeight / (t*signedAngle-Physics.gravity.magnitude*.5f*t*t);
        Vector3 velocity = new Vector3(horizontal*moveAmount.normalized.x, y, horizontal*moveAmount.normalized.z);
        // force /= Time.deltaTime;
        // velocity.x *= velocity.x;
        // velocity.y *= velocity.y;
        // velocity.z *= velocity.z;
        Debug.Log($"Calculated Velocity: {velocity}, {velocity.magnitude}");
        currentFoot.SetVelocity(velocity);


        return new Vector3();
    }

    Vector3 FindLegTarget(Transform legCastPoint)
    {
        RaycastHit hit;
        Vector3 dir = legCastPoint.forward;
        float moveAngle = Mathf.Atan(inputDir.x / inputDir.y) * Mathf.Rad2Deg;
        // dir = Quaternion.AngleAxis(moveAngle,transform.up)*dir;
        dir = Quaternion.AngleAxis(moveAngle, transform.up) *
              Quaternion.AngleAxis(-feetMoveAngle * inputDir.y, transform.right) * dir;

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

    void UpdateTransformPosition()
    {
        transform.position = AverageFeetPosition();
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
}