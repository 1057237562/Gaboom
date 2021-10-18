using RTEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBlock : MonoBehaviour, IRTEditorEventListener
{
    public Vector3 position;
    public Quaternion rotation;
    public List<IBlock> connector = new List<IBlock>();
    [HideInInspector]
    public List<Vector3> r_pos = new List<Vector3>();
    [HideInInspector]
    public PhysicCore core;

    public float breakForce = 100f;
    public float toughness = 10f;
    [Range(0, float.PositiveInfinity)]
    public float bouncy = 0.8f;
    public float health = 100f;
    public float mass;
    public Vector3 centerOfmass;

    float o_mass;

    [HideInInspector]
    public string alias;
    public void Load()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        foreach (IBlock block in connector)
        {
            //Debug.Log(gameObject.name);
            r_pos.Add(block.transform.localPosition - position);
        }
        core = transform.parent.GetComponent<PhysicCore>();
        alias = name;
        o_mass = GetComponent<Rigidbody>().mass;
    }

    public void ReloadRPos()
    {
        foreach (IBlock block in connector)
        {
            r_pos.Add(block.transform.localPosition - position);
        }
    }

    public void DoBreak()
    {
        if (health <= 0)
        {
            foreach (IBlock block in connector)
            {
                block.connector.Remove(this);
            }
        }
    }

    void IRTEditorEventListener.OnAlteredByTransformGizmo(Gizmo gizmo)
    {
        mass = o_mass * transform.localScale.x * transform.localScale.y * transform.localScale.z;
        if(core == null)
            core = transform.parent.GetComponent<PhysicCore>();
        core.RecalculateRigidbody();
        ReloadRPos();
    }

    bool IRTEditorEventListener.OnCanBeSelected(ObjectSelectEventArgs selectEventArgs)
    {
        return true;
    }

    void IRTEditorEventListener.OnDeselected(ObjectDeselectEventArgs deselectEventArgs)
    {

    }

    void IRTEditorEventListener.OnSelected(ObjectSelectEventArgs selectEventArgs)
    {

    }
}
