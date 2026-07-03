using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net;
using System.Net.Sockets;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text ipText;

    private void Start()
    {
        // Hide error message at start
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        // Listen for failed connections
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private string GetLocalIPAddress()
    {
        foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ni.AddressFamily == AddressFamily.InterNetwork)
                return ni.ToString();
        }

        return "No IPv4 address found";
    }

    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();

        string ip = GetLocalIPAddress();
        Debug.Log($"Host started on IP: {ip}");
        ipText.text = $"{ip}";
    }

    public void JoinGame()
    {
        string ip = ipInputField.text;

        if (string.IsNullOrWhiteSpace(ip))
            ip = "127.0.0.1";

        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.ConnectionData.Address = ip;

        NetworkManager.Singleton.StartClient();
        Debug.Log($"Client connecting to {ip}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Only show error for the local client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            errorText.text = "Could not connect to a host";
            errorText.gameObject.SetActive(true);
        }
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Main",
            LoadSceneMode.Single
        );
    }
}
