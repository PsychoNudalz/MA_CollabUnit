using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager current;

    [SerializeField]
    private float score = 0;

    private void Awake()
    {
        if (current)
        {
            Destroy(current.gameObject);
        }

        current = this;
        // try
        // {
        //     score = PlayerPrefs.GetFloat("score");
        // }
        // catch (Exception e)
        // {
        //     score = 0;
        //     Console.WriteLine(e);
        // }
        UIManager.SetScoreUI(current.score);

    }

    private void OnDestroy()
    {
        // PlayerPrefs.SetFloat("score",score);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void AddScore_Raw(float score)
    {
        current.score += score;
        UIManager.SetScoreUI(current.score);
    }

    public static void AddScore_BySize(float score, float size)
    {
        current.score += score*Mathf.RoundToInt(size);
        UIManager.SetScoreUI(current.score);
    }
}
