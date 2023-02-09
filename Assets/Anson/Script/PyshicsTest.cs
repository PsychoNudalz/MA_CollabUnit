using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyshicsTest : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Vector3 torque = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddTorque(torque);
    }

    [ContextMenu("Reset")]
    public void ResetRB()
    {
        rb.velocity = new Vector3();
        rb.angularVelocity = new Vector3();
        transform.rotation = Quaternion.identity;
    }
}