using UnityEngine;
using TMPro;

public class PlayerUIEntry : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text healthText;

    private PlayerHealth target;

    public void Initialize(PlayerHealth player)
    {
        target = player;
        
        var players = PlayerListManager.Instance.Players;

        string playerName = $"Player {player.OwnerClientId}";

        foreach (var p in players)
        {
            if (p.ClientId == player.OwnerClientId)
            {
                playerName = p.PlayerName.ToString();
                break;
            }
        }

        nameText.text = playerName;


        UpdateHealth(player.CurrentHealth.Value);

        player.CurrentHealth.OnValueChanged += (oldVal, newVal) =>
        {
            UpdateHealth(newVal);
        };
    }

    private void UpdateHealth(int value)
    {
        healthText.text = $"HP: {value}";
    }
}
