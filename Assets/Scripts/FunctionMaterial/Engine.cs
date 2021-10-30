using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Engine : PhysXInterface
{
    public float rotationSpeed = 10f;
   
    public void Rotate() {
        if (!Dispatched)
        {
            Dispatch();
        }
        joint.GetComponent<Rigidbody>().AddRelativeTorque(joint.transform.InverseTransformPoint(transform.position) * rotationSpeed, ForceMode.Force);
    }
}
