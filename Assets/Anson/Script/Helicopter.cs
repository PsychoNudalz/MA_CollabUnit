using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private Vector2 radius = new Vector2(15, 20);

    [SerializeField]
    private AnimationCurve radiusCurve;

    [SerializeField]
    private Vector2 height = new Vector2(5, 10);

    [SerializeField]
    private AnimationCurve heightCurve;

    [SerializeField]
    private float speed = .5f;

    [SerializeField]
    float lerpSpeed = .5f;

    private float time = 0f;
    private Vector3 targetPos = new Vector3();
    private Quaternion targetRot = new Quaternion();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        UpdateTarget();
        UpdateTransformToTarget();
    }

    void UpdateTarget()
    {
        //position
        Vector3 newPos = new Vector3();
        newPos.y = height.x + (height.y - height.x) * heightCurve.Evaluate(time * speed);
        float newR = (radius.x + (radius.y - radius.x) * radiusCurve.Evaluate(time));
        newPos.x = Mathf.Sin(time * speed) * newR;
        newPos.z = Mathf.Cos(time * speed) * newR;
        newPos += target.transform.position;
        targetPos = newPos;
        // transform.position = newPos + target.transform.position;
        transform.LookAt(target);
    }

    void UpdateTransformToTarget()
    {
        transform.position = Vector3.Lerp(transform.position,targetPos,lerpSpeed*Time.deltaTime);
    }
}