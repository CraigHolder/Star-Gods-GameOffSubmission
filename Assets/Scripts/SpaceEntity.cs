using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEntity : MonoBehaviour
{
    public static List<Planet> planets;
    public static List<BeltControl> belts;
    public static List<Planet> suns;

    public List<Planet> nearPlanets;
    public Planet sun;

    public float diameter;

    public Rigidbody2D RB;
    public float health = 100;
    public Vector3 gravity;

    public Planet currentPlanet;
    public Planet prevPlanet;

    public bool belt = false;
    protected int frameCount = 0;

    // Start is called before the first frame update
    public void Start()
    {
        if(planets == null)
        {
            suns = new List<Planet>();
            planets = new List<Planet>();
        }
        RB = GetComponent<Rigidbody2D>();

        RB.interpolation = RigidbodyInterpolation2D.None;
    }

    // Update is called once per frame
    public void Update()
    {
        
        if(frameCount > 2)
        {
            frameCount = Random.Range(-3,1);
            if(RB.isKinematic == false)
            {
                ClosestSun(transform);
                ClosestPlanet(transform);
            }
        }
        else
        {
            frameCount++;
        }
            if (currentPlanet)
            {
                if (Vector3.Distance(currentPlanet.transform.position, transform.position) < ((currentPlanet.diameter / 2) + transform.lossyScale.x) + (diameter/2) + 10)
                {
                    if (!currentPlanet.full)
                    {
                        AttachToPlanet();
                    }
                }
            }
    }

    public void FixedUpdate()
    {
    }

    public void ClosestSun(Transform t)
    {
        float dist = Mathf.Infinity;
        int closest = 0;

        for (int i = 0; i < suns.Count; i++)
        {
            //float curDist = Vector2.Distance(planets[i].transform.position, avatarGameObject.transform.position);
            float curDist = Vector2.Distance(suns[i].position, t.position) - (suns[i].diameter / 2);
            if (dist > curDist)
            {
                dist = curDist;
                closest = i;
            }
        }
        sun = suns[closest];
        nearPlanets = sun.nearPlanets;
    }

    public void CalculateGravity()
    {
        gravity = Vector3.zero;

        //RB.velocity = Vector2.zero;

        for (int i = 0; i < nearPlanets.Count; i++)
        {
            switch (nearPlanets[i].type)
            {
                case PlanetType.Belt:
                    gravity += ((nearPlanets[i].transform.position - RB.transform.position).normalized * nearPlanets[i].diameter) / Mathf.Max(0.00001f, (Vector3.Distance(nearPlanets[i].transform.position, RB.transform.position)));   //.normalized * (Vector3.Distance(planets[i].transform.position, RB.transform.position)) ;

                    break;
                
                case PlanetType.Sun:
                    gravity += ((nearPlanets[i].transform.position - RB.transform.position).normalized * (nearPlanets[i].diameter * 4)) / Mathf.Max(0.00001f, (Vector3.Distance(nearPlanets[i].transform.position, RB.transform.position) * 0.6f));//.normalized * (Vector3.Distance(planets[i].transform.position, RB.transform.position)) ;
                    break;
                case PlanetType.Damned:
                    if (nearPlanets[i])
                    {
                        gravity += ((nearPlanets[i].transform.position - RB.transform.position).normalized * (nearPlanets[i].diameter * 2000)) / Mathf.Pow(Mathf.Max(nearPlanets[i].diameter, (Vector3.Distance(nearPlanets[i].transform.position, RB.transform.position))), 1.85f);   //.normalized * (Vector3.Distance(planets[i].transform.position, RB.transform.position)) ;
                    }
                    break;

                default:
                    if(nearPlanets[i])
                    {
                        gravity += ((nearPlanets[i].transform.position - RB.transform.position).normalized * (nearPlanets[i].diameter * 1000)) / Mathf.Pow(Mathf.Max(nearPlanets[i].diameter, (Vector3.Distance(nearPlanets[i].transform.position, RB.transform.position))), 1.85f);   //.normalized * (Vector3.Distance(planets[i].transform.position, RB.transform.position)) ;
                    }
                    break;
            }

            

            //RB.transform.parent = null;
            if (nearPlanets[i].fision)
            {

            }
            else
            {
            }
            
        }

        if (GodControl.instance.constructive)
        {
            gravity += ((GodControl.instance.RB.transform.position - RB.transform.position).normalized * (GodControl.instance.diameter * 2000)) / Mathf.Pow(Mathf.Max(GodControl.instance.diameter, (Vector3.Distance(GodControl.instance.RB.transform.position, RB.transform.position))), 1.85f);   //.normalized * (Vector3.Distance(planets[i].transform.position, RB.transform.position)) ;

        }

        if (sun)
        {
            if (Vector2.Distance(RB.transform.position, sun.transform.position) > 12000)
            {
                RB.velocity = (sun.transform.position - RB.transform.position).normalized * 10;

            }
            else
            {
                RB.velocity += (Vector2)gravity;

            }
        }
        
    }

    public virtual void AttachToPlanet()
    {
        //gravity = Vector3.zero;
        //gravity += (currentPlanet.transform.position - RB.transform.position).normalized;
        transform.parent = currentPlanet.transform;
        RB.velocity *= 0;
        RB.isKinematic = true;
        //RB.GetComponent<CircleCollider2D>().enabled = false;
        //RB.velocity += (Vector2)movement.normalized * speed;
        //transform.RotateAround(currentPlanet.transform.position, Vector3.forward, -movement.x * Time.deltaTime * ((speed / currentPlanet.planetRadius) * 800));
        transform.position = currentPlanet.transform.position + ((transform.position - currentPlanet.transform.position).normalized * ((currentPlanet.diameter / 2)));
        //RB.velocity
    }

    public void ClosestPlanet(Transform t)
    {
        if (belt)
        {
            return;
        }

        float dist = Mathf.Infinity;
        int closest = 0;

        for (int i = 0; i < nearPlanets.Count; i++)
        {
            //float curDist = Vector2.Distance(planets[i].transform.position, avatarGameObject.transform.position);
            float curDist = Vector2.Distance(nearPlanets[i].position, t.position) - (nearPlanets[i].diameter / 2);
            if (dist > curDist)
            {
                dist = curDist;
                closest = i;
            }
        }
        currentPlanet = nearPlanets[closest];
        //if(prevPlanet != currentPlanet)
        //{
        //    FallOffPlanet();
        //}
    }

    public virtual void FallOffPlanet()
    {

    }

    public void CalcRadius()
    {

        SpriteRenderer sR = GetComponent<SpriteRenderer>();
        diameter = sR.bounds.size.x;//(sR.sprite.pixelsPerUnit / transform.lossyScale.x);
    }
}
