using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public interface IObtainer
{
    void Obtain(IObtainTarget target);
    Component GetComponent();
    GameObject GetGameObject();
    
    Transform GetTransform();
} 
public interface IObtainTarget
{
    void GetObtained(IObtainer obtainer);
    void Released(IObtainer obtainer);
    
}

public class Seed : InteractableItem, IObtainTarget
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
        putong,
        tantiao
    }

    public enum SeedState
    {
        OnGround,
        Floating,
        Fixed
    }

    [FormerlySerializedAs("seed_state")] public SeedState seedState = SeedState.OnGround;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = false;
        //_audio.playOnAwake = false;
        // this.GetComponent<Rigidbody>().e
    }

    private Transform ObtainerCenter;
    private IObtainer Obtainer = null;
    private IObtainer RealObtainer = null;

    public void GetObtained(IObtainer obtainer)
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
                ObtainerCenter = obtainer.GetTransform();
                Obtainer = obtainer;
                break;
        }
    }

    public void Released(IObtainer obtainer)
    {
        GetComponent<Collider>().isTrigger = false;
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().WakeUp();
        GetComponent<Rigidbody>().velocity = RealObtainer.GetTransform().forward * 3;
        RealObtainer = null;
        Obtainer = null;
        seedState = SeedState.OnGround;
        
    }

    public void Grow()
    {
        if (type == SeedType.putong)
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
        if (type == SeedType.tantiao)
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
                transform.position = Vector3.SmoothDamp(transform.position,
                    ObtainerCenter.position, ref _upSpeed, 0.1f);
                var delta = (transform.position - ObtainerCenter.position);
                delta.y = 0;
                if(delta.magnitude < 0.1f)
                {
                    seedState = SeedState.Fixed;
                    RealObtainer = Obtainer;
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