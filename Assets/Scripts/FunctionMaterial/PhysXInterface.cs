using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PhysXInterface : IBlock
{
    [HideInInspector]
    public Vector3 connectDirection;
    [HideInInspector]
    public Quaternion connectRotation;
    [HideInInspector]
    public ConfigurableJoint joint;
    [HideInInspector]
    public HashSet<IBlock> conblocks = new HashSet<IBlock>();
    [HideInInspector]
    public GameObject connectors;
    [HideInInspector]
    public bool Dispatched = false;

    public bool ResetRotation = false;

    public void Dispatch()
    {
        Dispatched = true;
        foreach (IBlock b in connector)
        {

            if ((b.transform.position - transform.position - transform.forward).magnitude < 1)
            {
                conblocks.Add(b);
                foreach (IBlock n in b.connector)
                {
                    if (n != this)
                    {
                        conblocks.Add(n);
                        if (!dfs(n))
                        {
                            conblocks.Clear();
                            return;
                        }
                    }
                }
                break;
            }
        }
        if (conblocks.Count == 0)
        {
            return;
        }
        connectors = CreateNewCore(conblocks.ToList());
        Rigidbody nrigid = connectors.GetComponent<Rigidbody>();
        connectDirection = transform.InverseTransformPoint(nrigid.position);
        connectRotation = core.transform.rotation;
        //rigid = gameObject.AddComponent<Rigidbody>();
        Rigidbody origid = core.GetComponent<Rigidbody>();

        origid.centerOfMass = origid.centerOfMass * origid.mass - (nrigid.worldCenterOfMass - origid.position) * nrigid.mass;
        origid.mass -= nrigid.mass;
        origid.centerOfMass /= origid.mass;

        joint = connectors.AddComponent<ConfigurableJoint>();
        joint.connectedBody = origid;
        joint.connectedAnchor = transform.localPosition;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
    }

    bool dfs(IBlock block)
    {
        foreach (IBlock con in block.connector)
        {
            if (con == this)
            {
                return false;
            }
            if (con == block)
            {
                continue;
            }
            if (!conblocks.Contains(con))
            {
                conblocks.Add(con);
                if (!dfs(con))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /*private void Update()
    {
        Debug.DrawLine(transform.position,transform.position + transform.forward);
    }*/

    public virtual void Reattached()
    {
        if(ResetRotation) connectors.transform.rotation = core.transform.rotation * Quaternion.Inverse(connectRotation);
        connectors.transform.position = transform.TransformPoint(connectDirection);
        Attach();
    }

    public void Attach()
    {
        Dispatched = false;
        foreach (IBlock block in conblocks)
        {
            block.transform.parent = core.transform;
        }
        Rigidbody origid = core.GetComponent<Rigidbody>();
        Rigidbody nrigid = connectors.GetComponent<Rigidbody>();
        origid.centerOfMass = origid.centerOfMass * origid.mass + (nrigid.worldCenterOfMass - origid.position) * nrigid.mass;
        origid.mass += nrigid.mass;
        origid.centerOfMass /= origid.mass;
        Destroy(joint);
        Destroy(connectors);
        conblocks.Clear();
    }

    GameObject CreateNewCore(List<IBlock> list)
    {
        GameObject newObj = Instantiate(PhysicCore.emptyGameObject, transform.position + transform.forward, transform.rotation);
        Rigidbody rigidbody = newObj.GetComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        foreach (IBlock block in list)
        {
            block.transform.parent = newObj.transform;
            rigidbody.mass += block.mass;
            rigidbody.centerOfMass += (block.transform.localPosition + block.centerOfmass) * block.mass;
        }
        rigidbody.centerOfMass /= rigidbody.mass;
        PhysicCore physicCore = newObj.GetComponent<PhysicCore>();
        physicCore.Load(list);
        physicCore.deriveFrom = this;
        return newObj;
    }
}
