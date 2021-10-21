using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombindObject : MonoBehaviour
{
    public List<GameObject> objects = new List<GameObject>();
    public bool generateIBlock = true;

    private void Start()
    {
        Combind();
    }

    public void Combind()
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if(rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        List<IBlock> list = new List<IBlock>();
        if (generateIBlock)
        {
            foreach(Transform child in transform)
            {
                child.gameObject.AddComponent<IBlock>();
            }
        }
        foreach (Transform child in transform)
        {
            Rigidbody m_rigid = child.GetComponent<Rigidbody>();
            rigidbody.mass += m_rigid.mass;
            rigidbody.centerOfMass += (child.localPosition + m_rigid.centerOfMass) * m_rigid.mass;
            if (generateIBlock)
            {
                IBlock component = child.gameObject.GetComponent<IBlock>();
                list.Add(component);
                component.mass = m_rigid.mass;
                component.centerOfmass = m_rigid.centerOfMass;
                foreach(FixedJoint fixedJoint in child.GetComponents<FixedJoint>())
                {
                    IBlock connected = fixedJoint.connectedBody.GetComponent<IBlock>();
                    if (!component.connector.Contains(connected))
                    {
                        component.connector.Add(connected);
                        connected.connector.Add(component);
                    }
                    Destroy(fixedJoint);
                }
            }
            Destroy(m_rigid);
        }
        rigidbody.centerOfMass /= rigidbody.mass;
        //Debug.Log(rigidbody.centerOfMass);
        if (generateIBlock)
        {
            gameObject.AddComponent<PhysicCore>().Load(list);
        }
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<IBlock>().Load();
        }
    }
}
