using Gaboom.Scene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Btn_LoadMap : MonoBehaviour
{
    public Text mapName;

    public void OnClick()
    {
        SceneMaterial.filepath = Application.dataPath + "/maps/" + mapName.text + ".gmap";
        SceneManager.LoadSceneAsync("GameScene");
    }
}
