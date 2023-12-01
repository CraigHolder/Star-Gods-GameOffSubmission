using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : SpaceEntity
{
    public PlanetType type;
    public bool fision = false;
    Transform orbitTarget;
    Planet parentPlanet;
    public GameObject dayCycle;
    public float orbitRadius = 0;
    public float orbitSpeed = 1;
    public float rotateSpeed = 10;
    //public LineRenderer LR;
    public Vector3 position;

    public PlanetEffect effect;
    public delegate void PlanetEffect(bool on, Vector3 pos);
    public string effectDescription;

    public Vector2 velocity;
    Vector2 prevPos;
    public Vector3 angularVelocity;
    Vector3 prevRotation;
    LineRenderer LR;
    float testTime = 10;

    public bool living = false;
    public bool pushed = false;
    public Vector3 pushDir = Vector3.zero;
    public float pushForce = 0;


    public GameObject psContainer;
    public ParticleSystem ps;

    public List<LineRenderer> lines;
    public List<Planet> children;

    GameObject lastCollision;

    [Header("Resources")]
    public List<Resource.ResourceType> resources;
    public List<int> generationLimit;
    public List<float> generationTimes;
    public List<float> generationDelays;
    public List<bool> generationNatural;

    public List<Structs.CraftingRecipe> crafting;

    public bool full = false;
    public int maxResources = 0;

    public int metal = 0;
    public int life = 0;
    public int water = 0;
    public int fuel = 0;
    public int energy = 0;



    // Start is called before the first frame update
    void Awake()
    {
        base.Start();

        if(resources == null)
        { 
            resources = new List<Resource.ResourceType>();
            generationTimes = new List<float>();
            generationDelays = new List<float>();
            generationLimit = new List<int>();
        }

        

        CalcRadius();

        if (type != PlanetType.Belt)
        {
            planets.Add(this);

            RB = GetComponent<Rigidbody2D>();
            RB.gravityScale = 0;


            float scale = transform.lossyScale.x;

            orbitTarget = transform.parent;


            if (orbitTarget)
            {
                parentPlanet = orbitTarget.GetComponent<Planet>();
                if (parentPlanet)
                {
                    orbitRadius = Vector3.Distance(transform.position, parentPlanet.transform.position);
                }
            }
        }

        psContainer = GameObject.Find("GravityFX");

        if (type == PlanetType.Sun)
        {
            suns.Add(this);

            nearPlanets = new List<Planet>( GetComponentsInChildren<Planet>());
        }
        else
        {
            ps = Instantiate(Resources.Load<ParticleSystem>("Gravity Particles"));
            ps.transform.parent = psContainer.transform;
            ps.transform.localScale = new Vector3(diameter/15f, diameter/15f, 1f);

            //ps = GetComponent<ParticleSystem>();
            //ps = GetComponentInChildren<ParticleSystem>();

            /*
            if(ps != null)
            {
                //var s = ps.shape;
                //s.radius = diameter * 4f;
                //var e = ps.emission;
                //e.rateOverDistance = diameter / 15f;
                
                if (diameter != 0f && scales.Length <= 2)
                {
                    ps.transform.localScale = new Vector3(diameter / 15f, diameter / 15f, 1f);
                }
                else
                {
                    ps.transform.localScale = Vector3.one;
                }
            }
            */
        }

        sun = transform.root.GetComponent<Planet>();

        LR = GetComponent<LineRenderer>();
        

        //LR = GetComponent<LineRenderer>();
        //if(LR)
        //{
        //    float s2 = (scale / 2);
        //    LR.SetWidth(1 / s2, 1 / s2);
        //}


        //Calculate Radius


        switch (type)
        {
            case PlanetType.Dust:
                effectDescription = "Generates belt asteroids";
                break;
            case PlanetType.Fire:
                effect = FireEffect;
                effectDescription = "Grants a boost to the Launch ability";
                break;
            case PlanetType.Life:
                effect = LifeEffect;
                effectDescription = "Creates worshipers";
                break;
            case PlanetType.Water:
                break;
            case PlanetType.Gas:
                break;
            case PlanetType.Cursed:
                break;
            case PlanetType.Chaotic:
                break;
            case PlanetType.Storm:
                break;
            case PlanetType.Ice:
                break;
            case PlanetType.Rock:
                break;
            case PlanetType.Iron:
                break;
            default:
                break;
        }

        health = diameter * 10;
    }

    void Craft()
    {
        if (crafting.Count > 0)
        {

        }
    }

    private void Start()
    {
        DrawOrbits();
        CircleCollider2D c = gameObject.GetComponent<CircleCollider2D>();
        CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
        circle.radius = c.radius;
        circle.isTrigger = true;
    }

    void DrawOrbits()
    {
        if(lines.Count > 0)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Destroy(lines[i].gameObject);
            }
            lines.Clear();
        }

        Planet[] childPlanets = new Planet[transform.childCount];


        for (int i = 0; i < childPlanets.Length; i++)
        {
            if(transform.GetChild(i).gameObject.layer == 6)
            {
                LineRenderer lr = Instantiate(Resources.Load<GameObject>("OrbitPath"), transform).GetComponent<LineRenderer>();//gameObject.AddComponent<LineRenderer>();
                lines.Add(lr);
                children.Add(transform.GetChild(i).GetComponent<Planet>());
                OrbitTrace(lr, children[i]);
            }
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        velocity = (((Vector2)transform.position - prevPos)) / Time.deltaTime;
        prevPos = transform.position;
        angularVelocity = transform.rotation.eulerAngles - prevRotation;
        prevRotation = transform.rotation.eulerAngles;

        if (type == PlanetType.Belt)
        {
            return;
        }

        position = transform.position;

        int numResources = water + metal + energy + fuel + life;
        if(numResources >= maxResources)
        {
            full = true;
        }
        else
        {
            full = false;
        }

        if(!pushed)
        {
            if (orbitTarget)
            {
                transform.RotateAround(orbitTarget.transform.position, Vector3.forward, orbitSpeed * Time.deltaTime);
                transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

                if (parentPlanet)
                {
                    transform.RotateAround(orbitTarget.transform.position, Vector3.forward, -parentPlanet.rotateSpeed * Time.deltaTime);
                }

                if (dayCycle)
                {
                    dayCycle.transform.up = transform.root.position - dayCycle.transform.position;
                }
            }

            if (ps != null)
                ps.transform.position = transform.position;
                //ps.transform.Rotate(Vector3.forward * -rotateSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += (pushDir * pushForce) / diameter * Time.deltaTime;
            pushForce -= (diameter * 5) * Time.deltaTime;
            if(pushForce <= 0)
            {
                pushed = false;
                if(parentPlanet != null)
                {
                    parentPlanet.DrawOrbits();
                }
            }
        }
        


        if (resources.Count > 0)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                switch (resources[i])
                {
                    case Resource.ResourceType.Metal:
                        if (generationTimes[i] > generationDelays[i])
                        {
                            var rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
                            // that's a local direction vector that points in forward direction but also 45 upwards.
                            var lDirection = rot * Vector3.right;

                            // If you need the direction in world space you need to transform it.
                            var wDirection = transform.TransformDirection(lDirection);
                            if (generationLimit[i] > metal)
                            {
                                generationTimes[i] = 0;
                                PlaceResource("Metal", wDirection);
                            }
                            else
                            {
                                generationTimes[i] = 0;
                                if (!generationNatural[i])
                                {
                                    LaunchResource("Metal", wDirection);
                                }

                            }
                        }
                        else
                        {
                            generationTimes[i] += Time.deltaTime;
                        }
                        break;
                    case Resource.ResourceType.Life:
                        if (generationTimes[i] > generationDelays[i])
                        {
                            var rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
                            // that's a local direction vector that points in forward direction but also 45 upwards.
                            var lDirection = rot * Vector3.right;

                            // If you need the direction in world space you need to transform it.
                            var wDirection = transform.TransformDirection(lDirection);
                            if (generationLimit[i] > life)
                            {
                                generationTimes[i] = 0;
                                PlaceResource("Life", wDirection);
                            }
                            else
                            {
                                generationTimes[i] = 0;
                                if (!generationNatural[i])
                                {
                                    LaunchResource("Life", wDirection);
                                }
                            }
                        }
                        else
                        {
                            generationTimes[i] += Time.deltaTime;
                        }
                        break;
                    case Resource.ResourceType.Water:

                        if (generationTimes[i] > generationDelays[i])
                        {
                            var rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
                            // that's a local direction vector that points in forward direction but also 45 upwards.
                            var lDirection = rot * Vector3.right;

                            // If you need the direction in world space you need to transform it.
                            var wDirection = transform.TransformDirection(lDirection);
                            if (generationLimit[i] > water)
                            {
                                generationTimes[i] = 0;
                                PlaceResource("Water", wDirection);
                            }
                            else
                            {
                                generationTimes[i] = 0;
                                if (!generationNatural[i])
                                {
                                    LaunchResource("Water", wDirection);
                                }

                            }
                        }
                        else
                        {
                            generationTimes[i] += Time.deltaTime;
                        }
                        
                        break;
                    case Resource.ResourceType.Energy:
                        if (generationTimes[i] > generationDelays[i])
                        {
                            var rot = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
                            // that's a local direction vector that points in forward direction but also 45 upwards.
                            var lDirection = rot * Vector3.right;

                            // If you need the direction in world space you need to transform it.
                            var wDirection = transform.TransformDirection(lDirection);

                            if (generationLimit[i] > energy)
                            {
                                generationTimes[i] = 0;
                                PlaceResource("Energy", wDirection);
                            }
                            else
                            {
                                generationTimes[i] = 0;

                                if (!generationNatural[i])
                                {
                                    LaunchResource("Energy", wDirection);
                                }
                               
                            }
                        }
                        else
                        {
                            generationTimes[i] += Time.deltaTime;
                        }

                        break;
                    case Resource.ResourceType.Fuel:
                        break;
                    default:
                        break;
                }

            }


            //if(LR)
            //{
            //    LR.SetPosition(0, transform.position);
            //    LR.SetPosition(1, transform.position);
            //}

            //LR.SetPosition(2, transform.position);
        }

        // Debugging stuff:
        //OnDrawGizmos();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, diameter/2);
    }
    void OrbitTrace(LineRenderer lr, Planet p)
    {
        Vector3[] positions = new Vector3[181];

        lr.positionCount = 181;
        lr.useWorldSpace = false;

        lr.SetColors(new Color(1,1,1, 0.05f), new Color(1, 1, 1, 0.05f));

        if (p != null && p.orbitTarget != null)
        {
            for (int i = 0; i <= 180; i++)
            {
                p.transform.RotateAround(p.orbitTarget.position, Vector3.forward, 2);

                positions[i] = (p.transform.localPosition);
            }

        }

        lr.SetPositions(positions);
    }

    public void LaunchResource(string resource, Vector3 dir)
    {
        Resource r = Instantiate(Resources.Load<GameObject>(resource), transform.position + (dir.normalized * ((diameter / 2) + 20)), Quaternion.identity).GetComponent<Resource>();
        r.RB.isKinematic = false;
        r.init();

        r.RB.velocity = velocity;
        r.RB.velocity += (Vector2)(dir.normalized) * 400f;
    }

    public void LaunchResource(Resource r, Vector3 dir, float power)
    {
        //Resource r = Instantiate(Resources.Load<GameObject>(resource), transform.position + (dir.normalized * ((radius / 2) + 20)), Quaternion.identity).GetComponent<Resource>();
        r.RB.isKinematic = false;
        r.init();

        r.RB.velocity = velocity;
        r.RB.velocity += (Vector2)(dir.normalized) * power;
    }

    public void PlaceResource(string resource, Vector3 dir)
    {
        Resource r = Instantiate(Resources.Load<GameObject>(resource), transform.position + (dir.normalized * ((diameter / 2) + 20)), Quaternion.identity).GetComponent<Resource>();

    }

    private void OnMouseOver()
    {
        GodControl.instance.SelectPlanet(transform);
    }

    private void OnMouseExit()
    {
        GodControl.instance.planetUI.SetActive(false);
    }

    public void ActivateAbility(bool on)
    {
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector3 dir = (mousePos - transform.position).normalized;
        //Rigidbody2D rb = Instantiate(Resources.Load<GameObject>("Asteroid"), transform.position + (dir * ((planetRadius/2) + 0.8f)),transform.rotation).GetComponent<Rigidbody2D>();
        //rb.velocity = dir * 100;

        if(transform.position != null)
        {
            effect?.Invoke(on, transform.position);
        }
    }

    public void Push(Vector3 dir, float force)
    {
        pushDir = dir;
        pushForce = force;
        pushed = true;
    }











    public static void FireEffect(bool on, Vector3 pos)
    {
        if(on)
        {
            GodControl.instance.launchPowerMax = 40;
            GodControl.instance.launchSpeed = 80;
        }
        else
        {
            GodControl.instance.launchPowerMax = 30;
            GodControl.instance.launchSpeed = 60;
        }
        
    }

    public static void LifeEffect(bool on, Vector3 pos)
    {
        if (on)
        {
            
            
        }
        else
        {
            Debug.Log("This shouldnt have happened(LifeEffect(false))");
        }

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Planet p = collision.gameObject.GetComponent<Planet>();
        if (p && collision.gameObject != lastCollision && p.gameObject != GodControl.instance.RB.gameObject)
        {
            p.lastCollision = gameObject;
            lastCollision = p.gameObject;

            float hp = health;
            health -= p.health;
            p.health -= hp;

            if (type == PlanetType.Sun)
            {
                diameter += p.diameter;
                health = diameter * 10;
            }
            else if (p.type == PlanetType.Sun)
            {
                p.diameter += diameter;
                p.health = p.diameter * 10;
            }
            

            if (p.health <= 0)
            {
                sun.nearPlanets.Remove(p);
                planets.Remove(p);
                if (p.type == PlanetType.Belt)
                {
                    p.transform.parent.GetComponent<BeltControl>().beltTransforms.Remove(p.transform);
                }
                Planet parent = p.parentPlanet;

                if(p.transform.childCount > 0)
                {
                    for (int i = 0; i < p.transform.childCount; i++)
                    {
                        Transform t = p.transform.GetChild(i);
                        if (t.tag != "Destroy")
                        {
                            t.parent = p.transform.parent;
                        }
                    }
                }

                Destroy(p.gameObject);
                if (parent)
                {
                    parent.DelayDrawOrbits();
                }
            }

            if (health <= 0)
            {
                sun.nearPlanets.Remove(this);
                planets.Remove(this);
                if(type == PlanetType.Belt)
                {
                    transform.parent.GetComponent<BeltControl>().beltTransforms.Remove(transform);
                }
                Planet parent = parentPlanet;

                if (transform.childCount > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform t = transform.GetChild(i);
                        if (t.tag != "Destroy")
                        {
                            t.parent = transform.parent;
                        }
                    }
                }

                Destroy(gameObject);
                if (parent)
                {
                    parent.DelayDrawOrbits();
                }
            }

        }
        
    }


    void DelayDrawOrbits()
    {
        Invoke("DrawOrbits", 0.5f);

    }
}



public enum PlanetType
{
    Dust,
    Fire,
    Life,
    Water,
    Gas,
    Cursed,
    Chaotic,
    Storm,
    Ice,
    Rock,
    Iron,
    Belt,
    Sun,
    Damned,


}

