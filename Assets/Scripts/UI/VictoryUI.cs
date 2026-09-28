using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;

    private void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            winnerText.text = "No winner found.";
            return;
        }

        ulong winnerId = gm.WinnerId.Value;

        // Default fallback
        string winnerName = $"Player {winnerId}";

        // PlayerListManager muss existieren
        if (PlayerListManager.Instance != null)
        {
            foreach (var p in PlayerListManager.Instance.Players)
            {
                if (p.ClientId == winnerId)
                {
                    winnerName = p.PlayerName.ToString();
                    break;
                }
            }
        }

        winnerText.text = $"{winnerName} won!!!";
    }


    public void LoadSceneByName(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
}
