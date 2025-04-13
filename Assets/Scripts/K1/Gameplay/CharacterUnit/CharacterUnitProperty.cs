using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace K1.Gameplay
{
    public class PropertyModifier
    {
        public CharacterUnit.CharacterProperty ModifierType;
        public bool IsFixed;
        public float Value;
        public float RealModifyValue;
    }

    public partial class CharacterUnit : GameUnit
    {
        #region PropertyModifier

        public Dictionary<CharacterProperty, float> mOriginProperty = new();
        public Dictionary<CharacterProperty, float> mRealProperty = new();

        public float RealPhysicalDamage
        {
            get
            {
                _ApplyModifier(CharacterProperty.PhysicalDamage,
                    () => mOriginProperty[CharacterProperty.PhysicalDamage],
                    (value) => mRealProperty[CharacterProperty.PhysicalDamage] = value);
                return mRealProperty[CharacterProperty.PhysicalDamage];
            }
        }

        public float RealMagicDamage
        {
            get
            {
                _ApplyModifier(CharacterProperty.MagicDamage, () => mOriginProperty[CharacterProperty.MagicDamage],
                    (value) => mRealProperty[CharacterProperty.MagicDamage] = value);
                return mRealProperty[CharacterProperty.MagicDamage] + Random.Range(0, 5);
            }
        }

        public float RealMagicDamageCriticalChance
        {
            get
            {
                _ApplyModifier(CharacterProperty.MagicDamageCriticalChance,
                    () => mOriginProperty[CharacterProperty.MagicDamageCriticalChance],
                    (value) => mRealProperty[CharacterProperty.MagicDamageCriticalChance] = value);
                return mRealProperty[CharacterProperty.MagicDamageCriticalChance];
            }
        }

        public float RealMagicDamageCriticalMultiple
        {
            get
            {
                _ApplyModifier(CharacterProperty.MagicDamageCriticalMultiple,
                    () => mOriginProperty[CharacterProperty.MagicDamageCriticalMultiple],
                    (value) => mRealProperty[CharacterProperty.MagicDamageCriticalMultiple] = value);
                return mRealProperty[CharacterProperty.MagicDamageCriticalMultiple];
            }
        }

        protected void InitProperty()
        {
            mPropertyMin[CharacterProperty.WalkSpeed] = 1.0f;


            mOriginProperty[CharacterProperty.Defence] = 0.0f;
            mOriginProperty[CharacterProperty.TakeDamageMultiple] = 1.0f;

            mOriginProperty[CharacterProperty.Mana] = 200;
            mOriginProperty[CharacterProperty.WalkSpeed] = mCharacterConfig.OriginWalkSpeed;
            mOriginProperty[CharacterProperty.Health] = mCharacterConfig.OriginLife;
            mOriginProperty[CharacterProperty.MagicDamage] = mCharacterConfig.OriginMaigcDamage;
            mOriginProperty[CharacterProperty.PhysicalDamage] = mCharacterConfig.OriginPhysicalDamage;

            mOriginProperty[CharacterProperty.MagicDamageCriticalChance] = 0.2f;
            mOriginProperty[CharacterProperty.MagicDamageCriticalMultiple] = 1.5f;
            mOriginProperty[CharacterProperty.PhysicalDamageCriticalChance] = 0.2f;
            mOriginProperty[CharacterProperty.PhysicalDamageCriticalMultiple] = 1.5f;

            mOriginProperty[CharacterProperty.CounterActionDamageMultiplier] = 1.0f;
            foreach (var it in mOriginProperty)
            {
                mRealProperty[it.Key] = it.Value;
            }
        }

        public float WalkSpeed
        {
            get
            {
                _ApplyModifier(CharacterProperty.WalkSpeed, () => mOriginProperty[CharacterProperty.WalkSpeed],
                    (value) => mRealProperty[CharacterProperty.WalkSpeed] = value);
                return mRealProperty[CharacterProperty.WalkSpeed];
            }
        }

        public float FixedDefenceValue
        {
            get
            {
                _ApplyModifier(CharacterProperty.Defence, () => mOriginProperty[CharacterProperty.Defence],
                    (value) => mRealProperty[CharacterProperty.Defence] = value);
                return mRealProperty[CharacterProperty.Defence];
            }
        }

        public float TakeDamageMultiple
        {
            get
            {
                _ApplyModifier(CharacterProperty.TakeDamageMultiple,
                    () => mOriginProperty[CharacterProperty.TakeDamageMultiple],
                    (value) => mRealProperty[CharacterProperty.TakeDamageMultiple] = value);
                return mRealProperty[CharacterProperty.TakeDamageMultiple];
            }
        }

        float OriginPropertyAt(CharacterProperty characterProperty)
        {
            return mOriginProperty[characterProperty];
        }

        float RealPropertyAt(CharacterProperty characterProperty)
        {
            return mRealProperty[characterProperty];
        }

        public float mOriginMaxHealth = 100.0f;

        [FormerlySerializedAs("mCurrentHealth")]
        public float _currentHealth = 10.0f;

        public float CurrentHealth
        {
            get => _currentHealth;
        }

        public float HealthPercent
        {
            get => _currentHealth / mOriginMaxHealth;
        }

        public float ManaPercent
        {
            get => mRealProperty[CharacterProperty.Mana] / mOriginProperty[CharacterProperty.Mana];
        }


        public enum CharacterProperty
        {
            Health = 1,
            Mana,

            WalkSpeed = 100,
            TurnSpeed,
            CastSpeed,
            Defence,
            TakeDamageMultiple,

            //魔法伤害
            MagicDamage = 200,

            //物理伤害
            PhysicalDamage,

            //魔法伤害暴击概率
            MagicDamageCriticalChance,

            //魔法伤害暴击倍数
            MagicDamageCriticalMultiple,

            //物理伤害暴击概率
            PhysicalDamageCriticalChance,

            //物理伤害暴击倍数
            PhysicalDamageCriticalMultiple,

            //反击伤害倍数
            CounterActionDamageMultiplier,
        }

        public Dictionary<CharacterProperty, List<PropertyModifier>> Modifiers = new();
        public Dictionary<CharacterProperty, bool> ModifierDirtyFlag = new();

        public Dictionary<CharacterProperty, float> mPropertyMin = new();
        public Dictionary<CharacterProperty, float> mPropertyMax = new();

        public PropertyModifier AddPropertyModifier(CharacterProperty type, bool isFixed, float value)
        {
            PropertyModifier modifier = new PropertyModifier();
            modifier.ModifierType = type;
            modifier.IsFixed = isFixed;
            modifier.Value = value;
            if (!Modifiers.ContainsKey(type))
                Modifiers[type] = new List<PropertyModifier>();
            Modifiers[type].Add(modifier);
            ModifierDirtyFlag[type] = true;
            return modifier;
        }

        public void RemovePropertyModifier(PropertyModifier modifier)
        {
            var type = modifier.ModifierType;
            ModifierDirtyFlag[type] = true;
            Modifiers[type].Remove(modifier);
        }


        public void _ApplyModifier(CharacterProperty modifierType, Func<float> getter, Action<float> setter)
        {
            if (!ModifierDirtyFlag.ContainsKey(modifierType))
            {
                setter.Invoke(getter.Invoke());
                return;
            }

            if (ModifierDirtyFlag[modifierType])
            {
                float oldValue = getter.Invoke();
                float newValue = oldValue;
                float multiplier = 1;
                foreach (var modifier in Modifiers[modifierType])
                {
                    float prevValue = newValue;
                    if (modifier.IsFixed)
                        newValue += modifier.Value;
                    else
                        multiplier += modifier.Value;
                    modifier.RealModifyValue = modifier.Value;
                    if (mPropertyMin.ContainsKey(modifierType))
                    {
                        if (newValue < mPropertyMin[modifierType])
                        {
                            newValue = mPropertyMin[modifierType];
                            modifier.RealModifyValue = newValue - prevValue;
                        }
                    }
                }

                setter.Invoke(newValue * multiplier);
            }

            ModifierDirtyFlag[modifierType] = false;
        }

        #endregion
    }
}