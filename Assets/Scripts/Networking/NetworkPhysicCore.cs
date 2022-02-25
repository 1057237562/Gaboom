using Gaboom.IO;
using Gaboom.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(PhysicCore))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkPhysicCore : NetworkBehaviour
{

    private Action DataListener;
    private PhysicCore physicCore;

    private void Start()
    {
        physicCore = GetComponent<PhysicCore>();

        DataListener = () =>
        {
            // Case sync
            if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly || NetworkManager.singleton.mode == NetworkManagerMode.Host)
            {
                SyncDataClientRpc(SLMechanic.SerializeToXml(physicCore));
            }
            else if (hasAuthority)
            {
                CmdSyncData(SLMechanic.SerializeToXml(physicCore));
            }
        };
        GetComponent<PhysicCore>().mring.data_m = DataListener;
        if(isServer) 
            DataListener.Invoke();
        if (hasAuthority)
        {
            LifeCycle.gameObjects.Add(gameObject);
            GetComponent<PhysicCore>().acceleration = Vector3.zero;
            physicCore.Recombine = (list) =>
            {
                Camera.main.GetComponent<Communicator>().CmdAttemptGeneratePhysicCoreServerRpc(SLMechanic.SerializeToXml(list, transform.position, transform.rotation, Vector3.zero));
            };
        }
    }

    [Command]
    public void CmdAppliedForce(ContactPoint cp,Vector3 force)
    {
        AppliedForceClientRpc(cp,force);
    }

    [ClientRpc]
    public void AppliedForceClientRpc(ContactPoint cp, Vector3 force)
    {
        if (hasAuthority)
        {
            Collider[] collider = Physics.OverlapSphere(cp.point, 1);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {//collision.GetContact(0).thisCollider.transform.parent.GetComponent<IBlock>(),
        CmdAppliedForce(collision.GetContact(0),transform.InverseTransformVector(collision.impulse / Time.fixedDeltaTime));
    }

    [ClientRpc]
    public void SyncDataClientRpc(string xmlstr)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlstr);
        XmlElement parent = (XmlElement)xml.GetElementsByTagName("PhysicCore")[0];
        //GameObject core = Instantiate(PhysicCore.emptyGameObject, GetVec3ByString(parent.GetAttribute("position")), GetQuaByString(parent.GetAttribute("rotation")));
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        PhysicCore physic = GetComponent<PhysicCore>();
        List<IBlock> content = new List<IBlock>();
        Dictionary<string, IBlock> reflection = new Dictionary<string, IBlock>();

        foreach (XmlElement xmlElement in ((XmlElement)parent.GetElementsByTagName("Blocks")[0]).GetElementsByTagName("Block"))
        {
            GameObject block = Instantiate(SceneMaterial.Instance.BuildingPrefabs[int.Parse(xmlElement.GetAttribute("type"))], transform);
            block.transform.localPosition = SLMechanic.GetVec3ByString(xmlElement.GetAttribute("position"));
            block.transform.localRotation = SLMechanic.GetQuaByString(xmlElement.GetAttribute("rotation"));
            block.transform.localScale = SLMechanic.GetVec3ByString(xmlElement.GetAttribute("scale"));
            IBlock iblock = block.GetComponent<IBlock>();
            iblock.health = int.Parse(xmlElement.GetAttribute("health"));

            XmlNodeList attributeList = xmlElement.GetElementsByTagName("Attribute");
            foreach (XmlElement ele in attributeList)
            {
                FieldInfo field = iblock.GetType().GetField(ele.Attributes[0].Name);
                TypeConverter tc = TypeDescriptor.GetConverter(field.GetCustomAttribute<AttributeField>().type);
                field.SetValue(iblock, tc.ConvertFromString(ele.Attributes[0].Value));
            }

            XmlNodeList funcs = xmlElement.GetElementsByTagName("KeyListener");
            KeyFunction[] functions = iblock.GetComponents<KeyFunction>();
            for (int i = 0; i < funcs.Count; i++)
            {
                XmlElement ele = (XmlElement)funcs[i];
                functions[i].keycode = (KeyCode)int.Parse(ele.GetAttribute("keycode"));
            }

            reflection.Add(xmlElement.GetAttribute("InstanceID"), iblock);
            content.Add(iblock);
            foreach (Collider col in block.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = false;
            }
        }

        XmlElement conn = (XmlElement)parent.GetElementsByTagName("Connections")[0];
        foreach (XmlElement con in conn.GetElementsByTagName("Connect"))
        {
            IBlock a = reflection[con.GetAttribute("a")];
            IBlock b = reflection[con.GetAttribute("b")];
            a.connector.Add(b);
            b.connector.Add(a);
        }

        foreach (IBlock iblock in content)
        {
            iblock.Load();
            iblock.OnScale();
        }

        Ring ring = new Ring();
        ring.SetBlocks(content);
        ring.data_m = DataListener;
        physic.mring = ring;
        if(content.Count == 1)
        {
            physic.enabled = false;
        }
        physic.StartCheck();
        physic.RecalculateRigidbody();
    }

    [Command]
    public void CmdSyncData(string xmlstr)
    {
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlstr);
        XmlElement parent = (XmlElement)xml.GetElementsByTagName("PhysicCore")[0];
        //GameObject core = Instantiate(PhysicCore.emptyGameObject, GetVec3ByString(parent.GetAttribute("position")), GetQuaByString(parent.GetAttribute("rotation")));
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        PhysicCore physic = GetComponent<PhysicCore>();
        List<IBlock> content = new List<IBlock>();
        Dictionary<string, IBlock> reflection = new Dictionary<string, IBlock>();

        foreach (XmlElement xmlElement in ((XmlElement)parent.GetElementsByTagName("Blocks")[0]).GetElementsByTagName("Block"))
        {
            GameObject block = Instantiate(SceneMaterial.Instance.BuildingPrefabs[int.Parse(xmlElement.GetAttribute("type"))], transform);
            block.transform.localPosition =  SLMechanic.GetVec3ByString(xmlElement.GetAttribute("position"));
            block.transform.localRotation = SLMechanic.GetQuaByString(xmlElement.GetAttribute("rotation"));
            block.transform.localScale = SLMechanic.GetVec3ByString(xmlElement.GetAttribute("scale"));
            IBlock iblock = block.GetComponent<IBlock>();
            iblock.health = int.Parse(xmlElement.GetAttribute("health"));

            XmlNodeList attributeList = xmlElement.GetElementsByTagName("Attribute");
            foreach (XmlElement ele in attributeList)
            {
                FieldInfo field = iblock.GetType().GetField(ele.Attributes[0].Name);
                TypeConverter tc = TypeDescriptor.GetConverter(field.GetCustomAttribute<AttributeField>().type);
                field.SetValue(iblock, tc.ConvertFromString(ele.Attributes[0].Value));
            }

            XmlNodeList funcs = xmlElement.GetElementsByTagName("KeyListener");
            KeyFunction[] functions = iblock.GetComponents<KeyFunction>();
            for (int i = 0; i < funcs.Count; i++)
            {
                XmlElement ele = (XmlElement)funcs[i];
                functions[i].keycode = (KeyCode)int.Parse(ele.GetAttribute("keycode"));
            }

            reflection.Add(xmlElement.GetAttribute("InstanceID"), iblock);
            content.Add(iblock);
            foreach (Collider col in block.GetComponentsInChildren<Collider>())
            {
                col.isTrigger = false;
            }
        }

        XmlElement conn = (XmlElement)parent.GetElementsByTagName("Connections")[0];
        foreach (XmlElement con in conn.GetElementsByTagName("Connect"))
        {
            IBlock a = reflection[con.GetAttribute("a")];
            IBlock b = reflection[con.GetAttribute("b")];
            a.connector.Add(b);
            b.connector.Add(a);
        }

        foreach (IBlock iblock in content)
        {
            iblock.Load();
            iblock.OnScale();
        }

        physic.Load(content);        
        physic.RecalculateRigidbody();
    }

    [Command]
    public void CmdDespawn()
    {
        NetworkServer.UnSpawn(gameObject);
        Destroy(gameObject);
    }
}
