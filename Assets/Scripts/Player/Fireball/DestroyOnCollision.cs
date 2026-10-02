using Unity.Netcode;
using UnityEngine;

public class DestroyOnCollision : NetworkBehaviour
{
    [SerializeField] private GameObject firePrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamageRpc(10);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("World"))
        {
            Vector3 hitPoint = transform.position;
            SpawnFireAtImpact(hitPoint);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Fire"))
        {
            return;
        }

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
        netObj.Spawn();
    }
}
