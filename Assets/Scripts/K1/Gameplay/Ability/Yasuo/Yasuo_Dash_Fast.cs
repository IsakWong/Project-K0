using System;
using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Dash_Fast : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastDistance = 10.0f;
            CastType = ActionCastType.Location;
            OnActionActingEnd += () =>
            {
                AbiOwner.SetAnimatorBool("Yasuo_Dashing", false);
                var pos = AbiOwner.WorldPosition + AbiOwner.transform.forward * 0.5f;
                //var obj = VfxAPI.CreateVisualEffect(DataVisualAt(ActAbiDataKey.Key2), pos, AbiOwner.transform.forward);
                // VfxAPI.CreateVisualEffect(DataVisualAt(ActAbiDataKey.Key1), AbiOwner.WorldPosition + AbiOwner.transform.forward * 0.5f,
                //     AbiOwner.transform.forward);
                OverlapSphereEnemy<CharacterUnit>(
                    AbiOwner.WorldPosition + AbiOwner.transform.forward * DataBoxAreaAt().z,
                    DataBoxAreaAt().z, out var result);
                foreach (var selection in result)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = 10.0f,
                        DamageType = DamageType.PhysicalDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level2
                    };
                    selection.TryTakeDamage(param);
                    HitParam hit = new HitParam(AbiOwner)
                    {
                        HitAnimTrigger = "Hit",
                        HitTime = 0.3f,
                        ValueLevel = ValueLevel.Level2
                    };
                    selection.TryGetHitted(hit);
                    MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                        .SetLifetime(0.2f) as MovementBuff;
                    buff.AddTo(AbiOwner, selection);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                        .SetMoveSpeed(5)
                        .SetAcceleration(10.0f);
                }

                ;
            };
            float distance = -1;
            int count = 1;
            OnActionActing += () =>
            {
                int loops = (int)(TargetDistance / 7.0f) + 1;
                if (distance < 0 || distance > 7.0f)
                {
                    var pos = AbiOwner.WorldPosition + AbiOwner.transform.forward * 0.5f;
                    pos.y += 0.75f;
                    var vfx = VfxAPI.CreateVisualEffect(DashMiddleVFX, pos,
                        AbiOwner.transform.forward);
                    float scale = (0.2f + count / loops * 0.5f);
                    scale = Math.Max(1.5f, scale);
                    //vfx.transform.localScale = scale  * Vector3.one;
                    distance = 0.0f;
                    count++;
                }

                distance += DashSpeed * KTime.scaleDeltaTime;
            };
        }

        public VariantRef<GameObject> DashVFX = new VariantRef<GameObject>();
        public VariantRef<GameObject> DashMiddleVFX = new VariantRef<GameObject>();

        public void CreateVisual(Vector3 position, Vector3 target, bool playAudio = true)
        {
            var obj = VfxAPI.CreateVisualEffect(DashVFX, position, target);
            if (playAudio == false)
                //obj.gameObject.GetComponent<RandomAudioClip>().AutoPlay = false;
                return;
        }


        public float DashSpeed = 30;

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
            AbiOwner.SetAnimatorBool("Yasuo_Dashing", true);
            CreateVisual(AbiOwner.WorldPosition, TargetDirectionNoY);
            float speed = DashSpeed;

            var pos = AbiOwner.WorldPosition + AbiOwner.transform.forward * 0.5f;
            pos.y += 0.75f;
            var vfx = VfxAPI.CreateVisualEffect(DashMiddleVFX, pos,
                AbiOwner.transform.forward);

            var movementMentBuff = DataBuffAt().CreateBuff() as MovementBuff;
            movementMentBuff.SetDirection(TargetDirectionNoY);
            movementMentBuff.SetMoveSpeed(speed);
            var lifeTime = TargetDistance / speed;
            movementMentBuff.SetLifetime(lifeTime)
                .AddTo(AbiOwner, AbiOwner);

            var buff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
            buff.SetLifetime(lifeTime);
            buff.AddTo(AbiOwner, AbiOwner);
        }

        protected override void AbiBegin()
        {
            CustomActingTimeOnce = TargetDistance / DashSpeed;
            base.AbiBegin();
        }
    }
}