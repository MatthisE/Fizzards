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
        Debug.Log("[PlayerUIManager] Awake()");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("[PlayerUIManager] OnNetworkSpawn()");

        if (!IsClient)
        {
            Debug.Log("[PlayerUIManager] Not a client → abort");
            return;
        }

        Debug.Log("[PlayerUIManager] Subscribing to OnLoadEventCompleted");
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnSceneLoaded(
        string sceneName,
        LoadSceneMode mode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        Debug.Log($"[PlayerUIManager] OnSceneLoaded() → Scene: {sceneName}");

        // Only run in your game scene
        if (sceneName != "Game")
        {
            Debug.Log("[PlayerUIManager] Scene is not 'Game' → ignoring");
            return;
        }

        Debug.Log("[PlayerUIManager] Scene is Game → adding existing players");

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject;

            Debug.Log($"[PlayerUIManager] Checking player {kvp.Key} → PlayerObject: {playerObj}");

            TryAddPlayer(playerObj);
        }

        Debug.Log("[PlayerUIManager] Registering connect/disconnect callbacks");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        Debug.Log("[PlayerUIManager] Unsubscribing from OnLoadEventCompleted");
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[PlayerUIManager] OnClientConnected({clientId})");

        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        Debug.Log($"[PlayerUIManager] PlayerObject for {clientId}: {playerObj}");

        TryAddPlayer(playerObj);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[PlayerUIManager] OnClientDisconnected({clientId})");

        if (entries.TryGetValue(clientId, out var entry))
        {
            Debug.Log($"[PlayerUIManager] Removing UI entry for {clientId}");
            Destroy(entry.gameObject);
            entries.Remove(clientId);
        }
        else
        {
            Debug.Log($"[PlayerUIManager] No UI entry found for {clientId}");
        }
    }

    public void TryAddPlayer(NetworkObject playerObj)
    {
        Debug.Log($"[PlayerUIManager] TryAddPlayer() → PlayerObject: {playerObj}");

        if (playerObj == null)
        {
            Debug.Log("[PlayerUIManager] PlayerObject is NULL → abort");
            return;
        }

        var health = playerObj.GetComponent<PlayerHealth>();
        Debug.Log($"[PlayerUIManager] PlayerHealth component: {health}");

        if (health == null)
        {
            Debug.Log("[PlayerUIManager] No PlayerHealth found → abort");
            return;
        }

        ulong clientId = playerObj.OwnerClientId;

        if (entries.ContainsKey(clientId))
        {
            Debug.Log($"[PlayerUIManager] UI entry already exists for {clientId} → abort");
            return;
        }

        Debug.Log($"[PlayerUIManager] Instantiating UI entry for {clientId}");
        var entryGO = Instantiate(uiEntryPrefab, uiContainer);

        Debug.Log("[PlayerUIManager] Getting PlayerUIEntry component");
        var entry = entryGO.GetComponent<PlayerUIEntry>();

        Debug.Log("[PlayerUIManager] Initializing UI entry");
        entry.Initialize(health);

        Debug.Log($"[PlayerUIManager] Storing UI entry for {clientId}");
        entries[clientId] = entry;
    }
}
