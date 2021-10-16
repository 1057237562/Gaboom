using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyListener : MonoBehaviour
{
    public bool isListening;
    public UnityAction<KeyCode> variable;
    public Text ui;
    public Text funcname;

    public void StartListen()
    {
        isListening = true;
    }
    Event e;
    // Update is called once per frame
    void OnGUI()
    {
        e = Event.current;
        if (e != null && e.isKey && isListening)
        {
            variable.Invoke(e.keyCode);
            ui.text = e.keyCode.ToString();
            isListening = false;
        }
    }
}
