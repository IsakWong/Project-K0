using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace K1.Gameplay
{
    [CreateAssetMenu(fileName = "Character", menuName = "角色配置")]
    public class CharacterConfig : CommonConfig
    {
        public float OriginWalkSpeed = 4.0f;
        public float OriginLife = 100.0f;
        public float AttackCastPoint = 0.0f;
        public float AttackBackswingTime = 0.0f;
        public float OriginPhysicalDamage = 5;
        public float OriginMaigcDamage = 5;


        public string Name = "英雄";
        public string ShortName = "英雄";

        [TextArea(5, 10)] public string Description = "";

        public float AttackRange = 1;
        public List<GameObject> AttackProjectile;
        public List<string> AttackTrigger;
        public List<AbilityConfig> OriginAbilityList = new List<AbilityConfig>();
        public List<AudioClip> CharacterAudio = new();
        public List<AudioClip> ChangeAudio = new();
        public List<AudioClip> DeathAudio = new();
        public List<AudioClip> BreakActionAudio = new();
        public List<AudioClip> TakeDamageAudio = new();
        public List<AudioClip> StunAudio = new();


        [SerializedDictionary("Key", "Variant")]
        public SerializedDictionary<AudioClip, string> CharacterAudioTitle = new();

        public Sprite Avatar;
        public List<TalentNodeConfig> TalentConfigs;
    }
}