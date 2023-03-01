using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CP_Behaver : MonoBehaviour
{
 
    private NavMeshAgent navMashAgent;
    [SerializeField] private Transform movePositionTrasform;
   
    private void Awake()
    {
        GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        navMashAgent.destination = movePositionTrasform.position;
    }

}

/*
/// City people detecte cat state. NOT FINISHED!!!

public float speed;
public Transform target;
public float minimumDistance;
Vector3 targetPosition;

private void Start()
{
  targetPosition = FindObjectOfType<playerBehaver>().transform.position;
}

// Update is called once per frame
void Update()
{
    navMashAgent.destination = movePositionTrasform.position;




   if (Vector3.Distance(transform.position, targetPosition) < minimumDistance)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, -speed * Time.deltaTime);
    }

} 
*/

