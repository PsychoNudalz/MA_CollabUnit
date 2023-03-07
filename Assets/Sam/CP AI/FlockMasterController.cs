using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FlockMasterController : MonoBehaviour
{
    public NavMeshAgent agent;
    public float range;

    public Transform centrePoint;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private float castRange = 10f;

    public bool inStop => agent.remainingDistance <= agent.stoppingDistance;
    // public bool inStop => true;

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
    }


    void Update()
    {
        
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        // Vector3 randomPoint = center + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, range, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        Debug.LogWarning($"Failed find {center}");
        return false;
    }

    public void SetDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}