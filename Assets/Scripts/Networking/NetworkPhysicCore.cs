using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PhysicCore))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkPhysicCore : NetworkBehaviour
{
    private void Start()
    {
        GetComponent<PhysicCore>().mring.data_m = new Action(() =>
        {
            // Case sync
        });
    }
}
