using Unity.Netcode;
using UnityEngine;

public class DestroyOnCollision : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // only server can destroy network objects
        NetworkObject.Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // only server can destroy network objects
        NetworkObject.Despawn();
    }
}
