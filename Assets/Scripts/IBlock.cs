using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBlock : MonoBehaviour
{
    public Vector3 position;
    public Quaternion rotation;
    public List<IBlock> connector = new List<IBlock>();
    [HideInInspector]
    public List<Vector3> r_pos = new List<Vector3>();
    [HideInInspector]
    public PhysicCore core;

    public float breakForce = 200f;
    public float toughness = 10f;

    public float mass;
    public Vector3 centerOfmass;
    public void Load()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        foreach(IBlock block in connector)
        {
            //Debug.Log(gameObject.name);
            r_pos.Add(block.transform.localPosition - position);
        }
        core = transform.parent.GetComponent<PhysicCore>();
    }
}
