using System;
using UnityEngine;

namespace K1.Gameplay
{
    //Character相关的API
    public static class CharacterUnitAPI
    {
        public static bool CheckTargetCanParry(CharacterUnit target, out DefensiveBuff defensiveBuff)
        {
            defensiveBuff = target.GetBuff<DefensiveBuff>();
            return defensiveBuff != null;
        }


        public static bool DamageBoxArea(Vector3 location, Vector3 range, Vector3 direction, DamageParam damage,
            Action<CharacterUnit> action = null)
        {
            bool ret = false;
            ForCharacterInBox(location, range, Quaternion.LookRotation(direction), (CharacterUnit target) =>
            {
                if (damage.Source.IsEnemy(target))
                {
                    ret = true;
                    DamageParam param = damage.Clone();
                    target.TakeDamage(damage);
                }
            });
            return ret;
        }

        public static bool DamageSphereArea(Vector3 location, float range, DamageParam damage,
            Action<CharacterUnit> action = null)
        {
            bool ret = false;
            ForCharacterInSphere(location, range, (CharacterUnit target) =>
            {
                if (damage.Source.IsEnemy(target))
                {
                    ret = true;
                    DamageParam param = damage.Clone();
                    target.TakeDamage(damage);
                }
            });
            return ret;
        }

        public static MovementBuff CreateMovementBuff()
        {
            return GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
        }

        public static StunBuff CreateStunBuff()
        {
            return GameplayConfig.Instance().DefaultStun.CreateBuff() as StunBuff;
        }

        public static EndureBuff CreateEndureBuff()
        {
            return GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
        }

        public static PropertyModifyBuff CreatePropertyModifyBuff()
        {
            return GameplayConfig.Instance().DefaultPropertyModify.CreateBuff() as PropertyModifyBuff;
        }

        public static void CharacterDamage(GameUnit source, CharacterUnit target, DamageParam param)
        {
            param.Source = source as CharacterUnit;
            target.TakeDamage(param);
        }

        public static void CharacterDamage(GameUnit source, CharacterUnit target, float value,
            DamageType type = DamageType.PhysicalDamage)
        {
            if (source == null || target == null) return;
            DamageParam param = new DamageParam();
            param.DamageValue = value;
            param.DamageType = type;
            param.Source = source as CharacterUnit;
            target.TakeDamage(param);
        }

        public static bool ForCharacterInSector(Vector3 location, Vector3 direction, float range, float angle,
            Action<CharacterUnit> action, Func<CharacterUnit, bool> condition = null)
        {
            return GameUnitAPI.OverlapGameUnitInSector<CharacterUnit>(
                location,
                direction,
                range,
                angle,
                GameUnitAPI.GetCharacterLayerMask(),
                action, condition);
        }

        public static bool ForCharacterInBox(Vector3 location, Vector3 range, Quaternion rotation,
            Action<CharacterUnit> action, Func<CharacterUnit, bool> condition = null)
        {
            return GameUnitAPI.OverlapGameUnitInBox<CharacterUnit>(
                location,
                range,
                rotation,
                GameUnitAPI.GetCharacterLayerMask(),
                action, condition);
        }

        public static bool ForCharacterInSphere(Vector3 location, float range, Action<CharacterUnit> action,
            Func<CharacterUnit, bool> condition = null)
        {
            return GameUnitAPI.OverlapGameUnitInSphere<CharacterUnit>(
                location, range,
                GameUnitAPI.GetCharacterLayerMask(),
                action, condition);
        }

        public static bool GenericEnemyCondition(CharacterUnit a, CharacterUnit target)
        {
            if (a == null || target == null) return false;
            if (target.IsDead) return false;
            return a.PlayerID != target.PlayerID;
        }

        public static bool IsEnemy(CharacterUnit a, CharacterUnit b)
        {
            if (a == null || b == null) return false;
            return a.PlayerID != b.PlayerID;
        }
    }
}