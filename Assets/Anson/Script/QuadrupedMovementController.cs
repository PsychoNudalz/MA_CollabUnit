using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrupedMovementController : MonoBehaviour
{

    [Header("Feet")]
    [SerializeField]
    private FootMovementSphere[] feet;

    [SerializeField]
    private int footIndex = 0;

    [SerializeField]
    private float moveAngle = 15f;
    [Header("Cast Points")]
    [SerializeField]
    private Transform[] feetCastPoints;

    [SerializeField]
    private float castDistance = 7f;

    [SerializeField]
    private LayerMask castLayer;
    // Start is called before the first frame update
    void Start()
    {
        foreach (FootMovementSphere footMovementSphere in feet)
        {
            footMovementSphere.Initialize(this);
        }

        if (feet.Length != feetCastPoints.Length)
        {
            Debug.LogError($"{this} feet array length mismatch.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateFeetToCastPosition()
    {
        
    }
}
