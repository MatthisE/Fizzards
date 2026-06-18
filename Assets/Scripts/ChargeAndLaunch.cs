using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChargeAndLaunch : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private MouseAimRay aimRay;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private GameObject spherePrefab;

    [Header("Fire Settings")]
    [SerializeField] private float launchForce = 20f;

    [Header("Cooldown")]
    [SerializeField] private float launchCooldown = 0.2f;
    private float cooldownTimer = 0.2f;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = false;   // disable until ownership is assigned
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // enable input ONLY when ownership is confirmed
            playerInput.enabled = true;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        // Only fire on actual button press
        if (!context.started) return;

        // Cooldown
        if (cooldownTimer > 0f) return;

        Vector3 dir = aimRay.GetAimDirection().normalized;
        SpawnFireballServerRpc(rayOrigin.position, dir);

        cooldownTimer = launchCooldown;
    }

    [ServerRpc]
    private void SpawnFireballServerRpc(Vector3 pos, Vector3 dir)
    {
        GameObject fb = Instantiate(spherePrefab, pos, Quaternion.identity);

        NetworkObject netObj = fb.GetComponent<NetworkObject>();
        netObj.Spawn(); // everyone sees it

        Rigidbody rb = fb.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = dir * launchForce;
    }
}
