using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveableObject : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    private Vector3 previousPosition;

    private Vector3 velocity;

    public Vector3 Velocity => (transform.position - previousPosition)/Time.deltaTime;

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
    private void LateUpdate()
    {
        StartCoroutine(DelayUpdateVelocity());
    }
}
