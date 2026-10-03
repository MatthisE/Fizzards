using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_InputField ipText;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private GameObject startButton;

    public static string LocalPlayerName = "Unnamed";
    private const int MaxPlayers = 4;


    private void Start()
    {
        if (NetworkManager.Singleton.ConnectionApprovalCallback == null)
        {
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayers = NetworkManager.Singleton.ConnectedClients.Count;

        if (currentPlayers >= MaxPlayers)
        {
            response.Approved = false;
            response.Reason = "Lobby is full";
            Debug.Log("Lobby full");
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
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

        startButton.SetActive(true);

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

        StartCoroutine(ConnectionTimeout(ip));
    }

    private IEnumerator ConnectionTimeout(string ip)
    {
        float timeout = 5f;
        float timer = 0f;

        while (timer < timeout)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(ShowError($"Could not connect to IP: {ip}"));
        NetworkManager.Singleton.Shutdown();
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
            StartCoroutine(
                ShowError("Could not connect to the IP address.")
            );

            Debug.Log($"Could not connect to a host");
        }
    }

    private IEnumerator ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        errorText.gameObject.SetActive(false);
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

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.ConnectionApprovalCallback = null;
        }
    }

}
