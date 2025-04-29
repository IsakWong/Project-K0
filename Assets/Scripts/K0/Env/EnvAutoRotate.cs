using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnvAutoRotate : MonoBehaviour
{
    public float RotateSpeed = 1.0f;
    public Vector3 RotateAxis = Vector3.up;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.RotateAround(RotateAxis, Time.fixedDeltaTime * RotateSpeed);
    }
}