using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LifeCycle : MonoBehaviour
{
    public bool gameStart = false;

    public List<UnityEvent> startEvent;
    public List<UnityEvent> stopEvent;
    public void setGameState(bool isStart)
    {
        gameStart = isStart;
        ValidateGameState();
    }

    void ValidateGameState()
    {
        if (gameStart)
        {
            Time.timeScale = 1;
            foreach (UnityEvent e in startEvent) { e.Invoke(); }
        }
        else
        {
            Time.timeScale = 0;
            foreach (UnityEvent e in stopEvent) { e.Invoke(); }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0;
    }
}
