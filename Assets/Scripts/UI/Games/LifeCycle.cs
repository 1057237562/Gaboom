using Gaboom.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LifeCycle : MonoBehaviour
{
    public static bool gameStart = false;

    public static List<GameObject> gameObjects = new List<GameObject>();

    public List<string> physics = new List<string>();

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
            UnFreezePhysic();
            foreach (UnityEvent e in startEvent) { e.Invoke(); }
        }
        else
        {
            FreezePhysic();
            foreach (UnityEvent e in stopEvent) { e.Invoke(); }
        }
    }

    public void FreezePhysic()
    {
        for(int i =0; i < gameObjects.Count; i++) { 
            Destroy(gameObjects[i]);
            GameObject physic  = SLMechanic.DeserializeToGameObject(physics[i]);
            physic.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            gameObjects[i] = physic;
        }
    }

    public void UnFreezePhysic()
    {
        physics = new List<string>();
        for(int i = 0; i < gameObjects.Count; i++)
        {
            GameObject physic = gameObjects[i];
            physics.Add(SLMechanic.SerializeToXml(physic.GetComponent<PhysicCore>()));
            physic.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        FreezePhysic();
    }
}
