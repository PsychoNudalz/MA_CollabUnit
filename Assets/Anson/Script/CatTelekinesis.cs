using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class CatTelekinesis : MonoBehaviour
{
    enum TelekinesisState
    {
        Idle,
        Aim,
        Pull,
        Shoot
    }

    private TelekinesisState telekinesisState = TelekinesisState.Idle;

    [SerializeField]
    private Transform hoverPoint;

    [SerializeField]
    private Transform parent;

    [SerializeField]
    private Vector3 hoverOffset;

    [Header("Aim")]
    [SerializeField]
    private Transform camera;

    [SerializeField]
    private LineRenderer lineBase;

    [SerializeField]
    private Transform lineParent;

    [SerializeField]
    private LineRenderer[] lines;


    [Header("Pull")]
    [SerializeField]
    private float pullCastRayRange = 20f;

    private float cameraRayCastRange = 300f;

    [SerializeField]
    private float pullCastRadius = 5f;

    [SerializeField]
    private LayerMask pullLayer;

    [SerializeField]
    private float pullDeadzone = 10f;

    [SerializeField]
    private float pullForce = 100000f;

    [SerializeField]
    private float pullTorque = 3f;

    [SerializeField]
    private float pullVelocity_Max = 2f;

    [SerializeField]
    private List<BreakablePart> parts = new List<BreakablePart>();

    [SerializeField]
    private int maxPartsSize = 5;

    [Header("Shoot")]
    [SerializeField]
    private float shootAccel = 100f;

    [Header("Effects")]
    [SerializeField]
    private VisualEffect vfx_BlackHole;

    [SerializeField]
    private UnityEvent OnAimEvent;

    [SerializeField]
    private UnityEvent OnPullEvent;

    [SerializeField]
    private UnityEvent OnShootEvent;


    private void Awake()
    {
        if (!hoverPoint)
        {
            hoverPoint = transform;
        }

        if (!parent)
        {
            parent = transform.parent;
        }

        hoverOffset = hoverPoint.position - parent.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main.transform;
        InitialiseLines();
    }

    // Update is called once per frame
    void Update()
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                break;
            case TelekinesisState.Aim:

                break;
            case TelekinesisState.Pull:
                UpdateHoverPoint();
                ShowLines();

                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FixedUpdate()
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                break;
            case TelekinesisState.Aim:
                break;
            case TelekinesisState.Pull:
                // FindPiece(camera.forward, camera.position);
                FindPiece_AOE();
                if (parts.Count > 0)
                {
                    MovePieces();
                    OnTele_Pull();
                }


                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void InitialiseLines()
    {
        lines = new LineRenderer[maxPartsSize];
        if (lineBase)
        {
            for (int i = 0; i < maxPartsSize; i++)
            {
                lines[i] = Instantiate(lineBase.gameObject, lineParent.position, lineParent.rotation, lineParent)
                    .GetComponent<LineRenderer>();
            }
        }

        SetLines(false);
    }

    public void OnTelekinesis_Press(Vector3 dir, Vector3 castPoint)
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:
                telekinesisState = TelekinesisState.Pull;
                OnAimEvent.Invoke();
                OnPullEvent.Invoke();

                break;
            case TelekinesisState.Aim:
                break;
            case TelekinesisState.Pull:
                
                break;
            case TelekinesisState.Shoot:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnTelekinesis_Release(Vector3 dir, Vector3 castPoint)
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Idle:

                break;
            case TelekinesisState.Aim:


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


    void OnTele_Pull()
    {
        if (parts.Count == 0)
        {
            return;
        }

        TelekinesisParts();
        telekinesisState = TelekinesisState.Pull;
    }

    void OnTele_Shoot(Vector3 dir)
    {

        ShootPieces(dir);
        telekinesisState = TelekinesisState.Idle;
        SetLines(false);
        OnShootEvent.Invoke();
        parts = new List<BreakablePart>();
    }

    void OnTele_Aim()
    {
        telekinesisState = TelekinesisState.Aim;
        // camera
    }

    void FindPiece(Vector3 dir, Vector3 castPoint)
    {
        parts = new List<BreakablePart>();
        RaycastHit hit;
        if (Physics.Raycast(castPoint, dir, out hit, cameraRayCastRange, pullLayer))
        {
            dir = hit.point - transform.position;
            // Debug.DrawLine(castPoint, hit.point, Color.magenta, 5f);
            if (Physics.Raycast(transform.position, dir.normalized, out hit, pullCastRayRange, pullLayer))
            {
                // Debug.DrawLine(transform.position, hit.point, Color.magenta, 5f);

                Collider[] hits = Physics.OverlapSphere(hit.point, pullCastRadius, pullLayer);
                foreach (Collider collider in hits)
                {
                    if (collider.TryGetComponent(out BreakablePart bp))
                    {
                        if (bp.CanTelekinesis)
                        {
                            parts.Add(bp);
                        }
                    }

                    if (parts.Count >= maxPartsSize)
                    {
                        return;
                    }
                }
            }
        }

        // Debug.Log($"Tele Parts: {parts.Count}");
    }

    void FindPiece_AOE()
    {
        if (parts.Count >= maxPartsSize)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, pullCastRadius, pullLayer);
        foreach (Collider collider in hits)
        {
            if (parts.Count >= maxPartsSize)
            {
                return;
            }
            if (collider.TryGetComponent(out BreakablePart bp))
            {
                if (bp.CanTelekinesis)
                {
                    parts.Add(bp);
                }
            }

        }
    }

    void ShowLines()
    {
        Vector3 pos;
        for (int i = 0; i < parts.Count; i++)
        {
            lines[i].gameObject.SetActive(true);
            // pos = parts[i].transform.position - lineParent.position;
            lines[i].SetPosition(0, lineParent.position);
            lines[i].SetPosition(1, parts[i].transform.position);
        }

        for (int i = parts.Count; i < lines.Length; i++)
        {
            lines[i].gameObject.SetActive(false);
        }
    }

    void SetLines(bool b)
    {
        foreach (LineRenderer lineRenderer in lines)
        {
            lineRenderer.gameObject.SetActive(b);
            if (!b)
            {
                lineRenderer.SetPosition(1, Vector3.zero);
            }
        }
    }

    bool CanCast(Vector3 dir, Vector3 castPoint, out Vector3 point)
    {
        RaycastHit hit;
        point = default;
        if (Physics.Raycast(castPoint, dir, out hit, cameraRayCastRange, pullLayer))
        {
            dir = hit.point - transform.position;
            Debug.DrawLine(castPoint, hit.point, Color.magenta, 5f);
            if (Physics.Raycast(transform.position, dir.normalized, out hit, pullCastRayRange, pullLayer))
            {
                Debug.DrawLine(transform.position, hit.point, Color.magenta, 5f);
                point = hit.point;
                return true;
            }
        }

        return false;
    }

    void TelekinesisParts()
    {
        foreach (BreakablePart breakablePart in parts)
        {
            breakablePart.Telekinesis();
        }
    }

    void MovePieces()
    {
        foreach (BreakablePart breakablePart in parts)
        {
            Vector3 displacement = hoverPoint.position - breakablePart.transform.position;
            if (displacement.magnitude > pullDeadzone)
            {
                breakablePart.SelfRb.AddForce(displacement.normalized * pullForce, ForceMode.Impulse);
                breakablePart.SelfRb.AddTorque(displacement*pullTorque);
            }
            

            breakablePart.SelfRb.velocity = Vector3.ClampMagnitude(breakablePart.SelfRb.velocity,
                Mathf.Min(displacement.magnitude, pullVelocity_Max));
        }
    }

    void ShootPieces(Vector3 dir)
    {
        // Debug.Log($"Tele Shoot: {parts.Count}");

        foreach (BreakablePart breakablePart in parts)
        {
            breakablePart.Launch(dir * shootAccel,parts);
        }

        parts = new List<BreakablePart>();
    }


    void UpdateHoverPoint()
    {
        float angle = Vector3.SignedAngle(Vector3.forward, camera.forward, Vector3.up);
        hoverPoint.position = parent.position + Quaternion.Euler(0, angle, 0) * hoverOffset;
        vfx_BlackHole.SetVector3("CentrePosition", hoverPoint.position);
    }
}