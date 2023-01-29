using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsQuadrapet : MonoBehaviour
{
    [Header("Body Parts")]
    [SerializeField]
    private Transform body;

    [SerializeField]
    private Transform[] feet;

    [Header("Stats")]
    [SerializeField]
    private float bodyToFeetHeight = 2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 avgPoint = new Vector3();
        foreach (Transform foot in feet)
        {
            avgPoint += foot.position;
        }

        avgPoint /= 4f;
        body.transform.position = avgPoint + transform.up * bodyToFeetHeight;
    }
}