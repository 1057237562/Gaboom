using Gaboom.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SLSystemDebugger : MonoBehaviour
{
    public PhysicCore target;
    public string temp;

    [ContextMenu("SaveToMemory")]
    public void SaveToMem()
    {
        temp = SLMechanic.SerializeToXml(target);
    }

    [ContextMenu("LoadFromMem")]
    public void LoadFromMem()
    {
        SLMechanic.DeserializeToGameObject(temp);
    }
}
