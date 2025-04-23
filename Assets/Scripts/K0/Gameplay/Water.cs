using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : InteractableItem, IAbsorbTarget
{
    public float WaterAmount = 10.0f;
    void Start()
    {
        //_audio.playOnAwake = false;
        // this.GetComponent<Rigidbody>().e
    }
    private IAbsorbSource _realAbsorbSource = null;
    private IAbsorbSource _absorbSource = null;

    public void GetAbsorbed(IAbsorbSource absorbSource)
    {
        base.GetAbsorbed(absorbSource);
        if (_absorbSource != null)
            return;
        _absorbSource = absorbSource;
    }
    
    public bool IsAbsorbed()
    {
        return _absorbSource != null && _realAbsorbSource != null;
    }
    
    public void Released(IAbsorbSource absorbSource)
    {
        base.Released(absorbSource);
        
    }

    
    // Update is called once per frame
    private Vector3 _upSpeed;

    void FixedUpdate()
    {
        if (_absorbSource != null && _realAbsorbSource == null)
        {
            transform.position = Vector3.SmoothDamp(transform.position,
                _absorbSource.GetTransform().position, ref _upSpeed, 0.1f);
            var delta = (transform.position - _absorbSource.GetTransform().position);
            delta.y = 0;
            if (delta.magnitude < 0.1f)
            {
                _realAbsorbSource = _absorbSource;
                _realAbsorbSource.OnTargetAbsorbed(this);
            }
        }
    }

}