using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    public NetworkVariable<int> CurrentHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Hit Flash (multiple meshes)")]
    [SerializeField] private Renderer[] renderers;   // assign all body parts here
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    private Color[] originalColors;
    private bool isFlashing = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            CurrentHealth.Value = maxHealth;

        CacheOriginalColors();

        CurrentHealth.OnValueChanged += OnHealthChanged;
    }

    private void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].material.color;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        //Debug.Log($"Player {OwnerClientId} health: {newValue}");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageRpc(int amount)
    {
        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - amount, 0);

        HitFlashClientRpc();

        if (CurrentHealth.Value <= 0)
        {
            Debug.Log($"Player {OwnerClientId} died.");

            // Despawn the player on the server
            NetworkObject.Despawn();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void HitFlashClientRpc()
    {
        if (!isFlashing)
            StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        isFlashing = true;

        // flash all meshes red
        foreach (var r in renderers)
            r.material.color = hitColor;

        yield return new WaitForSeconds(flashDuration);

        // restore original colors
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material.color = originalColors[i];

        isFlashing = false;
    }
}
