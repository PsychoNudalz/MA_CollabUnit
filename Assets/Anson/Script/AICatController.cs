using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICatController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private float castRange = 30f;

    [SerializeField]
    private LayerMask layerMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NavMeshPath path = new NavMeshPath();
        
        // if (navMeshAgent.CalculatePath(target.position, path))
        Vector3 pos = transform.position;
        if (Physics.Raycast(transform.position, Vector3.down,out RaycastHit hit, castRange, layerMask))
        {
            pos = hit.point;
        }
        if (NavMesh.CalculatePath(pos,target.position,NavMesh.AllAreas,path))
        {
            if (path.corners.Length > 0)
            {
                Vector3 dir = path.corners[1] - transform.position;
                dir = dir.normalized;
                quadrupedMovementController.OnMove_World(dir);
                // navMeshAgent.SetDestination(target.position);
            }
        }
    }
}
