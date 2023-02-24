using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSensitivity : MonoBehaviour
{
    [SerializeField] private float controllerCamSensitivity;
    private CinemachineFreeLook cam;
    private void Start()
    {
        cam = GetComponent<CinemachineFreeLook>();
        if (Gamepad.all.Count > 0)
        {
            cam.m_XAxis.m_MaxSpeed = controllerCamSensitivity;
        }
    }
}
