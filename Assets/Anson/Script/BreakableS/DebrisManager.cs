using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class DebrisManager : MonoBehaviour
{
    [SerializeField]
    private List<BreakableComponent> debris = new List<BreakableComponent>();

    [SerializeField]
    private List<BreakableComponent> buffer = new List<BreakableComponent>();

    [Header("Settings")]
    [SerializeField]
    private Vector2 debrisSize = new Vector2(100f, 200f);

    [SerializeField]
    private float debrisClearTime = 5f;

    private float debrisClearTime_Now = 5f;

    [SerializeField]
    private float bufferCheckTime = 1f;

    private float bufferCheckTime_now = 1f;

    [SerializeField]
    private AnimationCurve indexDistribution;

    public static DebrisManager current;

    private void Awake()
    {
        if (current)
        {
            Destroy(current.gameObject);
        }
        else
        {
            current = this;
        }
    }

    private void FixedUpdate()
    {
        debrisClearTime_Now -= Time.deltaTime;
        int r = 0;
        if (debrisClearTime_Now < 0)
        {
            debrisClearTime_Now = debrisClearTime;
            if (debris.Count >= debrisSize.x)
            {
                int x = debris.Count - (int) debrisSize.x;
                for (int i = 0; i < x; i++)
                {
                    r = GetRandomIndex();
                    buffer.Add(debris[r]);
                    debris.RemoveAt(r);
                }
            }
        }


        if (buffer.Count > 0)
        {
            bufferCheckTime_now -= Time.deltaTime;
            if (bufferCheckTime_now < 0)
            {
                bufferCheckTime_now = bufferCheckTime;
                foreach (BreakableComponent breakableComponent in buffer.ToArray())
                {
                    if (breakableComponent)
                    {
                        buffer.Remove(breakableComponent);

                        if (breakableComponent.BreakableState is BreakableState.Telekinesis
                            or BreakableState.Telekinesis_Shoot)
                        {
                            Add(breakableComponent);
                        }
                        else
                        {
                            try
                            {
                                breakableComponent.Despawn(false);
                            }
                            catch (Exception e)
                            {
                                breakableComponent.Despawn(true);
                            }
                        }
                    }
                    else
                    {
                        buffer.Remove(breakableComponent);
                    }
                }
            }
        }
    }

    public static void Add(BreakableComponent bc)
    {
        if (!current)
        {
            Debug.LogError("Missing Breakable Manager!");
            return;
        }

        current.debris.Add(bc);
        if (current.debris.Count > current.debrisSize.y)
        {
            int r = GetRandomIndex_S();
            current.buffer.Add(current.debris[r]);
            current.debris.RemoveAt(r);
        }
    }

    public int GetRandomIndex()
    {
        return Mathf.FloorToInt(indexDistribution.Evaluate(Random.Range(0f, .999f)) * debris.Count);
    }

    public static int GetRandomIndex_S()
    {
        return Mathf.FloorToInt(current.indexDistribution.Evaluate(Random.Range(0f, .999f)) * current.debris.Count);
    }

    [Command("SetDM_Max")]
    public static void SetDM_Max(int v)
    {
        current.debrisSize.y = v;
    }
    [Command("SetDM_Min")]
    public static void SetDM_Min(int v)
    {
        current.debrisSize.x = v;
    }
}