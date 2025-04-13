using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public partial class CharacterUnit : GameUnit
    {
        protected List<AbilityBase> abiList = new();
        protected List<ActionAbility> actionAbiList = new();

        public List<ActionAbility> ActionAbilities
        {
            get => actionAbiList;
        }

        public ActionAbility GetActionByKey(AbilityKey key)
        {
            foreach (var actionAbi in actionAbiList)
            {
                if (actionAbi.Config.mDefaultAbiKey == key)
                    return actionAbi;
            }

            return null;
        }

        public int AbilityCount
        {
            get => abiList.Count;
        }

        public List<AbilityBase> Abilities
        {
            get => abiList;
        }

        public void AddAbility(AbilityBase abi)
        {
            if (abi == null)
            {
                global::KLog.LogError("Ability is null");
                return;
            }

            abi.AbiOwner = this;
            abi.Init();
            if (abi is ActionAbility ability)
                actionAbiList.Add(ability);
            abiList.Add(abi);
        }

        public T GetAbility<T>() where T : class
        {
            foreach (var it in abiList)
            {
                T t = it as T;
                if (t != null)
                    return t;
            }

            return null;
        }

        public ActionAbility GetAbility(string name)
        {
            foreach (var it in abiList)
            {
                if (it.GetType().Name == name)
                    return it as ActionAbility;
            }

            return null;
        }

        public AbilityBase GetAbility(int idx)
        {
            return abiList[idx];
        }

        public override void Die()
        {
            if (IsInState(CharacterStateID.DeathState))
                return;
            PendingState(CharacterStateID.DeathState);
            CharEvent.OnDie?.Invoke(this);
            var deathAudio = mCharacterConfig.DeathAudio.RandomAccess();
            if (deathAudio != null)
            {
                KGameCore.SystemAt<AudioModule>().PlayAudio(deathAudio);
            }

            Buffbase[] buffs = new Buffbase[BuffList.Count];
            BuffList.CopyTo(buffs);
            foreach (var buf in buffs)
            {
                if (buf.ForceRemoveWhenOwnerDie)
                    RemoveBuff(buf);
            }
        }
    }
}