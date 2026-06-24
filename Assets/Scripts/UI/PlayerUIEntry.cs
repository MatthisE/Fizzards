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
        nameText.text = $"Player {player.OwnerClientId}";
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
