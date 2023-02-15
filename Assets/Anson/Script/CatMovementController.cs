using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CatMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private Camera camera;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    private Transform cameraTransform;

    private Vector2 moveInput = new Vector2();
    // Start is called before the first frame update

    private void Awake()
    {
        if (!camera)
        {
            camera = Camera.main;
        }

        cameraTransform = camera.transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCat();
    }

    public void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
        // Vector3 inputDir = new Vector3(inputDir_v2.x, 0, inputDir_v2.y);
    }

    public void OnJump()
    {
        // print("Jump");
        quadrupedMovementController.Jump();
    }

    public void OnSwipe()
    {
        quadrupedMovementController.OnSwipe(cameraTransform.forward,cameraTransform.position);
    }

    public void OnHardReset()
    {
        quadrupedMovementController.HardReset();
    }

    private void MoveCat()
    {
        Vector2 moveDir = GetAngle_Local() * moveInput;
        quadrupedMovementController.OnMove(moveDir);
    }

    private Quaternion GetAngle_Local()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        float angle = Vector3.SignedAngle(transform.forward, cameraForward, transform.up);
        return Quaternion.Euler(0, 0, -angle);
    }

    private Quaternion GetAngle_World()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, cameraForward, transform.up);
        return Quaternion.Euler(0, 0, -angle);
    }
}