using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpIn : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        GetComponent<Rigidbody>().AddForce(new Vector3(0, 8, 1.8f), ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
    }
}