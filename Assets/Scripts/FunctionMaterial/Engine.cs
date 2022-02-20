using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Engine : PhysXInterface
{
    [AttributeField(type = typeof(float))]
    public float rotationSpeed = 10f;
    [AttributeField(type = typeof(bool))]
    public bool reverse = false;
    public override void Reattached()
    {
        float angle;
        (connectors.transform.rotation * Quaternion.Inverse(transform.rotation)).ToAngleAxis(out angle, out _);
        connectors.transform.rotation = Quaternion.AngleAxis(angle, transform.forward) * transform.rotation;
        Attach();
    }

    public void Rotate()
    {
        if (!Dispatched)
        {
            Dispatch();
        }
        if(joint != null)
        joint.GetComponent<Rigidbody>().AddRelativeTorque((reverse ? 1:-1)*joint.transform.InverseTransformPoint(transform.position) * rotationSpeed, ForceMode.Force);

        core.mring.Invoke();
    }
}
