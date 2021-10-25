using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class CameraShotIO : MonoBehaviour
{
    public string saveFile;

    private void OnEnable()
    {
        RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;
    }

    private void RenderPipelineManager_endFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
    {
        OnPostRender();
    }

    private void OnDisable()
    {
        RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
    }

    private void OnPostRender()
    {
        if(saveFile.Length != 0) // use in quicksave
        {
            Save(saveFile, CreateFrom(GetComponent<Camera>().targetTexture));
            Destroy(gameObject);
        }
    }

    public static void Save(string path, Texture2D texture2D)
    {
        Debug.Log("Save Path:" + path);
        var bytes = texture2D.EncodeToPNG();
        //var bytes = texture2D.EncodeToJPG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    public static Texture2D CreateFrom(RenderTexture renderTexture)
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
