using Unity.Netcode;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance;

    public NetworkList<PlayerData> Players;

    private void Awake()
    {
        Instance = this;
        Players = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Host + Clients, die bereits verbunden sind, hinzufügen
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            AddPlayerIfMissing(kvp.Key);
        }

        // Neue Verbindungen tracken
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        AddPlayerIfMissing(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
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
}
