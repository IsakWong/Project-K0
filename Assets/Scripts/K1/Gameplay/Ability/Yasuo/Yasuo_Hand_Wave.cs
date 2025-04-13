using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Hand_Wave : ActionAbility
    {
        private Vfx Vfx;
        public VariantRef<GameObject> ChargeVFX = new();
        public VariantRef<GameObject> ImpactVFX = new();

        public override void Init()
        {
            CastType = ActionCastType.Location;
            base.Init();
            OnActionCastBegin += () =>
            {
                Vfx = AbiOwner.CreateSocketVisual(ChargeVFX, BuiltinCharacterSocket.RightHand.ToString(), Vector3.zero,
                    Vector3.one);
                Vfx.mLifeTime = DataCastPointAt();
            };
            OnActionActingBegin += () =>
            {
                Trigger(AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.LeftHand), TargetDirectionNoY);
            };
        }

        public VariantRef<float> StunTime = new VariantRef<float>(2.0f);
        public float MoveDistance = 4.0f;

        public void Trigger(Vector3 postion, Vector3 diection)
        {
            Vfx.Die();
            VfxAPI.CreateVisualEffect(ImpactVFX,
                postion, diection);
            KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);

            GameUnitAPI.OverlapGameUnitInBox<EnvUnit>(postion, DataBoxAreaAt(),
                Quaternion.LookRotation(diection),
                GameUnitAPI.GetEnvLayerMask(),
                (envUnit) =>
                {
                    var rigibody = envUnit.GetComponent<Rigidbody>();
                    if (rigibody)
                        rigibody.AddForce(TargetDirectionNoY * 10, ForceMode.Impulse);
                });
            bool hitted = false;
            OverlapBox<CharacterUnit>(postion + diection * DataBoxAreaAt().z * 0.5f, DataBoxAreaAt(),
                diection, out var ret);
            foreach (var selection in ret)
            {
                DamageParam param = new DamageParam()
                {
                    DamageType = DamageType.PhysicalDamage,
                    Source = AbiOwner,
                    DamageValue = DataMultipleAt() * AbiOwner.RealPhysicalDamage,
                    ValueLevel = ValueLevel.Level2
                };
                selection.TryTakeDamage(param);
                var stun = GameplayConfig.Instance().CreateStunBuff() as StunBuff;
                stun.StunLevel = ValueLevel.LevelMax;
                stun.SetLifetime(StunTime);
                stun.AddTo(AbiOwner, selection);

                float moveDistance = DataBoxAreaAt(ActAbiDataKey.Key2).z * 2 -
                                     GameUnitAPI.DistanceBetweenGameUnit(selection, AbiOwner);
                hitted = true;
                if (moveDistance > 0)
                {
                    var movementBuff = CharacterUnitAPI.CreateMovementBuff();
                    float moveSpeed = MathUtility.CalculateSpeed(moveDistance, 0.5f);
                    movementBuff.SetDirection(TargetDirectionNoY)
                        .SetAcceleration(moveSpeed)
                        .SetMoveSpeed(-moveSpeed)
                        .SetLifetime(0.5f)
                        .AddTo(AbiOwner, selection);
                }
            }

            if (hitted)
            {
                AbiOwner.SetAnimatorSpeed(0.0f, 0.21f);
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);
            }

            var movementBuff2 = CharacterUnitAPI.CreateMovementBuff();
            float moveSpeed2 = MathUtility.CalculateSpeed(MoveDistance, 0.5f);
            movementBuff2.SetDirection(-TargetDirectionNoY)
                .SetMoveSpeed(moveSpeed2)
                .SetAcceleration(-MathUtility.CaclulateAcc(MoveDistance, 0.5f))
                .SetLifetime(0.5f)
                .AddTo(AbiOwner, AbiOwner);
        }
    }
}