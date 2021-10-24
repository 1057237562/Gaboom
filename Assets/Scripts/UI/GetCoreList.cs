using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCoreList : MonoBehaviour
{
    public GameObject item;
    public GameObject list;

    private void OnEnable()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("PhysicCore");
        foreach(GameObject gameObject in objs)
        {
            GameObject listItem = Instantiate(item, list.transform);
        }
    }
}
