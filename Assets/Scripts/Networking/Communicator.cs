using Gaboom.IO;
using Gaboom.Scene;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
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
    public void AttemptGeneratePhysicCoreServerRpc(Vector3 point, Quaternion rotation, int selectedPrefab ,ulong clientId)
    {
        if (!IsServer && !IsHost) return;

        GameObject parent = Instantiate(emptyGameObject, point, Quaternion.identity);

        PhysicCore core = parent.GetComponent<PhysicCore>();
        parent.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        parent.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
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

    [ServerRpc]
    public void AttemptGeneratePhysicCoreServerRpc(string xmlstr, ulong clientId)
    {
        if (!IsServer && !IsHost) return;
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlstr);
        XmlElement parent = (XmlElement)xml.GetElementsByTagName("PhysicCore")[0];
        //GameObject core = Instantiate(PhysicCore.emptyGameObject, GetVec3ByString(parent.GetAttribute("position")), GetQuaByString(parent.GetAttribute("rotation")));
        GameObject core = Instantiate(emptyGameObject, SLMechanic.GetVec3ByString(parent.GetAttribute("position")), SLMechanic.GetQuaByString(parent.GetAttribute("rotation")));

        core.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

        PhysicCore physic = core.GetComponent<PhysicCore>();
        List<IBlock> content = new List<IBlock>();
        Dictionary<string, IBlock> reflection = new Dictionary<string, IBlock>();

        foreach (XmlElement xmlElement in ((XmlElement)parent.GetElementsByTagName("Blocks")[0]).GetElementsByTagName("Block"))
        {
            GameObject block = Instantiate(SceneMaterial.Instance.BuildingPrefabs[int.Parse(xmlElement.GetAttribute("type"))], core.transform);
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

        physic.Load(content);
        physic.RecalculateRigidbody();
        physic.enabled = true;
    }

}
