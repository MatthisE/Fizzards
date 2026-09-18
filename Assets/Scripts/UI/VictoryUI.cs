using UnityEngine;
using TMPro;

public class VictoryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text winnerText;

    private void Start()
    {
        winnerText.text = $"Player {GlobalGameState.WinnerId} won!!!";
    }
}
