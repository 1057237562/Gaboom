using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabPanel : MonoBehaviour
{
    public List<string> tabs;
    public GameObject item;
    public UILogic controller;
    public int index = 1;
    // Start is called before the first frame update
    void Start()
    {
        foreach(string tab in tabs)
        {
            GameObject m_item = Instantiate(item, transform);
            Text text = m_item.GetComponentInChildren<Text>();
            text.text = tab;
            int temp = index;
            m_item.GetComponent<Button>().onClick.AddListener(new UnityAction(() => { controller.Select(temp); }));
            index++;
        }
        Destroy(this);
    }
}
