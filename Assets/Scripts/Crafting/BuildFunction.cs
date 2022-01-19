using RTEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gaboom.Util;
public class BuildFunction : MonoSingletonBase<BuildFunction>
{
    public Material preview;
    public Material deny;

    public List<GameObject> prefabs;
    public List<GameObject> ignores;
    public GameObject keypanel;
    public int selectedPrefab { get; set; }
    GameObject generated;
    public bool align = true;
    public bool autoConnect = true;

    public void Toggle()
    {
        enabled = !enabled;
    }
    bool occupied = true;

    public PhysicCore Reattach(PhysicCore core)
    {
        if(core.deriveFrom != null)
        {
            PhysXInterface pxi = core.deriveFrom;
            pxi.Reattached();
            return Reattach(pxi.core);
        }
        else
        {
            return core;
        }
    }

    private void FixedUpdate()
    {
        if (generated != null)
        {
            occupied = generated.GetComponentInChildren<CollisionProbe>().isIntersect;
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
                    generated = Instantiate(prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                    generated.transform.position = Align(generated, raycastHit);
                    if (raycastHit.collider.transform.parent != null)
                        generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                }
                else
                {
                    generated = Instantiate(prefabs[selectedPrefab], raycastHit.point + prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
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
        if (!GameLogic.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            Physics.Raycast(ray, out raycastHit);
            if (selectedPrefab > -1)
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
                        if (align && !ignores.Contains(raycastHit.collider.gameObject))
                        {
                            Collider hitObj = raycastHit.collider;
                            generated = Instantiate(prefabs[selectedPrefab], Vector3.zero, Quaternion.identity);
                            generated.transform.position = Align(generated, raycastHit);
                            if(hitObj.transform.parent != null)
                                generated.transform.rotation = Quaternion.FromToRotation(hitObj.transform.parent.forward, generated.transform.position - hitObj.transform.parent.position) * hitObj.transform.parent.rotation;
                        }
                        else
                        {
                            generated = Instantiate(prefabs[selectedPrefab], raycastHit.point + prefabs[selectedPrefab].transform.lossyScale / 2, Quaternion.Euler(raycastHit.collider.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, raycastHit.collider.transform.rotation.eulerAngles.z));
                        }

                        if (raycastHit.collider.transform.parent != null)
                        {
                            IBlock block = generated.GetComponent<IBlock>();
                            PhysicCore parent = raycastHit.collider.transform.parent.parent.GetComponent<PhysicCore>();

                            //Building logic
                            if (block.GetType() == typeof(Engine))
                            {
                                Vector3 rpos = parent.transform.InverseTransformPoint(generated.transform.position);
                                if (rpos.x < 0)
                                {
                                    ((Engine)block).reverse = true;
                                }
                            }

                            parent = Reattach(parent);
                            generated.transform.parent = parent.transform;
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

                            PhysicCore core = parent.GetComponent<PhysicCore>();
                            LifeCycle.gameObjects.Add(parent);
                            List<IBlock> blocks = new List<IBlock>();

                            IBlock block = generated.GetComponent<IBlock>();
                            //block.mass = generated.GetComponent<Rigidbody>().mass;
                            //block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                            generated.transform.parent = parent.transform;
                            foreach (Collider child in generated.GetComponentsInChildren<Collider>())
                            {
                                child.isTrigger = false;
                            }
                            block.Load();
                            blocks.Add(block);

                            core.RecalculateRigidbody(blocks);
                            core.enabled = true;
                        }
                        generated = null;
                    }
                }
            }else if (raycastHit.collider != null)
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
                    case -7:
                        if (raycastHit.collider != null && raycastHit.collider.transform.parent != null)
                        {
                            IBlock block = raycastHit.collider.transform.parent.GetComponent<IBlock>();
                            block.Break();
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
