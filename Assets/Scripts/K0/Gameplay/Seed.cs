using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public interface IAbsorbSource
{
    void Absorb(IAbsorbTarget target);
    void OnTargetAbsorbed(IAbsorbTarget target);
    
    Component GetComponent();
    GameObject GetGameObject();
    Transform GetTransform();
} 

public interface IAbsorbTarget
{
    void GetAbsorbed(IAbsorbSource absorbSource);
    void Released(IAbsorbSource absorbSource);
    
    bool IsAbsorbed();
    
}

public class Seed : InteractableItem, IAbsorbTarget
{
    public float need_water = 10.0f;
    public float num_water = 0;
    public SeedType type;
    //public bool IsSelected = false;

    public GameObject hole;

    private AudioSource _audio;

    // Use this for initialization
    public enum SeedType
    {
        PlatformTree,
        JumpTree
    }

    public enum SeedState
    {
        OnGround,
        Floating,
        Fixed
    }

    [FormerlySerializedAs("seed_state")] public SeedState seedState = SeedState.OnGround;

    public bool IsAbsorbed()
    {
        return _absorbSource != null && _realAbsorbSource != null;
    }
    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = false;
        //_audio.playOnAwake = false;
        // this.GetComponent<Rigidbody>().e
    }

    private Transform ObtainerCenter;
    private IAbsorbSource _absorbSource = null;
    private IAbsorbSource _realAbsorbSource = null;

    public void GetAbsorbed(IAbsorbSource absorbSource)
    {
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
        switch (seedState)
        {
            case SeedState.Fixed:
                break;
            case SeedState.Floating:
                break;
            case SeedState.OnGround:
                seedState = SeedState.Floating;
                ObtainerCenter = absorbSource.GetTransform();
                var t = DOTween.To(
                    () => transform.position - ObtainerCenter.position, // Value getter
                    x => transform.position = x + ObtainerCenter.position, // Value setter
                    Vector3.zero, 
                    0.5f);
                t.SetTarget(transform);
                _absorbSource = absorbSource;
                break;
        }
    }

    public void Released(IAbsorbSource absorbSource)
    {
        GetComponent<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().WakeUp();
        if(_realAbsorbSource != null)
            GetComponent<Rigidbody>().velocity = _realAbsorbSource.GetTransform().forward * 3;
        _realAbsorbSource = null;
        _absorbSource = null;
        seedState = SeedState.OnGround;
        
    }

    public void Grow()
    {
        if (type == SeedType.PlatformTree)
        {
            _audio.Play();
            GameObject tree = (GameObject)Instantiate(Resources.Load("生长平台树_p"));
            tree.transform.position = hole.transform.position + new Vector3(0, -0.12f, 0);

            GameObject pong = (GameObject)Instantiate(Resources.Load("PONG"));
            pong.transform.position = hole.transform.position;

            GameObject yw = (GameObject)Instantiate(Resources.Load("烟雾效果"));
            yw.transform.position = hole.transform.position;

            Destroy(this.gameObject);
            SeedUI.Current.AddSeed();
        }
        if (type == SeedType.JumpTree)
        {
            _audio.Play();
            GameObject tree = (GameObject)Instantiate(Resources.Load("弹跳棉花树"));
            tree.transform.position = hole.transform.position;
            Destroy(this.gameObject);
            SeedUI.Current.AddSeed();
        }
    }
    // Update is called once per frame
    private Vector3 _upSpeed;
    void Update()
    {
        switch (seedState)
        {
            case SeedState.OnGround:
                break;
            case SeedState.Floating:               
                var delta = (transform.position - ObtainerCenter.position);
                delta.y = 0;
                if(delta.magnitude < 0.1f)
                {
                    seedState = SeedState.Fixed;
                    _realAbsorbSource = _absorbSource;
                }
                break;
            case SeedState.Fixed:
                transform.position = ObtainerCenter.position;
                
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (num_water >= need_water)
        {
            Grow();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "hole")
        {
            hole = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "hole")
        {
            hole = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "hole")
        {
            hole = other.gameObject;
        }
    }
}