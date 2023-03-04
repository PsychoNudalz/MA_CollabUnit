using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float velocityMultiplier = 1f;

    private Vector3 tempPosition = new Vector3();
    private Vector3 previousPosition = new Vector3();

    private Vector3 velocity;

    [SerializeField]
    private bool isDebug = false;

    public Rigidbody Rb => rb;

    public Vector3 Velocity => GetVelocity();


    private float checkTime = .2f;
    private float checkTimeNow = 0f;
    private Vector3 GetVelocity()
    {
        Vector3 temp = (transform.position - previousPosition) / checkTime;
        return temp*velocityMultiplier;
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
        if (checkTimeNow < 0)
        {
            checkTimeNow = checkTime;
            previousPosition = tempPosition;
            tempPosition = transform.position;
        }
        else
        {
            checkTimeNow -= Time.deltaTime;
        }
    }
    

    public void CarryOnVelocity(Vector3 velocity)
    {
        rb.velocity = velocity / velocityMultiplier;
    }
}