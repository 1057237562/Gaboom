using RTEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gaboom.Util;
using System;

public class EditorFunction : MonoSingletonBase<EditorFunction>
{
    public Material preview;
    public Material deny;

    public List<GameObject> prefabs;
    public List<GameObject> ignores;
    public GameObject toolSet;
    GameObject generated;
    public bool align = true;
    public int brushSize = 5;
    [Range(0, 10)]
    public int power = 5;

    public void Toggle()
    {
        enabled = !enabled;
    }
    bool occupied = true;

    private void FixedUpdate()
    {
        if (generated != null)
        {
            occupied = generated.GetComponentInChildren<CollisionProbe>().isIntersect;
            Destroy(generated);
        }
        if (BuildFunction.selectedPrefab > -1 && !GameLogic.IsPointerOverGameObject())
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit);

            if (raycastHit.collider != null)
            {
                if (align && !ignores.Contains(raycastHit.collider.gameObject))
                {
                    Collider hitObj = raycastHit.collider;
                    generated = Instantiate(prefabs[BuildFunction.selectedPrefab], Vector3.zero, Quaternion.identity);
                    generated.transform.position = Align(generated, raycastHit);
                    if (raycastHit.collider.transform.parent != null)
                        generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                }
                else
                {
                    generated = Instantiate(prefabs[BuildFunction.selectedPrefab], raycastHit.point + prefabs[BuildFunction.selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                }
                foreach (MeshRenderer child in generated.GetComponentsInChildren<MeshRenderer>())
                {
                    child.material = occupied ? deny : preview;
                }
                generated.layer = LayerMask.NameToLayer("Ignore Raycast");
                foreach (Transform child in generated.transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
                if (generated.GetComponent<MeshRenderer>() != null)
                    generated.GetComponent<MeshRenderer>().material = preview;
                generated.GetComponentInChildren<Collider>().gameObject.AddComponent<CollisionProbe>();
            }
        }
    }

    private void Update()
    {
        if (!GameLogic.IsPointerOverGameObject() && Input.GetMouseButton(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            switch (BuildFunction.selectedPrefab)
            {
                case -8:
                    Physics.Raycast(ray, out raycastHit, Mathf.Infinity, 1 << 6);
                    if (raycastHit.collider != null)
                    {
                        Terrain terrain = raycastHit.collider.GetComponent<Terrain>();
                        float[,] height = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                        float ratio = terrain.terrainData.heightmapResolution / terrain.terrainData.size.x;
                        Vector3 hitpoint = raycastHit.point - terrain.GetPosition();
                        for (int x = (int)Math.Max(hitpoint.x  * ratio - brushSize, 0); x < Math.Min(hitpoint.x * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); x++)
                        {
                            for (int y = (int)Math.Max(hitpoint.z  * ratio - brushSize, 0); y < Math.Min(hitpoint.z * ratio + brushSize, terrain.terrainData.heightmapResolution - 1); y++)
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
                                       sum += heightmap[Math.Max(Math.Min(j, terrain.terrainData.heightmapResolution - 1),0), Math.Max(Math.Min(i, terrain.terrainData.heightmapResolution - 1), 0)];
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
                if (BuildFunction.selectedPrefab > -1)
                {
                    if (raycastHit.collider != null)
                    {
                        if (!occupied)
                        {
                            if (generated != null)
                            {
                                Destroy(generated);
                                generated = null;
                            }
                            if (align)
                            {
                                Collider hitObj = raycastHit.collider;
                                generated = Instantiate(prefabs[BuildFunction.selectedPrefab], Vector3.zero, Quaternion.identity);
                                generated.transform.position = Align(generated, raycastHit);
                                if (hitObj.transform.parent != null)
                                    generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                            }
                            else
                            {
                                generated = Instantiate(prefabs[BuildFunction.selectedPrefab], raycastHit.point + prefabs[BuildFunction.selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                            }
                            generated = null;
                        }
                    }
                }
                else if (raycastHit.collider != null)
                {
                    switch (BuildFunction.selectedPrefab)
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
