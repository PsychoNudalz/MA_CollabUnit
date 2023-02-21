using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager current;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private float scoreIncreaseAmount = 10f;

    private float score_Target = 0;
    private float score_Current = 0;

    private void Awake()
    {
        if (current)
        {
            Destroy(current.gameObject);
        }

        current = this;
    }

    void FixedUpdate()
    {
        if (Math.Abs(score_Current - score_Target) > 0.1f)
        {
            UpdateScoreUI();
        }
    }

    void UpdateScoreUI()
    {
        if (Math.Abs(score_Current - score_Target) < scoreIncreaseAmount)
        {
            score_Current = score_Target;
        }
        else
        {
            score_Current = Mathf.Lerp(score_Current, score_Target, scoreIncreaseAmount * Time.deltaTime);
        }

        current.scoreText.text = score_Current.ToString("0");
    }

    public static void SetScoreUI(float score)
    {
        if (!current)
        {
            return;
        }
        current.score_Target = score;
    }

    void Start()
    {
    }

    // Update is called once per frame
}