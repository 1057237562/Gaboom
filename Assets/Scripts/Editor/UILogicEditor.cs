using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILogic))]
public class UILogicEditor : Editor
{
    int index = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //DrawDefaultInspector();

        UILogic logic = (UILogic)target;
        if(logic.uiElement == null || logic.uiElement.Count == 0)
        {
            return;
        }
        List<string> names = new List<string>();
        foreach(GameObject obj in logic.uiElement)
        {
            names.Add(obj.name);
        }
        index = GUILayout.SelectionGrid(logic.selectedIndex, names.ToArray(),1,GUILayout.ExpandHeight(true));
        logic.Select(index);
    }
}
