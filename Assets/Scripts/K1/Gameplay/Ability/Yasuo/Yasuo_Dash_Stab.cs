using System;
using System.Collections.Generic;
using System.Timers;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Dash_Stab : ActionAbility
    {
        public bool Y = false;
        public VariantRef<float> Height = new();
        public VariantRef<float> UpDuration = new();
        public VariantRef<float> FloatDuration = new();
        public VariantRef<GameObject> StartVFX = new();
        public VariantRef<GameObject> TargetVFX = new();
        Vector3 startPos = Vector3.zero;
        float timePassed = 0.0f;
        float ySpeed = 0.0f;

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnActionActingBegin += () =>
            {
                startPos = AbiOwner.WorldPosition;
                ySpeed = MathUtility.CalculateSpeed(Height, UpDuration * DataActingTimeAt());
                timePassed = 0;
                VfxAPI.CreateVisualEffect(StartVFX
                    , AbiOwner.WorldPosition, TargetDirectionNoY);
            };
            OnActionActingEnd += () =>
            {
                var _hitted = new HashSet<CharacterUnit>();
                var floor = AbiOwner.DetectFloor();
                AbiOwner.EnableGravity = true;

                AbiOwner.characterMovement.constrainToGround = true;
                AbiOwner.WorldPosition = floor;
                VfxAPI.CreateVisualEffect(TargetVFX, AbiOwner.WorldPosition, TargetDirectionNoY);
                OverlapSphereEnemy<CharacterUnit>(AbiOwner.WorldPosition, DataBoxAreaAt().z, out var ret);
                foreach (var selection in ret)
                {
                    if (_hitted.Contains(selection))
                        continue;
                    _hitted.Add(selection);
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = AbiOwner.RealPhysicalDamage * DataMultipleAt(),
                        DamageType = DamageType.PhysicalDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level2
                    };
                    selection.TryTakeDamage(param);

                    var stun = GameplayConfig.Instance().CreateStunBuff();
                    stun.StunLevel = ValueLevel.LevelMax;
                    stun.SetLifetime(1.0f);
                    stun.AddTo(AbiOwner, selection);
                    MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                        .SetLifetime(0.4f) as MovementBuff;
                    buff.AddTo(AbiOwner, selection);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                        .SetMoveSpeed(15);
                    buff.SetAcceleration(-15.0f);
                    KGameCore.SystemAt<CameraModule>()
                        .ShakeCamera(0.5f, KGameCore.SystemAt<CameraModule>().mHighShake);
                }
            };
            OnActionActing += () =>
            {
                Vector3 newPos = AbiOwner.WorldPosition;
                float upDuration = UpDuration * DataActingTimeAt();
                float floatingDuration = FloatDuration * DataActingTimeAt();
                AbiOwner.EnableGravity = false;
                AbiOwner.characterMovement.constrainToGround = false;
                float downDuration = (1 - FloatDuration - UpDuration) * DataActingTimeAt();

                float lerp = timePassed / DataActingTimeAt();
                if (lerp > 1)
                    lerp = 1;
                Vector3 newX = Vector3.Lerp(startPos, TargetLocation, lerp);
                if (timePassed <= upDuration)
                {
                    float yAccleration = MathUtility.CaclulateAcc(Height, upDuration);
                    newPos.x = newX.x;
                    newPos.z = newX.z;
                    newPos.y += ySpeed * KTime.scaleDeltaTime;
                    ySpeed -= KTime.scaleDeltaTime * yAccleration;

                    AbiOwner.WorldPosition = newPos;
                }
                else if (timePassed >= upDuration && timePassed <= upDuration + floatingDuration)
                {
                    ySpeed = 0.0f;
                }
                else if (timePassed >= upDuration + floatingDuration)
                {
                    float yAccleration =
                        MathUtility.CaclulateAcc(AbiOwner.WorldPosition.y - TargetLocation.y, downDuration);
                    newPos.x = newX.x;
                    newPos.z = newX.z;
                    newPos.y += ySpeed * KTime.scaleDeltaTime;

                    AbiOwner.WorldPosition = newPos;
                    ySpeed -= KTime.scaleDeltaTime * yAccleration;
                }

                timePassed += KTime.scaleDeltaTime;
            };
        }
    }
}