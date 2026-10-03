using Unity.Netcode;
using UnityEngine;

public class ClientOnlyUI : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost);
    }
}
