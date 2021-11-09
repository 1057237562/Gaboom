using Gaboom.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class PhysicCore : MonoBehaviour
{
    public Thread worker = new Thread(new ParameterizedThreadStart(ThreadWorker));

    public List<IBlock> GetBlocks()
    {
        return mring.blocks;
    }

    public void Load(List<IBlock> list)
    {
        mring.blocks = list;
        if (!worker.IsAlive)
        {
            worker.Start(this);
        }
        if (list.Count == 1)
        {
            enabled = false;
        }
    }

    public static void Solve(PhysicCore core, IBlock block, Vector3 force)
    {
        if (!openList.Contains(block))
        {
            openList.Add(block);
            for (int i = 0; i < block.connector.Count; i++)
            {
                IBlock m = block.connector[i];
                Vector3 collisionforce;
                core.collideEvent.TryGetValue(block, out collisionforce);
                Vector3 f = (force + collisionforce - core.acceleration * block.mass) / block.connector.Count;
                f *= IMath.Sigmoid(1 / Vector3.Dot(f.normalized, block.r_pos[i].normalized), 2 / (block.bouncy + m.bouncy));
                if (f.magnitude > block.breakForce + m.breakForce)
                {
                    block.connector.Remove(m);
                    m.connector.Remove(block);
                    core.dirty = true;
                    f = Vector3.zero;
                }
                if (!openList.Contains(m))
                {
                    Solve(core, m, f);
                }
            }
        }
    }

    public void setDirty()
    {
        dirty = true;
    }

    public static void ThreadWorker(object obj)
    {
        PhysicCore physicCore = (PhysicCore)obj;
        while (physicCore.run)
        {
            if (physicCore.collideEvent.Count != 0)
            {
                openList.Clear();
                physicCore.locked = true;
                try
                {
                    foreach (KeyValuePair<IBlock, Vector3> cevent in physicCore.collideEvent)
                    {
                        Solve(physicCore, cevent.Key, Vector3.zero);
                        break;
                    }
                }
                catch
                {

                }
                physicCore.locked = false;
                physicCore.collideEvent.Clear();
                //CalculateRelativeForce(openList);
            }
            if (physicCore.dirty)
            {
                physicCore.CheckBlock();
                physicCore.dirty = false;
            }
            Thread.Sleep(deltaT);
        }
    }
    public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2, out Vector3 vecProj)
    {
        Vector3 vec1 = point - linePoint1;
        Vector3 vec2 = linePoint2 - linePoint1;
        vecProj = Vector3.Project(vec1, vec2);
        float dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
        return dis;
    }

    public void AppendForce(IBlock block, Vector3 force)
    {
        if (!collideEvent.ContainsKey(block))
        {
            collideEvent.Add(block, transform.InverseTransformVector(force));
        }
        else
        {
            collideEvent[block] += transform.InverseTransformVector(force);
        }
    }

    public void CalculateAngularForce(Quaternion rotation)
    {
        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        foreach (IBlock block in mring.blocks)
        {
            Vector3 vecProj;
            float radius = DisPoint2Line(block.transform.position, transform.TransformPoint(GetComponent<Rigidbody>().centerOfMass), transform.TransformPoint(GetComponent<Rigidbody>().centerOfMass) + axis, out vecProj);
            float multiplier = radius * Mathf.Pow(Mathf.Deg2Rad * angle, 2);
            Vector3 force = (block.transform.position - transform.TransformPoint(GetComponent<Rigidbody>().centerOfMass) - vecProj).normalized * multiplier;
            //Debug.DrawLine(block.transform.position, block.transform.position + force * 200, Color.green);
            AppendForce(block, force);
        }
    }

    public void AppendIBlock(IBlock iblock)
    {
        if (mring.blocks.Contains(iblock)) return;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        float mass = rigidbody.mass + iblock.mass;
        rigidbody.centerOfMass *= rigidbody.mass;
        rigidbody.centerOfMass += (transform.InverseTransformPoint(iblock.transform.position + iblock.centerOfmass)) * iblock.mass;
        rigidbody.centerOfMass /= mass;
        rigidbody.mass = mass;

        mring.blocks.Add(iblock);
    }

    public void AppendRigidBody(GameObject gameObject)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        Rigidbody appendRigidbody = gameObject.GetComponent<Rigidbody>();

        float mass = rigidbody.mass + appendRigidbody.mass;
        rigidbody.centerOfMass *= rigidbody.mass;
        rigidbody.centerOfMass += (transform.InverseTransformPoint(appendRigidbody.transform.position + appendRigidbody.centerOfMass)) * appendRigidbody.mass;
        rigidbody.centerOfMass /= mass;
        rigidbody.mass = mass;

        Destroy(appendRigidbody);

        mring.blocks.Add(gameObject.GetComponent<IBlock>());
    }

    public void RemoveIBlock(IBlock iblock)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();

        float mass = rigidbody.mass - iblock.mass;
        rigidbody.centerOfMass *= rigidbody.mass;
        rigidbody.centerOfMass -= (transform.InverseTransformPoint(iblock.transform.position + iblock.centerOfmass)) * iblock.mass;
        rigidbody.centerOfMass /= mass;
        rigidbody.mass = mass;

        mring.blocks.Remove(iblock);
        if(mring.blocks.Count == 0)
        {
            Destroy(gameObject);
        }
    }

    public static HashSet<IBlock> openList = new HashSet<IBlock>();

    public Dictionary<IBlock, Vector3> collideEvent = new Dictionary<IBlock, Vector3>();
    public Vector3 acceleration;
    public Vector3 angular;
    public float sensitive = 5f;
    Vector3 lastV;
    bool run = true;
    public static int deltaT;
    [HideInInspector]
    public PhysXInterface deriveFrom;
    [HideInInspector]
    bool dirty = false;
    [HideInInspector]
    public Ring mring = new Ring();
    bool locked = false;

    private void Start()
    {
        deltaT = (int)(Time.fixedDeltaTime * 1000);
    }
    
    private void Update()
    {
        Debug.DrawLine(GetComponent<Rigidbody>().worldCenterOfMass, GetComponent<Rigidbody>().worldCenterOfMass + new Vector3(0, -1, 0));
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!locked)
        {
            Vector3 deltaV = GetComponent<Rigidbody>().velocity - lastV;
            acceleration = transform.InverseTransformVector(deltaV / Time.fixedDeltaTime);
            angular = GetComponent<Rigidbody>().angularVelocity;
            if (angular.magnitude > sensitive)
            {
                CalculateAngularForce(Quaternion.Euler(angular));
            }
            //Debug.Log(acceleration);
            lastV = GetComponent<Rigidbody>().velocity;
        }
        if (rings.Count > 1)
        {
            foreach (Ring ring in rings)
            {
                ReCombind(ring.blocks);
            }
            rings.Clear();
            Destroy(gameObject);
        }
    }

    List<Ring> rings = new List<Ring>();
    void CheckBlock()
    {
        rings.Clear();
        HashSet<IBlock> ol = new HashSet<IBlock>();
        foreach (IBlock block in mring.blocks)
        {
            if (!ol.Contains(block))
            {
                rings.Add(new Ring());
                HashSet<IBlock> blocks = new HashSet<IBlock>();
                blocks.Add(block);
                ol.Add(block);
                dfs(block, ref blocks,ref ol);
                rings[rings.Count - 1].blocks = blocks.ToList();
            }
        }
    }

    public static GameObject emptyGameObject;

    void ReCombind(List<IBlock> list)
    {
        GameObject newObj = Instantiate(emptyGameObject, transform.position, transform.rotation);
        Rigidbody rigidbody = newObj.GetComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        foreach (IBlock block in list)
        {
            block.transform.parent = newObj.transform;
            rigidbody.mass += block.mass;
            rigidbody.centerOfMass += (block.transform.localPosition + block.centerOfmass) * block.mass;
        }
        rigidbody.centerOfMass /= rigidbody.mass;
        PhysicCore physicCore = newObj.GetComponent<PhysicCore>();
        physicCore.Load(list);
    }

    public void RecalculateRigidbody(List<IBlock> list)
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        foreach (IBlock block in list)
        {
            block.transform.parent = transform;
            rigidbody.mass += block.mass;
            rigidbody.centerOfMass += (block.transform.localPosition + block.centerOfmass) * block.mass;
        }
        rigidbody.centerOfMass /= rigidbody.mass;
        Load(list);
    }

    public void RecalculateRigidbody()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        foreach (IBlock block in mring.blocks)
        {
            block.transform.parent = transform;
            rigidbody.mass += block.mass;
            rigidbody.centerOfMass += (block.transform.localPosition + block.centerOfmass) * block.mass;
        }
        rigidbody.centerOfMass /= rigidbody.mass;
    }

    void dfs(IBlock block, ref HashSet<IBlock> blocks, ref HashSet<IBlock> ol)
    {
        foreach (IBlock m in block.connector)
        {
            if(m == block)
            {
                continue;
            }
            if (!blocks.Contains(m))
            {
                blocks.Add(m);
                ol.Add(m);
                dfs(m, ref blocks, ref ol);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (enabled)
        {
            Vector3 deltaV = GetComponent<Rigidbody>().velocity - lastV;
            acceleration = transform.InverseTransformVector(deltaV / Time.fixedDeltaTime);
            AppendForce(collision.GetContact(0).thisCollider.transform.parent.GetComponent<IBlock>(), transform.InverseTransformVector(collision.impulse / Time.fixedDeltaTime));
            if (!worker.IsAlive)
            {
                worker.Start(this);
            }
        }
    }

    
}

public class Ring
{
    public List<IBlock> blocks = new List<IBlock>();
}
