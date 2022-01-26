using Gaboom.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GetCoreList : MonoBehaviour
{
    public GameObject item;
    public GameObject list;

    public MachineButton selected;
    public InputField machinName;
    public RenderTexture render;

    private void OnEnable()
    {
        foreach (Transform child in list.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PhysicCore");
        foreach (GameObject gameObject in objs)
        {
            ConfigurableJoint cjoint = gameObject.GetComponent<ConfigurableJoint>();
            if (cjoint != null)
            {
                cjoint.connectedBody.GetComponent<PhysXInterface>().Reattached();
                continue;
            }
            GameObject listItem = Instantiate(item, list.transform);
            MachineButton mb = listItem.GetComponent<MachineButton>();
            listItem.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { selected = mb; }));
            mb.SetTarget(gameObject.GetComponent<PhysicCore>());
        }
    }

    public void SaveMachine()
    {
        if (!Directory.Exists(SLMechanic.machineFolder))
        {
            Directory.CreateDirectory(SLMechanic.machineFolder);
        }
        if (machinName.text.Length == 0)
        {
            int i = 0;
            while (File.Exists(SLMechanic.machineFolder + "Untitled"+ (i != 0 ? "(" + i + ")" : "") + ".gsp"))
            {
                i++;
            }
            machinName.text = "Untitled" + (i != 0 ? "(" + i + ")" : "");
        }
        RenderPreviewImage.SaveRenderTextureToPNG(render, SLMechanic.machineFolder + machinName.text + ".gsp");
        SLMechanic.SaveObjToFile(selected.target, machinName.text);
    }
}
