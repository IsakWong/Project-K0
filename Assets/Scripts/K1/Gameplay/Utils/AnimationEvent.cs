using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace K1.Gameplay
{
    public class AnimationEvent
    {
        public Action<AnimationEvent, string> OnAnimationEvent;

        public bool AutoPlayAudio = true;

        public SerializedDictionary<string, List<AudioClip>> Audios;


        public void DoAnimationEvent(string eventName)
        {
            if (AutoPlayAudio && Audios.ContainsKey(eventName))
            {
                KGameCore.SystemAt<AudioModule>().PlayAudio(Audios[eventName].RandomAccess());
            }

            OnAnimationEvent?.Invoke(this, eventName);
        }
    }
}