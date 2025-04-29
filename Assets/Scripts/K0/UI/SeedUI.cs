using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedUI : MonoBehaviour
{
    public static SeedUI Current;
    public int MaxSeed;
    private AudioSource _audio;

    public AudioClip bombSFX;

    // Use this for initialization
    void Start()
    {
        Current = this;
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.loop = false;
    }

    int i = 0;

    // Update is called once per frame
    void Update()
    {
    }

    public void AddSeed()
    {
        i++;
        //_audio.clip = bombSFX;
        _audio.PlayOneShot(bombSFX);
    }
}