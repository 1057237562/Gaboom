using Gaboom.Scene;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Communicator : NetworkBehaviour
{
    public GameObject emptyGameObject;

    private void Start()
    {
        PhysicCore.emptyGameObject = emptyGameObject;
    }

    [ServerRpc]
    public void AttemptGeneratePhysicCoreServerRpc(Vector3 point, Quaternion rotation, int selectedPrefab)
    {
        if (!IsServer && !IsHost) return;

        GameObject parent = Instantiate(emptyGameObject, point, Quaternion.identity);

        PhysicCore core = parent.GetComponent<PhysicCore>();
        parent.GetComponent<NetworkObject>().Spawn();
        List<IBlock> blocks = new List<IBlock>();

        GameObject n_obj = Instantiate(SceneMaterial.Instance.BuildingPrefabs[selectedPrefab], Vector3.zero, rotation);

        IBlock block = n_obj.GetComponent<IBlock>();
        //block.mass = generated.GetComponent<Rigidbody>().mass;
        //block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
        n_obj.transform.parent = n_obj.transform;
        foreach (Collider child in n_obj.GetComponentsInChildren<Collider>())
        {
            child.isTrigger = false;
        }
        block.Load();
        blocks.Add(block);

        core.RecalculateRigidbody(blocks);
        core.enabled = true;
    }

}
