using System;
using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Buffbase
    {
        // Buff读取配置
        public BuffConfig BuffConfig;

        //Buff被添加
        public Action<Buffbase> OnBuffBegin;

        //Buff更新
        public Action<Buffbase> OnBuffLogic;

        //Buff强行Remove
        public Action<Buffbase> OnBuffBreak;

        //Buff移除
        public Action<Buffbase> OnBuffEnd;

        // Buff拥有者
        public CharacterUnit BuffSource;

        // Buff拥有者
        public CharacterUnit BuffOwner;

        // 不允许添加多个
        [SerializeField] public bool IsDebuff = false;
        [SerializeField] public bool EnableVisual = true;

        // Buff生命周期
        protected float BuffLifeTime;

        public float BuffLifetime
        {
            get => BuffLifeTime;
        }

        public List<UnitVfxConfig> mVisualEffectConfigs = new();

        public List<Vfx> mBuffVisuals = new();

        //死亡的角色能否添加Buff
        public bool mDeathCharacterAddable = false;

        //拥有者死亡时强制移除Buff
        public bool ForceRemoveWhenOwnerDie = true;


        // 准备强制移除的Buff
        public bool ForceRemove = false;

        // 生命周期结束的Buff
        public bool _isFinish = false;

        public float RemainLifeTime
        {
            get { return _remainLifetime; }
        }

        public bool IsAlive
        {
            get => !_isFinish;
        }

        public float Percent
        {
            get
            {
                if (_remainLifetime > 0) return _remainLifetime / _remainLifetime;
                return 1;
            }
        }

        private float _remainLifetime;

        public Buffbase SetLifetime(float value)
        {
            BuffLifeTime = value;
            _remainLifetime = BuffLifeTime;
            return this;
        }

        protected Buffbase()
        {
        }

        public void AddTo(CharacterUnit source, CharacterUnit target)
        {
            if (BuffOwner != null)
                return;
            BuffSource = source;
            BuffOwner = target;
            if (source.IsEnemy(target))
                IsDebuff = true;
            if (CheckBuffAddable(source, target))
            {
                target.AddBuff(this);
            }
        }

        public virtual void Logic(float logicTime)
        {
            if (_remainLifetime > 0)
            {
                _remainLifetime -= logicTime;
                OnLogic();
            }
            else
            {
                _isFinish = true;
            }
        }

        public virtual void Init()
        {
            if (BuffConfig != null)
            {
                mVisualEffectConfigs = BuffConfig.mBuffVisualEffects;
            }
        }

        public bool HasVFX = true;

        public virtual void BuffAdd()
        {
            if (BuffConfig)
                BuffOwner.Log($"获得Buff效果 <{BuffConfig.Prototype.TypeName}>");
            _remainLifetime = BuffLifeTime;
            foreach (var it in mVisualEffectConfigs)
            {
                if (it.mVisualPrefab && HasVFX)
                {
                    var socket = it.mCustomSocket != "" ? it.mCustomSocket : it.mBuiltinSocket.ToString();
                    var visual = BuffOwner.CreateSocketVisual(it.mVisualPrefab,
                        socket, it.mOffset, it.mScale, -1f);
                    mBuffVisuals.Add(visual);
                }
            }

            OnBuffBegin?.Invoke(this);
        }

        public virtual void OnLogic()
        {
            OnBuffLogic?.Invoke(this);
        }

        public virtual void BuffEnd()
        {
            if (BuffConfig != null)
                BuffOwner.Log($"失去Buff效果 <{BuffConfig.Prototype.TypeName}>");
            foreach (var it in mBuffVisuals)
            {
                if (it)
                {
                    it.Die();
                }
            }

            mBuffVisuals.Clear();
            OnBuffEnd?.Invoke(this);
        }

        public virtual void BuffBreak()
        {
            OnBuffBreak?.Invoke(this);
        }

        public virtual bool CheckBuffAddable(CharacterUnit source, CharacterUnit target)
        {
            return true;
        }
    }
}