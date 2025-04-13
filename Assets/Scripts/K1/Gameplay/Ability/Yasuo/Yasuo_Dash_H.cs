using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Dash_H : ActionAbility
    {
        public VariantRef<GameObject> DashVFX = new(null);
        public VariantRef<float> DashSpeed = new();
        public VariantRef<BuffConfig> Buff = new();
        public float SpeedOnce = 0.0f;
        public float DashAngle = 90.0f;

        public override void Init()
        {
            base.Init();
            float FinalSpeed = DashSpeed * 0.5f;
            CastType = ActionCastType.Location;
            OnActionCastBegin += () =>
            {
                if (SpeedOnce == 0.0f)
                {
                    SpeedOnce = DashSpeed;
                }

                var movementMentBuff = Buff.As().CreateBuff() as MovementBuff;
                movementMentBuff.SetDirection(MathUtility.RotateDirectionY(TargetDirectionNoY, DashAngle));
                movementMentBuff.SetMoveSpeed(MathUtility.CalculateSpeed(5.0f, DataActingTimeAt()));
                movementMentBuff.Acceleration = -MathUtility.CaclulateAcc(5.0f, DataActingTimeAt());
                var lifeTime = DataActingTimeAt();
                movementMentBuff.SetLifetime(lifeTime)
                    .AddTo(AbiOwner, AbiOwner);
                var invincible = GameplayConfig.Instance().DefaultInvicible.CreateBuff() as InvincibleBuff;
                invincible.SetLifetime(lifeTime)
                    .AddTo(AbiOwner, AbiOwner);
            };
            OnActionActingBegin += () => { };
            OnAbiBegin += (abi) =>
            {
                DashAngle = Random.Range(0, 2) == 0 ? 90.0f : -90.0f;
                if (DashAngle == 90.0f)
                    ActionIdx = 0;
                else
                    ActionIdx = 1;
            };
            OnAbiEnd += (abi) => { SpeedOnce = 0.0f; };
        }
    }
}