using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeAndLaunch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MouseAimRay aimRay;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private GameObject spherePrefab;

    [Header("Charge Settings")]
    [SerializeField] private float growSpeed = 2f;
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float maxSize = 5f;

    [Header("Cooldown")]
    [SerializeField] private float launchCooldown = 0.2f;
    private float cooldownTimer = 0f;

    private GameObject currentSphere;
    private bool charging = false;

    private void Update()
    {
        // Cooldown countdown
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (charging && currentSphere != null)
        {
            float radius = currentSphere.transform.localScale.y * 0.5f;
            currentSphere.transform.position = rayOrigin.position + Vector3.up * radius;

            Vector3 scale = currentSphere.transform.localScale;

            if (scale.x < maxSize)
            {
                float growAmount = growSpeed * Time.deltaTime;
                scale += Vector3.one * growAmount;
                scale = Vector3.one * Mathf.Min(scale.x, maxSize);
                currentSphere.transform.localScale = scale;
            }

            aimRay.SetIndicatorScale(scale.x);
        }
    }

    public void OnFire(InputValue value)
    {
        bool pressed = value.Get<float>() > 0.5f;

        if (pressed)
            StartCharging();
        else
            ReleaseSphere();
    }

    private void StartCharging()
    {
        // Block charging if still cooling down
        if (cooldownTimer > 0f)
            return;

        if (currentSphere != null)
            return;

        charging = true;

        currentSphere = Instantiate(spherePrefab, rayOrigin.position, Quaternion.identity);
        currentSphere.transform.localScale = Vector3.one * 0.5f;

        float radius = currentSphere.transform.localScale.y * 0.5f;
        currentSphere.transform.position = rayOrigin.position + Vector3.up * radius;
    }

    private void ReleaseSphere()
    {
        if (currentSphere == null)
            return;

        charging = false;

        Vector3 dir = aimRay.GetAimDirection();

        Rigidbody rb = currentSphere.GetComponent<Rigidbody>();
        if (rb == null)
            rb = currentSphere.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = dir * launchForce;

        currentSphere = null;

        // Reset indicator size
        aimRay.SetIndicatorScale(1f);

        // Start cooldown
        cooldownTimer = launchCooldown;
    }
}
