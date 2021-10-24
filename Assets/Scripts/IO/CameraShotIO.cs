using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShotIO : MonoBehaviour
{
    public string saveFile;

    private void OnPostRender()
    {
        Save(saveFile, CreateFrom(GetComponent<Camera>().targetTexture));
        Destroy(gameObject);
    }

    public void Save(string path, Texture2D texture2D)
    {
        Debug.Log("Save Path:" + path);
        var bytes = texture2D.EncodeToPNG();
        //var bytes = texture2D.EncodeToJPG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public Texture2D CreateFrom(RenderTexture renderTexture)
    {
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        var previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        RenderTexture.active = previous;

        texture2D.Apply();

        return texture2D;
    }
}
