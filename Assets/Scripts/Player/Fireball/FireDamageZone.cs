using Unity.Netcode;
using UnityEngine;

public class FireDamageZone : NetworkBehaviour
{
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 0.01f;

    private float nextTickTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        if (Time.time < nextTickTime)
            return;

        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            if (!health.IsDead.Value)
            {
                health.TakeDamageRpc(damagePerTick);
                nextTickTime = Time.time + tickInterval;
            }
        }
    }
}
