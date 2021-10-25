using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPreview
{
    public static GameObject GetShot(PhysicCore coreObj)
    {
        GameObject shotCam = Object.Instantiate(VariableInitializer.Instance.camPrefab,coreObj.transform.position + coreObj.transform.InverseTransformPoint(new Vector3(4,4,0) * Mathf.Pow(coreObj.mring.blocks.Count,1/3f)),Quaternion.identity,coreObj.transform);
        shotCam.transform.LookAt(coreObj.transform);
        return shotCam;
    }
}
