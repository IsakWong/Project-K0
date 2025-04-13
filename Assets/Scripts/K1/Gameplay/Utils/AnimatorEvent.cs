using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using K1.Gameplay;
using UnityEngine;

public class AnimatorEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Owner = GetComponentInParent<CharacterUnit>();
    }

    [SerializedDictionary("Key", "Value")] public SerializedDictionary<string, List<AudioClip>> Clips;

    [SerializedDictionary("Key", "Value")] public SerializedDictionary<string, UnitVfxConfig> Vfx;

    public CharacterUnit Owner;

    public void CreateVFX(string vfx)
    {
        var it = Vfx[vfx];
        if (it.mVisualPrefab)
        {
            var socket = it.mCustomSocket != "" ? it.mCustomSocket : it.mBuiltinSocket.ToString();
            var visual = Owner.CreateSocketVisual(it.mVisualPrefab,
                socket, it.mOffset, it.mScale, 0);
        }
    }

    public void PlayAudio(string audioTag)
    {
        if (Clips.ContainsKey(audioTag))
            KGameCore.SystemAt<AudioModule>()
                .PlayAudioAtPosition(Clips[audioTag].RandomAccess(), transform.position, true);
    }

    public void PlayCharacterAudio(string audioTag)
    {
        if (Clips.ContainsKey(audioTag) && Owner != null)
            Owner.PlayAudioByChannel(Clips[audioTag].RandomAccess(), stopOthers: false);
    }

    // Update is called once per frame
    public void OnEvent(string audioTag)
    {
        if (Clips.ContainsKey(audioTag) && Owner != null)
            Owner.PlayAudioByChannel(Clips[audioTag].RandomAccess(), stopOthers: false);
    }
}