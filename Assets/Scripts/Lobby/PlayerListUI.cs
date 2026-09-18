using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerListText;

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // Wait until PlayerListManager exists
        while (PlayerListManager.Instance == null)
            yield return null;

        // Wait until the NetworkList is populated
        while (PlayerListManager.Instance.Players.Count == 0)
            yield return null;

        // Subscribe to list changes
        PlayerListManager.Instance.Players.OnListChanged += OnPlayerListChanged;

        // Initial UI build
        UpdateUI();
    }

    private void OnPlayerListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        playerListText.text = "";

        foreach (var player in PlayerListManager.Instance.Players)
        {
            playerListText.text += $"Player {player.ClientId}\n";
        }
    }
}
