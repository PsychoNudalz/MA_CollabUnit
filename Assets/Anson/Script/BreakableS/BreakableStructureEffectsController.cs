using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BreakableStructureEffectsController : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField]
    private VisualEffect[] vfx_Breaks;
    private float breakTime = 0.1f;

    private float breakTime_Now = -1;

    [SerializeField]
    private VisualEffect[] vfx_Collision;

    [Header("SFX")]
    [SerializeField]
    private SoundAbstract breakSound;

    [SerializeField]
    private float soundTime = 0.1f;

    [Header("Rubble")]
    [SerializeField]
    private GameObject rubbleGO;

    [SerializeField]
    private float rubbleSpawnTime = 1.5f;
    private float soundTime_Now = -1;    
    private Queue<Tuple<Vector3, Mesh>> breakPosQueue = new Queue<Tuple<Vector3, Mesh>>();

    private int QUEUESIZE = 20;
    // [SerializeField]

    // Start is called before the first frame update
    void Start()
    {
        if (rubbleGO)
        {
            rubbleGO.transform.localScale = Vector3.zero;
            rubbleGO.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (soundTime_Now > 0)
        {
            soundTime_Now -= Time.deltaTime;
        }
        PlayBreakEffectsFromQueue();
    }
    
    public void QueueBreakEffects(Vector3 position,Mesh mesh)
    {
        if (breakPosQueue.Count < QUEUESIZE)
        {
            breakPosQueue.Enqueue(new Tuple<Vector3, Mesh>(position,mesh));
        }
        // print(breakPosQueue.Count);
    }

    void PlayBreakEffectsFromQueue()
    {
        
        if (breakPosQueue.Count>0)
        {
            Tuple<Vector3, Mesh> tuple = breakPosQueue.Dequeue();
            PlayBreakEffects(tuple.Item1,tuple.Item2);

            
        }
    }
    
    public void PlayBreakEffects(Vector3 position,Mesh mesh)
    {
        foreach (VisualEffect vfx_Break in vfx_Breaks)
        {
            if (vfx_Break)
            {
                vfx_Break.SetVector3("Position",position);
                vfx_Break.SetMesh("SpawnMesh",mesh);
                vfx_Break.Play();
            }
        }

        breakSound.Play();

        if (soundTime_Now <= 0)
        {
            soundTime_Now = soundTime;
        }
    }

    public void ShowRubble()
    {
        if (!rubbleGO)
        {
            return;
        }
        if (rubbleGO.activeSelf)
        {
            return;
        }
        rubbleGO.SetActive(true);
        LeanTween.scale(rubbleGO, Vector3.one, rubbleSpawnTime);
    }
}