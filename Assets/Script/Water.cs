using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : InteractableItem, IObtainTarget
{
    public float WaterAmount = 10.0f;
    void Start()
    {
        //_audio.playOnAwake = false;
        // this.GetComponent<Rigidbody>().e
    }
    private IObtainer RealObtainer = null;
    private IObtainer Obtainer = null;

    public void GetObtained(IObtainer obtainer)
    {
        if (Obtainer != null)
            return;
        Obtainer = obtainer;
    }
    public bool IsObtained()
    {
        return Obtainer != null && RealObtainer != null;
    }
    
    public void Released(IObtainer obtainer)
    {
        
        
    }

    
    // Update is called once per frame
    private Vector3 _upSpeed;

    void FixedUpdate()
    {
        if (Obtainer != null && RealObtainer == null)
        {
            transform.position = Vector3.SmoothDamp(transform.position,
                Obtainer.GetTransform().position, ref _upSpeed, 0.1f);
            var delta = (transform.position - Obtainer.GetTransform().position);
            delta.y = 0;
            if (delta.magnitude < 0.1f)
            {
                RealObtainer = Obtainer;
                RealObtainer.OnObtained(this);
            }
        }
    }

}