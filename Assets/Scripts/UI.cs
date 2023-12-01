using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    Camera mainCam;
    float camStartSize;
    Vector3 startSize;
    public GameObject thisGuy;
    public bool isArrow;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        //camStartSize = mainCam.orthographicSize;
        camStartSize = 215f;
        startSize = thisGuy.transform.localScale;
        /*Transform[] temp2 = GetComponentsInChildren<Transform>();
        for (int i = 0; i < temp2.Length; i++)
        {
            temp2[i].rotation = new Quaternion(0f, 0f, 180f, 0f);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void LateUpdate()
    {
        FixOrientation();
        FixSize();
    }

    public void FixSize()
    {
        thisGuy.transform.localScale = new Vector3(startSize.x * mainCam.orthographicSize / camStartSize,
                                            startSize.y * mainCam.orthographicSize / camStartSize,
                                            startSize.z * mainCam.orthographicSize / camStartSize);
    }

    public void FixOrientation()
    {
        if(isArrow) transform.position = GodControl.instance.RB.transform.position;
        transform.up = Vector3.up;

    }
}
