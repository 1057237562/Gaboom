using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public GameObject shell;
    public float force = 100f;
    public Vector3 firespot;

    public void FireFunc()
    {
        GameObject n_s = Instantiate(shell, transform.TransformPoint(firespot), transform.rotation);
        n_s.transform.localScale = transform.localScale;
        n_s.GetComponent<Rigidbody>().mass *= transform.localScale.x * transform.localScale.y * transform.localScale.z;
        n_s.GetComponent<Rigidbody>().AddForce(-transform.forward * force * transform.localScale.x*transform.localScale.y*transform.localScale.z);
    }
}
