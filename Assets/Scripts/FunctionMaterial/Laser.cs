using Gaboom.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Vector3 firespot = new Vector3(0,0.125f,0.75f);
    public PhysicMaterial glass;
    public float range = 100f;
    // Update is called once per frame
    public void FireIteration()
    {
        Ray ray = new Ray(transform.TransformPoint(firespot),transform.forward);
        RaycastHit raycastHit;
        Physics.Raycast(ray, out raycastHit,range);
        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.material == glass)
            {
                ray = new Ray(raycastHit.point, Vector3.Reflect(transform.forward, raycastHit.normal));
                float leftRange = range - Vector3.Distance(ray.origin, raycastHit.point);
                Physics.Raycast(ray, out raycastHit, leftRange);
                DeltDamage(raycastHit.collider.transform.parent.GetComponent<IBlock>(), leftRange - Vector3.Distance(ray.origin, raycastHit.point));
            }
            else
            {
                DeltDamage(raycastHit.collider.transform.parent.GetComponent<IBlock>(), range - Vector3.Distance(ray.origin, raycastHit.point));
            }
        }
    }

    public void DeltDamage(IBlock obj , float leftrange)
    {
        float power = transform.localScale.x * transform.localScale.y * transform.localScale.z*5 + 2.5f;
        obj.health -= IMath.Sigmoid(leftrange/range,power) + power/2;
        obj.DoBreak();
    }
}
