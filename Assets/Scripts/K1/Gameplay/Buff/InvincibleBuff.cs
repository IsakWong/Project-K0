using System;
using UnityEngine;

namespace K1.Gameplay
{
    //无敌Buff
    public class InvincibleBuff : Buffbase
    {
        public Action OnGetHitted;

        public virtual void OnInvicibleEffect(CharacterUnit damageSource)
        {
        }
    }
}