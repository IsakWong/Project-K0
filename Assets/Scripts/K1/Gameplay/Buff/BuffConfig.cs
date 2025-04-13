using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    [CreateAssetMenu(fileName = "Buff", menuName = "Buff配置")]
    public class BuffConfig : PrototypeConfig<Buffbase>
    {
        public string mBuffPrototypeName;

        public Sprite mBuffIcon;
        public string mBuffName;


        [TextArea] public string mBuffDesc;

        public List<UnitVfxConfig> mBuffVisualEffects = new List<UnitVfxConfig>();


        public Buffbase CreateBuff()
        {
            var buffInstance = CreateInstance();
            buffInstance.BuffConfig = this;
            return buffInstance;
        }
    }
}