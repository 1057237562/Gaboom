using Gaboom.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadPanel : MonoBehaviour
{
    public GameObject newItem;
    public GameObject list;
    private void OnEnable()
    {
        foreach(Transform child in list.transform)
        {
            Destroy(child.gameObject);
        }
        if (Directory.Exists(SLMechanic.machineFolder))
        {
            foreach (string filename in Directory.GetFiles(SLMechanic.machineFolder, "*.gm", SearchOption.AllDirectories))
            {
                GameObject listItem = Instantiate(newItem, list.transform);
                Image img = listItem.GetComponent<Image>();
                Texture2D texture = new Texture2D(256, 256);

                FileStream fileStream = new FileStream(filename.Replace(".gm",".gsp"), FileMode.Open, FileAccess.Read);
                fileStream.Seek(0, SeekOrigin.Begin);
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, (int)fileStream.Length);
                fileStream.Close();
                fileStream.Dispose();

                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                img.sprite = sprite;

                Text ui = listItem.GetComponentInChildren<Text>();
                ui.text = filename.Replace(SLMechanic.machineFolder, "").Replace(".gm", "");
                string machineName = Path.GetFileName(filename);
                listItem.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { SLMechanic.LoadObjFromFile(machineName).name = machineName; }));
            }
        }
    }


}
