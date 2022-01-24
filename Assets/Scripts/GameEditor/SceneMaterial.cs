using Gaboom.IO;
using Gaboom.Util;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gaboom.Scene
{

    public class SceneMaterial : MonoBehaviour
    {
        public static SceneMaterial Instance { get; private set; }
        public static string filepath;

        public Terrain terrain;

        public List<GameObject> prefabs = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameScene") && filepath != null)
            {
                string dataPath = Application.dataPath + "/Workspace";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }
                Directory.CreateDirectory(dataPath);
                LZMAHelper.DeCompress(filepath, filepath + ".upk", null);
                UPKExtra.ExtraUPK(filepath + ".upk", dataPath, null);
                File.Delete(filepath + ".upk");
                terrain.terrainData.SetHeights(0, 0, FileSystem.DeserializeFromFile<float[,]>(dataPath + "/Terrain.tr"));
                XmlDocument doc = new XmlDocument();
                doc.Load(Application.dataPath + "/Workspace/" + Path.GetFileName(filepath));
                SLMechanic.DeserializeToScene(doc.GetElementsByTagName("Objects")[0]);
            }
        }
    }
}
