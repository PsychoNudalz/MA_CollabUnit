using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPlayerController : MonoBehaviour
{
    public static CatPlayerController current;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    [SerializeField]
    private CatInputController catInputController;

    [Header("AP")] //this should be on a different script but fk it
    [SerializeField]
    private float AP_current = 0;

    [SerializeField]
    private Vector2 AP_range = new Vector2(0, 100f);

    [SerializeField]
    private float scoreToAPMultiplier = 0.001f;

    [SerializeField]
    private float teleBombCost = 25f;

    public QuadrupedMovementController QuadrupedMovementController => quadrupedMovementController;

    // Start is called before the first frame update
    void Awake()
    {
        current = this;
        if (!quadrupedMovementController)
        {
            quadrupedMovementController = GetComponentInChildren<QuadrupedMovementController>();
        }

        if (!catInputController)
        {
            catInputController = GetComponent<CatInputController>();
        }

        catInputController.CatPlayerController = this;
    }

    private void Start()
    {
        AddAP(0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddAP(float score)
    {
        AP_current = Mathf.Clamp(score * scoreToAPMultiplier + AP_current, AP_range.x, AP_range.y);
        UIManager.SetAPUI(AP_current / AP_range.y);
    }

    public bool UseTeleBomb()
    {
        if (AP_current >= teleBombCost)
        {
            AP_current = Mathf.Clamp(AP_current-teleBombCost, AP_range.x, AP_range.y);
            return true;
        }
        else
        {
            return false;
        }
    }
}