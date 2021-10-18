using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector3 firespot;
    public Vector3 direction;
    // Update is called once per frame
    void FireIteration()
    {
        Ray ray = new Ray(firespot,direction);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit);
    }
}
