using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Final_Spark : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnActionCastBreak += () =>
            {
                if (castingVFX)
                    castingVFX.Die();
            };
            OnActionActingEnd += () => { castingVFX = null; };
        }

        int count = 1;

        public VariantRef<float> StunTime = new VariantRef<float>(2.0f);
        public VariantRef<GameObject> VFX = new();
        private Vfx castingVFX = null;

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
            var pos = OwnerLocation + DataBoxAreaAt().z * 0.5f * TargetDirectionNoY + TargetLocation;
            //WarningBox(pos, DataBoxAreaAt(), TargetDirectionNoY, 1.0f, ValueLevel.LevelMax);
            Vector3 direction = TargetDirectionNoY;

            KGameCore.SystemAt<CameraModule>().FieldView(90, DataCastPointAt() - 0.2f, 0.1f, 0.1f);
            castingVFX = VfxAPI.CreateVisualEffect(VFX, AbiOwner.transform.position, direction);
            // for (int i = 1; i < count; i++)
            // {
            //     var j = i;
            //     var timer = AddTimer(i * 0.1f, () =>
            //     {
            //         Vector3 direction2 = Quaternion.AngleAxis(30 * j, Vector3.up) * TargetDirectionNoY;
            //         VfxAPI.CreateVisualEffect(vfxPrefab, AbiOwner.transform.position, direction2);
            //     });
            //     timer.Start();;
            //
            // }
            // for (int i = 1; i < count; i++)
            // {
            //     var j = i;
            //     var timer = AddTimer(i * 0.1f, () =>
            //     {
            //         Vector3 direction2 = Quaternion.AngleAxis(-30 * j, Vector3.up) * TargetDirectionNoY;
            //         VfxAPI.CreateVisualEffect(vfxPrefab, AbiOwner.transform.position, direction2);
            //     });
            // }
        }


        protected override void ActionActBegin()
        {
            KGameCore.SystemAt<CameraModule>().ShakeCamera(0.5f, KGameCore.SystemAt<CameraModule>().mHighShake);
            KGameCore.SystemAt<CameraModule>().PostProcess(0.1f, 0.5f);
            Vector3 direction = TargetDirectionNoY;
            Vector3 halfSize = DataBoxAreaAt();

            OverlapBox<CharacterUnit>(AbiOwner.transform.position + direction * halfSize.z,
                halfSize,
                direction, out var result);
            foreach (var selection in result)
            {
                DamageParam param = new DamageParam()
                {
                    Source = AbiOwner,
                    DamageType = DamageType.MagicDamage,
                    DamageValue = AbiOwner.RealMagicDamage * DataMultipleAt(),
                    ValueLevel = ValueLevel.LevelMax
                };
                if (selection.TryTakeDamage(param))
                {
                    var movementBuff = CharacterUnitAPI.CreateMovementBuff();
                    movementBuff.SetDirection(TargetDirectionNoY);
                    movementBuff.MoveLevel = ValueLevel.LevelMax;
                    movementBuff.SetAcceleration(-40)
                        .SetMoveSpeed(20)
                        .SetLifetime(0.5f)
                        .AddTo(AbiOwner, selection);

                    var buff = GameplayConfig.Instance().CreateStunBuff();
                    buff.StunLevel = ValueLevel.LevelMax;
                    buff.SetLifetime(StunTime);
                    buff.AddTo(AbiOwner, selection);
                }
            }
        }
    }
}