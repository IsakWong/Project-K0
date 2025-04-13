using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Dash : ActionAbility
    {
        public VariantRef<GameObject> DashVFX = new(null);
        public VariantRef<float> DashSpeed = new();
        public float SpeedOnce = 0.0f;

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnActionActingBegin += () =>
            {
                var movementMentBuff = DataBuffAt().CreateBuff() as MovementBuff;
                movementMentBuff.SetDirection(TargetDirectionNoY);
                movementMentBuff.SetMoveSpeed(MathUtility.CalculateSpeed(TargetDistance, CustomActingTimeOnce));
                movementMentBuff.Acceleration = -MathUtility.CaclulateAcc(TargetDistance, CustomActingTimeOnce);
                var lifeTime = CustomActingTimeOnce;
                movementMentBuff.SetLifetime(lifeTime)
                    .AddTo(AbiOwner, AbiOwner);
            };
            OnActionActingEnd += () => { AbiOwner.SetAnimatorTrigger("Yasuo_Dash_Out"); };
            OnAbiBegin += (abi) =>
            {
                if (SpeedOnce == 0.0f)
                {
                    SpeedOnce = DashSpeed;
                }

                CustomActingTimeOnce = TargetDistance / SpeedOnce;
            };
            OnAbiEnd += (abi) => { SpeedOnce = 0.0f; };
        }
    }
}