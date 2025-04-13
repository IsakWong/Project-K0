using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Stab_Ground : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.NoTarget;
        }

        protected override void AbiBegin()
        {
            ActionIdx = Random.Range(0, 2);
            base.AbiBegin();
            var position = AbiOwner.transform.position;
            WarningCircle(position, DataBoxAreaAt(), DataCastPointAt(), ValueLevel.LevelMax);
        }

        public VariantRef<GameObject> BladeVFX = new();
        public VariantRef<GameObject> GroundVFX = new();

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            VfxAPI.CreateVisualEffect(ActionIdx == 0 ? BladeVFX : GroundVFX, AbiOwner.WorldPosition,
                AbiOwner.transform.forward);
            if (ActionIdx == 1)
            {
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.3f);
            }

            var damgeCenterPosition = AbiOwner.transform.position;
            float damage = DataMultipleAt() * AbiOwner.RealPhysicalDamage;
            HashSet<CharacterUnit> seletions = new();
            OverlapSphereEnemy<CharacterUnit>(damgeCenterPosition, DataBoxAreaAt(ActAbiDataKey.Key0).x, out var ret);
            foreach (var selection in ret)
            {
                DamageParam param = new DamageParam()
                {
                    DamageValue = damage,
                    DamageType = DamageType.PhysicalDamage,
                    Source = AbiOwner,
                    ValueLevel = ValueLevel.Level2,
                };
                selection.TryTakeDamage(param);

                float distance = GameUnitAPI.DistanceBetweenGameUnit(AbiOwner, selection);
                MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                    .SetLifetime(0.1f + 0.2f * (1 - distance / 5.0f)) as MovementBuff;
                buff.AddTo(AbiOwner, selection);
                buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                    .SetMoveSpeed(15)
                    .SetAcceleration(-10);

                var stun = GameplayConfig.Instance().CreateStunBuff();
                stun.StunLevel = ValueLevel.LevelMax;
                stun.SetLifetime(1.0f);
                stun.AddTo(AbiOwner, selection);
            }
        }
    }
}