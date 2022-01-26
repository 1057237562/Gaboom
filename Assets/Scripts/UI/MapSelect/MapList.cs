using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapList : MonoBehaviour
{
    public GameObject listItem;
    public GameObject viewport;

    // Start is called before the first frame update
    void Start()
    {
        string mapPath = Application.dataPath + "/maps";
        foreach(string filename in Directory.GetFiles(mapPath)){
            GameObject n_item = Instantiate(listItem,viewport.transform);
            n_item.GetComponentInChildren<Text>().name = filename;
        }
    }
}
