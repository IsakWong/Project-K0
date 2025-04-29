using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using K0;
using K0.Gameplay;
using Obi;
using UnityEngine;
using UnityEngine.Serialization;


public class ItemSeed : ItemInteractable
{
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

    public SeedState seedState = SeedState.OnGround;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.loop = false;
        
        TreeAsset.AssetRef.LoadAssetAsync<GameObject>().WaitForCompletion();
        VFX_Pong.AssetRef.LoadAssetAsync<GameObject>().WaitForCompletion();
        JumpTreeAsset.AssetRef.LoadAssetAsync<GameObject>().WaitForCompletion();
    }

    
    public Variant TreeAsset = new Variant();
    public Variant JumpTreeAsset = new Variant();
    public Variant VFX_Pong = new Variant();

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Interactable"))
            return;
        var otherGO = other.gameObject.GetComponent<ItemInteractable>();
        var water = otherGO as ItemWater;
        if (water)
        {
            Destroy(water.gameObject);
            Grow();
        }
    }

    public void Grow()
    {
        if (type == SeedType.PlatformTree)
        {
            _audio.Play();
            var ret = SlimeCharacter.Overlap<SlimeCharacter>(transform.position, 2.0f, 1 << ProjectConstantK0.kLayerPlayer);
            foreach (var it in ret)
            {
                it.SlimeRigidBody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);
            }
            GameObject tree = (GameObject)Instantiate(TreeAsset.AssetRef.Asset);
            tree.transform.position = transform.position + new Vector3(0, -0.12f, 0);
            
            GameObject pong = (GameObject)Instantiate(VFX_Pong.AssetRef.Asset);
            pong.transform.position = transform.position;  

            Destroy(this.gameObject);
        }
        if (type == SeedType.JumpTree)
        {
            _audio.Play();
            
            GameObject tree = (GameObject)Instantiate(JumpTreeAsset.AssetRef.Asset);
            tree.transform.position = transform.position + new Vector3(0, -0.12f, 0);
                
            GameObject pong = (GameObject)Instantiate(VFX_Pong.AssetRef.Asset);
            pong.transform.position = transform.position;
            
            Destroy(this.gameObject);
            //SeedUI.Current.AddSeed();
        }
        var gameMode = KGameCore.Instance.CurrentGameMode as K0GameMode;
        gameMode.SeedGrow();
    }
}