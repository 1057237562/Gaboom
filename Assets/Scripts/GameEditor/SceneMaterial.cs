using Gaboom.IO;
using Gaboom.Util;
using RTEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gaboom.Scene
{

    public class SceneMaterial : MonoBehaviour
    {
        public static SceneMaterial Instance { get; private set; }
        public static string filepath;

        public Terrain terrain;
        public GameObject cameraPrefab;
        public GameObject networkcameraPrefab;

        //public List<GameObject> ignores;
        public GameObject keypanel;

        public List<GameObject> TerrainPrefabs;
        public List<GameObject> BuildingPrefabs;

        public int selectedPrefab { get; set; }
        public RuntimeEditorApplication runtimeEditor;

        public void DelegateChangeSelected(int index)
        {
            selectedPrefab = index;
        }

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
            if (GameObject.FindGameObjectsWithTag("MainCamera").Length == 0 && SceneManager.GetActiveScene().name == "GameScene")
            {
                if (NetworkManager.singleton == null)
                {
                    GameObject cam = Instantiate(cameraPrefab);
                    runtimeEditor.CustomCamera = cam.GetComponent<Camera>();
                    //mainController = cam.GetComponent<BuildFunction>();
                }
                else
                {
                    if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly || NetworkManager.singleton.mode == NetworkManagerMode.Host)
                    {
                        GameObject cam = Instantiate(networkcameraPrefab);
                        runtimeEditor.CustomCamera = cam.GetComponent<Camera>();
                        NetworkServer.Spawn(cam);
                        //mainController = cam.GetComponent<BuildFunction>();
                    }
                    else
                    {
                        NetworkManager.singleton.GetComponent<NetworkController>().SpawnNetworkCamera();
                    }
                }
            }
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
                SLMechanic.DeserializeToScene(doc.GetElementsByTagName("Objects")[0], true).ForEach((x) => { x.GetComponentInChildren<Collider>().tag = "Terrain"; });
                File.Copy(dataPath + "/thumbnail.png", Application.dataPath + "/maps/" + Path.GetFileNameWithoutExtension(filepath) + "_thumbnail.png", true);
            }
        }
    }
}
