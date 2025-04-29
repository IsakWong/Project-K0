using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    // Use this for initialization

    public Transform target;

    private Vector3 offset;

    // Use this for initialization
    void Start()
    {
        offset = target.position - this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = Vector3.Slerp(this.transform.position, target.position - offset, Time.deltaTime);
    }
}