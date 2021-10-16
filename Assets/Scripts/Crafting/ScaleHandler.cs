using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScaleHandler : MonoBehaviour
{
    public Camera cam;
    public void OnGUI()
    {
        Handles.BeginGUI();
        Handles.SetCamera(cam);
        Vector3 vector = Handles.DoScaleHandle(transform.localScale, transform.position, transform.rotation, HandleUtility.GetHandleSize(transform.position));
        GetComponent<Rigidbody>().mass *= vector.x * vector.y * vector.z / (transform.localScale.x * transform.localScale.y * transform.localScale.z);
        transform.localScale = vector;
        Handles.EndGUI();
    }

    private void Update()
    {
        if (BuildFunction.selectedObj != gameObject || BuildFunction.selectedPrefab != -4)
        {
            Destroy(this);
        }
    }
}
