using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float radius = 5f;
    public float force = 1000f;
    public float invokeTime = 3.5f;
    public float multiplier = 1;
    bool trigger = false;
    // Start is called before the first frame update
    void Start()
    {
        //Invoke("Explode", invokeTime);
    }

    public void Explode()
    {
        if (trigger)
        {
            return;
        }
        else
        {
            trigger = true;
        }
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in colliders)
        {
            if (hit.GetComponent<Rigidbody>())
            {
                hit.GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius);
            }
        }

        var systems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem system in systems)
        {
            ParticleSystem.MainModule mainModule = system.main;
            mainModule.startSizeMultiplier *= multiplier;
            mainModule.startSpeedMultiplier *= multiplier;
            mainModule.startLifetimeMultiplier *= Mathf.Lerp(multiplier, 1, 0.5f);
            system.Clear();
            system.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
