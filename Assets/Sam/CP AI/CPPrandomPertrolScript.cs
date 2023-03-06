using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CPPrandomPertrolScript : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;

    public Transform centrePoint;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private float castRange = 10f;

    public bool inStop => agent.remainingDistance <= agent.stoppingDistance;

    void Awake()
    {
        if (!agent)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (!centrePoint)
        {
            centrePoint = transform;
        }

        if (Physics.Raycast(transform.position + new Vector3(0, 1f, 0), Vector3.down, out RaycastHit hit, castRange,
                layerMask))
        {
            transform.position = hit.point;
        }

        Vector3 point;
        if (RandomPoint(centrePoint.position, range, out point))
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f);
            agent.SetDestination(point);
        }
    }


    void Update()
    {
        
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    public void SetDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}