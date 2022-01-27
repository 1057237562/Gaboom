using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMap : MonoBehaviour
{
    public Text mapname;
    public Text display;
    public NetworkController networkController;
    
    public void OnClick()
    {
        networkController.mapname = mapname.text;
        //networkController.OnMapChanged();
        display.text = mapname.text;
    }
}
