using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionProbe : MonoBehaviour
{
    public bool isIntersect = false;

    private void OnCollisionEnter(Collision collision)
    {
        isIntersect = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isIntersect = false;
    }
}
