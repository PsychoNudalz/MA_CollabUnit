using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager current;

    [SerializeField]
    private float score = 0;

    [Header("Kitten")]
    [SerializeField]
    private KittenManager kittenManager;

    [SerializeField]
    private float startingScore = 1000000;

    [SerializeField]
    private float scoreAmount = 700000;

    [SerializeField]
    private float[] scoreThresholds = new float[8];

    private int scoreThreshold_Index = 0;
    private float scoreThresholds_current;


    [ContextMenu("Generate Score")]
    public void GenerateScore()
    {
        for (int i = 0; i < scoreThresholds.Length; i++)
        {
            scoreThresholds[i] = startingScore + (i * scoreAmount);
        }
    }

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

        if (!kittenManager)
        {
            kittenManager = FindObjectOfType<KittenManager>();
        }
    }

    void Start()
    {
        if (scoreThresholds.Length != kittenManager.kittenObjects.Count)
        {
            Debug.LogWarning($"{scoreThresholds.Length} != {kittenManager.kittenObjects.Count}");
        }

        scoreThresholds_current = scoreThresholds[scoreThreshold_Index];
    }

    private void OnDestroy()
    {
        // PlayerPrefs.SetFloat("score",score);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void CheckScore()
    {
        while (scoreThreshold_Index < scoreThresholds.Length && score > scoreThresholds_current)
        {
            kittenManager.SpawnKitten();
            scoreThresholds_current = scoreThresholds[scoreThreshold_Index];
            scoreThreshold_Index++;
        }
    }

    public static void AddScore_Raw(float score)
    {
        AddScore(score);
    }

    private static void AddScore(float score)
    {
        current.score += score;
        current.CheckScore();
        UIManager.SetScoreUI(current.score);
    }

    public static void AddScore_BySize(float score, float size)
    {
        AddScore(score * Mathf.RoundToInt(size));
    }
}