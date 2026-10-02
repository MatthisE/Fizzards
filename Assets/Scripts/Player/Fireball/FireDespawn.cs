using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class FireDespawn : NetworkBehaviour
{
    [SerializeField] private float despawnDelay = 5f;
    [SerializeField] private ParticleSystem[] particleSystems;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            StartCoroutine(StopAndDespawn());
    }

    private IEnumerator StopAndDespawn()
    {
        yield return new WaitForSeconds(despawnDelay);
        StopEmissionClientRpc();
        yield return new WaitForSeconds(2f);
        NetworkObject.Despawn(true);
    }

    [Rpc(SendTo.Everyone)]
    private void StopEmissionClientRpc()
    {
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}
