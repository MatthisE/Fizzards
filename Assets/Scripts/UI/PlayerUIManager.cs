using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerUIManager : NetworkBehaviour
{
    public static PlayerUIManager Instance;

    [SerializeField] private Transform uiContainer;
    [SerializeField] private GameObject uiEntryPrefab;

    private Dictionary<ulong, PlayerUIEntry> entries = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;

        // ⭐ Add UI for players that already exist (important for lobby → game)
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            TryAddPlayer(kvp.Value.PlayerObject);
        }

        // Listen for new players joining (rare in game scene, but correct)
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        TryAddPlayer(playerObj);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (entries.TryGetValue(clientId, out var entry))
        {
            Destroy(entry.gameObject);
            entries.Remove(clientId);
        }
    }

    public void TryAddPlayer(NetworkObject playerObj)
    {
        if (playerObj == null) return;

        var health = playerObj.GetComponent<PlayerHealth>();
        if (health == null) return;

        if (entries.ContainsKey(playerObj.OwnerClientId))
            return; // prevent duplicates

        var entryGO = Instantiate(uiEntryPrefab, uiContainer);
        var entry = entryGO.GetComponent<PlayerUIEntry>();
        entry.Initialize(health);

        entries[playerObj.OwnerClientId] = entry;
    }
}
