using System;
using System.Collections.Generic;
using UnityEngine;

public class SplashManager : MonoBehaviour
{
    public SplashManager Instance;

    public GameObject SplashPrefab;

    public List<GameObject> SplashPool;
    public List<AudioClip> SplashSFX;

    public int MaxSplashes = 3;

    private void Awake()
    {
        Instance = this;
    }

    void GenerateSplash(Vector3 position, Quaternion rotation)
    {
        Vector3 pos = position;
        bool found = false;
        foreach (var it in SplashPool)
        {
            if ((pos - it.transform.position).magnitude < 1.0f)
            {
                found = true;
            }
        }

        if (!found)
        {
            var newSplash = GameObject.Instantiate(SplashPrefab);
            SplashPool.Add(newSplash);
            newSplash.transform.position = pos;
            newSplash.transform.rotation = rotation;
        }
    }
}