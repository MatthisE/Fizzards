using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance;

    public NetworkList<PlayerData> Players;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        // NetworkList MUSS existieren, egal ob Server oder Client
        if (Players == null)
            Players = new NetworkList<PlayerData>();

        if (IsServer)
        {
            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                AddPlayerIfMissing(kvp.Key);
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void OnClientConnected(ulong clientId)
    {
        AddPlayerIfMissing(clientId);
    }

    public void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                Players.RemoveAt(i);
                break;
            }
        }
    }

    private void AddPlayerIfMissing(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
                return;
        }

        Players.Add(new PlayerData
        {
            ClientId = clientId,
            PlayerName = $"Player {clientId}"
        });
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SubmitNameRpc(string name, ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                var data = Players[i];
                data.PlayerName = name;
                Players[i] = data;
                break;
            }
        }
    }

    private new void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    [Rpc(SendTo.NotServer)]
    public void ClearPlayersRpc()
    {
        Players.Clear();
    }


}
