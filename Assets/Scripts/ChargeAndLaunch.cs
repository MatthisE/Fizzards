using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeAndLaunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MouseAimRay aimRay;     // your existing script
    [SerializeField] private Transform rayOrigin;    // point above head
    [SerializeField] private GameObject spherePrefab;

    [Header("Charge Settings")]
    [SerializeField] private float growSpeed = 2f;   // how fast it grows
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float maxSize = 3f;


    private GameObject currentSphere;
    private bool charging = false;

    private void Update()
    {
        if (charging && currentSphere != null)
        {
            // Always follow the player
            float radius = currentSphere.transform.localScale.y * 0.5f;
            currentSphere.transform.position = rayOrigin.position + Vector3.up * radius;

            // Only grow if below max size
            Vector3 scale = currentSphere.transform.localScale;

            if (scale.x < maxSize)
            {
                float growAmount = growSpeed * Time.deltaTime;

                // Grow uniformly (stay a sphere)
                scale += Vector3.one * growAmount;

                // Clamp to max size
                scale = Vector3.one * Mathf.Min(scale.x, maxSize);

                currentSphere.transform.localScale = scale;
            }
        }
    }


    public void OnFire(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        if (pressed)
        {
            StartCharging();
        }
        else
        {
            ReleaseSphere();
        }
    }

    private void StartCharging()
    {
        if (currentSphere != null) return;

        charging = true;

        currentSphere = Instantiate(spherePrefab, rayOrigin.position, Quaternion.identity);

        // Start small, as a sphere
        currentSphere.transform.localScale = Vector3.one * 0.5f;

        // Position it just above the head
        float radius = currentSphere.transform.localScale.y * 0.5f;
        currentSphere.transform.position = rayOrigin.position + Vector3.up * radius;
    }


    private void ReleaseSphere()
    {
        if (currentSphere == null) return;

        charging = false;

        // Get direction from your MouseAimRay script
        Vector3 dir = aimRay.GetAimDirection();

        // Add Rigidbody if not present
        Rigidbody rb = currentSphere.GetComponent<Rigidbody>();
        if (rb == null)
            rb = currentSphere.AddComponent<Rigidbody>();

        rb.useGravity = false;

        // Launch it
        rb.linearVelocity = dir * launchForce;

        // Clear reference so a new one can spawn
        currentSphere = null;
    }
}
