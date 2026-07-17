using Unity.Netcode;
using UnityEngine;

public class DestroyOnCollision : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // only server can damage/despawn

        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamageRpc(25);
        }

        var netObj = GetComponent<NetworkObject>();

        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
        else
        {
            Destroy(gameObject); // fallback for non-networked or not-yet-spawned objects
        }
    }
}