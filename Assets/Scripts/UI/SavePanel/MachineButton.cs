using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineButton : MonoBehaviour
{
    public PhysicCore target;
    public Text ui;
    [HideInInspector]
    public GameObject cam;

    public void SetTarget(PhysicCore t)
    {
        target = t;
        ui.text = target.name;
    }

    private void OnDisable()
    {
        if (cam != null)
        {
            Destroy(cam);
        }
    }

    public void OnClick()
    {
        foreach (Transform subBtn in transform.parent)
        {
            GameObject cam = subBtn.GetComponent<MachineButton>().cam;
            if(cam != null)
            {
                Destroy(cam);
            }
            subBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(500f,30f);
        }
        cam = GetPreview.GetShot(target);
        GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 150f);
    }
}
