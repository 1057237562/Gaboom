using Gaboom.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyFunction : MonoBehaviour
{
    public string func_name;
    public KeyCode keycode;
    public KeyPressed action = new KeyPressed();

    public int pattern = 0;
    // Update is called once per frame
    void Update()
    {
        if (GameLogic.inputingKey) return;
        if (Input.GetKeyDown(keycode) && pattern == 0)
        {
            action.Invoke();
        }
        if (Input.GetKey(keycode) && pattern == 1)
        {
            action.Invoke();
        }
    }
}

[Serializable]
public class KeyPressed:UnityEvent
{

}
