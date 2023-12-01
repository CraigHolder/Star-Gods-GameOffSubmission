using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltControl : MonoBehaviour
{
    public float orbitSpeed = 15;

    public int num = 200;
    public int beltDistance = 1000;
    public int layers = 1;
    public float width;
    public List<Transform> beltTransforms;
    public bool inside = false;

    // Start is called before the first frame update
    void Start()
    {
        if(Planet.belts == null)
        {
            Planet.belts = new List<BeltControl>();
        }
        Planet.belts.Add(this);
        beltTransforms = new List<Transform>();
        for (int i = 0; i < num; i++)
        {
            Transform t = Instantiate(Resources.Load<GameObject>("Belt"), transform.position + ((new Vector3(Random.Range(-width / 2, width / 2), -beltDistance + Random.Range(-width/2, width / 2), 0))), Quaternion.identity).transform;
            beltTransforms.Add(t);
            transform.Rotate(0, 0, (360f) / (num/layers));
            t.parent = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, orbitSpeed * Time.deltaTime);
       if(GodControl.instance.launchDur >= GodControl.instance.launchLength)
        if (Vector3.Distance(GodControl.instance.RB.transform.position, transform.position) > beltDistance - width/2)
        {
            if (Vector3.Distance(GodControl.instance.RB.transform.position, transform.position) < beltDistance + width / 2)
            {
                inside = true;
                float shortest = Mathf.Infinity;
                int index = 0;
                Planet p = beltTransforms[index].GetComponent<Planet>();
                for (int i = 0; i < beltTransforms.Count; i++)
                {
                    float dist = Vector3.Distance(beltTransforms[i].position, GodControl.instance.RB.transform.position);
                    p = beltTransforms[i].GetComponent<Planet>();
                    if (dist < shortest)
                    {
                        shortest = dist;
                        index = i;
                        p.enabled = true;
                    }
                    else
                    {
                        p.enabled = false;
                    }
                }
                    p = beltTransforms[index].GetComponent<Planet>();
                    GodControl.instance.currentPlanet = p;
                    p.enabled = true;
                    p.CalcRadius();
                    //GodControl.instance.Land();


            }
            else
            {
                inside = false;
            }
        }
        else
        {
            inside = false;
        }


        if(Planet.belts[0] == this)
        {
            bool check = false;
            for (int i = 0; i < Planet.belts.Count; i++)
            {
                if(Planet.belts[i].inside)
                {
                    check = true;
                }
            }
            if(check)
            {
                GodControl.instance.belt = true;
            }
            else
            {
                GodControl.instance.belt = false;
            }
        }
    }
}
