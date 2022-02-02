using Gaboom.IO;
using Gaboom.Util;
using RTEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Unity.Netcode;
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

        public List<GameObject> ignores;
        public GameObject keypanel;

        public List<GameObject> TerrainPrefabs;
        public List<GameObject> BuildingPrefabs;

        public BuildFunction mainController;
        public RuntimeEditorApplication runtimeEditor;

        private void Awake()
        {
            if (GameObject.FindGameObjectsWithTag("MainCamera").Length == 0 && SceneManager.GetActiveScene().name == "GameScene")
            {
                if (NetworkManager.Singleton == null)
                {
                    GameObject cam = Instantiate(cameraPrefab);
                    runtimeEditor.CustomCamera = cam.GetComponent<Camera>();
                    mainController = cam.GetComponent<BuildFunction>();
                }
                else
                {
                    GameObject cam = Instantiate(networkcameraPrefab);
                    runtimeEditor.CustomCamera = cam.GetComponent<Camera>();
                    cam.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
                    mainController = cam.GetComponent<BuildFunction>();
                }
            }
        }

        public void DelegateChangeSelected(int index)
        {
            mainController.selectedPrefab = index;
        }

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
                File.Copy(dataPath + "/thumbnail.png", Application.dataPath + "/maps/" + Path.GetFileNameWithoutExtension(filepath) + "_thumbnail.png", true);
            }
        }
    }
}
