using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class KeyPanel : MonoBehaviour
{
    public void Close()
    {
        gameObject.SetActive(false);
        foreach(KeyFunction function in funcs)
        {
            function.enabled = true;
        }
    }

    KeyFunction[] funcs;

    public void CreateItem(KeyFunction[] functions)
    {
        funcs = functions;
        foreach(Transform child in list)
        {
            Destroy(child.gameObject);
        }
        foreach(KeyFunction function in functions)
        {
            function.enabled = false;
            GameObject n_item = Instantiate(keyItem, list);
            KeyListener listener = n_item.GetComponent<KeyListener>();
            listener.ui.text = function.keycode.ToString();
            listener.funcname.text = function.func_name;
            listener.variable = new UnityAction<KeyCode>((x)=> { function.keycode = x; });
            n_item.SetActive(true);
        }
    }

    public Transform list;
    public GameObject keyItem;
    Vector3 point;
    bool drag = false;
    public RectTransform[] ignore;
    public Text objname;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransform rect = GetComponent<RectTransform>();
            
            if (rect.rect.Contains(transform.InverseTransformPoint(Input.mousePosition)))
            {
                point = Input.mousePosition - rect.position;
                drag = true;
                foreach (RectTransform rt in ignore)
                {
                    if (rt.rect.Contains(rt.InverseTransformPoint(Input.mousePosition)))
                    {
                        drag = false;
                        break;
                    }
                }
            }
        }
        if (drag && Input.GetMouseButton(0))
        {
            transform.position = Input.mousePosition - point;
        }
        if (Input.GetMouseButtonUp(0))
        {
            drag = false;
        }
    }
}
