using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : SpaceEntity
{
    public ResourceType type;
    public bool grabbed = false;
    float delay = 0;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        init();
    }

    public void init()
    {
        ClosestSun(transform);
        ClosestPlanet(transform);
        CalcRadius();
    }

    // Update is called once per frame
    void Update()
    {
        if(RB.isKinematic || grabbed)
        {
            delay = 0.2f;
            return;
        }else if(delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        base.Update();
        if(currentPlanet)
        {
            if (currentPlanet.name == "Sun")
            {
                if (Vector3.Distance(currentPlanet.transform.position, transform.position) < ((currentPlanet.diameter / 2) + transform.lossyScale.x) + (diameter/2) + 0)
                {
                    //if (type == ResourceType.Energy)
                    //{
                    //
                    //}
                    //else
                    //{
                    //    
                    //}
                    Destroy(gameObject);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        base.FixedUpdate();

        if(RB.isKinematic == false)
        {
            CalculateGravity();

        }

    }

    public enum ResourceType { 
    Metal,
    Life,
    Water,
    Energy,
    Fuel
    }


    public override void AttachToPlanet()
    {
        base.AttachToPlanet();

        switch (type)
        {
            case ResourceType.Metal:
            currentPlanet.metal++;
                break;
            case ResourceType.Life:
            currentPlanet.life++;
            break;
            case ResourceType.Water:
            currentPlanet.water++;
            break;
            case ResourceType.Energy:
            currentPlanet.energy++;
            break;
            case ResourceType.Fuel:
            currentPlanet.fuel++;
            break;
            default:
                break;
        }


    }
}
