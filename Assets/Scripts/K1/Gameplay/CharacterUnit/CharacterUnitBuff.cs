using System;
using System.Collections.Generic;

namespace K1.Gameplay
{
    public partial class CharacterUnit : GameUnit
    {
        #region Buff

        protected List<Buffbase> _toAddBuff = new List<Buffbase>();
        protected List<Buffbase> BuffList = new List<Buffbase>();

        //添加Buff回调
        public Action<Buffbase> OnBuffAdd;

        //移除Buff回调
        public Action<Buffbase> OnBuffRemove;

        public List<Buffbase> Buffs
        {
            get => BuffList;
        }

        public T GetBuff<T>() where T : Buffbase
        {
            foreach (var buff in BuffList)
            {
                if (buff is T & !buff.ForceRemove)
                {
                    return buff as T;
                }
            }

            foreach (var buff in _toAddBuff)
            {
                if (buff is T)
                {
                    return buff as T;
                }
            }

            return null;
        }

        public bool AddBuff(Buffbase buff)
        {
            GameplayModule.CheckInLogic();
            if (IsDead)
                return false;
            buff.BuffOwner = this;
            _toAddBuff.Add(buff);
            buff.Init();
            return true;
        }

        public bool RemoveBuff(Buffbase buff)
        {
            if (buff == null)
                return false;
            GameplayModule.CheckInLogic();
            if (buff.BuffOwner is null || buff.ForceRemove)
                return false;
            buff.ForceRemove = true;
            return true;
        }

        #endregion
    }
}