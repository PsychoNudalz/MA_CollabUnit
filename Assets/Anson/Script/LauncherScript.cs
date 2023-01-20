using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;

public class LauncherScript : MonoBehaviour
{
    [SerializeField]
    private GameObject launchGO;

    [SerializeField]
    private float force = 200;


    private void Start()
    {
        Fire();
    }

    [ContextMenu("Fire")]
    public void Fire()
    {
        Rigidbody rb = GameObject.Instantiate(launchGO,transform.position,Quaternion.identity).GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * force * rb.mass);
        Destroy(rb.gameObject, 10f);
    }
}