using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformVector : MonoBehaviour
{
    public Vector3 target;
    // Start is called before the first frame update
    void Update()
    {
        Debug.Log(transform.InverseTransformVector(target) + ":"+ transform.InverseTransformVector(target).magnitude);
    }
}
