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

        NetworkObject.Despawn();
    }
}