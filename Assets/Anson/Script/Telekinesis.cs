using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class Telekinesis : MonoBehaviour
{
    enum TelekinesisState
    {
        Idle,
        Aim,
        Pull,
        Shoot
    }

    private TelekinesisState telekinesisState = TelekinesisState.Idle;


    [Header("Aim")]
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
    private SoundAbstract sfx_tele_Pull;

    [SerializeField]
    private SoundAbstract sfx_tele_Shoot;


    [SerializeField]
    private UnityEvent OnAimEvent;

    [SerializeField]
    private UnityEvent OnPullEvent;

    [SerializeField]
    private UnityEvent OnShootEvent;

    [Header("Settings")]
    [SerializeField]
    private bool pullOnStart = true;
    

    void Start()
    {
        InitialiseLines();
        if (pullOnStart)
        {
            OnTele_Pull();
        }
    }

    void Update()
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Pull:
                ShowLines();

                break;
        }
    }

    private void FixedUpdate()
    {
        switch (telekinesisState)
        {
            case TelekinesisState.Pull:
                if (parts.Count < maxPartsSize)
                {
                    FindPiece_AOE();
                }

                if (parts.Count > 0)
                {
                    MovePieces();
                }
                break;
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
                OnTele_Pull();
                break;

        }
    }


    public void OnTelekinesis_Release(Vector3 dir, Vector3 castPoint)
    {
        switch (telekinesisState)
        {

            case TelekinesisState.Pull:
                OnTele_Shoot(dir);


                break;

        }
    }


    public void OnTele_Pull()
    {
        OnAimEvent.Invoke();
        OnPullEvent.Invoke();
        sfx_tele_Pull.Play();
        telekinesisState = TelekinesisState.Pull;
    }

    public void OnTele_Shoot(Vector3 dir)
    {
        ShootPieces(dir);
        telekinesisState = TelekinesisState.Idle;
        SetLines(false);
        OnShootEvent.Invoke();
        parts = new List<BreakablePart>();
        sfx_tele_Pull.Stop();
        sfx_tele_Shoot.PlayF();
    }

    public void OnTele_Explode()
    {
        ExplodePieces();
        telekinesisState = TelekinesisState.Idle;
        SetLines(false);
        OnShootEvent.Invoke();
        parts = new List<BreakablePart>();
        sfx_tele_Pull.Stop();
        sfx_tele_Shoot.PlayF();
    }



    void FindPiece(Vector3 dir, Vector3 castPoint)
    {
        if (parts.Count >= maxPartsSize)
        {
            return;
        }

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
                            AddPart(bp);
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

    private void AddPart(BreakablePart bp)
    {
        if (!transform.parent.Equals(bp.transform))
        {
            // print($"{transform.parent} and {bp.transform}");
            parts.Add(bp);
            bp.Telekinesis();
        }
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
                    AddPart(bp);
                }
            }
        }
    }

    void ShowLines()
    {
        Vector3 pos;
        for (int i = 0; i < parts.Count; i++)
        {
            
            if (parts[i]&&parts[i].gameObject)
            {
                lines[i].gameObject.SetActive(true);
                // pos = parts[i].transform.position - lineParent.position;
                lines[i].SetPosition(0, lineParent.position);
                lines[i].SetPosition(1, parts[i].transform.position);
            }
            else
            {
                lines[i].gameObject.SetActive(false);
            }
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
            Vector3 displacement = transform.position - breakablePart.transform.position;
            if (displacement.magnitude > pullDeadzone)
            {
                breakablePart.SelfRb.AddForce(displacement.normalized * pullForce, ForceMode.Impulse);
                breakablePart.SelfRb.AddTorque(displacement * pullTorque);
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
            breakablePart.Launch(dir * shootAccel, parts);
        }

        parts = new List<BreakablePart>();
    }

    void ExplodePieces()
    {
        foreach (BreakablePart breakablePart in parts)
        {
            breakablePart.Launch((new Vector3(Random.Range(-1f,1f),Random.Range(-1f,1f),Random.Range(-1f,1f)).normalized) * shootAccel, parts);
        }

        parts = new List<BreakablePart>();
    }


    void UpdateHoverPoint()
    {
        // float angle = Vector3.SignedAngle(Vector3.forward, camera.forward, Vector3.up);
        // hoverPoint.position = parent.position + Quaternion.Euler(0, angle, 0) * hoverOffset;
        vfx_BlackHole.SetVector3("CentrePosition", transform.position);
    }
}