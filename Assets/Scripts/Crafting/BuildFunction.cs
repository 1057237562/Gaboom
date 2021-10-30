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

    public void Toggle()
    {
        enabled = !enabled;
    }
    bool occupied = true;

    private void FixedUpdate()
    {
        if (selectedPrefab > -1)
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit);
            if (generated != null)
            {
                occupied = generated.GetComponentInChildren<CollisionProbe>().isIntersect;
                Destroy(generated);
            }

            if (raycastHit.collider != null && !ignores.Contains(raycastHit.collider.gameObject))
            {
                if (align)
                {
                    Collider hitObj = raycastHit.collider;
                    generated = Instantiate(prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                    generated.transform.position = Align(generated, raycastHit);
                    generated.transform.rotation = Quaternion.FromToRotation(generated.transform.forward, generated.transform.position - hitObj.transform.position);
                }
                else
                {
                    generated = Instantiate(prefabs[selectedPrefab], raycastHit.point + prefabs[selectedPrefab].transform.lossyScale / 2, transform.rotation);
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit);
            if (selectedPrefab > -1)
            {
                if (raycastHit.collider != null && !ignores.Contains(raycastHit.collider.gameObject))
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
                            generated = Instantiate(prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                            generated.transform.position = Align(generated, raycastHit);
                            generated.transform.rotation = Quaternion.FromToRotation(generated.transform.forward, generated.transform.position - hitObj.transform.position);
                        }
                        else
                        {
                            generated = Instantiate(prefabs[selectedPrefab], raycastHit.point + prefabs[selectedPrefab].transform.lossyScale / 2, transform.rotation);
                        }

                        if (raycastHit.collider.transform.parent != null)
                        {
                            IBlock block = generated.GetComponent<IBlock>();
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

                            IBlock block = generated.GetComponent<IBlock>();
                            //block.mass = generated.GetComponent<Rigidbody>().mass;
                            //block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                            block.Load();
                            blocks.Add(block);

                            core.RecalculateRigidbody(blocks);
                        }
                        generated = null;
                    }
                }
            }
            if (raycastHit.collider != null)
            {
                switch (selectedPrefab)
                {
                    case -1:
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
                        break;
                    default:
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
                        break;
                }
            }
        }
    }
    public Vector3 Align(GameObject preview, RaycastHit hit)
    {
        return hit.collider.transform.position + Vector3.Project(hit.point - hit.collider.transform.position, hit.normal) + hit.collider.transform.rotation * preview.transform.GetComponentInChildren<Collider>().ClosestPoint(hit.collider.transform.InverseTransformDirection(hit.normal));
    }
}
