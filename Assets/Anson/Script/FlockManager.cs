using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockManager : MonoBehaviour
{
    public static FlockManager current;

    [SerializeField]
    private CPPrandomPertrolScript[] flockMasters;

    [SerializeField]
    private int index = 0;

    [SerializeField]
    private float newPosTime = 4f;
    private float newPosTime_Now = 0f;

    [Header("Settings")]
    [SerializeField]
    private float posRange = 50f;

    private CatPlayerController currentCat;
    

    private void Awake()
    {
        current = this;
        if (flockMasters.Length == 0)
        {
            SetFlockMaster();
        }

        currentCat = CatPlayerController.current;
    }

    [ContextMenu("set list")]
    public void SetFlockMaster()
    {
        flockMasters = GetComponentsInChildren<CPPrandomPertrolScript>();
    }

    private void Start()
    {
        foreach (CPPrandomPertrolScript cpPrandomPertrolScript in flockMasters)
        {
            
            Vector3 point;
            if (RandomPoint(currentCat.transform.position, posRange, out point))
            {
                cpPrandomPertrolScript.SetDestination(point);

            }
        }
    }

    private void FixedUpdate()
    {
        newPosTime_Now -= Time.deltaTime;
        if (newPosTime_Now < 0)
        {
            newPosTime_Now = newPosTime;
            
            Vector3 point;
            if (flockMasters[index].inStop)
            {
                if (RandomPoint(currentCat.transform.position, posRange, out point))
                {
                    flockMasters[index].SetDestination(point);
                }
            }

            index = (index + 1) % flockMasters.Length;
        }
    }


    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
