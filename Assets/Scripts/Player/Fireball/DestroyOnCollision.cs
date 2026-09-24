using Unity.Netcode;
using UnityEngine;

public class DestroyOnCollision : NetworkBehaviour
{
    [SerializeField] private GameObject firePrefab; // MUST have NetworkObject

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // only server handles collision + spawning

        // Damage player
        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamageRpc(10);
        }

        // Spawn fire if world layer hit
        if (other.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            Vector3 hitPoint = transform.position; // fireball center
            SpawnFireAtImpact(hitPoint);
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Fire"))
        {
            return;
        }

        // Despawn fireball
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
            netObj.Despawn();
        else
            Destroy(gameObject);
    }

    private void SpawnFireAtImpact(Vector3 position)
    {
        var fire = Instantiate(firePrefab, position, Quaternion.identity);
        var netObj = fire.GetComponent<NetworkObject>();
        netObj.Spawn(); // sync to all clients
    }
}
