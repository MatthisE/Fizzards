using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerPrefab;

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
        int index = (int)(clientId % (ulong)spawnPoints.Length);

        Transform spawn = spawnPoints[index];

        var player = Instantiate(
            playerPrefab,
            spawn.position,
            Quaternion.LookRotation(spawn.forward, Vector3.up)
        );

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

}
