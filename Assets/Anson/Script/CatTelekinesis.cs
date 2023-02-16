using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatTelekinesis : MonoBehaviour
{
    enum TelekinesisState
    {
        Idle,
        Pull,
        Shoot
    }

    private TelekinesisState telekinesisState = TelekinesisState.Idle;
    [SerializeField]
    private Transform hoverPoint;

    [Header("Pull")]
    [SerializeField]
    private float pullCastRayRange = 20f;

    private float cameraRayCastRange = 200f;

    [SerializeField]
    private float pullCastRadius = 5f;

    [SerializeField]
    private LayerMask pullLayer;

    [SerializeField]
    private float pullDeadzone = 10f;
    [SerializeField]
    private float pullForce = 100000f;

    [SerializeField]
    private float pullVelocity_Max = 2f;
    
    [SerializeField]
    private List<BreakablePart> parts = new List<BreakablePart>();

    [SerializeField]
    private int maxPartsSize = 5;

    [Header("Shoot")]
    [SerializeField]
    private float shootAccel = 100f;

    private void Awake()
    {
        if (!hoverPoint)
        {
            hoverPoint = transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                break;
            case TelekinesisState.Pull:
                if (parts.Count > 0)
                {
                    MovePieces();
                }
                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnTelekinesis_Press(Vector3 dir,Vector3 castPoint)
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                break;
            case TelekinesisState.Pull:
                OnTele_Shoot(dir);
                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void OnTelekinesis_Release(Vector3 dir,Vector3 castPoint)
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                OnTele_Pull(dir,castPoint);
 
                break;
            case TelekinesisState.Pull:
                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    void OnTele_Pull(Vector3 dir,Vector3 castPoint)
    {
        FindPiece(dir,castPoint);
        if (parts.Count > 0)
        {
            telekinesisState = TelekinesisState.Pull;

        }
    }

    void OnTele_Shoot(Vector3 dir)
    {
        telekinesisState = TelekinesisState.Shoot;
        
        ShootPieces(dir);
        telekinesisState = TelekinesisState.Idle;
        

    }

    void FindPiece(Vector3 dir,Vector3 castPoint)
    {
        parts = new List<BreakablePart>();
        RaycastHit hit;
        if (Physics.Raycast(castPoint, dir, out hit, cameraRayCastRange, pullLayer))
        {
            dir = hit.point- transform.position;
            Debug.DrawLine(castPoint,hit.point,Color.magenta,5f);
            if (Physics.Raycast(transform.position, dir.normalized, out hit, pullCastRayRange, pullLayer))
            {
                Debug.DrawLine(transform.position,hit.point,Color.magenta,5f);

                RaycastHit[] hits = Physics.SphereCastAll(hit.point, pullCastRadius, Vector3.up, 0, pullLayer);
                foreach (RaycastHit raycastHit in hits)
                {
                    if (raycastHit.collider.TryGetComponent(out BreakablePart bp))
                    {
                        parts.Add(bp);
                        bp.Telekinesis();
                    }
                }
            }
        }

        Debug.Log($"Tele Parts: {parts.Count}");
    }

    void MovePieces()
    {
        foreach (BreakablePart breakablePart in parts)
        {
            Vector3 displacement = hoverPoint.position - breakablePart.transform.position;
            if (displacement.magnitude > pullDeadzone)
            {
                breakablePart.SelfRb.AddForce(displacement.normalized*pullForce,ForceMode.Impulse);
                breakablePart.SelfRb.velocity = Vector3.ClampMagnitude(breakablePart.SelfRb.velocity,
                    Mathf.Min(displacement.magnitude, pullVelocity_Max));
            }
        }
    }

    void ShootPieces(Vector3 dir)
    {
        Debug.Log($"Tele Shoot: {parts.Count}");

        foreach (BreakablePart breakablePart in parts)
        {
            breakablePart.Launch(dir*shootAccel);
        }
    }
}
