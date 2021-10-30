using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollisionProbe : MonoBehaviour
{
    public bool isIntersect = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.transform.position - transform.position).magnitude < (other.transform.localScale.magnitude + transform.localScale.magnitude) / 4)
        {
            isIntersect = true;
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if ((other.transform.position - transform.position).magnitude < (other.transform.localScale.magnitude + transform.localScale.magnitude) / 4)
        {
            isIntersect = true;
        }
    }
}
