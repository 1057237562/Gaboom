using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildFunction : MonoBehaviour
{
    public Material preview;
    public Material deny;

    public List<GameObject> prefabs;
    public List<GameObject> ignores;
    public static int selectedPrefab;
    GameObject generated;
    public bool align = true;
    public bool autoConnect = true;

    private void Update()
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        if (generated != null)
        {
            Destroy(generated);
        }
        if (selectedPrefab > -1)
        {
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
                    if(raycastHit.collider.transform.parent != null)
                    {
                        IBlock block = generated.AddComponent<IBlock>();
                        generated.transform.parent = raycastHit.collider.transform.parent;
                        IBlock relativeBlock = raycastHit.collider.GetComponent<IBlock>();
                        relativeBlock.connector.Add(block);
                        block.connector.Add(raycastHit.collider.GetComponent<IBlock>());
                        block.mass = generated.GetComponent<Rigidbody>().mass;
                        block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                        block.Load();
                        relativeBlock.ReloadRPos();
                        block.core.AppendRigidBody(generated);
                        foreach(Transform child in generated.transform)
                        {
                            child.GetComponent<Collider>().isTrigger = false;
                        }
                        generated.GetComponent<Collider>().isTrigger = false;
                    }
                    else
                    {
                        GameObject parent = Instantiate(PhysicCore.emptyGameObject, raycastHit.point, Quaternion.identity);
                        generated.transform.parent = parent.transform;

                        PhysicCore core = parent.GetComponent<PhysicCore>();

                        List<IBlock> blocks = new List<IBlock>();

                        IBlock block = generated.AddComponent<IBlock>();
                        block.mass = generated.GetComponent<Rigidbody>().mass;
                        block.centerOfmass = generated.GetComponent<Rigidbody>().centerOfMass;
                        block.Load();
                        blocks.Add(block);

                        core.RecalculateRigidbody(blocks);
                    }
                    generated = null;
                }
                else
                {

                    foreach (Transform child in generated.transform)
                    {
                        child.GetComponent<MeshRenderer>().material = preview;
                    }
                    generated.layer = LayerMask.NameToLayer("Ignore Raycast");
                    generated.GetComponent<MeshRenderer>().material = preview;
                }
            }
        }
    }
    public Vector3 Align(Vector3 normal, GameObject preview, GameObject hitObj)
    {
        return hitObj.transform.position + normal * (hitObj.transform.lossyScale.x + preview.transform.lossyScale.x) / 2;
    }
}
