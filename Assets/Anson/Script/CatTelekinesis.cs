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

        hoverOffset = hoverPoint.position - transform.position;
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
                UpdateHoverPoint();
                ShowLines();

                break;
            case TelekinesisState.Pull:
                UpdateHoverPoint();
                ShowLines();

                break;
            case TelekinesisState.Shoot:
                UpdateHoverPoint();
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
                FindPiece(camera.forward, camera.position);
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
                OnTele_Aim();
                OnAimEvent.Invoke();
                break;
            case TelekinesisState.Aim:
                break;
            case TelekinesisState.Pull:
                SetLines(false);
                OnTele_Shoot(dir);
                OnShootEvent.Invoke();
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
                OnShootEvent.Invoke();

                break;
            case TelekinesisState.Aim:
                if (parts.Count > 0)
                {
                    OnTele_Pull(dir, castPoint);
                    OnPullEvent.Invoke();

                }
                else
                {
                    telekinesisState = TelekinesisState.Idle;
                    OnShootEvent.Invoke();

                }

                break;
            case TelekinesisState.Pull:
                break;
            case TelekinesisState.Shoot:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    void OnTele_Pull(Vector3 dir, Vector3 castPoint)
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
        telekinesisState = TelekinesisState.Shoot;

        ShootPieces(dir);
        telekinesisState = TelekinesisState.Idle;
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
                lineRenderer.SetPosition(1,Vector3.zero);
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
                breakablePart.SelfRb.velocity = Vector3.ClampMagnitude(breakablePart.SelfRb.velocity,
                    Mathf.Min(displacement.magnitude, pullVelocity_Max));
            }
        }
    }

    void ShootPieces(Vector3 dir)
    {
        // Debug.Log($"Tele Shoot: {parts.Count}");

        foreach (BreakablePart breakablePart in parts)
        {
            breakablePart.Launch(dir * shootAccel);
        }

        parts = new List<BreakablePart>();
    }


    void UpdateHoverPoint()
    {
        hoverPoint.position = transform.position + hoverOffset;
        vfx_BlackHole.SetVector3("CentrePosition",hoverPoint.position);
    }
}