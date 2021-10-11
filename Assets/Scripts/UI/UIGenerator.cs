using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{

    public List<Sprite> ui = new List<Sprite>();

    public GameObject model;
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        //Run the logic
        foreach(Sprite sprite in ui)
        {
            GameObject obj = Instantiate(model, transform);
            obj.GetComponent<Image>().sprite = sprite;
            int tag = i;
            obj.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { BuildFunction.selectedPrefab = tag; }));
            i++;
            obj.SetActive(true);
        }
        enabled = false;
    }
}
