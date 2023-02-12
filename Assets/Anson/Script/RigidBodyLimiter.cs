using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyLimiter : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float maxVelocity = 50f;

    [SerializeField]
    private float maxAngular = 50f;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
        rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngular);
    }
}
