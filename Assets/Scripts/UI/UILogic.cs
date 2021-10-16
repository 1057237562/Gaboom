using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UILogic : MonoBehaviour
{
    public List<GameObject> uiElement;
    public int defaultSelect = 0;
    [HideInInspector]
    public int selectedIndex = 0;

    public UIEvent OnChange;
    private void OnEnable()
    {
        Select(defaultSelect);
    }

    public void Select(int index)
    {
        foreach(GameObject ui in uiElement)
        {
            ui.SetActive(false);
        }
        uiElement[index].SetActive(true);
        selectedIndex = index;
        OnChange.Invoke();
    }

    public void Next()
    {
        Select(selectedIndex + 1 == uiElement.Count ? 0 : selectedIndex + 1);
    }
}

[Serializable]
public class UIEvent : UnityEvent
{

}
