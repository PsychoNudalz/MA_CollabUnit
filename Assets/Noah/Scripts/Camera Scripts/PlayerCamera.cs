using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Vector3 camEuler;

    private void Update()
    {
        camEuler = transform.eulerAngles;
    }
}
