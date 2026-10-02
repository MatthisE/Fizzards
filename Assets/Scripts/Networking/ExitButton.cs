using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void OnExitClicked()
    {
        var nm = NetworkManager.Singleton;

        if (nm.IsHost)
        {
            GameManager.Instance.ReturnToLobbyRpc();

            // Host: cleanup
            if (PlayerListManager.Instance != null)
            {
                nm.OnClientConnectedCallback -= PlayerListManager.Instance.OnClientConnected;
                nm.OnClientDisconnectCallback -= PlayerListManager.Instance.OnClientDisconnected;

                PlayerListManager.Instance.ClearPlayersRpc();
                Object.Destroy(PlayerListManager.Instance.gameObject);
            }

            nm.Shutdown();
            SceneManager.LoadScene("Title");
            return;
        }

        // Client: user manually exits
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
