using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class PlayerCleanup : MonoBehaviour
{
    [SerializeField] private GameObject newRoundButton;
    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        newRoundButton.SetActive(false);
        StartCoroutine(DespawnRoutine());
    }

    private IEnumerator DespawnRoutine()
    {
        DespawnAllPlayers();

        bool cleared = false;

        while (!cleared)
        {
            cleared = true;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                {
                    cleared = false;
                    break;
                }
            }

            yield return null;
        }

        var fireballs = GameObject.FindGameObjectsWithTag("Fireball");

        foreach (var fb in fireballs)
        {
            var netObj = fb.GetComponent<NetworkObject>();
            netObj.Despawn(true); 
        }

        newRoundButton.SetActive(true);
    }

    public void DespawnAllPlayers()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj != null)
            {
                Debug.Log($"Despawning player {client.ClientId}");
                playerObj.Despawn();
            }
        }
    }
}
