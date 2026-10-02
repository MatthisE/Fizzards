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
    [SerializeField] private TMP_InputField playerNameInput;

    public static string LocalPlayerName = "Unnamed";


    private void Start()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
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

        PlayerListManager.Instance.SubmitNameRpc(LocalPlayerName, NetworkManager.Singleton.LocalClientId);
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

    public void OnNameChanged()
    {
        LocalPlayerName = playerNameInput.text;
    }

    public void SetPlayerName()
    {
        LocalPlayerName = playerNameInput.text;

        PlayerListManager.Instance.SubmitNameRpc(
            LocalPlayerName,
            NetworkManager.Singleton.LocalClientId
        );
    }



    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"Client connected: {clientId}");
        }

        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            PlayerListManager.Instance.SubmitNameRpc(LocalPlayerName, clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //errorText.text = "Could not connect to a host";
            //errorText.gameObject.SetActive(true);

            Debug.Log($"Could not connect to a host");
        }
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            LoadSceneMode.Single
        );
    }
}
