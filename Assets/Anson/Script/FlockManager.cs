using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockManager : MonoBehaviour
{
    public static FlockManager current;

    [SerializeField]
    private FlockMasterController[] flockMasters;

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
        if (flockMasters is {Length: 0})
        {
            SetFlockMaster();
        }

        currentCat = CatPlayerController.current;
    }

    [ContextMenu("set list")]
    public void SetFlockMaster()
    {
        flockMasters = GetComponentsInChildren<FlockMasterController>();
    }

    private void Start()
    {
        foreach (FlockMasterController cpPrandomPertrolScript in flockMasters)
        {
            Vector3 point;
            if (RandomPoint(currentCat.transform.position, posRange,cpPrandomPertrolScript.transform.position.y, out point))
            {
                cpPrandomPertrolScript.SetDestination(point);
            }
        }
    }

    bool RandomPoint(Vector3 center, float range, float floor, out Vector3 result)
    {
        Vector2 random = Random.insideUnitCircle;
        Vector3 randomPoint = center + new Vector3(random.x, 0, random.y) * range;
        randomPoint.y = floor;
        UnityEngine.AI.NavMeshHit hit;

        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    private void FixedUpdate()
    {
        newPosTime_Now -= Time.deltaTime;
        if (newPosTime_Now < 0)
        {
            newPosTime_Now = newPosTime;

            Vector3 point;
            if (RandomPoint(currentCat.transform.position, posRange,flockMasters[index].transform.position.y, out point))
            {
                flockMasters[index].SetDestination(point);
            }

            index = (index + 1) % flockMasters.Length;
        }
    }
}