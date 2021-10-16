using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttributeObject : MonoBehaviour
{
    public object value;
    public ValueChangeStr vc_string;
    public ValueChangeInt vc_int;
    public ValueChangeSingle vc_single;
    public ValueChangeBool vc_bool;

    public void OnValueChanged()
    {
        vc_string.Invoke(value.ToString());
        int data; int.TryParse(value.ToString(),out data);
         vc_int.Invoke(data);
        float data2; float.TryParse(value.ToString(), out data2);
        vc_single.Invoke(data2);
        bool data3; bool.TryParse(value.ToString(), out data3);
        vc_bool.Invoke(data3);
    }

    public void ChangeValue(string v)
    {
        value = v;
    }

    public void ChangeValue(int v)
    {
        value = v;
    }

    public void ChangeValue(float v)
    {
        value = v;
    }

    public void ChangeValue(bool v)
    {
        value = v;
    }
}

[Serializable]
public class ValueChangeStr:UnityEvent<string>
{ }

[Serializable]
public class ValueChangeInt : UnityEvent<int>
{ }

[Serializable]
public class ValueChangeSingle : UnityEvent<float>
{ }

[Serializable]
public class ValueChangeBool : UnityEvent<bool>
{ }


