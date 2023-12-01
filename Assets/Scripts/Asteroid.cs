using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    float health = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Planet p = collision.transform.GetComponent<Planet>();

        if (p)
        {
            if(p.fision)
            {
                p.health += health;
            }
            else
            {
                p.health -= health;
            }
        }
        else
        {
            Asteroid a = collision.transform.GetComponent<Asteroid>();

            a.health -= health;
        }

        Destroy(gameObject);
    }
}
