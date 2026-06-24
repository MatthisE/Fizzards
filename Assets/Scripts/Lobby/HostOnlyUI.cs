using Unity.Netcode;
using UnityEngine;

public class HostOnlyUI : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(NetworkManager.Singleton.IsHost);
    }
}
