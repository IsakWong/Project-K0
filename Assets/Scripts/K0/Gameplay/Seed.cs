using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


public class Seed : InteractableItem
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

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = false;
        var handle = TreeAsset.AssetRef.LoadAssetAsync<GameObject>();
        handle.WaitForCompletion();
        //_audio.playOnAwake = false;
        // this.GetComponent<Rigidbody>().e
    }

    public Variant TreeAsset = new Variant();

    public void Grow()
    {
        if (type == SeedType.PlatformTree)
        {
            _audio.Play();
            GameObject tree = (GameObject)Instantiate(TreeAsset.AssetRef.Asset);
            tree.transform.position = transform.position + new Vector3(0, -0.12f, 0);
            
            GameObject pong = (GameObject)Instantiate(Resources.Load("PONG"));
            pong.transform.position = transform.position;  

            GameObject yw = (GameObject)Instantiate(Resources.Load("烟雾效果"));
            yw.transform.position = transform.position;

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