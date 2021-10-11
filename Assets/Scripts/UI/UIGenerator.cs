using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGenerator : MonoBehaviour
{

    public List<Sprite> ui = new List<Sprite>();
    public int i = 0;
    public GameObject model;
    public bool positive = true;
    // Start is called before the first frame update
    void Start()
    {
        //Run the logic
        foreach(Sprite sprite in ui)
        {
            GameObject obj = Instantiate(model, transform);
            obj.GetComponent<Image>().sprite = sprite;
            int tag = i;
            obj.GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(() => { BuildFunction.selectedPrefab = tag; }));
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
