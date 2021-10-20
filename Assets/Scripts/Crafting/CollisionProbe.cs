using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProbe : MonoBehaviour
{
    public bool isIntersect = false;

    private void OnTriggerEnter(Collider other)
    {
        isIntersect = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isIntersect = false;
    }
}
