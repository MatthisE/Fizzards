using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public NetworkVariable<ulong> WinnerId = new NetworkVariable<ulong>();

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
        Debug.Log($"Game Over! Winner is {winnerId}");

        // networked state
        WinnerId.Value = winnerId;

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
