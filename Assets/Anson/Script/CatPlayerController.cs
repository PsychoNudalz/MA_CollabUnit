using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPlayerController : MonoBehaviour
{
    public static CatPlayerController current;

    [SerializeField]
    private QuadrupedMovementController quadrupedMovementController;

    public QuadrupedMovementController QuadrupedMovementController => quadrupedMovementController;

    // Start is called before the first frame update
    void Awake()
    {
        current = this;
        if (!quadrupedMovementController)
        {
            quadrupedMovementController = GetComponentInChildren<QuadrupedMovementController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
