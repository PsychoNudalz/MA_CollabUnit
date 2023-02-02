using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    private Vector3 tempPosition = new Vector3();
    private Vector3 previousPosition = new Vector3();

    private Vector3 velocity;

    [SerializeField]
    private bool isDebug = false;

    public Rigidbody Rb => rb;

    public Vector3 Velocity => GetVelocity();

    private Vector3 GetVelocity()
    {
        Vector3 temp = (transform.position - previousPosition) / Time.deltaTime;
        return temp;
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    // Update is called once per frame

    IEnumerator DelayUpdateVelocity()
    {
        yield return new WaitForEndOfFrame();
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        previousPosition = tempPosition;
        tempPosition = transform.position;
    }

    private void Update()
    {
        if (isDebug)
        {
            Debug.Log($"{this} position difference: {(transform.position - previousPosition).magnitude}, {Velocity}");
        }
    }
}