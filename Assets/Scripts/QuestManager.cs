using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    public TMPro.TMP_Text questUI;    // Start is called before the first frame update
    public TMPro.TMP_Text bigTextUI;
    public GameObject sun;
    public GameObject decision;
    public GameObject avatar;
    public GameObject psContainer;
    public int step = 0;
    bool[] complete;
    public bool dialog = false;
    int damnedVoicelines = 0;
    int damnedChoice = 0;
    public bool takenDeal = false;

    public enum QuestList
    {
        Emerge = 0,
        Move = 1,
        Launch = 2,
        DiscoverLife = 3,
        RevivePlanet = 4,
        FindDamned = 5,
        SpeakDamned = 6,
        MakeChoice = 7,
        DeclineDeal = 8,
        AcceptDeal = 9

    };
    public QuestList allQuests;


    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        complete = new bool[20];
    }
    

    void QuestMaster()
    {
        // For loop cycles through quests, discovers which quest we're on and checks if any quests are completed. 
        bool lowestQuest = false;
        for (int i = 0; i < complete.Length; i++)
        {
            if (!complete[i])
            {
                if (!lowestQuest) { step = i; }
                lowestQuest = true;

                // ----------------------------------------------------------------- HAVE NOT ACCEPTED DEAL
                if (!takenDeal)
                {
                    switch (i)
                    {
                        case (int)QuestList.Emerge:
                            GodControl.instance.RB.transform.position = Vector3.zero;
                            GodControl.instance.avatarActive = true;

                            if (Input.GetKeyDown(KeyCode.Z))
                            {
                                GodControl.instance.RB.velocity = Vector2.zero;
                                GodControl.instance.RB.angularVelocity = 0;
                                GodControl.instance.RB.transform.position = new Vector3(0, -110, 0);
                                //GodControl.instance.RB.isKinematic = false;
                                complete[(int)QuestList.Emerge] = true;
                            }
                            i = complete.Length;
                            break;
                        case (int)QuestList.Move:
                            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                            {
                                complete[(int)QuestList.Move] = true;
                            }
                            break;
                        case (int)QuestList.Launch:
                            if (Input.GetMouseButtonDown(1))
                            {
                                complete[(int)QuestList.Launch] = true;
                            }
                            break;
                        case (int)QuestList.DiscoverLife:
                            //if (avatar.GetComponentInParent<Planet>().type != PlanetType.Sun && avatar.transform.parent != null)
                            if (avatar.transform.parent != null && avatar.GetComponentInParent<Planet>().type == PlanetType.Life)
                            {
                                complete[(int)QuestList.DiscoverLife] = true;
                            }
                            break;
                        case (int)QuestList.RevivePlanet:
                            if (avatar.transform.parent != null && avatar.GetComponentInParent<Planet>().type != PlanetType.Sun
                                && avatar.GetComponentInParent<Planet>().type != PlanetType.Life)
                            {
                                complete[(int)QuestList.RevivePlanet] = true;
                            }
                            break;
                        case (int)QuestList.FindDamned:
                            if (avatar.transform.parent != null && avatar.GetComponentInParent<Planet>().type == PlanetType.Damned)
                            {
                                complete[(int)QuestList.FindDamned] = true;
                                for (int c = 0; c < (int)QuestList.FindDamned; c++)
                                {
                                    complete[c] = true;
                                }
                            }
                            break;
                        case (int)QuestList.SpeakDamned:
                            if (damnedVoicelines >= 5)
                            {
                                complete[(int)QuestList.SpeakDamned] = true;
                                /*for (int c = 0; c < 7; c++)
                                {
                                    complete[c] = true;
                                }*/
                            }
                            break;
                        case (int)QuestList.MakeChoice:
                            break;
                        default:
                            break;
                    }
                }
                // ----------------------------------------------------------------- ACCEPTED THE DEAL OF THE DAMNED
                else
                {
                    switch (i)
                    {

                    }
                }
            }
        }

        // This section displays current quest the player is on. 
        // ----------------------------------------------------------------- HAVE NOT ACCEPTED DEAL 
        if (!takenDeal)
        {
            switch (step)
            {
                case (int)QuestList.Emerge:
                    bigTextUI.text = "[Z] Emerge";
                    bigTextUI.gameObject.SetActive(true);
                    questUI.gameObject.SetActive(false);
                    break;
                case (int)QuestList.Move:
                    questUI.text = "Quest: " + "Move";
                    questUI.gameObject.SetActive(true);
                    bigTextUI.gameObject.SetActive(false);
                    break;
                case (int)QuestList.Launch:
                    questUI.text = "Quest: " + "Launch";
                    break;
                case (int)QuestList.DiscoverLife:
                    questUI.text = "Quest: " + "Discover Life";
                    break;
                case (int)QuestList.RevivePlanet:
                    questUI.text = "Quest: " + "Revive a planet";
                    break;
                case (int)QuestList.FindDamned:
                    questUI.text = "Quest: " + "Find what’s at the edges of the galaxy";
                    break;
                case (int)QuestList.SpeakDamned:
                    questUI.text = "Quest: " + "Speak with the Damned";
                    dialog = true;
                    bigTextUI.gameObject.SetActive(true);
                    //damnedVoicelines = nextVoiceline(damnedVoicelines);
                    if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                    {
                        damnedVoicelines++;
                    }

                    if (!(Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position)
                        < (GodControl.instance.currentPlanet.diameter / 2) + 1f))
                    {
                        if (damnedVoicelines >= 2)
                        {
                            bigTextUI.gameObject.SetActive(false);
                            complete[(int)QuestList.SpeakDamned] = true;
                            Reject();
                        }
                        else if (damnedVoicelines < 2)
                        {
                            bigTextUI.gameObject.SetActive(false);
                            damnedVoicelines = -1;
                            complete[(int)QuestList.FindDamned] = false;
                            GodControl.instance.RB.transform.parent = null;
                            avatar.transform.parent = null;
                            //SpaceEntity.planets.Remove(GodControl.instance.currentPlanet);
                            //GodControl.instance.currentPlanet.sun.nearPlanets.Remove(GodControl.instance.currentPlanet);
                        }

                    }

                    GodControl.instance.avatarActive = true;
                    switch (damnedVoicelines)
                    {
                        case -1:
                            bigTextUI.text = "Got that out of your system, did you?";
                            break;
                        case 0:
                            bigTextUI.text = "Greetings, new spark";
                            break;
                        case 1:
                            bigTextUI.text = "You have come far to get here";
                            break;
                        case 2:
                            bigTextUI.text = "I offer you a gift of knowledge";
                            // if player leaves, decline
                            break;
                        case 3:
                            bigTextUI.text = "The knowledge of how to ASCEND!";
                            break;
                        case 4:
                            bigTextUI.text = "Consume your planets or thier pieces";
                            GodControl.instance.currentPlanet.transform.parent = null;
                            sun.SetActive(false);
                            break;
                        case 5:
                            bigTextUI.text = "and forge something far more powerful";
                            //bigTextUI.gameObject.SetActive(true)
                            decision.SetActive(true);
                            break;
                        default:
                            break;
                    }
                    break;
                case (int)QuestList.MakeChoice:
                    questUI.text = "Quest: " + "DECIDE";
                    break;
                case (int)QuestList.AcceptDeal:
                    bigTextUI.text = "Take my blessing,\n your fires will burn bright.";
                    decision.SetActive(false);
                    if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                    {
                        complete[(int)QuestList.AcceptDeal] = true;
                        takenDeal = true;
                    }
                    GodControl.instance.avatarActive = true;
                    break;
                case (int)QuestList.DeclineDeal:
                    bigTextUI.text = "You fill with energy as the strange being leaves";
                    //if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                    if (Input.anyKeyDown)
                    {
                        complete[(int)QuestList.DeclineDeal] = true;
                        complete[(int)QuestList.AcceptDeal] = true;
                        takenDeal = false;
                    }
                    GodControl.instance.avatarActive = true;
                    break;
                // Will need to change this later.
                case 10:
                    {
                        GodControl.instance.burnTimeMax = 100;
                        questUI.text = "Quest: " + "Win! End of Build";

                        sun.SetActive(true);
                        dialog = false;
                        bigTextUI.gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
            }
        }
        // ----------------------------------------------------------------- ACCEPTED THE DEAL OF THE DAMNED
        else
        {
            switch (step)
            {
                case 10:
                    {
                        GodControl.instance.burnTimeMax = 100;
                        questUI.text = "Quest: " + "Win! End of Build";

                        sun.SetActive(true);
                        dialog = false;
                        bigTextUI.gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    /*int nextVoiceline(int current)
    {
        if (Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            return current++;
        }
        return current;
    }*/

    // Update is called once per frame
    void Update()
    {
        //QuestUpdate();
        QuestMaster();
        
    }
    void LateUpdate()
    {
        /*
        if (step == 0)
        {
            QuestUpdate();
        }
        */
    }
    void FixedUpdate()
    {
        /*
        if(step == 0)
        {
            QuestUpdate();
        }
        */
    }

    public void Accept()
    {
        /*
        switch (step)
        {
            case 9:
                step = 10;
                break;
            default:
                break;
        }
        */

        // This will need to fix.
        complete[(int)QuestList.MakeChoice] = true;
        complete[(int)QuestList.DeclineDeal] = true;

        GodControl.instance.destructive = true;
        decision.SetActive(false);
    }

    public void Reject()
    {
        /*
        switch (step)
        {
            case 9:
                step = 11;
                break;
            default:
                break;
        }
        */

        // This will need to fix.
        complete[(int)QuestList.MakeChoice] = true;
        complete[(int)QuestList.AcceptDeal] = true;
        GodControl.instance.ps = Instantiate(Resources.Load<ParticleSystem>("Gravity Particles"));
        GodControl.instance.constructive = true;
        GodControl.instance.ps.transform.parent = psContainer.transform;
        decision.SetActive(false);
        //GodControl.instance.ps.transform.localScale = new Vector3(diameter / 15f, diameter / 15f, 1f);

    }
}


/*
 *  Old unused code
 * 
private void QuestUpdate()
    {
        switch (step)
        {
            case 0:
                GodControl.instance.RB.transform.position = Vector3.zero;
                bigTextUI.text = "[F] Emerge";
                bigTextUI.gameObject.SetActive(true);
                GodControl.instance.avatarActive = true;

               //questUI.gameObject.SetActive(false);
                questUI.gameObject.SetActive(false);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    step++;
                    questUI.text = "Quest: " + "Move";
                    GodControl.instance.RB.velocity = Vector2.zero;
                    GodControl.instance.RB.angularVelocity = 0;
                    GodControl.instance.RB.transform.position = new Vector3(0, -110, 0);
                    //GodControl.instance.RB.isKinematic = false;
                    bigTextUI.gameObject.SetActive(false);
                    questUI.gameObject.SetActive(true);
                }
                break;
            case 1:

                step++;
                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    step++;
                    questUI.text = "Quest: " + "Launch";
                }
                break;
            case 3:
                if (Input.GetMouseButtonDown(1))
                {
                    step++;
                    questUI.text = "Quest: " + "Reach The Damned";
                }
                break;
            case 4:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {
                        
                        questUI.text = "Quest: " + "Talk to The Damned";

                        bigTextUI.text = "Greetings, new spark";
                        dialog = true;
                        bigTextUI.gameObject.SetActive(true);
                    }

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step++;
                    }

                    GodControl.instance.avatarActive = true;
                }
                break;

            case 5:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {

                        bigTextUI.text = "You have come far to get here";
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step++;
                    }

                    GodControl.instance.avatarActive = true;

                }
                break;
            case 6:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {

                        bigTextUI.text = "I offer you a gift of knowledge";
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step++;
                    }

                    GodControl.instance.avatarActive = true;

                }
                break;
            case 7:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {

                        bigTextUI.text = "The knowledge of how to ASCEND!";
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step++;
                    }

                    GodControl.instance.avatarActive = true;

                }
                break;
            case 8:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {
                        bigTextUI.text = "Consume your planets or thier pieces";

                        GodControl.instance.currentPlanet.transform.parent = null;

                        sun.SetActive(false);

                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step++;
                    }

                    GodControl.instance.avatarActive = true;

                }
                break;
            case 9:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {

                        bigTextUI.text = "and forge something far more powerful";
                        //bigTextUI.gameObject.SetActive(true)
                        decision.SetActive(true);
                    }

                    GodControl.instance.avatarActive = true;
                }
                break;
            case 10:
                if (GodControl.instance.currentPlanet.name == "The Damned")
                {
                    if (Vector2.Distance(GodControl.instance.RB.transform.position, GodControl.instance.currentPlanet.transform.position) < (GodControl.instance.currentPlanet.diameter / 2) + 1f)
                    {
                        bigTextUI.text = "Take my blessing,\n your fires will burn bright.";

                        decision.SetActive(false);
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step = 100;
                    }


                    GodControl.instance.avatarActive = true;
                }
                break;
            case 11:
                    
                        bigTextUI.text = "FOOl!\n You reject my GIFT?!\nI will be back...";

                    decision.SetActive(false);

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                    GodControl.instance.RB.transform.parent = null;
                    SpaceEntity.planets.Remove(GodControl.instance.currentPlanet);

                    GodControl.instance.currentPlanet.sun.nearPlanets.Remove(GodControl.instance.currentPlanet);

                    Destroy(GodControl.instance.currentPlanet.gameObject);
                    sun.SetActive(true);

                    step = 12;
                    }


                    GodControl.instance.avatarActive = true;
                break;
            case 12:

                        

                       bigTextUI.text = "You fill with energy as the strange being leaves";
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        step = 100;
                    }


                    GodControl.instance.avatarActive = true;
                break;
            case 100:
                {
                    {
                        GodControl.instance.burnTimeMax = 100;
                        questUI.text = "Quest: " + "Win! End of Build";

                        sun.SetActive(true);
                        dialog = false;
                        bigTextUI.gameObject.SetActive(false);
                    }


                }
                break;
            default:
                break;
        }
    }
 

 */