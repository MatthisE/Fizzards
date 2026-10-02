using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class MouseAimRay : NetworkBehaviour
{
    [Header("Ray Origin (point above player)")]
    [SerializeField] private Transform rayOrigin;

    [Header("Aim Indicator")]
    [SerializeField] private GameObject aimIndicator;
    [SerializeField] private LayerMask worldLayer;

    [Header("Indicator Growth")]
    [SerializeField] private float indicatorGrowthMultiplier = 1.5f;
    [SerializeField] private float shrinkSpeed = 15f;

    private float targetIndicatorScale = 1f;
    private float currentIndicatorScale = 1f;

    private LineRenderer lr;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        lr = rayOrigin.GetComponent<LineRenderer>();
        lr.enabled = false;

        if (aimIndicator != null)
        {
            currentIndicatorScale = aimIndicator.transform.localScale.x;
            targetIndicatorScale = currentIndicatorScale;
            aimIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (rayOrigin == null || lr == null) return;

        Vector3 target = GetMouseWorldPoint(out bool hitWorld, out RaycastHit hit);

        Debug.DrawLine(rayOrigin.position, target, Color.red);

        UpdateAimIndicator(hitWorld, hit);

        currentIndicatorScale = Mathf.Lerp(
            currentIndicatorScale,
            targetIndicatorScale,
            Time.deltaTime * shrinkSpeed
        );

        if (aimIndicator != null)
        {
            aimIndicator.transform.localScale = Vector3.one * currentIndicatorScale;
        }
    }

    public void ResetIndicator()
    {
        if (!IsOwner) return;

        if (aimIndicator != null)
        {
            aimIndicator.SetActive(true);
            currentIndicatorScale = aimIndicator.transform.localScale.x;
            targetIndicatorScale = currentIndicatorScale;
        }
    }

    private void UpdateAimIndicator(bool hitWorld, RaycastHit hit)
    {
        if (aimIndicator == null) return;

        if (!hitWorld)
        {
            aimIndicator.SetActive(false);
            return;
        }

        aimIndicator.SetActive(true);
        aimIndicator.transform.position = hit.point + hit.normal * 0.02f;
        aimIndicator.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
    }

    private Vector3 GetMouseWorldPoint(out bool hitWorld, out RaycastHit hit)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 1000f, worldLayer))
        {
            hitWorld = true;
            return hit.point;
        }

        hitWorld = false;
        return ray.origin + ray.direction * 100f;
    }

    private Vector3 GetMouseWorldPoint()
    {
        return GetMouseWorldPoint(out _, out _);
    }

    public Vector3 GetAimDirection()
    {
        Vector3 target = GetMouseWorldPoint();
        return (target - rayOrigin.position).normalized;
    }

    public void SetIndicatorScale(float sphereScale)
    {
        float radius = sphereScale * 0.5f;
        float diameter = radius * 2f;

        targetIndicatorScale = diameter * indicatorGrowthMultiplier;
    }
}
