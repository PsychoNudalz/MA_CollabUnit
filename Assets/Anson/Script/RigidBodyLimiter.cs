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

    private const bool USELIMITER = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (USELIMITER)
        {
            //     rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
            //     rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngular);
            if (rb.velocity.magnitude > maxVelocity || rb.angularVelocity.magnitude > maxAngular)
            {
                Debug.LogWarning($"{this} reached max RB limiter");
                rb.velocity = new Vector3();
                rb.angularVelocity = new Vector3();
                rb.transform.localPosition = new Vector3();
                rb.transform.localRotation = Quaternion.identity;
            }
        }
    }
}