using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Linq;
using Gaboom.Util;
using System;
using Object = UnityEngine.Object;

namespace Gaboom.IO
{
    public class SLMechanic
    {

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
            parent.SetAttribute("position", core.transform.position.ToString());
            parent.SetAttribute("rotation", core.transform.rotation.ToString());
            parent.SetAttribute("velocity", core.GetComponent<Rigidbody>().velocity.ToString());
            XmlNode root = xml.CreateElement("Blocks");
            foreach (IBlock block in core.GetBlocks())
            {
                XmlElement ele = xml.CreateElement("Block");
                ele.SetAttribute("InstanceID", block.GetInstanceID().ToString());
                ele.SetAttribute("type", BuildFunction.Instance.prefabs.IndexOf(BuildFunction.Instance.prefabs.First((x) => { return block.name.Contains(x.name); })).ToString());
                ele.SetAttribute("position", block.position.ToString());
                ele.SetAttribute("rotation", block.rotation.ToString());
                ele.SetAttribute("scale", block.transform.localScale.ToString());
                ele.SetAttribute("health", block.health.ToString());
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
                    connection.AppendChild(ele);
                }
            }
            parent.AppendChild(connection);
            xml.AppendChild(parent);

            return xml.InnerXml;
        }

        public static void DeserializeToGameObject(string xmlstr)
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
                reflection.Add(xmlElement.GetAttribute("InstanceID"), iblock);
                content.Add(iblock);
                foreach (Collider col in block.GetComponentsInChildren<Collider>())
                {
                    col.isTrigger = false;
                }
            }

            XmlElement conn = (XmlElement)parent.GetElementsByTagName("Connections")[0];
            foreach (XmlElement con in conn.GetElementsByTagName("Connection"))
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

        /// <summary>
        /// 反序列化XML到指定位置
        /// </summary>
        /// <param name="xmlstr">XML内容</param>
        /// <param name="position">指定位置</param>
        /// <param name="rotation">旋转量</param>
        /// <returns></returns>
        public static void DeserializeToGameObject(string xmlstr, Vector3 pos, Quaternion rota)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlstr);
            XmlElement parent = (XmlElement)xml.GetElementsByTagName("PhysicCore")[0];
            GameObject core = Object.Instantiate(PhysicCore.emptyGameObject, pos, rota);

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
                reflection.Add(xmlElement.GetAttribute("InstanceID"), iblock);
                content.Add(iblock);
                foreach (Collider col in block.GetComponentsInChildren<Collider>())
                {
                    col.isTrigger = false;
                }
            }

            XmlElement conn = (XmlElement)parent.GetElementsByTagName("Connections")[0];
            foreach (XmlElement con in conn.GetElementsByTagName("Connection"))
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

        public static void SaveObjToFile(PhysicCore obj, string filename)
        {
            if(!Directory.Exist(machineFolder)){
                Directory.CreateDirectory(machineFolder);
            }
            FileSystem.WriteFile(machineFolder + filename, SerializeToXml(obj));
        }

        public static void LoadObjFromFile(string filename)
        {
            DeserializeToGameObject(FileSystem.ReadFile(machineFolder + filename));
        }

        public static void LoadObjFromFile(string filename, Vector3 position, Quaternion rotation)
        {
            DeserializeToGameObject(FileSystem.ReadFile(machineFolder + filename), position, rotation);
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
