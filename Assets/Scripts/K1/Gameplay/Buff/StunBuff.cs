using UnityEngine;

namespace K1.Gameplay
{
    //眩晕Buff
    public class StunBuff : Buffbase
    {
        public ValueLevel StunLevel = ValueLevel.Level2;

        public StunBuff()
        {
            IsDebuff = true;
        }

        public override void BuffAdd()
        {
            base.BuffAdd();
            BuffOwner.Stun(BuffSource, BuffLifeTime);
        }

        public override bool CheckBuffAddable(CharacterUnit source, CharacterUnit target)
        {
            if (target.GetBuff<EndureBuff>() != null)
            {
                if (target.GetBuff<EndureBuff>().EndureLevel >= StunLevel)
                {
                    return false;
                }
            }

            if (target.GetBuff<InvincibleBuff>() != null)
                return false;
            return base.CheckBuffAddable(source, target);
        }
    }
}