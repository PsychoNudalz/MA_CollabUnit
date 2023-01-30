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
    private float moveAngle = 15f;
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
    }

    private void FixedUpdate()
    {
        
    }

    void LaunchCurrentFoot()
    {
        FootMovementSphere currentFoot = feet[footIndex];
        currentFoot.Launch(transform.up*100f);
        footIndex = (footIndex + 1) % feet.Length;
    }

    [ContextMenu("Force Update Feet")]
    private void UpdateFeetToCastPosition()
    {
        Transform currentCast;
        FootMovementSphere currentFoot;
        RaycastHit hit;
        for (int i = 0; i < Mathf.Min(feet.Length,feetCastPoints.Length); i++)
        {
            currentCast = feetCastPoints[i];
            currentFoot = feet[i];
            if (Physics.Raycast(currentCast.position, currentCast.forward,out hit,castDistance,castLayer))
            {
                currentFoot.position = hit.point;
            }
        }
    }
    
    void OnMove(InputValue inputValue)
    {
        inputDir = inputValue.Get<Vector2>();
    }
    
    
}
