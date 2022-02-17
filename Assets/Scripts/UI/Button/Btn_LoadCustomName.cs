using Gaboom.Scene;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Btn_LoadCustomName : MonoBehaviour
{
    public UnityEvent reloadEvent;

    public void LoadMap()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = Marshal.SizeOf(pth);
        pth.filter = "Map files(*.gmap)\0*.gmap";
        pth.file = new string(new char[1024]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[256]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath; //Ä¬ÈÏÂ·¾¶
        pth.title = "Open";
        pth.defExt = "gmap";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (OpenFileDialog.GetOpenFileName(pth))
        {
            string mapPath = Application.dataPath + "/maps";
            if (!Directory.Exists(mapPath))
            {
                Directory.CreateDirectory(mapPath);
            }
            File.Copy(pth.file, mapPath + "/" + Path.GetFileName(pth.file), true);
        }
        reloadEvent.Invoke();
    }
}
