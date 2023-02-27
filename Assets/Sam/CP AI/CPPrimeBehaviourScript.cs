using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CPPrimeBehaviourScript : MonoBehaviour
{
    [SerializeField] private Transform movePositionTrasform;
    
    private NavMeshAgent navMeshAgent; 

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
       navMeshAgent.destination = movePositionTrasform.position;


    }

}
