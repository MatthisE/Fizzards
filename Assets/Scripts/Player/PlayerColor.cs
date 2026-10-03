using Unity.Netcode;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColorValue =
        new NetworkVariable<Color>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    [SerializeField] private Renderer rend;

    public override void OnNetworkSpawn()
    {
        ApplyColor(PlayerColorValue.Value);

        PlayerColorValue.OnValueChanged += (oldColor, newColor) =>
        {
            ApplyColor(newColor);
        };
    }

    private void ApplyColor(Color c)
    {
        if (rend == null)
        return;

        var mats = rend.materials;

        if (mats.Length > 2)
        {
            mats[2].color = c;   //Color of outfit
            rend.materials = mats;
        }
    }
}
