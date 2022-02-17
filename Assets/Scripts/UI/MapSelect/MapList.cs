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
        if (!Directory.Exists(mapPath))
        {
            Directory.CreateDirectory(mapPath);
        }
        foreach (string filename in Directory.GetFiles(mapPath,"*.gmap")){
            GameObject n_item = Instantiate(listItem,viewport.transform);
            n_item.SetActive(true);
            n_item.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(filename);
            if(n_item.GetComponentsInChildren<Image>().Length >= 2)
                n_item.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create(RenderPreviewImage.GetTexrture2DFromPath(mapPath + "/" + Path.GetFileNameWithoutExtension(filename) + "_thumbnail.png"), new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
        }
    }

    public void Reload()
    {
        foreach(Transform child in viewport.transform)
        {
            if(child.gameObject.activeSelf)
                Destroy(child.gameObject);
        }
        string mapPath = Application.dataPath + "/maps";
        if (!Directory.Exists(mapPath))
        {
            Directory.CreateDirectory(mapPath);
        }
        foreach (string filename in Directory.GetFiles(mapPath, "*.gmap"))
        {
            GameObject n_item = Instantiate(listItem, viewport.transform);
            n_item.SetActive(true);
            n_item.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(filename);
            if (n_item.GetComponentsInChildren<Image>().Length >= 2)
                n_item.GetComponentsInChildren<Image>()[1].sprite = Sprite.Create(RenderPreviewImage.GetTexrture2DFromPath(mapPath + "/" + Path.GetFileNameWithoutExtension(filename) + "_thumbnail.png"), new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
        }
    }
}
