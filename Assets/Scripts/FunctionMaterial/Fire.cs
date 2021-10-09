using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public GameObject shell;
    public float force = 100f;
    public Vector3 firespot;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameObject n_s = Instantiate(shell,transform.position + firespot,transform.rotation);
            n_s.GetComponent<Rigidbody>().AddForce(transform.forward * force);
        }
    }
}
