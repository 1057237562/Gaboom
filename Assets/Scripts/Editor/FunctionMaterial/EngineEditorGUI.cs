using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Engine))]
public class EngineEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Engine engine = (Engine)target;
        if (GUILayout.Button("Dispatch"))
        {
            engine.Dispatch();
        }
        if (GUILayout.Button("Reattached"))
        {
            engine.Reattached();
        }
        if (GUILayout.Button("Rotate"))
        {
            engine.Rotate();
        }
    }
}
