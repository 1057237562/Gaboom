using Gaboom.Util;
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

    public static void ThreadWorker(object obj)
    {
        PhysicCore physicCore = (PhysicCore)obj;
        while (physicCore.run)
        {
            if (physicCore.collideEvent.Count != 0)
            {
                openList.Clear();
                physicCore.locked = true;
                foreach (KeyValuePair<IBlock, Vector3> cevent in physicCore.collideEvent)
                {
                    Solve(physicCore, cevent.Key, Vector3.zero);
                    break;
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

    public void CalculateAngularForce(Vector3 angularVelocity)
    {
        foreach(IBlock block in mring.blocks)
        {
            Vector3 dis = block.transform.position - transform.position;
            Vector3 force = new Vector3(0,dis.z,dis.y).normalized * angularVelocity.x * new Vector2(dis.y, dis.z).magnitude;
            force += new Vector3(dis.z, 0, dis.x).normalized * angularVelocity.y * new Vector2(dis.x, dis.z).magnitude;
            force += new Vector3(dis.y, dis.x, 0).normalized * angularVelocity.z * new Vector2(dis.x, dis.y).magnitude;
            Debug.DrawLine(block.transform.position,block.transform.position + force,Color.green);
            try
            {
                collideEvent.Add(block, transform.InverseTransformVector(force));
            }
            catch
            {

            }
            
        }
    }

    public static List<IBlock> openList = new List<IBlock>();

    public Dictionary<IBlock, Vector3> collideEvent = new Dictionary<IBlock, Vector3>();
    public Vector3 acceleration;
    public Vector3 angular;
    public float sensitive = 5f;
    Vector3 lastV;
    bool run = true;
    public static int deltaT;
    [HideInInspector]
    bool dirty = false;
    Ring mring = new Ring();
    bool locked = false;

    private void Start()
    {
        deltaT = (int)(Time.fixedDeltaTime * 1000);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!locked)
        {
            Vector3 deltaV = GetComponent<Rigidbody>().velocity - lastV;
            acceleration = transform.InverseTransformVector(deltaV / Time.fixedDeltaTime);
            angular = GetComponent<Rigidbody>().angularVelocity;
            if(angular.magnitude > sensitive)
            {
                CalculateAngularForce(angular);
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
        GameObject newObj = Instantiate(emptyGameObject, transform.position, transform.rotation);
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
        if (enabled)
        {
            Vector3 deltaV = GetComponent<Rigidbody>().velocity - lastV;
            acceleration = transform.InverseTransformVector(deltaV / Time.fixedDeltaTime);
            collideEvent.Add(collision.GetContact(0).thisCollider.GetComponent<IBlock>(), transform.InverseTransformVector(collision.impulse / Time.fixedDeltaTime));
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
