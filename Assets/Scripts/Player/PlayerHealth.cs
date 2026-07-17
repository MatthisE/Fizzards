using System.Diagnostics;
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

    [Header("Death Settings")]
    [SerializeField] private GameObject aimIndicator;
    [SerializeField] private Material transparentMaterial;

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

        if (CurrentHealth.Value <= 0)
        {
            UnityEngine.Debug.Log($"Player {OwnerClientId} died.");

            if (IsServer)
                GameManager.Instance.NotifyPlayerDied();

            ApplyTransparentMaterialClientRpc();
            DisableAimIndicatorClientRpc();
            SetDeadLayerClientRpc();
            DisableLocalScriptsClientRpc();
            HideForOthersClientRpc();
        } else
        {
            UnityEngine.Debug.Log($"Player {OwnerClientId} flashed.");

            HitFlashClientRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyTransparentMaterialClientRpc()
    {
        // Only the dead player should see himself transparent
        if (!IsOwner)
            return;

        foreach (var r in renderers)
        {
            r.material = new Material(transparentMaterial);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void DisableAimIndicatorClientRpc()
    {
        if (aimIndicator != null)
            aimIndicator.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    private void SetDeadLayerClientRpc()
    {
        int deadLayer = LayerMask.NameToLayer("DeadPlayer");
        SetLayerRecursively(gameObject, deadLayer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    [Rpc(SendTo.Everyone)]
    private void DisableLocalScriptsClientRpc()
    {
        // Only disable scripts on the player who died
        if (!IsOwner)
            return;

        // Disable your gameplay scripts
        GetComponent<MouseAimRay>().enabled = false;
        GetComponent<ChargeAndLaunch>().enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    private void HideForOthersClientRpc()
    {
        // If this is *my* player, do NOT hide it
        if (IsOwner)
            return;

        foreach (var r in renderers)
            r.enabled = false;
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
