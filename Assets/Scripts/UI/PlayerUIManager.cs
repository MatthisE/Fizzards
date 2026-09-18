using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        if (!IsClient)
        {
            return;
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(
        string sceneName,
        LoadSceneMode mode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        // Only run in your game scene
        if (sceneName != "Game")
        {
            return;
        }

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject;

            TryAddPlayer(playerObj);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
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
            if (entry != null && entry.gameObject != null)
            {
                Destroy(entry.gameObject);
            }

            entries.Remove(clientId);
        }
    }


    public void TryAddPlayer(NetworkObject playerObj)
    {
        if (playerObj == null)
        {
            return;
        }

        var health = playerObj.GetComponent<PlayerHealth>();

        if (health == null)
        {
            return;
        }

        ulong clientId = playerObj.OwnerClientId;

        if (entries.ContainsKey(clientId))
        {
            return;
        }

        var entryGO = Instantiate(uiEntryPrefab, uiContainer);
        var entry = entryGO.GetComponent<PlayerUIEntry>();

        entry.Initialize(health);
        entries[clientId] = entry;
    }

    private new void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }

}
