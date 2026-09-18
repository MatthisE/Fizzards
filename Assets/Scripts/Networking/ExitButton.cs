using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void OnExitClicked()
    {
        var nm = NetworkManager.Singleton;

        // Case 1: You are a CLIENT connected to a host
        if (nm.IsClient && !nm.IsHost)
        {
            nm.Shutdown(); // disconnect from host
            SceneManager.LoadScene("Title");
            return;
        }

        // Case 2: You are the HOST
        if (nm.IsHost)
        {
            // Unregister callbacks BEFORE shutdown
            if (PlayerListManager.Instance != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= PlayerListManager.Instance.OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerListManager.Instance.OnClientDisconnected;
            }

            nm.Shutdown();
            SceneManager.LoadScene("Title");
            return;
        }

        // Case 3: You are not connected at all (just in lobby without hosting)
        SceneManager.LoadScene("Title");
    }
}
