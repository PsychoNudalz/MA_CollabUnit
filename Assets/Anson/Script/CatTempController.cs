using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CatTempController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float speed;

    [SerializeField]
    private Vector3 dir;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void UpdateVelocity(Vector3 input)
    {
        dir = input;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (rb)
        {
            rb.velocity = dir*speed;
        }

        transform.position += dir * speed * Time.deltaTime;
    }

    void OnMove(InputValue inputValue)
    {
        Vector2 temp = inputValue.Get<Vector2>();
        dir.x = temp.x;
        dir.z = temp.y;
    }
}
