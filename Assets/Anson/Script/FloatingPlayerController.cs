using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FloatingPlayerController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float speed = 20f;

    [SerializeField]
    private float mouseRotation = 10f;

    [Space(10)]
    [SerializeField]
    private LauncherScript launcherScript;
    private Vector3 moveDir = new Vector2();


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        transform.position += (moveDir.z * transform.forward+ moveDir.y * Vector3.up+
                               moveDir.x * transform.right) * (speed * Time.deltaTime);
    }

    public void OnMove(InputValue  inputValue)
    {
        Vector2 dir = inputValue.Get<Vector2>();
        moveDir.x = dir.x;
        moveDir.z = dir.y;
    }

    public void OnUp(InputValue  inputValue)
    {
        if (inputValue.isPressed)
        {
            moveDir.y = 1;
        }
        if(inputValue.Get<float>()<=.3f)
        {
            moveDir.y = 0;
        }
    }
    public void OnDown(InputValue inputValue)
    {
        if (inputValue.isPressed)
        {
            moveDir.y = -1;
        }
        if(inputValue.Get<float>()<=.3f)
        {
            moveDir.y = 0;
        }
    }

    public void OnLook(InputValue inputValue)
    {
        Vector2 look = inputValue.Get<Vector2>();
        transform.eulerAngles += new Vector3(-look.y * mouseRotation, look.x * mouseRotation, 0);
    }

    public void OnFire()
    {
        if (launcherScript)
        {
            launcherScript.Fire();
        }
    }
}
