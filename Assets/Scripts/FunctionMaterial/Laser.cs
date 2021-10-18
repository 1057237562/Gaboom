using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector3 firespot = new Vector3(0,0.5f,2.34f);
    public Vector3 direction = new Vector3(0,0,1);
    public PhysicMaterial glass;
    // Update is called once per frame
    void FireIteration()
    {
        Ray ray = new Ray(transform.InverseTransformPoint(firespot),transform.InverseTransformDirection(direction));
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
        if(raycastHit.collider.material == glass)
        {

        }
        else
        {

        }
    }
}
