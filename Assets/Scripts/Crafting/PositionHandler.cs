using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PositionHandler : MonoBehaviour
{
    public Camera cam;
    public void OnGUI()
    {
        Handles.BeginGUI();
        Handles.SetCamera(cam);
        transform.position = Handles.DoPositionHandle(transform.position, transform.rotation);
        Handles.EndGUI();
    }

    private void Update()
    {
        if(BuildFunction.selectedObj != gameObject || BuildFunction.selectedPrefab != -2)
        {
            Destroy(this);
        }
    }
}
