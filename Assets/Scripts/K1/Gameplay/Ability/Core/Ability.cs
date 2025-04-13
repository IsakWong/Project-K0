using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace K1.Gameplay
{
    public enum AbilityType
    {
        None,
        PassiveAbility, // 被动技能
        ActionAbility, // 动作技能（需要人物执行Action）
        ChannelAbility, // 持续施法技能
        HandoffAbility // 离手技能
    }

    public enum AbilityKey
    {
        AbilityNone,
        Ability_1,
        Ability_2,
        Ability_3,
        Ability_4,
        Ability_Q,
        Ability_W,
        Ability_E,
        Ability_R,
        Ability8,
        Ability9,
        Ability10,
        Ability_Space,
        Ability_Ctrl,
        AbilityShift,
        Ability_LeftMouse,
        Ability_RightMouse,
    }

    public enum AbilityUIType
    {
        None,
        LeftBottom,
        MiddleBottom,
        RightBottom
    }

    public class AbilityConfig : PrototypeConfig<AbilityBase>
    {
        public string mName;
        [TextArea(5, 10)] public string mDescription;
        public Sprite mSprite;

        public AbilityKey mDefaultAbiKey = AbilityKey.AbilityNone;
        public AbilityUIType AbiUIType = AbilityUIType.None;

        public virtual AbilityBase CreateAbi()
        {
            return null;
        }
    }

    public class AbilityBase
    {
        public CharacterUnit AbiOwner;
        public string mAbiName = "None";
        protected AbilityType abilityType = AbilityType.None;

        public AbilityType mAbilityType
        {
            get { return abilityType; }
        }

        private AbilityConfig _Config;

        public AbilityConfig Config
        {
            set { _Config = value; }
            get => _Config;
        }

        #region Cooldown

        public float mCooldownTime = 1f;

        private float remainingCooldownTime = 0.0f;

        public float CooldownPercent
        {
            get
            {
                if (mCooldownTime > 0)
                    return remainingCooldownTime / mCooldownTime;
                return 0;
            }
        }

        public void BeginCoolDown()
        {
            remainingCooldownTime = mCooldownTime;
        }

        public bool IsCoolingDown
        {
            get { return remainingCooldownTime > 0.0f; }
        }

        public Vector3 TargetLocation
        {
            get { return targetLocation; }
        }

        public Vector3 OwnerLocation
        {
            get { return AbiOwner.WorldPosition; }
        }

        protected Vector3 targetLocation;

        protected float TargetDistance
        {
            get { return (targetLocation - AbiOwner.WorldPosition).magnitude; }
        }

        protected Vector3 TargetDirection
        {
            get
            {
                var newDirection = DirectionToTarget;
                newDirection.Normalize();
                if (newDirection == Vector3.zero)
                {
                    return Vector3.one;
                }

                return newDirection;
            }
        }

        protected Vector3 TargetDirectionNoY
        {
            get
            {
                var newDirection = DirectionToTarget;
                newDirection.Normalize();
                newDirection.y = 0;
                newDirection.Normalize();
                if (newDirection == Vector3.zero)
                {
                    return Vector3.one;
                }

                return newDirection;
            }
        }

        protected Vector3 DirectionToTarget;

        protected GameUnit mTargetUnit;

        public GameUnit TargetUnit
        {
            get { return mTargetUnit; }
        }


        public void BeginCooldown()
        {
            if (mCooldownTime > 0)
                remainingCooldownTime = mCooldownTime;
        }

        public bool IsCooldowning()
        {
            return remainingCooldownTime > 0.0f;
        }

        #endregion


        #region Timer

        protected KTimerManager internalTimer = new KTimerManager();

        public KTimer AddTimer(float duration, Action onTimerComplete = null, int loops = 1)
        {
            return internalTimer.AddTimer(duration, onTimerComplete, loops);
        }

        #endregion

        private List<Sequence> _sequences = new();
        private List<Sequence> _toRemoveSes = new();

        public Sequence MakeSequence()
        {
            var seq = DOTween.Sequence();
            seq.SetUpdate(UpdateType.Manual);
            _sequences.Add(seq);
            seq.onKill += () => { _toRemoveSes.Add(seq); };
            return seq;
        }

        public void KillSequence(Sequence seq)
        {
            _toRemoveSes.Add(seq);
        }


        public AbilityBase()
        {
            
        }

        public float CastDistance = 10.0f;

        public virtual Vector3 CorrectTargetLocation(Vector3 walkDirection, Quaternion cameraDirection)
        {
            Vector3 newPos;
            newPos = AbiOwner.WorldPosition + cameraDirection * new Vector3(0, 0, CastDistance);
            return newPos;
        }

        public virtual void Init()
        {
            if (_Config.mName == "")
            {
                mAbiName = _Config.mName;
            }
            else
            {
                mAbiName = _Config.name;
            }
        }

        public virtual bool EndWhenKeyRelease()
        {
            return false;
        }


        public virtual bool CheckTriggerable()
        {
            if (IsCoolingDown)
                return false;
            return true;
        }

        public virtual void OnLogic(float logicTime)
        {
            if (remainingCooldownTime > 0.0f)
                remainingCooldownTime -= logicTime;
            internalTimer.OnLogic();
            // Logic Update Sequence
            foreach (var seq in _toRemoveSes)
            {
                _sequences.Remove(seq);
            }

            _toRemoveSes.Clear();
            foreach (var seq in _sequences)
            {
                seq.ManualUpdate(logicTime, logicTime);
            }

            foreach (var seq in _toRemoveSes)
            {
                _sequences.Remove(seq);
            }

            _toRemoveSes.Clear();
        }
    }
}