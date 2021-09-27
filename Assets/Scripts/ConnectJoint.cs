using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectJoint : MonoBehaviour
{
    public GameObject connectedBody;
    public float breakForce;
    public float breakTorque;
    public bool enableCollision;
    public bool preProcessing = true;
    public float massScale = 1;
    public float connectedMassScale = 1;
    [Tooltip("The minimum velocity to call the calculate force function.")]
    public float sensitive = 0.5f;
    FixedJoint fixedJoint;
    PhysicCore core;
    Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = true;
        fixedJoint.connectedBody = connectedBody.transform.parent.GetComponent<Rigidbody>();
        //fixedJoint.connectedAnchor = connectedBody.transform.localPosition;
        fixedJoint.breakForce = breakForce;
        fixedJoint.breakTorque = breakTorque;
        fixedJoint.enableCollision = enableCollision;
        fixedJoint.enablePreprocessing = preProcessing;
        fixedJoint.massScale = massScale;
        fixedJoint.connectedMassScale = connectedMassScale;
        core = connectedBody.transform.parent.GetComponent<PhysicCore>();
    }

    private void FixedUpdate()
    {
        if(core == null)
        {
            Reload();
        }
        if (rigid.velocity.magnitude > sensitive)
        {
            try
            {
                core.collideEvent.Add(connectedBody.GetComponent<IBlock>(), core.transform.InverseTransformVector(fixedJoint.currentForce));
            }
            catch { }
        }
    }

    public void Reload()
    { 
        core = connectedBody.transform.parent.GetComponent<PhysicCore>();
        fixedJoint.connectedBody = connectedBody.transform.parent.GetComponent<Rigidbody>();
    }

}
