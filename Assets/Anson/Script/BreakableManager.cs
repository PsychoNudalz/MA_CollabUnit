using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableManager : MonoBehaviour
{
    [SerializeField]
    private Queue<BreakableComponent> debris = new Queue<BreakableComponent>();

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
    
    public static BreakableManager current;

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
        if (debrisClearTime_Now < 0)
        {
            debrisClearTime_Now = debrisClearTime;
            if (debris.Count >= debrisSize.x)
            {
                int x = debris.Count - (int) debrisSize.x;
                for (int i = 0; i < x; i++)
                {
                    buffer.Add(debris.Dequeue());
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
                        breakableComponent.Despawn();
                        buffer.Remove(breakableComponent);
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
        current.debris.Enqueue(bc);
        if (current.debris.Count > current.debrisSize.y)
        {
            current.buffer.Add(current.debris.Dequeue());

        }
    }
    
}
