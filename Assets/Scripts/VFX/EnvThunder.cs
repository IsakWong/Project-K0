using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnvThunder : MonoBehaviour
{
    // Start is called before the first frame update
    public List<AudioClip> SFX;
    public Light light;

    void Start()
    {
    }

    public void PlayThunder()
    {
        var seq = DOTween.Sequence();
        seq.Append(light.DOIntensity(7, 0.1f));
        seq.Append(light.DOIntensity(1.4f, 0.1f));
        seq.Play();
        KGameCore.SystemAt<AudioModule>().PlayAudio(SFX.RandomAccess());
    }

    public void PlayThunderLoop()
    {
        PlayThunder();
        Invoke("PlayThunderLoop", Random.Range(10, 20));
    }

    // Update is called once per frame
    void Update()
    {
    }
}