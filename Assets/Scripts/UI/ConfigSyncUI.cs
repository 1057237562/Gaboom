using Gaboom.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class ConfigSyncUI : MonoBehaviour
{
    public string configName;
    public List<GameObject> ui;
    // Start is called before the first frame update
    public void ApplyConfig()
    {
        List<Reflection> reflections = new List<Reflection>();
        for(int i = 0; i < ui.Count;i++)
        {
            GameObject obj = ui[i];
            reflections.Add(new Reflection(i, obj.GetComponentInChildren<Attribute>().value));
        }
        FileSystem.WriteConfigFile(configName,reflections);
    }

    public void ReadConfig()
    {
        List<Reflection> reflections = FileSystem.ReadConfigFile(configName);
        foreach(Reflection refl in reflections)
        {
            Attribute attr = ui[int.Parse(refl.key.ToString())].GetComponentInChildren<Attribute>();
            attr.value = refl.value;
            attr.OnValueChanged();
        }
    }
}

[Serializable]
public class Reflection
{
    public object key;
    public object value;
    public Reflection(object key,object value)
    {
        this.key = key;
        this.value = value;
    }
}
