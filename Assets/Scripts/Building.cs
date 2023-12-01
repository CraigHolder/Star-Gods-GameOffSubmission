using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : SpaceEntity
{

    public List<Resource.ResourceType> resources;
    public List<int> generationLimit;
    public List<float> generationTimes;
    public List<float> generationDelays;
    // Start is called before the first frame update
    void Start()
    {

        ClosestSun(transform);
        ClosestPlanet(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if(!currentPlanet)
        {
            if (resources.Count > 0)
            {
                for (int i = 0; i < resources.Count; i++)
                {
                    switch (resources[i])
                    {
                        case Resource.ResourceType.Metal:
                            break;
                        case Resource.ResourceType.Life:
                            if (generationLimit[i] > currentPlanet.life && currentPlanet.living)
                            {
                                if (generationTimes[i] > generationDelays[i])
                                {
                                    generationTimes[i] = 0;
                                    Resource r = Instantiate(Resources.Load<GameObject>("Life"), transform.position, Quaternion.identity).GetComponent<Resource>();
                                }
                                else
                                {
                                    generationTimes[i] += Time.deltaTime;
                                }
                            }
                            break;
                        case Resource.ResourceType.Water:
                            break;
                        case Resource.ResourceType.Energy:
                            break;
                        case Resource.ResourceType.Fuel:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
    void StackCheck()
    {

    }
}
