using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Blink : ActionAbility
    {
        private Lux_Attack abiLuxAttack;
        public VariantRef<GameObject> PortalVFX = new VariantRef<GameObject>();
        public VariantRef<BuffConfig> MovementConfig = new VariantRef<BuffConfig>();

        public override void Init()
        {
            base.Init();
            Indicator = new AbilityIndicator();
            Indicator.Abi = this;
            Indicator.Owner = AbiOwner;

            HighPiority = true;
            AutoVFX = fakeLux;
            CastType = ActionCastType.Location;
            OnAbiBegin += (ability) =>
            {
                var pos = AbiOwner.WorldPosition;
            };
            HashSet<CharacterUnit> hitCounts = new HashSet<CharacterUnit>();
            OnActionActingBegin += () =>
            {
                hitCounts.Clear();
                var movementBuff = MovementConfig.As().CreateBuff() as MovementBuff;
                movementBuff.SetDirection(TargetDirectionNoY);
                movementBuff.Speed = 20.0f;
                movementBuff.SetAcceleration(-10.0f);
                movementBuff.SetLifetime(1.0f);
                movementBuff.AddTo(AbiOwner, AbiOwner);
                KGameCore.SystemAt<CameraModule>().FieldView(100, 0.3f, 0.5f, 0.1f);
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);
                KGameCore.SystemAt<CameraModule>().PostProcess(0.1f, 0.8f);
            };
            OnActionActing += () =>
            {
                OverlapSphereEnemy<CharacterUnit>(AbiOwner.WorldPosition, DataBoxAreaAt().z, out var ret);
                foreach (var selection in ret)
                {
                    if (hitCounts.Contains(selection))
                        continue;
                    DamageParam param = new DamageParam()
                    {
                        Source = AbiOwner,
                        DamageType = DamageType.MagicDamage,
                        DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage
                    };
                    hitCounts.Add(selection);
                    if (selection.TryTakeDamage(param))
                    {
                        KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.1f, 0.3f);
                        var movementBuff = CharacterUnitAPI.CreateMovementBuff();
                        float moveSpeed = 5;
                        moveSpeed = Mathf.Min(15, moveSpeed);
                        movementBuff.SetDirection((AbiOwner.DirectionToTarget(selection)).normalized)
                            .SetMoveSpeed(moveSpeed)
                            .SetLifetime(0.3f)
                            .AddTo(AbiOwner, selection);
                    }
                }
            };
        }

        private Vfx fakeLux;

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
        }
    }
}