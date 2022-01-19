using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{

    public List<Sprite> ui = new List<Sprite>();
    public List<AllocateEvent> events = new List<AllocateEvent>();
    public int i = 0;
    public GameObject model;
    public bool positive = true;
    public UnityEvent<int> selection;
    // Start is called before the first frame update
    void Start()
    {
        //Run the logic
        foreach (Sprite sprite in ui)
        {
            GameObject obj = Instantiate(model, transform);
            obj.GetComponent<Image>().sprite = sprite;
            int tag = i;
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(new UnityAction(() => { if (!positive) { events[(-tag - 1) >= events.Count ? 0 : (-tag - 1)].Invoke(); } else { events[tag >= events.Count ? 0 : tag].Invoke(); } selection.Invoke(tag); }));
            if (positive)
            {
                i++;
            }
            else
            {
                i--;
            }
            obj.SetActive(true);
        }
        enabled = false;
    }
}

[Serializable]
public class AllocateEvent : UnityEvent
{

}
