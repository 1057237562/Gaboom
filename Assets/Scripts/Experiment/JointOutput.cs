using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointOutput : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        float force = GetComponent<Joint>().currentForce.magnitude;
        float torque = GetComponent<Joint>().currentTorque.magnitude;
        Debug.Log(gameObject.name + ":"+force+":"+torque+":"+collision.impulse/Time.fixedDeltaTime);
    }
}
