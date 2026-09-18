using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Called by PlayerHealth when a player dies.
    public void NotifyPlayerDied()
    {
        if (!IsServer)
            return;

        int aliveCount = 0;
        ulong lastAliveClient = 0;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null)
                continue;

            var health = playerObj.GetComponent<PlayerHealth>();
            if (health == null)
                continue;

            if (health.CurrentHealth.Value > 0)
            {
                aliveCount++;
                lastAliveClient = client.ClientId;
            }
        }

        if (aliveCount == 1)
        {
            Debug.Log($"Winner: Client {lastAliveClient}");
            OnWinnerFound(lastAliveClient);
        }
    }

    private void OnWinnerFound(ulong winnerId)
    {
        // TODO: show winner UI, restart game, etc.
        Debug.Log($"Game Over! Winner is {winnerId}");

        // Save globally
        GlobalGameState.WinnerId = winnerId;

        LoadSceneByName("Victory");
    }

    public void LoadSceneByName(string sceneName)
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }

}
