using RTEditor;
using System.Collections.Generic;
using UnityEngine;
using Gaboom.Util;
using System.Runtime.InteropServices;
using System;
using System.Xml;
using Gaboom.IO;
using System.IO;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.Events;
using static Gaboom.Util.FileSystem;

namespace Gaboom.Scene
{
    public class EditorFunction : MonoSingletonBase<EditorFunction>
    {
        public Material preview;
        public Material deny;

        //public List<GameObject> prefabs;
        public List<GameObject> ignores;
        List<GameObject> obstacles = new List<GameObject>();
        List<string> modelImported = new List<string> ();
        public UIGenerator modelPanel;
        public GameObject toolSet;
        GameObject generated;

        public bool align = true;
        public int brushSize = 5;
        [Range(0, 10)]
        public int power = 5;

        public Terrain terrain;
        
        public void Toggle()
        {
            enabled = !enabled;
        }
        CodeProgress m_CodeProgress = null;

        public Slider slider;

        UnityAction action = null;

        private void Start()
        {
            terrain.terrainData.SetHeights(0, 0, new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution]);
            m_CodeProgress = new CodeProgress(SetProgressPercent);
        }

        float precentage = 0;

        void SetProgressPercent(long fileSize, long processSize)
        {
            precentage = (float)processSize / fileSize;
        }

        bool isCustomModel = false;

        public void ChangeSelect(int selection)
        {
            isCustomModel = selection == 1;
        }

        public void LoadObj(int selection)
        {
            if(preloadObj != null)
                Destroy(preloadObj);
            string dataPath = Application.dataPath + "/Workspace";
            preloadObj = new GameObject();
            //preloadObj.AddComponent<CollisionProbe>();
            ObjLoader.LoadObjFile(dataPath + "/" + modelImported[selection] + ".obj").transform.parent = preloadObj.transform;
            preloadObj.tag = "ImportedModel";
            preloadObj.name = modelImported[selection];
            preloadObj.SetActive(false);
            selectedPrefab = selection;
            AddCollider(preloadObj);
        }

        public static void AddCollider(GameObject target)
        {
            MeshFilter filter = target.GetComponent<MeshFilter>();
            if (filter != null)
            {
                MeshCollider mc = target.AddComponent<MeshCollider>();
                mc.sharedMesh = filter.mesh;
                mc.convex = true;
                mc.isTrigger = true;
            }
            foreach(Transform child in target.transform)
            {
                AddCollider(child.gameObject);
            }
        }

        public void SaveToFile()
        {
            SaveFileDlg pth = new SaveFileDlg();
            pth.structSize = Marshal.SizeOf(pth);
            pth.filter = "Map files (*.gmap)\0*.gmap";
            pth.file = new string(new char[1024]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[256]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = Application.dataPath; //默认路径
            pth.title = "Save";
            pth.defExt = "gmap";
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (SaveFileDialog.GetSaveFileName(pth))
            {
                string filepath = pth.file; //选择的文件路径;  
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                SLMechanic.SerializeTerrainObjects(obstacles, doc, doc);
                string dataPath = Application.dataPath + "/Workspace";
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                doc.Save(dataPath+"/"+pth.fileTitle);
                slider.gameObject.SetActive(true);
                Thread streamThread = new Thread(new ThreadStart(() =>{
                    SerializeToFile(terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution), dataPath + "/Terrain.tr");
                    SerializeToFile(modelImported, dataPath + "/import.dat");
                    PackRes.PackFolder(dataPath, filepath + ".upk", m_CodeProgress);
                    LZMAHelper.Compress(filepath + ".upk", filepath, m_CodeProgress);
                    File.Delete(filepath + ".upk");
                    Directory.Delete(dataPath, true);
                    action = new UnityAction(() => { 
                        slider.gameObject.SetActive(false);
                    });
                }));
                streamThread.Start();
                
            }
        }

        public void CreateNewFile()
        {
            foreach(GameObject obj in obstacles) 
                Destroy(obj);
            obstacles.Clear();
            modelImported.Clear();
            modelPanel.ui.Clear();
            modelPanel.ReloadUI();
            terrain.terrainData.SetHeights(0, 0, new float[terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution]);
            string dataPath = Application.dataPath + "/Workspace";
            if (Directory.Exists(dataPath))
                Directory.Delete(dataPath, true);
            Directory.CreateDirectory(dataPath);
        }

        public void ImportObj()
        {
            OpenFileDlg pth = new OpenFileDlg();
            pth.structSize = Marshal.SizeOf(pth);
            pth.filter = "Obj files(*.obj)\0*.obj";
            pth.file = new string(new char[1024]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[256]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = Application.dataPath; //默认路径
            pth.title = "Open";
            pth.defExt = "obj";
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (OpenFileDialog.GetOpenFileName(pth))
            {
                string filepath = pth.file;
                string dataPath = Application.dataPath + "/Workspace";
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                File.Copy(filepath, dataPath + "/" + Path.GetFileNameWithoutExtension(filepath) + ".obj", true);
                modelImported.Add(Path.GetFileNameWithoutExtension(filepath));
                FileInfo ObjFileInfo = new FileInfo(filepath);
                foreach (string ln in File.ReadAllLines(filepath))
                {
                    string l = ln.Trim().Replace("  ", " ");
                    string[] cmps = l.Split(' ');
                    string data = l.Remove(0, l.IndexOf(' ') + 1);

                    if (cmps[0] == "mtllib")
                    {
                        //load cache
                        string mtlFile = ObjLoader.ObjGetFilePath(data, ObjFileInfo.Directory.FullName + Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(filepath));
                        if (mtlFile != null)
                            File.Copy(mtlFile, dataPath + "/" + Path.GetFileNameWithoutExtension(filepath) + ".mtl", true);
                        RenderTexture texture = RenderPreviewImage.GetAssetPreview(ObjLoader.LoadObjFile(filepath));
                        RenderPreviewImage.SaveTextureToPNG(texture, dataPath + "/" + Path.GetFileNameWithoutExtension(filepath) + "_thumbnail.png");
                        modelPanel.ui.Add(Sprite.Create(RenderPreviewImage.RenderTextureToTexture2D(texture), new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f)));
                        modelPanel.ReloadUI();
                    }
                }
            }
        }

        public void OpenFromFile()
        {
            foreach(GameObject obj in obstacles)
                Destroy(obj);
            obstacles.Clear();
            modelImported.Clear();
            OpenFileDlg pth = new OpenFileDlg();
            pth.structSize = Marshal.SizeOf(pth);
            pth.filter = "Map files(*.gmap)\0*.gmap";
            pth.file = new string(new char[1024]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[256]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = Application.dataPath; //默认路径
            pth.title = "Open";
            pth.defExt = "gmap";
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            if (OpenFileDialog.GetOpenFileName(pth))
            {
                string filepath = pth.file;
                string dataPath = Application.dataPath + "/Workspace";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }
                Directory.CreateDirectory(dataPath);
                slider.gameObject.SetActive(true);
                slider.name = pth.fileTitle;
                Thread thread = new Thread(new ThreadStart(() => {
                    LZMAHelper.DeCompress(filepath, filepath + ".upk", m_CodeProgress);
                    UPKExtra.ExtraUPK(filepath + ".upk", dataPath, m_CodeProgress);
                    File.Delete(filepath + ".upk");
                    modelImported = DeserializeFromFile<List<string>>(dataPath + "/import.dat");
                    modelPanel.ui.Clear();
                    foreach(string item in modelImported)
                    {
                        modelPanel.ui.Add(Sprite.Create(RenderPreviewImage.GetTexrture2DFromPath(dataPath + "/" + item + "_thumbnail.png"), new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f)));
                    }
                    action = new UnityAction(() => {
                        terrain.terrainData.SetHeights(0,0,DeserializeFromFile<float[,]>(dataPath+"/Terrain.tr"));
                        modelPanel.ReloadUI();
                        XmlDocument doc = new XmlDocument();
                        doc.Load(Application.dataPath + "/Workspace/" + slider.name);
                        obstacles = SLMechanic.DeserializeToScene(doc.GetElementsByTagName("Objects")[0]);
                        slider.gameObject.SetActive(false);
                    });
                }));
                thread.Start();
            }
        }

        GameObject preloadObj;
        public int selectedPrefab { get; set; }

        private void FixedUpdate()
        {
            if (generated != null)
            {
                //occupied = generated.GetComponentInChildren<CollisionProbe>().isIntersect;
                Destroy(generated);
            }
            if (selectedPrefab > -1 && !GameLogic.IsPointerOverGameObject())
            {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit raycastHit;
                Physics.Raycast(ray, out raycastHit);

                if (raycastHit.collider != null)
                {
                    if (align && !ignores.Contains(raycastHit.collider.gameObject))
                    {
                        Collider hitObj = raycastHit.collider;
                        if (isCustomModel)
                        {
                            generated = Instantiate(preloadObj, Vector3.zero, Quaternion.identity);
                            generated.SetActive(true);
                        }
                        else
                            generated = Instantiate(SceneMaterial.Instance.prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                        generated.transform.position = Align(generated, raycastHit);
                        if (raycastHit.collider.transform.parent != null)
                            generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                    }
                    else
                    {
                        if (isCustomModel)
                        {
                            generated = Instantiate(preloadObj,raycastHit.point + SceneMaterial.Instance.prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                            generated.SetActive(true);
                        }
                        else
                            generated = Instantiate(SceneMaterial.Instance.prefabs[selectedPrefab], raycastHit.point + SceneMaterial.Instance.prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                    }
                    foreach (MeshRenderer child in generated.GetComponentsInChildren<MeshRenderer>())
                    {
                        child.material = preview;
                    }
                    generated.layer = LayerMask.NameToLayer("Ignore Raycast");
                    foreach (GameObject child in generated.GetAllChildren())
                    {
                        child.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }
                    if (generated.GetComponent<MeshRenderer>() != null)
                        generated.GetComponent<MeshRenderer>().material = preview;
                    //generated.GetComponentInChildren<Collider>().gameObject.AddComponent<CollisionProbe>();
                }
            }
        }

        private void Update()
        {
            if (slider.IsActive())
            {
                slider.value = precentage;
                if (action != null)
                    action.Invoke();
                action = null;
            }
            if (!GameLogic.IsPointerOverGameObject() && Input.GetMouseButton(0))
            {
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                RaycastHit raycastHit;
                switch (selectedPrefab)
                {
                    case -8:
                        Physics.Raycast(ray, out raycastHit, Mathf.Infinity, 1 << 6);
                        if (raycastHit.collider != null)
                        {
                            Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
                            float[,] height = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                            float ratio = terrain.terrainData.heightmapResolution / terrain.terrainData.size.x;
                            Vector3 hitpoint = raycastHit.point - terrain.GetPosition();
                            for (int x = (int)Math.Max(hitpoint.x * ratio - brushSize, 0); x < Math.Min(hitpoint.x * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); x++)
                            {
                                for (int y = (int)Math.Max(hitpoint.z * ratio - brushSize, 0); y < Math.Min(hitpoint.z * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); y++)
                                {
                                    if (Input.GetKey(KeyCode.LeftShift))
                                    {
                                        height[y, x] -= Mathf.Max(0, (-(Mathf.Pow(x - (int)hitpoint.x * ratio, 2) + Mathf.Pow(y - (int)hitpoint.z * ratio, 2)) * power / Mathf.Pow(brushSize, 2) + power) / 1000);
                                    }
                                    else
                                    {
                                        height[y, x] += Mathf.Max(0, (-(Mathf.Pow(x - (int)hitpoint.x * ratio, 2) + Mathf.Pow(y - (int)hitpoint.z * ratio, 2)) * power / Mathf.Pow(brushSize, 2) + power) / 1000);
                                    }
                                }
                            }
                            terrain.terrainData.SetHeights(0, 0, height);
                        }
                        break;
                    case -9:
                        Physics.Raycast(ray, out raycastHit, Mathf.Infinity, 1 << 6);
                        if (raycastHit.collider != null)
                        {
                            Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
                            float[,] heightmap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                            float[,] height = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                            float ratio = terrain.terrainData.heightmapResolution / terrain.terrainData.size.x;
                            Vector3 hitpoint = raycastHit.point - terrain.GetPosition();
                            int startx = (int)Math.Max(hitpoint.x * ratio - brushSize, 0);
                            int starty = (int)Math.Max(hitpoint.y * ratio - brushSize, 0);
                            /*int count = 0;
                            for (int x = startx; x < Math.Min(((int)hitpoint.x + brushSize/2) * ratio, terrain.terrainData.heightmapResolution - 1); x++)
                            {
                                for (int y = starty; y < Math.Min(((int)hitpoint.z + brushSize/2) * ratio, terrain.terrainData.heightmapResolution - 1); y++)
                                {
                                    sum += height[y,x];
                                    ++count;
                                }
                            }
                            sum/=count;*/
                            for (int x = startx; x < Math.Min(hitpoint.x * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); x++)
                            {
                                for (int y = starty; y < Math.Min(hitpoint.z * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); y++)
                                {
                                    float sum = 0;
                                    for (int i = x - 1; i <= x + 1; i++)
                                    {
                                        for (int j = y - 1; j <= y + 1; j++)
                                        {
                                            sum += heightmap[Math.Max(Math.Min(j, terrain.terrainData.heightmapResolution - 1), 0), Math.Max(Math.Min(i, terrain.terrainData.heightmapResolution - 1), 0)];
                                        }
                                    }
                                    height[y, x] = sum / 9;
                                }
                            }
                            terrain.terrainData.SetHeights(0, 0, height);
                        }
                        break;
                }

                if (!GameLogic.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
                {
                    Physics.Raycast(ray, out raycastHit);
                    if (selectedPrefab > -1)
                    {
                        if (raycastHit.collider != null)
                        {
                            if (generated != null)
                            {
                                Destroy(generated);
                                generated = null;
                            }
                            if (align && !ignores.Contains(raycastHit.collider.gameObject))
                            {
                                Collider hitObj = raycastHit.collider;
                                if (isCustomModel)
                                {
                                    generated = Instantiate(preloadObj, Vector3.zero, Quaternion.identity);
                                    generated.SetActive(true);
                                }
                                else
                                    generated = Instantiate(SceneMaterial.Instance.prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                                generated.transform.position = Align(generated, raycastHit);
                                if (hitObj.transform.parent != null)
                                    generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                            }
                            else
                            {
                                if (isCustomModel)
                                {
                                    generated = Instantiate(preloadObj, raycastHit.point + SceneMaterial.Instance.prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                                    generated.SetActive(true);
                                }
                                else
                                    generated = Instantiate(SceneMaterial.Instance.prefabs[selectedPrefab], raycastHit.point + SceneMaterial.Instance.prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                            }
                            obstacles.Add(generated);

                            foreach (Collider child in generated.GetComponentsInChildren<Collider>())
                            {
                                child.isTrigger = false;
                            }
                            if (generated.GetComponent<Collider>() != null)
                                generated.GetComponent<Collider>().isTrigger = false;

                            generated = null;
                        }
                    }
                    else if (raycastHit.collider != null)
                    {
                        switch (selectedPrefab)
                        {
                            case -7:
                                if (raycastHit.collider != null && !ignores.Contains(raycastHit.collider.gameObject))
                                {
                                    Destroy(raycastHit.collider.gameObject);
                                }
                                break;
                        }
                    }
                }
            }
        }
        public Vector3 Align(GameObject preview, RaycastHit hit)
        {
            return hit.collider.transform.position + Vector3.Project(hit.point - hit.collider.transform.position, hit.normal) + hit.collider.transform.rotation * preview.transform.GetComponentInChildren<Collider>().ClosestPoint(hit.collider.transform.InverseTransformDirection(hit.normal));
        }
    }
}