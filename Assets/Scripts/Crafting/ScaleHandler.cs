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
        transform.localScale = Handles.DoScaleHandle(transform.localScale, transform.position, transform.rotation, HandleUtility.GetHandleSize(transform.position));
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
