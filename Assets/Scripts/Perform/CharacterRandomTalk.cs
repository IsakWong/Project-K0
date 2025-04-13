using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace K1.Gameplay
{
    [Serializable]
    public class SubtitleAudio
    {
        public AudioClip audio;
        public string subtitle;
    }

    public class CharacterRandomTalk : MonoBehaviour
    {
        public CharacterUnit unit;

        public void Start()
        {
            Invoke("Talk", 5.0f);
        }

        public void Talk()
        {
        }
    }
}