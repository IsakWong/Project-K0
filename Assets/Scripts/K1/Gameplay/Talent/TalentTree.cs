using UnityEngine;
using UnityEngine.Rendering;

namespace K1.Gameplay
{
    public class TalentNode
    {
        public TalentNodeConfig Config;
        public bool mTalentEnable = false;
        public CharacterUnit mOwner;

        public virtual void OnTalentEnable()
        {
            mTalentEnable = true;
        }

        public virtual void OnTalentDisable()
        {
            mTalentEnable = false;
        }
    }


    public class TalentTree
    {
    }
}