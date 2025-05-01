using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class WaterReceiver : MonoBehaviour
{
    float _waterLevel = 0.0f;
    public float MaxWater = 100.0f;
    public ItemSeed attachSeed;
    private bool growed = false;
    public AudioSource Audio;
    private float delta = 0.0f;
    public MeshRenderer mesh;

    public void ReceiveWater(float val)
    {
        delta = val;
        _waterTime = 0.0f;
        if (_waterLevel > MaxWater && !growed)
        {
            growed = true;
            attachSeed.Grow();
        }
    }

    private float _waterTime = 0.0f;
    private Color oldColor;

    private void FixedUpdate()
    {
        if (delta == 0.0f && _waterLevel != 0.0f)
        {
            _waterTime += Time.fixedDeltaTime;
            if (_waterTime < 0.3f)
            {
                if (!Audio.isPlaying)
                {
                    Audio.Play();
                    Audio.UnPause();
                }

                _waterLevel += delta * Time.fixedDeltaTime;
                Audio.pitch = 1.0f + (_waterLevel / MaxWater);
                float colorFactor = 1.0f - (_waterLevel / MaxWater) * 0.4f;
                mesh.material.SetColor("_BaseColor", oldColor * colorFactor);
            }
            else
            {
                Audio.Pause();
            }
        }

        if (delta != 0.0f && _waterLevel < MaxWater)
        {
            _waterLevel += delta * Time.fixedDeltaTime;
            float colorFactor = 1.0f - (_waterLevel / MaxWater) * 0.4f;
            mesh.material.SetColor("_BaseColor", oldColor * colorFactor);
            if (!Audio.isPlaying)
            {
                Audio.Play();
                Audio.pitch = 1.0f + (_waterLevel / MaxWater);
                Audio.UnPause();
            }
        }

        delta = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        oldColor = mesh.material.GetColor("_BaseColor");
    }

    // Update is called once per frame
    void Update()
    {
    }
}