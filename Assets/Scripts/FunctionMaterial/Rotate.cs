using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float acceleration;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ConfigurableJoint joint = GetComponent<ConfigurableJoint>();
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.connectedBody.AddRelativeTorque(new Vector3(0, acceleration, 0), ForceMode.VelocityChange);
    }
}
