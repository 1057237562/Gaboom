using RTEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildFunction : MonoSingletonBase<BuildFunction>
{
    public Material preview;
    public Material deny;

    public List<GameObject> prefabs;
    public List<GameObject> ignores;
    public GameObject keypanel;
    public static int selectedPrefab = -1;
    GameObject generated;
    public bool align = true;
    public bool autoConnect = true;
    public static GameObject selectedObj;

    public void Toggle()
    {
        enabled = !enabled;
    }

    private void Update()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        bool occupied = true;
        if (generated != null)
        {
            occupied = generated.GetComponent<CollisionProbe>().isIntersect;
            Destroy(generated);
        }
        if (selectedPrefab > -1)
        {
            selectedObj = null;
            if (raycastHit.collider != null && !ignores.Contains(raycastHit.collider.gameObject))
            {
                if (align)
                {
                    Collider hitObj = raycastHit.collider;
                    generated = Instantiate(prefabs[selectedPrefab], Align(raycastHit.normal, prefabs[selectedPrefab], hitObj.gameObject), hitObj.transform.rotation);
                }
                else
                {
                    generated = Instantiate(prefabs[selectedPrefab], raycastHit.point + prefabs[selectedPrefab].transform.lossyScale / 2, transform.rotation);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (raycastHit.collider.transform.parent != null)
                    {
                        IBlock block = generated.AddComponent<IBlock>();
                        generated.transform.parent = raycastHit.collider.transform.parent.parent;
                        IBlock relativeBlock = raycastHit.collider.transform.parent.GetComponent<IBlock>();
                        relativeBlock.connector.Add(block);
                        block.connector.Add(raycastHit.collider.transform.parent.GetComponent<IBlock>());
                        //block.mass = generated.GetComponent<Rigidbody>().mass;
                        //block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                        block.Load();
                        relativeBlock.ReloadRPos();
                        block.core.AppendIBlock(block);
                        foreach (Collider child in generated.GetComponentsInChildren<Collider>())
                        {
                            child.isTrigger = false;
                        }
                        if (generated.GetComponent<Collider>() != null)
                            generated.GetComponent<Collider>().isTrigger = false;
                    }
                    else
                    {
                        GameObject parent = Instantiate(PhysicCore.emptyGameObject, raycastHit.point, Quaternion.identity);
                        generated.transform.parent = parent.transform;

                        PhysicCore core = parent.GetComponent<PhysicCore>();

                        List<IBlock> blocks = new List<IBlock>();

                        IBlock block = generated.AddComponent<IBlock>();
                        //block.mass = generated.GetComponent<Rigidbody>().mass;
                        //block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                        block.Load();
                        blocks.Add(block);

                        core.RecalculateRigidbody(blocks);
                    }
                    generated = null;
                }
                else
                {

                    foreach (MeshRenderer child in generated.GetComponentsInChildren<MeshRenderer>())
                    {
                        child.material = preview;
                    }
                    generated.layer = LayerMask.NameToLayer("Ignore Raycast");
                    foreach(Transform child in generated.transform)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }
                    if (generated.GetComponent<MeshRenderer>() != null)
                        generated.GetComponent<MeshRenderer>().material = preview;
                    generated.AddComponent<CollisionProbe>();
                }
            }
        }
        if (raycastHit.collider != null)
        {
            switch (selectedPrefab)
            {
                case -1:
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (raycastHit.collider != null && raycastHit.collider.transform.parent != null)
                        {
                            KeyFunction[] keyFunction = raycastHit.collider.transform.parent.GetComponents<KeyFunction>();
                            if (keyFunction.Length > 0)
                            {
                                keypanel.SetActive(true);
                                KeyPanel kp = keypanel.GetComponent<KeyPanel>();
                                kp.objname.text = raycastHit.collider.transform.parent.name;
                                kp.CreateItem(keyFunction);
                            }
                        }
                    }
                    break;
                default:
                    if (Input.GetMouseButtonDown(2))
                    {
                        if (raycastHit.collider.transform.parent != null)
                        {
                            KeyFunction[] keyFunction = raycastHit.collider.transform.parent.GetComponents<KeyFunction>();
                            if (keyFunction.Length > 0)
                            {
                                keypanel.SetActive(true);
                                KeyPanel kp = keypanel.GetComponent<KeyPanel>();
                                kp.objname.text = raycastHit.collider.transform.parent.name;
                                kp.CreateItem(keyFunction);
                            }
                        }
                    }
                    break;
            }
        }
    }
    public Vector3 Align(Vector3 normal, GameObject preview, GameObject hitObj)
    {
        return hitObj.transform.position + normal * (hitObj.transform.lossyScale.x + preview.transform.lossyScale.x) / 2;
    }
}
