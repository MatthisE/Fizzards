using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;
    private static readonly List<ulong> joinedPlayers = new List<ulong>();

    private static readonly Color[] playerColors = new Color[]
    {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow
    };

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerFor(client.ClientId);
        }
    }

    private void SpawnPlayerFor(ulong clientId)
    {
        if (!joinedPlayers.Contains(clientId))
            joinedPlayers.Add(clientId);

        int index = joinedPlayers.IndexOf(clientId);

        Color assignedColor = playerColors[Mathf.Clamp(index, 0, playerColors.Length - 1)];

        Transform spawn = spawnPoints[index];

        var player = Instantiate(
            playerPrefab,
            spawn.position,
            Quaternion.LookRotation(spawn.forward, Vector3.up)
        );

        var netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);

        var colorComponent = player.GetComponent<PlayerColor>();
        colorComponent.PlayerColorValue.Value = assignedColor;
    }
}
