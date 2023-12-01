using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = top.position - bottom.position;
        transform.up = dir;
        transform.position = bottom.position + (dir.normalized * 25f);
    }
}
