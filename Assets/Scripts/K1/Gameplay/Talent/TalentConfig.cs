using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace K1.Gameplay
{
    [CreateAssetMenu(fileName = "TalentTree", menuName = "天赋树")]
    public class TalentTreeConfig : PrototypeConfig<TalentTree>
    {
        [FormerlySerializedAs("mTalentConfigs")]
        public List<TalentNodeConfig> TalentConfigs;
    }

    [CreateAssetMenu(fileName = "Talent", menuName = "天赋")]
    [Serializable]
    public class TalentNodeConfig : PrototypeConfig<TalentNode>
    {
        public Sprite TalentIcon;
        public string TalentName;
        [TextArea] public string TalentDesc;

        public VideoClip Video;
        [SerializedDictionary("Key", "Value")] public SerializedDictionary<string, Variant> Data;

        public TalentNode CreateTalent()
        {
            TalentNode node = CreateInstance();
            node.Config = this;
            return node;
        }
    }
}