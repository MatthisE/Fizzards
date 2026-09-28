using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void OnExitClicked()
    {
        var nm = NetworkManager.Singleton;

        // HOST: end game for everyone
        if (nm.IsHost)
        {
            // Tell all clients to return to lobby (and destroy their PlayerListManager)
            GameManager.Instance.ReturnToLobbyRpc();

            // Host cleanup
            if (PlayerListManager.Instance != null)
            {
                nm.OnClientConnectedCallback -= PlayerListManager.Instance.OnClientConnected;
                nm.OnClientDisconnectCallback -= PlayerListManager.Instance.OnClientDisconnected;

                PlayerListManager.Instance.ClearPlayersRpc(); // server clears list
                Object.Destroy(PlayerListManager.Instance.gameObject);
            }

            nm.Shutdown();
            SceneManager.LoadScene("Title");
            return;
        }

        // CLIENT: user manually exits
        if (nm.IsClient && !nm.IsHost)
        {
            if (PlayerListManager.Instance != null)
            {
                Object.Destroy(PlayerListManager.Instance.gameObject);
            }

            nm.Shutdown();
            SceneManager.LoadScene("Title");
            return;
        }

        // Not connected at all
        SceneManager.LoadScene("Title");
    }
}
