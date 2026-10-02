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
        while (PlayerListManager.Instance == null)
            yield return null;

        while (PlayerListManager.Instance.Players.Count == 0)
            yield return null;

        PlayerListManager.Instance.Players.OnListChanged += OnPlayerListChanged;
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
            playerListText.text += $"{player.PlayerName}\n";
        }
    }
}
