using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class FlockAgent : MonoBehaviour
{

    Flock agentFlock;
    public Flock AgnetFlock { get { return agentFlock; } }

    Collider agentCollider;
    public Collider AgentCollider { get { return agentCollider; } }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider>();

    }

    public void Initialize(Flock flock)
    {
        agentFlock = flock;
    }

    public void Move(Vector3 velocity)
    {
        if (velocity.magnitude > 0)
        {
            transform.forward = velocity;
            transform.position += velocity * Time.deltaTime;
            transform.position= new Vector3(transform.position.x,agentFlock.transform.position.y,transform.position.z);
        }
    }
}
