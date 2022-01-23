using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class RenderPreviewImage
{
    public static RenderTexture GetAssetPreview(GameObject clone)
    {
        GameObject canvas_obj = null;
        Transform cloneTransform = clone.transform;
        bool isUINode = false;
        if (cloneTransform is RectTransform)
        {
            canvas_obj = new GameObject("render canvas", typeof(Canvas));
            Canvas canvas = canvas_obj.GetComponent<Canvas>();
            cloneTransform.parent = canvas_obj.transform;
            cloneTransform.localPosition = Vector3.zero;

            canvas_obj.transform.position = new Vector3(-1000, -1000, -1000);
            canvas_obj.layer = 21;
            isUINode = true;
        }
        else
            cloneTransform.position = new Vector3(-1000, -1000, -1000);

        Transform[] all = clone.GetComponentsInChildren<Transform>();
        foreach (Transform trans in all)
        {
            trans.gameObject.layer = 21;
        }

        Bounds bounds = GetBounds(clone);
        Vector3 Min = bounds.min;
        Vector3 Max = bounds.max;
        GameObject cameraObj = Object.Instantiate(VariableInitializer.Instance.camPrefab);
        cameraObj.layer = 21;
        Camera renderCamera = cameraObj.GetComponent<Camera>();

        renderCamera.backgroundColor = new Color(255f, 255f, 255f, 0f);
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.cullingMask = 1 << 21;
        if (isUINode)
        {
            cameraObj.transform.position = new Vector3((Max.x + Min.x) / 2f, (Max.y + Min.y) / 2f, cloneTransform.position.z - 100);
            Vector3 center = new Vector3(cloneTransform.position.x, (Max.y + Min.y) / 2f, cloneTransform.position.z);
            cameraObj.transform.LookAt(center);
            renderCamera.orthographic = true;
            float width = Max.x - Min.x;
            float height = Max.y - Min.y;
            float max_camera_size = width > height ? width : height;
            renderCamera.orthographicSize = max_camera_size / 2;
        }
        else
        {
            cameraObj.transform.position = new Vector3((Max.x + Min.x) / 2f, (Max.y + Min.y)/2 + (Max.y-Min.y)*2, Max.z + (Max.z - Min.z));
            Vector3 center = new Vector3(cloneTransform.position.x, (Max.y + Min.y) / 2f, cloneTransform.position.z);
            cameraObj.transform.LookAt(center);

            //int angle = (int)(Mathf.Atan2((Max.y - Min.y) / 2, (Max.z - Min.z)) * 180 / 3.1415f * 2);
            renderCamera.fieldOfView = 60;
        }

        if (cameraObj.GetComponent<HDAdditionalCameraData>() != null)
        {
            cameraObj.GetComponent<HDAdditionalCameraData>().backgroundColorHDR = new Color(1, 1, 1, 0f);
            cameraObj.GetComponent<HDAdditionalCameraData>().clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            cameraObj.GetComponent<HDAdditionalCameraData>().volumeLayerMask = 1 << 21;
        }
        RenderTexture texture = new RenderTexture(512, 512, 32, RenderTextureFormat.ARGB32);
        texture.antiAliasing = 4;
        renderCamera.targetTexture = texture;
        
        renderCamera.RenderDontRestore();
        RenderTexture tex = new RenderTexture(512, 512, 32, RenderTextureFormat.ARGB32);
        Graphics.Blit(texture, tex);

        Object.DestroyImmediate(canvas_obj);
        Object.DestroyImmediate(cameraObj);
        Object.DestroyImmediate(clone);
        return tex;
    }

    public static Texture2D TextureToTexture2D(Texture texture)
    {
        Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture2D;
    }

    public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
    {
        Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;

        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT;

        return texture2D;
    }


    public static Bounds GetBounds(GameObject obj)
    {
        Vector3 Min = new Vector3(99999, 99999, 99999);
        Vector3 Max = new Vector3(-99999, -99999, -99999);
        MeshRenderer[] renders = obj.GetComponentsInChildren<MeshRenderer>();
        if (renders.Length > 0)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                if (renders[i].bounds.min.x < Min.x)
                    Min.x = renders[i].bounds.min.x;
                if (renders[i].bounds.min.y < Min.y)
                    Min.y = renders[i].bounds.min.y;
                if (renders[i].bounds.min.z < Min.z)
                    Min.z = renders[i].bounds.min.z;

                if (renders[i].bounds.max.x > Max.x)
                    Max.x = renders[i].bounds.max.x;
                if (renders[i].bounds.max.y > Max.y)
                    Max.y = renders[i].bounds.max.y;
                if (renders[i].bounds.max.z > Max.z)
                    Max.z = renders[i].bounds.max.z;
            }
        }
        else
        {
            RectTransform[] rectTrans = obj.GetComponentsInChildren<RectTransform>();
            Vector3[] corner = new Vector3[4];
            for (int i = 0; i < rectTrans.Length; i++)
            {
                rectTrans[i].GetWorldCorners(corner);
                if (corner[0].x < Min.x)
                    Min.x = corner[0].x;
                if (corner[0].y < Min.y)
                    Min.y = corner[0].y;
                if (corner[0].z < Min.z)
                    Min.z = corner[0].z;

                if (corner[2].x > Max.x)
                    Max.x = corner[2].x;
                if (corner[2].y > Max.y)
                    Max.y = corner[2].y;
                if (corner[2].z > Max.z)
                    Max.z = corner[2].z;
            }
        }

        Vector3 center = (Min + Max) / 2;
        Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
        return new Bounds(center, size);
    }

    public static bool SaveTextureToPNG(Texture inputTex, string save_file_name)
    {
        RenderTexture temp = RenderTexture.GetTemporary(inputTex.width, inputTex.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(inputTex, temp);
        bool ret = SaveRenderTextureToPNG(temp, save_file_name);
        RenderTexture.ReleaseTemporary(temp);
        return ret;
    }

    public static bool SaveRenderTextureToPNG(RenderTexture rt, string save_file_name)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        string directory = Path.GetDirectoryName(save_file_name);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        FileStream file = File.Open(save_file_name, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Object.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
        return true;
    }
}
