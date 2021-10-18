using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector3 firespot = new Vector3(0,0.125f,0.75f);
    public PhysicMaterial glass;
    // Update is called once per frame
    public void FireIteration()
    {
        Ray ray = new Ray(transform.TransformPoint(firespot),transform.forward);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.material == glass)
            {

            }
            else
            {

            }
        }
    }

    private void Update()
    {
        Debug.Log(transform.TransformPoint(firespot));
        Debug.DrawRay(transform.TransformPoint(firespot), transform.forward);
    }
}
