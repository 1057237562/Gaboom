using Gaboom.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadPanel : MonoBehaviour
{
    public GameObject newItem;
    public GameObject list;
    private void OnEnable()
    {
        foreach(string filename in Directory.GetFiles(SLMechanic.machineFolder,"*.gm",SearchOption.AllDirectories))
        {
            Instantiate(newItem, list.transform);
        }
    }


}
