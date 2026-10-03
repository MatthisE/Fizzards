using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUIEntry : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text healthText;

    [SerializeField] private Image playerIcon;

    private PlayerHealth target;
    private PlayerColor playerColor;

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

        playerColor = player.GetComponent<PlayerColor>();

        if (playerColor != null)
        {
            ApplyColor(playerColor.PlayerColorValue.Value);

            playerColor.PlayerColorValue.OnValueChanged += (oldColor, newColor) =>
            {
                ApplyColor(newColor);
            };
        }
    }

    private void UpdateHealth(int value)
    {
        healthText.text = $"HP: {value}";

        if (playerIcon != null)
        {
            if (value <= 0)
            {
                playerIcon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
    }

    private void ApplyColor(Color c)
    {
        if (playerIcon != null)
        {
            c.a = 0.5f;
            playerIcon.color = c;
        }
    }
}
