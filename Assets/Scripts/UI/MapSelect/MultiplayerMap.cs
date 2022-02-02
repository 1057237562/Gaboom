using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMap : MonoBehaviour
{
    public Text mapname;
    public Text display;
    public Image thumbnail;
    public NetworkController networkController;
    
    public void OnClick()
    {
        networkController.mapname = mapname.text;
        networkController.OnMapChanged();
        display.text = mapname.text;
        string filepath = Application.dataPath + "/maps/"+Path.GetFileNameWithoutExtension(mapname.text) + "_thumbnail.png";
        thumbnail.sprite = Sprite.Create(RenderPreviewImage.GetTexrture2DFromPath(filepath),new Rect(0,0,512,512),new Vector2(0.5f,0.5f));
    }
}
