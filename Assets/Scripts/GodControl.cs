using System.Collections.Generic;
using UnityEngine;

public class GodControl : SpaceEntity
{
    
    public static GodControl instance;

    [Header("Camera")]
    public float scrollSpeed = 10;
    public float zoom = 215;
    public float rotateSpeed = 2;
    public Vector2 camOffset;
    GameObject zoomCamera;
    Vector3 mousePos;

    [Header("Simulation")]
    public float gameSpeed = 1;
    float prevGameSpeed = 1;
    public List<Planet> selectedPlanets;
    
    public Vector3 movement;

    [Header("UI")]
    public GameObject planetUI;

    public TMPro.TMP_Text nameUI;
    public TMPro.TMP_Text typeUI;
    public TMPro.TMP_Text orbitUI;
    public TMPro.TMP_Text rotationUI;
    public TMPro.TMP_Text sizeUI;
    public TMPro.TMP_Text velocityUI;
    public TMPro.TMP_Text effectUI;

    public UnityEngine.UI.Slider burnSlider;
    public UnityEngine.UI.Slider launchSlider;

    public GameObject avatarUI;

    public GameObject controlsUI;

    public TMPro.TMP_Text activateText;

    public TMPro.TMP_Text fpsCounter;

    public GameObject gravityArrow;
    public GameObject velocityArrow;
    //public GameObject QuestArrow;

    [Header("Avatar")]
    public GameObject avatarGameObject;

    public GameObject fireParticles;

    public float planetSpeed = 8;
    
    public bool avatarActive = false;
    public Vector3 avatarBaseSize;
    public float avatarSizeMod = 1f;

    public float burnTime = 5;
    public float burnTimeMax = 5;
    public float burnPower = 4;

    public float launchPower = 5;
    public float launchDur = 5;
    public float launchLength = 0;
    public Vector2 launchDir = Vector2.zero;
    public float launchSpeed = 200;
    public float launchPowerMax = 5f;
    public bool launchCharged = false;

    bool landed = true;

    public bool destructive = false;
    public bool constructive = false;

    public LineRenderer LR;
    public ParticleSystem ps;

    [Header("Resources")]
    public List<Resource> resources;
    public bool full = false;
    public int maxResources = 0;
    public float grabRadius = 5;

    public enum Directional { TopLeft, TopRight, BottomLeft, BottomRight };
    [Header("Controls")]
    public Vector3 mInput = Vector3.zero;
    public Directional mDirection = Directional.BottomLeft;

    int[] fps;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        zoomCamera = GameObject.Find("ZoomCamera");
        planetUI.SetActive(false);
        burnSlider.gameObject.SetActive(false);
        launchSlider.gameObject.SetActive(false);
        avatarUI.SetActive(false);

        avatarGameObject = RB.gameObject;
        avatarBaseSize = avatarGameObject.transform.localScale;

        fps = new int[5];
        for (int i = 0; i < fps.Length; i++)
        {
            fps[i] = 1;
        }
        LR = GetComponent<LineRenderer>();
    }
    private void Start()
    {
        ClosestSun(RB.transform);
        ClosestPlanet(RB.transform);

        Land();
        //CalcRadius();
        diameter = 10;
    }

    // Update is called once per frame
    void Update()
    {
        RB.gameObject.transform.localScale = avatarBaseSize * avatarSizeMod;
        movement = Vector3.zero;
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        planetUI.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);

        UpdateSimulation();

        AvatarControls();


        int fps = AverageFPS((int)(1f / Time.unscaledDeltaTime));
        if(fps < 30)
        {
            fpsCounter.color = Color.red;
        }
        else if (fps < 60)
        {
            fpsCounter.color = Color.white;
        }
        if (fps > 60)
        {
            fpsCounter.color = Color.green;
        }

        fpsCounter.text = "FPS: " + fps;
        //Debug.Log(fps);
    }

    int AverageFPS(int frames)
    {
        float average = 0;
        for (int i = fps.Length-1; i > 0; i--)
        {
            fps[i] = fps[i-1];
            average += fps[i];
        }

        fps[0] = frames;

        return (int)(average / 5);
    }

    public void GrabResources()
    {
        Collider2D[] resources = Physics2D.OverlapCircleAll(RB.transform.position, grabRadius);

        for (int i = 0; i < resources.Length; i++)
        {
            Resource r = resources[i].GetComponent<Resource>();
            if (r)
            {

                if (constructive)
                {
                    CollectResource(r);
                }
                else
                {
                    this.resources.Add(r);
                    r.RB.isKinematic = false;
                    r.grabbed = true;
                    r.transform.parent = RB.transform;

                    switch (r.type)
                    {
                        case Resource.ResourceType.Metal:
                            r.currentPlanet.metal -= 1;
                            break;
                        case Resource.ResourceType.Life:
                            r.currentPlanet.life -= 1;
                            break;
                        case Resource.ResourceType.Water:
                            r.currentPlanet.water -= 1;
                            break;
                        case Resource.ResourceType.Energy:
                            r.currentPlanet.energy -= 1;
                            break;
                        case Resource.ResourceType.Fuel:
                            r.currentPlanet.fuel -= 1;
                            break;
                        default:
                            break;
                    }
                }
                
                
            }
        }
    }
    public void DropbResources()
    {
        for (int i = 0; i < resources.Count; i++)
        {
            resources[i].RB.isKinematic = false;
            resources[i].grabbed = false;
            resources[i].transform.parent = null;
        }
        resources.Clear();
    }


    private void LateUpdate()
    {
        UpdateCamera();
        RB.rotation = 0;
        //RB.transform.rotation = Quaternion.Euler(0,0,0);
        transform.eulerAngles = new Vector3(0, 0, 0);
        // SetPOV();
    }

    public void FixedUpdate()
    {

        //if (avatarActive)
        // {
        UpdateAvatar();
        //}
    }

    void UpdateCamera()
    {
        if (avatarActive)
        {
            if (RB.transform.parent != null && currentPlanet.type != PlanetType.Sun)
            {
                transform.position = currentPlanet.RB.transform.position;
            }
            else
            {
                transform.position = RB.transform.position;
            }
        }
        else
        {
            camOffset += new Vector2(movement.x, movement.y) * Time.deltaTime * (zoom); //movement.x * transform.right;//

            transform.position = camOffset;
        }
            zoom = Mathf.Max(3f, zoom - Input.mouseScrollDelta.y * (scrollSpeed));
            Camera.main.orthographicSize = zoom;
        
    }


    void ChargeLaunch()
    {
        Targeting();

        launchSlider.gameObject.SetActive(true);

        // Checks if launch power has reached lowest or highest point, then swaps direction if so.
        // Potentially add a pause here.
        if (launchPower >= launchPowerMax)
        {
            launchCharged = true;
        }
        else if (launchPower <= 0f)
        {
            launchCharged = false;
        }
        //

        // Based on direction, increase or decrease launch power.
        if (!launchCharged)
        {
            launchPower = Mathf.Min(launchPower += Time.deltaTime * launchSpeed, launchPowerMax);
        }
        else
        {
            launchPower = Mathf.Max(launchPower -= Time.deltaTime * launchSpeed, 0f);
        }
        //

        launchSlider.value = launchPower / launchPowerMax;
    }

    private void StopLaunch()
    {
        launchCharged = false;
        launchSlider.gameObject.SetActive(false);
        launchPower = 0;

        LR.SetPosition(0, Vector3.zero);
        LR.SetPosition(1, Vector3.zero);
    }

    void AvatarControls()
    {
        if(avatarActive)
        {
            if (Input.GetMouseButtonDown(1) && landed)
            {
                StopLaunch();
            }
            else if (avatarActive && Input.GetMouseButton(1) && landed)
            {
                ChargeLaunch();

                if (Input.GetMouseButtonDown(0))
                {
                    StopLaunch();
                }
            }

            if (RB.transform.parent != null && Input.GetMouseButtonUp(1) && landed)
            {
                RB.transform.parent = null;

                launchCharged = false;
                landed = false;

                launchDur = 0;
                launchDir = (Vector2)(mousePos - RB.transform.position).normalized;

                RB.velocity += currentPlanet.velocity;
                RB.velocity += launchDir * (launchPower);

                launchSlider.gameObject.SetActive(false);

                LR.SetPosition(0, Vector3.zero);
                LR.SetPosition(1, Vector3.zero);

                RB.rotation = 0;

                ActivateFlightUI(true);
            }

            //if (Input.GetKeyDown(KeyCode.F))
            if (Input.GetKey(KeyCode.F))
            {
                DropbResources();
                GrabResources();

            }
        }

        if (Input.GetKeyUp(KeyCode.Z))
        {
            //spawn explosiona

            avatarExplode();

        }

        
        if(Input.GetMouseButtonDown(3))
        {
            DropbResources();
        }
    }

    public void avatarExplode()
    {
        Instantiate(Resources.Load<GameObject>("FireExplosion"), RB.transform.position, Quaternion.identity);
        RB.transform.position = new Vector3(0, -110, 0);
        launchDur = launchLength;
        RB.velocity *= 0;

    }


    void AvatarMovement()
    {
        burnSlider.value = burnTime / burnTimeMax;
        RB.rotation = 0;
        //transform.position = RB.transform.position;



        if (launchDur < launchLength)
        {
            launchDur += Time.fixedDeltaTime;
            RB.velocity += launchDir * launchPower;//* Time.fixedDeltaTime;

            switch (currentPlanet.type)
            {
                case PlanetType.Dust:
                    break;
                case PlanetType.Fire:
                    currentPlanet.ActivateAbility(false);
                    break;
                case PlanetType.Life:
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
                case PlanetType.Belt:
                    break;
                case PlanetType.Sun:
                    break;
                case PlanetType.Damned:
                    break;
                default:
                    break;
            }

            belt = false;
            ClosestPlanet(RB.transform);
            if (currentPlanet != prevPlanet)
            {
                if (currentPlanet != null && Vector3.Distance(currentPlanet.transform.position, avatarGameObject.transform.position) < ((currentPlanet.diameter / 2) + transform.lossyScale.x) + 10)
                {
                    launchDur = launchLength;
                    Land();
                }
            }
        }
        else
        {
            if (currentPlanet != null && Vector3.Distance(currentPlanet.transform.position, avatarGameObject.transform.position) < ((currentPlanet.diameter / 2) + transform.lossyScale.x) + 10)
            {
                Land();
            }
            else
            {
                switch (currentPlanet.type)
                {
                    case PlanetType.Dust:
                        break;
                    case PlanetType.Fire:
                        currentPlanet.ActivateAbility(false);
                        break;
                    case PlanetType.Life:
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
                    case PlanetType.Belt:
                        break;
                    case PlanetType.Sun:
                        break;
                    case PlanetType.Damned:
                        break;
                    default:
                        break;
                }
            }
        }
            
            
        
        //Space Movement using burntime
        if (burnTime > 0 && avatarActive && movement != Vector3.zero && !landed)
        {
            burnTime -= Time.fixedDeltaTime;

            RB.velocity += (Vector2)movement.normalized * burnPower;
        }
        
    }

    public void LaunchResource()
    {
        Vector3 dir = transform.position - currentPlanet.transform.position;
        for (int i = 0; i < resources.Count; i++)
        {
            currentPlanet.LaunchResource(resources[i], launchDir, launchPower);
        }
        DropbResources();
    }

    public void Attack()
    {
        Vector3 dir = RB.velocity.normalized;
        currentPlanet.Push(dir, RB.velocity.magnitude);

        avatarExplode();
    }
    
    public void Land()
    {
        if(destructive)
        {
            if(Input.GetKey(KeyCode.Z))
            {
                Attack();
                return;
            }
        }
        ActivateFlightUI(false);

        landed = true;
        prevPlanet = currentPlanet;
        //burnTime = burnTimeMax;


        gravity = Vector3.zero;
        gravity += (currentPlanet.transform.position - RB.transform.position).normalized;
        avatarGameObject.transform.parent = currentPlanet.transform;


        RB.velocity *= 0;
        RB.rotation = 0;
        RB.transform.parent = currentPlanet.transform;


        switch (currentPlanet.type)
        {
            case PlanetType.Dust:
                break;
            case PlanetType.Fire:
                currentPlanet.ActivateAbility(true);
                break;
            case PlanetType.Life:
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
            case PlanetType.Belt:
                break;
            case PlanetType.Sun:
                burnTime = burnTimeMax;
                break;
            case PlanetType.Damned:
                break;
            default:
                break;
        }
        //RB.velocity += (Vector2)movement.normalized * speed;

        if (avatarActive)
        {
            // Updates when the player changes which keys they're pressing.
            if (movement != mInput) CheckPlanetside();

            Vector2 dir = Vector2.zero;
            // Movement edits going here!
            // mDirection is based on where player is on planet
            switch (mDirection)
            {
                // Depending on the case used, changes the player's movement to follow the orientation of the planet.
                // This ensures speed will remain the same while allowing player to use either direction when in a corner
                //Based on which corner they're in, sets up directional values
                case Directional.BottomLeft:
                    dir = new Vector2(1f, -1f);
                    break;
                case Directional.BottomRight:
                    dir = new Vector2(1f, 1f);
                    break;
                case Directional.TopLeft:
                    dir = new Vector2(-1f, -1f);
                    break;
                case Directional.TopRight:
                    dir = new Vector2(-1f, 1f);
                    break;
            }// After determining the direction they should be moving, 
            if (movement.x != 0f)
            {
                PlanetMove(dir.x * movement.x);
            }
            else
            {
                PlanetMove(dir.y * movement.y);
            }
            avatarGameObject.transform.position = currentPlanet.transform.position + ((avatarGameObject.transform.position - currentPlanet.transform.position).normalized * ((currentPlanet.diameter / 2)));
            
            // Checks if the player has moved recently, unecessary now
            //if (mPreviousPos == avatarGameObject.transform.localPosition)
            //{
                //CheckPlanetside();
            //}
            //mPreviousPos = avatarGameObject.transform.localPosition;
        }

    }

    void PlanetMove(float d)
    {
        avatarGameObject.transform.RotateAround(currentPlanet.transform.position, Vector3.forward, d * Time.deltaTime * ((planetSpeed / currentPlanet.diameter) * 800));
    }
    void CheckPlanetside()
    {
        // note: create deadzones which allow you to use magnitude instead of what's going on here
        mInput = movement;

        // Use math to calculate where the god is compared to the current planet they're on
        // Using SOH CAH TOA
        float tempDir = Mathf.Atan2(avatarGameObject.transform.position.y - currentPlanet.transform.position.y, 
                                    avatarGameObject.transform.position.x - currentPlanet.transform.position.x) * Mathf.Rad2Deg;
        //Debug.Log("Direction of Avatar: " + tempDir);

        // Determines which direction the player should move based on where they are on the planet
        if (tempDir < -90f)
        {
            mDirection = Directional.BottomLeft;
            Debug.Log("In BottomLeft");
        }
        else if (tempDir > 90f)
        {
            mDirection = Directional.TopLeft;
            Debug.Log("In TopLeft");
        }
        else if (tempDir < 90f && tempDir > 0f)
        {
            mDirection = Directional.TopRight;
            Debug.Log("In TopRight");
        }
        else if (tempDir < 0f && tempDir > -90f)
        {
            mDirection = Directional.BottomRight;
            Debug.Log("In BottomRight");
        }

    }

    void ActivateFlightUI(bool on)
    {
        gravityArrow.SetActive(on);
        velocityArrow.SetActive(on);
        burnSlider.gameObject.SetActive(on);
    }
    
    void UpdateAvatar()
    {
        fireParticles.transform.localScale = new Vector3( diameter, diameter,1);

        frameCount++;

        if(frameCount > 6)
        {
            frameCount = 0;

            
            ClosestSun(RB.transform);
            ClosestPlanet(RB.transform);
        }

        if(launchDur > launchLength)
        {
            CalculateGravity();
        }
        

        gravityArrow.transform.localScale = new Vector3(Mathf.Min(gravity.magnitude, 10f), 1f, 1f) * 5f;
        gravityArrow.transform.right = gravity;
        gravityArrow.transform.position = RB.transform.position + (gravityArrow.transform.right * ((gravity.magnitude + 2f) / 5f));

        velocityArrow.transform.localScale = new Vector3(Mathf.Min(RB.velocity.magnitude / 180f, 10f), 1f, 1f) * 5f;
        velocityArrow.transform.right = RB.velocity;
        velocityArrow.transform.position = RB.transform.position + (velocityArrow.transform.right * ((RB.velocity.magnitude + 2f) / 5f));


        AvatarMovement();
        if (ps != null)
        {
            ps.transform.position = RB.position;
            ps.transform.localScale = new Vector3(0.75f * (diameter/10), 0.75f * (diameter / 10), 1f);
            //public Vector3 avatarBaseSize;
            //public float avatarSizeMod = 1f;
            //RB.gameObject.transform.localScale/2f;
            //GodControl.instance.ps.transform.localScale = new Vector3(diameter / 15f, diameter / 15f, 1f);
        }
        //Debug.Log(currentPlanet);



        if (resources.Count > 0)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                //resources[i]
                resources[i].RB.velocity = (transform.position - resources[i].transform.position) * 8;
            }
        }

    }

   

    void UpdateSimulation()
    {
        Controls();

        

        gameSpeed = Mathf.Max(0, gameSpeed);
        Time.timeScale = gameSpeed;
    }

    void Targeting()
    {
        //if(!Input.GetMouseButton(1))
        //{
        //    return;
        //}

        RB.gameObject.layer = 0;

        RaycastHit2D hit = Physics2D.Raycast(RB.transform.position, mousePos - RB.transform.position, Vector3.Distance(mousePos, RB.transform.position));
        LR.SetPosition(0, RB.transform.position);
        LR.SetPosition(1, mousePos);
        if (hit)
        {
            LR.SetPosition(1, hit.point);

            // LR.startColor = (Color.red);
            // LR.startColor = (Color.red);
        }
        else
        {
            
           // LR.startColor = (Color.green);
           // LR.startColor = (Color.green);
        }

        //selected

        //for (int i = 0; i < selectedPlanets.Count; i++)
        //{
        //    

        //    //Debug.DrawRay(selectedPlanets[i].transform.position, mousePos - selectedPlanets[i].transform.position);

        //}
    }

    void Controls()
    {
        
        //controls UI
        //if(Input.GetKeyDown(KeyCode.Tab))
        //{
        //    controlsUI.SetActive(!controlsUI.activeSelf);
        //}

        // transform.localPosition += movement.y * transform.up;
        //activate abilities
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }


        //deselect
        if (Input.GetMouseButtonDown(2))
        {
            planetUI.SetActive(false);
        }

        //rotate view
        //if (Input.GetKey(KeyCode.Q))
        //{
        //    transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        //}
        //if (Input.GetKey(KeyCode.E))
        //{
        //    transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);

        //}

        //reset view
        if (Input.GetKeyDown(KeyCode.R))
        {
            camOffset = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }


        //Avatar view
        if (Input.GetKeyDown(KeyCode.F) && QuestManager.instance.dialog == false)
        {
            
        }

        //GAME SPEED
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gameSpeed = 1;
            prevGameSpeed = gameSpeed;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gameSpeed = 2;
            prevGameSpeed = gameSpeed;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gameSpeed = 3;
            prevGameSpeed = gameSpeed;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gameSpeed = 4;
            prevGameSpeed = gameSpeed;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            gameSpeed = 5;
            prevGameSpeed = gameSpeed;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (avatarActive)
            {
                avatarActive = false;
                avatarUI.SetActive(false);
                activateText.text = "[Tab] Avatar";
            }
            else
            {
                avatarActive = true;
                avatarUI.SetActive(true);
                activateText.text = "[Tab] Overview";
            }

        }

            if (Input.GetKeyDown(KeyCode.Space))
        {
            if(gameSpeed == 0.25f)
            {
                gameSpeed = 0;
            }
            else if(gameSpeed == 0f)
            {
                gameSpeed = prevGameSpeed;
            }
            else
            {
                gameSpeed = gameSpeed = 0.25f;
            }
            
        }
    }

    public void setTime(int multiplier)
    {
        gameSpeed = multiplier;
        prevGameSpeed = gameSpeed;
    }

    public void SelectPlanet(Transform target)
    {
        selectedPlanets[0] = (target.GetComponent<Planet>());
        if (selectedPlanets[0] != null)
        {
            planetUI.SetActive(true);

            //zoomCamera.transform.position = new Vector3(RB.transform.position.x, RB.transform.position.y, -10);
            nameUI.text = selectedPlanets[0].name;
            typeUI.text = "Type: " + selectedPlanets[0].type.ToString();
            sizeUI.text = "Radius: " + selectedPlanets[0].diameter.ToString() + "m";
            rotationUI.text = "Rotation: " + (1 / (selectedPlanets[0].rotateSpeed / 360)).ToString() + "s";
            orbitUI.text = "Orbit: " + (1 / (selectedPlanets[0].orbitSpeed / 360)).ToString() + "s";
            velocityUI.text = "Velocity: " + (selectedPlanets[0].velocity.magnitude);
            effectUI.text = "Effect: " + (selectedPlanets[0].effectDescription);


        }
        else
        {
            planetUI.SetActive(false);
        }
            //selectedPlanets.Add(target.GetComponent<Planet>());
            
        //SetPOV();
    }

    void SetPOV()
    {
        if(selectedPlanets[0])
        {
            zoomCamera.transform.position = new Vector3(RB.transform.position.x, RB.transform.position.y, -10);

        }
        else
        {

        }

        //if(selectedPlanets.Count == 1)
        //{
        //    //transform.parent = selectedPlanets[0].transform;
        //    //transform.localPosition = Vector3.zero;
        //   // transform.rotation = selectedPlanets[0].transform.rotation;
        //   // transform.localPosition += (Vector3)camOffset;
        //}
        //else
        //{
        //    transform.rotation = Quaternion.identity;
        //    transform.parent = null;
        //    Vector3 centerPoint = Vector3.zero;
        //    for (int i = 0; i < selectedPlanets.Count; i++)
        //    {
        //        centerPoint += selectedPlanets[i].transform.position * selectedPlanets[i].planetRadius;
        //    }
        //    transform.position = centerPoint/2;
        //    transform.localPosition += (Vector3)camOffset;
        //}




    }

    public override void FallOffPlanet()
    {
        RB.transform.parent = null;

        launchCharged = false;
        landed = false;
        launchDur = 0;
        launchDir = (Vector2)(mousePos - RB.transform.position).normalized;

        launchSlider.gameObject.SetActive(false);
        RB.rotation = 0;
    }

    // Saving this collision stuff for when its handled
    public void CollectResource(Resource hitResource)
    {

         if (hitResource.type == Resource.ResourceType.Energy)
         {
             burnTime += 1f;
         }
         else if (QuestManager.instance.step >= 10 && !QuestManager.instance.takenDeal)
         {
            grabRadius += 1;
            diameter += 1;
        }
        resources.Remove(hitResource);
        Destroy(hitResource.gameObject);
         
    }

}

//Garb code;

// All this comment is way to complex and silly.
/*
// Sides: 
// In 90 degees, 
// 135, 45
// 45, -45
// -45, -135,
// -135, 135

// Updated: 60 degrees(top, right, bottom, left
// 120, 60
// 30, -30
// -60, -120
// -150, 150

// Final Update: 30 Degrees(top, right, bottom, left), found that the corners being bigger feels better.
// 105, 75
// 15, -15
// -75, -105
// -165, 165

// Base Directions

if(tempDir < 30f && tempDir > -30f)
{
    mDirection = Directional.Right;
    Debug.Log("Avatar Right");
}
else if (tempDir < -60f && tempDir > -120f)
{
    mDirection = Directional.Bottom;
    Debug.Log("Avatar Bottom");
}
else if (tempDir < 120f && tempDir > 60f)
{
    mDirection = Directional.Top;
    Debug.Log("Avatar Top");
}
else if (tempDir < -150f || tempDir > 150f)
{
    mDirection = Directional.Left;
    Debug.Log("Avatar Left");
}

// Corners
else if (tempDir < -120f && tempDir > -150f)
{
    mDirection = Directional.BottomLeft;
    Debug.Log("In BottomLeft");
}
else if (tempDir < 150f && tempDir > 120f)
{
    mDirection = Directional.TopLeft;
    Debug.Log("In TopLeft");
}
else if (tempDir < 60f && tempDir > 30f)
{
    mDirection = Directional.TopRight;
    Debug.Log("In TopRight");
}
else if (tempDir < -30f && tempDir > -60f)
{
    mDirection = Directional.BottomRight;
    Debug.Log("In BottomRight");
}
*/

// Completely unnecessary code below
/*
case Directional.Bottom:
    dir = new Vector2(1f, 1f);
    //PlanetMove(movement.x);
    break;
case Directional.Top:
    dir = new Vector2(-1f, -1f);
    //PlanetMove(-movement.x);
    break;
case Directional.Left:
    dir = new Vector2(-1f, -1f);
    //PlanetMove(-movement.y);
    break;
case Directional.Right:
    dir = new Vector2(1f, 1f);
    //PlanetMove(movement.y);
    break;
*/