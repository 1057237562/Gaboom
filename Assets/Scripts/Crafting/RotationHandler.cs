using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RotationHandler : MonoBehaviour
{
    public Camera cam;
    public void OnGUI()
    {
        Handles.BeginGUI();
        Handles.SetCamera(cam);
        transform.rotation = Handles.DoRotationHandle(transform.rotation, transform.position);
        Handles.EndGUI();
    }

    private void Update()
    {
        if(BuildFunction.selectedObj != gameObject || BuildFunction.selectedPrefab != -3)
        {
            Destroy(this);
        }
    }
}
