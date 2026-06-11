using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAimRay : MonoBehaviour
{
    [Header("Ray Origin (point above player)")]
    [SerializeField] private Transform rayOrigin;

    private LineRenderer lr;

    private void Awake()
    {
        lr = rayOrigin.GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (rayOrigin == null || lr == null)
            return;

        Vector3 target = GetMouseWorldPoint();

        // Draw in Game View
        lr.SetPosition(0, rayOrigin.position);
        lr.SetPosition(1, target);

        // Optional: Scene View debug
        Debug.DrawLine(rayOrigin.position, target, Color.red);
    }

    private Vector3 GetMouseWorldPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // If the ray hits something in the world, use that
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return hit.point;

        // If it hits nothing (sky), aim forward from the camera
        return ray.origin + ray.direction * 100f;
    }

    public Vector3 GetAimDirection()
    {
        Vector3 target = GetMouseWorldPoint();
        return (target - rayOrigin.position).normalized;
    }

}
