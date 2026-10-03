using Unity.Netcode;
using UnityEngine;
using System.Collections;

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

    public NetworkVariable<bool> IsDead = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Hit Flash (multiple meshes)")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    private float flashEndTime = 0f;
    private Color[][] originalColors;
    private bool isFlashing = false;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;


    public override void OnNetworkSpawn()
    {
        if (IsServer)
            CurrentHealth.Value = maxHealth;

        StartCoroutine(WaitForPlayerColor());
    }

    private IEnumerator WaitForPlayerColor()
    {
        var pc = GetComponent<PlayerColor>();

        while (pc != null && pc.PlayerColorValue.Value == default)
            yield return null;

        CacheOriginalColors();
    }


    private void Update()
    {
        if (IsDead.Value)
            return;

        if (Time.time >= flashEndTime && isFlashing)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                var mats = renderers[i].materials;
                for (int m = 0; m < mats.Length; m++)
                    mats[m].color = originalColors[i][m];
            }

            isFlashing = false;
        }
        else if (Time.time < flashEndTime)
        {
            isFlashing = true;
        }
    }

    public void CacheOriginalColors()
    {
        originalColors = new Color[renderers.Length][];

        Color playerColor = Color.white;
        var pc = GetComponent<PlayerColor>();
        if (pc != null){
            playerColor = pc.PlayerColorValue.Value;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];

            for (int m = 0; m < mats.Length; m++)
            {
                originalColors[i][m] = mats[m].color;

                if (m == 2)
                {
                    originalColors[i][m] = playerColor;
                    mats[m].color = playerColor;
                }
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageRpc(int amount)
    {
        if (IsDead.Value)
            return;

        CurrentHealth.Value = Mathf.Max(CurrentHealth.Value - amount, 0);

        if (CurrentHealth.Value <= 0)
        {
            IsDead.Value = true;

            if (IsServer)
                GameManager.Instance.NotifyPlayerDied();

            ApplyTransparentMaterialClientRpc();
            DisableAimIndicatorClientRpc();
            SetDeadLayerClientRpc();
            DisableLocalScriptsClientRpc();
            HideForOthersClientRpc();
            ForceGhostMaterialClientRpc();
            StopFlashClientRpc();
            PlayDeathSoundClientRpc();
        }
        else
        {
            HitFlashClientRpc();
            PlayDamageSoundClientRpc();
        }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDamageSoundClientRpc()
    {
        if (audioSource && damageSound)
            audioSource.PlayOneShot(damageSound);
    }

    [Rpc(SendTo.Everyone)]
    private void PlayDeathSoundClientRpc()
    {
        if (audioSource && deathSound)
            audioSource.PlayOneShot(deathSound);
    }

    [Rpc(SendTo.Everyone)]
    private void ApplyTransparentMaterialClientRpc()
    {
        if (!IsOwner)
            return;

        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int m = 0; m < mats.Length; m++)
                mats[m] = new Material(transparentMaterial);

            r.materials = mats;
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
        if (!IsOwner)
            return;

        GetComponent<MouseAimRay>().enabled = false;
        GetComponent<ChargeAndLaunch>().enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    private void HideForOthersClientRpc()
    {
        if (IsOwner)
            return;

        foreach (var r in renderers)
            r.enabled = false;
    }

    [Rpc(SendTo.Everyone)]
    private void HitFlashClientRpc()
    {
        if (IsDead.Value)
            return;

        flashEndTime = Time.time + flashDuration;

        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int m = 0; m < mats.Length; m++)
                mats[m].color = hitColor;
        }
    }

    [Rpc(SendTo.Everyone)]
    private void StopFlashClientRpc()
    {
        isFlashing = false;
        StopAllCoroutines();
    }

    [Rpc(SendTo.Everyone)]
    private void ForceGhostMaterialClientRpc()
    {
        if (!IsOwner)
            return;

        foreach (var r in renderers)
        {
            var mats = r.materials;
            for (int m = 0; m < mats.Length; m++)
                mats[m] = new Material(transparentMaterial);

            r.materials = mats;
        }
    }
}
