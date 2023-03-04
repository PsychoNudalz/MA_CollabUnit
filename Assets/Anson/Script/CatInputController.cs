using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CatInputController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private Camera camera;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    [Header("Look")]
    private bool isAim = false;

    [SerializeField]
    private CinemachineFreeLook originalCinemachine;

    [SerializeField]
    private CinemachineVirtualCamera aimCinemachine;

    [SerializeField]
    private Vector2 aimSpeed = new Vector2(10f, 10f);

    [SerializeField]
    private float cameraCastRange = 1000f;

    [SerializeField]
    private float originalFOV;

    [SerializeField]
    private float aimFOV = 20f;

    [SerializeField]
    private float FOVSpeed = 50f;

    private float targetFOV = 0;
    private float FOVDeadzone = 1f;

    [SerializeField]
    private LayerMask cameraLayer;

    [Header("Telekinesis")]
    [SerializeField]
    private CatTelekinesis catTelekinesis;

    [Header("Sounds For now")]
    [SerializeField]
    private SoundAbstract meowSound;


    private bool isTeleOn = false;
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
        originalFOV = originalCinemachine.m_Lens.FieldOfView;
        targetFOV = originalFOV;
    }

    // Update is called once per frame
    void Update()
    {
        MoveCat();
        UpdateFOV();
    }

    private void UpdateFOV()
    {
        float currentFOV = originalCinemachine.m_Lens.FieldOfView;
        if (Mathf.Abs(targetFOV - currentFOV) < 0.01f)
        {
            return;
        }

        else if (Mathf.Abs(targetFOV - currentFOV) < FOVDeadzone)
        {
            currentFOV = targetFOV;
        }
        else
        {
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, FOVSpeed * Time.deltaTime);
        }

        originalCinemachine.m_Lens.FieldOfView = currentFOV;
    }

    private void FixedUpdate()
    {
        quadrupedMovementController.GetClosestFoot(cameraTransform.forward, cameraTransform.position);
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
        meowSound?.Play();
        quadrupedMovementController.OnSwipe(cameraTransform.forward, cameraTransform.position);
    }

    public void OnHardReset()
    {
        quadrupedMovementController.HardReset();
    }

    public void OnTelekinesis(InputValue inputValue)
    {
        // print($"{inputValue.isPressed}  {inputValue.Get<float>()}");

        if (isTeleOn)
        {
            if (inputValue.Get<float>() < 0.5f)
            {
                catTelekinesis.OnTelekinesis_Release(GetDirToTarget(cameraTransform.position),
                    cameraTransform.position);
                isTeleOn = false;
            }
        }
        else
        {
            if (inputValue.Get<float>() >= 0.5f)
            {
                catTelekinesis.OnTelekinesis_Press(GetDirToTarget(cameraTransform.position), cameraTransform.position);
                isTeleOn = true;
            }
        }
    }

    public void OnAim(InputValue inputValue)
    {
        if (inputValue.Get<float>() < 0.5f)
        {
            isAim = false;
            targetFOV = originalFOV;
        }
        else
        {
            isAim = true;
            targetFOV = aimFOV;
        }
    }

    public void OnLook(InputValue inputValue)
    {
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

    public static Quaternion GetAngle_World_Static()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward = cameraForward.normalized;
        float angle = Vector3.SignedAngle(Vector3.forward, cameraForward, Vector3.up);
        return Quaternion.Euler(0, 0, -angle);
    }

    public Vector3 GetCameraTarget()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, cameraCastRange,
                cameraLayer))
        {
            return hit.point;
        }

        return default;
    }

    public Vector3 GetDirToTarget(Vector3 origin)
    {
        Vector3 target = GetCameraTarget();
        if (!target.Equals(default))
        {
            return (target - origin).normalized;
        }

        return cameraTransform.forward;
    }
}