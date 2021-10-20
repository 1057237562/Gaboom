using RTEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableInitializer : MonoSingletonBase<VariableInitializer>
{
    public GameObject physicCore;
    // Start is called before the first frame update
    void Start()
    {
        PhysicCore.emptyGameObject = physicCore;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
