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
        if (gm != null)
        {
            var winnerId = gm.WinnerId.Value;
            winnerText.text = $"Player {winnerId} won!!!";
        }
        else
        {
            winnerText.text = "No winner found.";
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
}
