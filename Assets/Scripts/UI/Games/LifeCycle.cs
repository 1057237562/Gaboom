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
    public List<GameObject> buildingPanel = new List<GameObject>();
    public UnityEvent restoreBuildState;

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
            Play();
            foreach (UnityEvent e in startEvent) { e.Invoke(); }
        }
        else
        {
            Pause();
            foreach (UnityEvent e in stopEvent) { e.Invoke(); }
        }
    }

    public void Pause()
    {
        for(int i =0; i < gameObjects.Count; i++) { 
            Destroy(gameObjects[i]);
            GameObject physic  = SLMechanic.DeserializeToGameObject(physics[i]);
            physic.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            gameObjects[i] = physic;
        }
        foreach(GameObject panel in buildingPanel)
        {
            panel.SetActive(true);
        }
    }

    public void Play()
    {
        physics = new List<string>();
        gameObjects.RemoveAll(x => x == null);
        for(int i = 0; i < gameObjects.Count; i++)
        {
            GameObject physic = gameObjects[i];
            physics.Add(SLMechanic.SerializeToXml(physic.GetComponent<PhysicCore>()));
            physic.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
        foreach(GameObject panel in buildingPanel)
        {
            panel.SetActive(false);
        }
        restoreBuildState.Invoke();
        BuildFunction.Instance.selectedPrefab = -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildFunction.Instance.selectedPrefab = -1;
        Pause();
    }
}
