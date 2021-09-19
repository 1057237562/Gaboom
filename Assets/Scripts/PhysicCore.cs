using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PhysicCore : MonoBehaviour
{
    public Thread worker = new Thread(new ParameterizedThreadStart(ThreadWorker));

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
        openList.Add(block);
        for (int i = 0; i < block.connector.Count; i++)
        {
            IBlock m = block.connector[i];
            Vector3 f = (force - core.acceleration * block.mass) / Vector3.Dot(force.normalized, block.r_pos[i]); // The function that calculate force taken
            if (f.magnitude > block.breakForce + m.breakForce)
            {
                block.connector.Remove(m);
                m.connector.Remove(block);
                core.dirty = true;
            }
            if (!openList.Contains(m))
            {
                Solve(core, m, f);
            }
        }
    }

    public static void ThreadWorker(object obj)
    {
        PhysicCore physicCore = (PhysicCore)obj;
        while (physicCore.run)
        {
            if (physicCore.collideEvent.Count != 0)
            {
                openList.Clear();
                foreach (KeyValuePair<IBlock, Vector3> cevent in physicCore.collideEvent)
                {
                    Solve(physicCore, cevent.Key, cevent.Value);
                }
                physicCore.collideEvent.Clear();
                CalculateRelativeForce(openList);
            }
            if (physicCore.dirty)
            {
                physicCore.CheckBlock();
                physicCore.dirty = false;
            }
            Thread.Sleep(deltaT);
        }
    }

    public static void CalculateRelativeForce(List<IBlock> blocks)
    {

    }

    public static List<IBlock> openList = new List<IBlock>();

    public Dictionary<IBlock, Vector3> collideEvent = new Dictionary<IBlock, Vector3>();
    public Vector3 acceleration;
    Vector3 lastV;
    bool run = true;
    public static int deltaT;
    [HideInInspector]
    public bool dirty = false;
    public Ring mring = new Ring();

    private void Start()
    {
        deltaT = (int)(Time.fixedDeltaTime * 1000);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 deltaV = GetComponent<Rigidbody>().velocity - lastV;
        acceleration = deltaV / Time.fixedDeltaTime;
        lastV = GetComponent<Rigidbody>().velocity;
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
        List<IBlock> openList = new List<IBlock>();
        foreach (IBlock block in mring.blocks)
        {
            if (!openList.Contains(block))
            {
                rings.Add(new Ring());
                List<IBlock> blocks = new List<IBlock>();
                blocks.Add(block);
                openList.Add(block);
                dfs(block, ref blocks, ref openList);
                rings[rings.Count - 1].blocks = blocks;
            }
        }
    }

    public static GameObject emptyGameObject;

    void ReCombind(List<IBlock> list)
    {
        GameObject newObj = Instantiate(emptyGameObject,transform.position,transform.rotation);
        Rigidbody rigidbody = newObj.GetComponent<Rigidbody>();
        rigidbody.mass = 0;
        rigidbody.centerOfMass = Vector3.zero;
        foreach (IBlock block in list)
        {
            block.transform.parent = newObj.transform;

            rigidbody.mass += block.mass;
            rigidbody.centerOfMass += (block.position + block.centerOfmass) * block.mass;
        }
        rigidbody.centerOfMass /= rigidbody.mass;
        PhysicCore physicCore = newObj.GetComponent<PhysicCore>();
        physicCore.Load(list);
    }

    void dfs(IBlock block, ref List<IBlock> blocks, ref List<IBlock> openList)
    {
        foreach (IBlock m in block.connector)
        {
            if (!blocks.Contains(m))
            {
                blocks.Add(m);
                openList.Add(m);
                dfs(m, ref blocks, ref openList);
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(enabled)
            collideEvent.Add(collision.GetContact(0).thisCollider.GetComponent<IBlock>(), collision.impulse / Time.fixedDeltaTime);
    }
}

public class Ring
{
    public List<IBlock> blocks = new List<IBlock>();
}
