using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Linq;
using Gaboom.Util;
using System;
using Object = UnityEngine.Object;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using Gaboom.Scene;

namespace Gaboom.IO
{
    public class SLMechanic
    {

        public static void SerializeTerrainObjects(List<GameObject> objects,XmlDocument document,XmlNode node)
        {
            XmlElement root = document.CreateElement("Objects");
            foreach(GameObject obj in objects)
            {
                XmlElement ele = document.CreateElement("Object");
                if (obj.tag == "ImportedModel")
                {
                    ele.SetAttribute("name", obj.name.Replace("(Clone)", ""));
                    ele.SetAttribute("type", "ImportedModel");
                }
                else
                    ele.SetAttribute("type", SceneMaterial.Instance.prefabs.IndexOf(SceneMaterial.Instance.prefabs.First((x) => { return obj.name.Contains(x.name); })).ToString());
                ele.SetAttribute("position", obj.transform.position.ToString("r"));
                ele.SetAttribute("rotation", obj.transform.localRotation.ToString("r"));
                ele.SetAttribute("scale", obj.transform.localScale.ToString("r"));
                root.AppendChild(ele);
            }
            node.AppendChild(root);
        }

        public static List<GameObject> DeserializeToScene(XmlNode xmlNode,bool isStatic = false)
        {
            List<GameObject> objects = new List<GameObject>();
            foreach(XmlElement xmlElement in xmlNode)
            {
                GameObject block;
                if(xmlElement.GetAttribute("type") == "ImportedModel")
                {
                    string dataPath = Application.dataPath + "/Workspace";
                    block = new GameObject();
                    //preloadObj.AddComponent<CollisionProbe>();
                    ObjLoader.LoadObjFile(dataPath + "/" + xmlElement.GetAttribute("name") + ".obj").transform.parent = block.transform;
                    block.tag = "ImportedModel";
                    block.name = xmlElement.GetAttribute("name");
                    EditorFunction.AddCollider(block);
                }
                else
                block= Object.Instantiate(SceneMaterial.Instance.prefabs[int.Parse(xmlElement.GetAttribute("type"))]);
                block.transform.position = GetVec3ByString(xmlElement.GetAttribute("position"));
                block.transform.rotation = GetQuaByString(xmlElement.GetAttribute("rotation"));
                block.transform.localScale = GetVec3ByString(xmlElement.GetAttribute("scale"));
                block.isStatic = isStatic;
                objects.Add(block);
            }
            return objects;
        }

        public static string machineFolder = Environment.CurrentDirectory + "/saves/";
        /// <summary>
        /// 将游戏物体序列化成为XML
        /// </summary>
        /// <param name="core">物理核心</param>
        /// <returns></returns>
        public static string SerializeToXml(PhysicCore core)
        {
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateXmlDeclaration("1.0", "utf-8", "yes"));

            XmlElement parent = xml.CreateElement("PhysicCore");
            parent.SetAttribute("position", core.transform.position.ToString("r"));
            parent.SetAttribute("rotation", core.transform.rotation.ToString("r"));
            parent.SetAttribute("velocity", core.GetComponent<Rigidbody>().velocity.ToString("r"));
            XmlNode root = xml.CreateElement("Blocks");
            foreach (IBlock block in core.GetBlocks())
            {
                XmlElement ele = xml.CreateElement("Block");
                ele.SetAttribute("InstanceID", block.GetInstanceID().ToString());
                ele.SetAttribute("type", BuildFunction.Instance.prefabs.IndexOf(BuildFunction.Instance.prefabs.First((x) => { return block.name.Contains(x.name); })).ToString());
                ele.SetAttribute("position", block.position.ToString("r"));
                ele.SetAttribute("rotation", block.transform.localRotation.ToString("r"));
                ele.SetAttribute("scale", block.transform.localScale.ToString("r"));
                ele.SetAttribute("health", block.health.ToString());

                FieldInfo[] fields = block.GetType().GetFields();  //Only retrun public field
                if (fields.Length > 0)
                {
                    XmlElement attr = xml.CreateElement("Attributes");
                    foreach (FieldInfo item in fields)
                    {
                        if (item.GetCustomAttribute<AttributeField>() == null)
                            continue;
                        XmlElement a = xml.CreateElement("Attribute");
                        object obj = item.GetValue(block);
                        a.SetAttribute(item.Name, obj.ToString());
                        attr.AppendChild(a);
                    }
                    ele.AppendChild(attr);
                }

                KeyFunction[] funcs = block.GetComponents<KeyFunction>();
                if(funcs.Length > 0)
                {
                    XmlElement attr = xml.CreateElement("KeyFunctions");
                    foreach (KeyFunction listener in funcs)
                    {
                        XmlElement a = xml.CreateElement("KeyListener");
                        a.SetAttribute("keycode", ((int)listener.keycode).ToString());
                        attr.AppendChild(a);
                    }
                    ele.AppendChild(attr);
                }
                root.AppendChild(ele);
            }
            parent.AppendChild(root);

            XmlNode connection = xml.CreateElement("Connections");
            foreach (IBlock block in core.GetBlocks())
            {
                foreach (IBlock connector in block.connector)
                {
                    XmlElement ele = xml.CreateElement("Connect");
                    ele.SetAttribute("a", block.GetInstanceID().ToString());
                    ele.SetAttribute("b", connector.GetInstanceID().ToString());
                    connector.connector.Remove(block);
                    connection.AppendChild(ele);
                }
            }
            parent.AppendChild(connection);
            xml.AppendChild(parent);

            return xml.InnerXml;
        }

        public static GameObject DeserializeToGameObject(string xmlstr)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlstr);
            XmlElement parent = (XmlElement)xml.GetElementsByTagName("PhysicCore")[0];
            GameObject core = Object.Instantiate(PhysicCore.emptyGameObject, GetVec3ByString(parent.GetAttribute("position")), GetQuaByString(parent.GetAttribute("rotation")));

            PhysicCore physic = core.GetComponent<PhysicCore>();
            List<IBlock> content = new List<IBlock>();
            Dictionary<string, IBlock> reflection = new Dictionary<string, IBlock>();

            foreach (XmlElement xmlElement in ((XmlElement)parent.GetElementsByTagName("Blocks")[0]).GetElementsByTagName("Block"))
            {
                GameObject block = Object.Instantiate(BuildFunction.Instance.prefabs[int.Parse(xmlElement.GetAttribute("type"))], core.transform);
                block.transform.localPosition = GetVec3ByString(xmlElement.GetAttribute("position"));
                block.transform.localRotation = GetQuaByString(xmlElement.GetAttribute("rotation"));
                block.transform.localScale = GetVec3ByString(xmlElement.GetAttribute("scale"));
                IBlock iblock = block.GetComponent<IBlock>();
                iblock.health = int.Parse(xmlElement.GetAttribute("health"));

                XmlNodeList attributeList = xmlElement.GetElementsByTagName("Attribute");
                foreach (XmlElement ele in attributeList)
                {
                    FieldInfo field = iblock.GetType().GetField(ele.Attributes[0].Name);
                    TypeConverter tc = TypeDescriptor.GetConverter(field.GetCustomAttribute<AttributeField>().type);
                    field.SetValue(iblock,  tc.ConvertFromString(ele.Attributes[0].Value));
                }

                XmlNodeList funcs = xmlElement.GetElementsByTagName("KeyListener");
                KeyFunction[] functions = iblock.GetComponents<KeyFunction>();
                for(int i = 0; i < funcs.Count;i++)
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
            return core;
        }

        public static void SaveObjToFile(PhysicCore obj, string filename)
        {
            if (!Directory.Exists(machineFolder))
            {
                Directory.CreateDirectory(machineFolder);
            }
            FileSystem.WriteFile(machineFolder + filename + ".gm", SerializeToXml(obj));
        }

        public static GameObject LoadObjFromFile(string filename)
        {
            return DeserializeToGameObject(FileSystem.ReadFile(machineFolder + filename));
        }

        public static void LoadObjFromFile(string filename, Vector3 position, Quaternion rotation)
        {
            GameObject gameObject = DeserializeToGameObject(FileSystem.ReadFile(machineFolder + filename));
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }

        /// <summary>
        /// 字符串转Vector3
        /// </summary>
        /// <param name="p_sVec3">需要转换的字符串</param>
        /// <returns></returns>
        public static Vector3 GetVec3ByString(string p_sVec3)
        {
            if (p_sVec3.Length <= 0)
                return Vector3.zero;

            string[] tmp_sValues = p_sVec3.Trim('(').Trim(')').Split(',');
            if (tmp_sValues != null && tmp_sValues.Length == 3)
            {
                float tmp_fX = float.Parse(tmp_sValues[0]);
                float tmp_fY = float.Parse(tmp_sValues[1]);
                float tmp_fZ = float.Parse(tmp_sValues[2]);

                return new Vector3(tmp_fX, tmp_fY, tmp_fZ);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// 字符串转换Quaternion
        /// </summary>
        /// <param name="p_sVec3">需要转换的字符串</param>
        /// <returns></returns>
        public static Quaternion GetQuaByString(string p_sVec3)
        {
            if (p_sVec3.Length <= 0)
                return Quaternion.identity;

            string[] tmp_sValues = p_sVec3.Trim('(').Trim(')').Split(',');
            if (tmp_sValues != null && tmp_sValues.Length == 4)
            {
                float tmp_fX = float.Parse(tmp_sValues[0]);
                float tmp_fY = float.Parse(tmp_sValues[1]);
                float tmp_fZ = float.Parse(tmp_sValues[2]);
                float tmp_fH = float.Parse(tmp_sValues[3]);

                return new Quaternion(tmp_fX, tmp_fY, tmp_fZ, tmp_fH);
            }
            return Quaternion.identity;
        }

    }
}

public class AttributeField : Attribute
{
    public Type type;
}
