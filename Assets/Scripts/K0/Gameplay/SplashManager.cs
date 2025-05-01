using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class SplashManager : MonoBehaviour
{
    public static SplashManager Instance;

    public GameObject SplashPrefab;
    public class SplashPoint
    {
        public Vector3 Pos;
        public Quaternion Rotation; 
        public DecalProjector Object;
        public float Count;
        public float LifeTime;
    }

    public List<SplashPoint> SplashPool = new List<SplashPoint>();
    public List<AudioClip> SplashSFX;

    public int MaxSplashes = 3;

    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        foreach (var it in SplashPool)
        {
            if (it.Count > 1000 && it.Object == null)
            {
                it.Object = GameObject.Instantiate(SplashPrefab).GetComponent<DecalProjector>();
                it.Object.transform.position = it.Pos;
                it.Object.transform.rotation = it.Rotation;
                it.LifeTime = 2.0f;
            }
        }
        List<SplashPoint> toRemove = new List<SplashPoint>();

        foreach (var it in SplashPool)
        {
            it.LifeTime -= Time.fixedDeltaTime;
            if (it.LifeTime <= 0)
            {
                DOTween.To( ()=> it.Object.fadeFactor, x => it.Object.fadeFactor = x, 0.0f, 1.0f).OnComplete(() =>
                {
                    Destroy(it.Object.gameObject);
                });
                
                toRemove.Add(it);
            }
        }

        foreach (var VARIABLE in toRemove)
        {
            SplashPool.Remove(VARIABLE);
        }
    }

    public void GenerateSplash(Vector3 position, Quaternion rotation)
    {
        Vector3 pos = position;
        bool found = false;
        for (int i = 0 ; i < SplashPool.Count; i++)
        {
            var it = SplashPool[i];
            if ((pos - it.Pos).magnitude < 1.0f)
            {
                it.Count = it.Count + 1.0f;
                it.LifeTime = 2.0f;
                found = true;
            }
        }
        if (!found)
        {
            if (SplashPool.Count < 10)
            {
                var splash = new SplashPoint();
                splash.LifeTime = 2.0f;
                splash.Pos = position;
                splash.Rotation = rotation;
                SplashPool.Add( splash);                
            }
        }
    }
}