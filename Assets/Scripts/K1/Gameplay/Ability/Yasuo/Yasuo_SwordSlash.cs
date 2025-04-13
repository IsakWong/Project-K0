using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_SwordSlash : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        public VariantRef<float> Speed = new VariantRef<float>(5.0f);
        public VariantRef<float> MaxDistance = new VariantRef<float>(30.0f);
        public VariantRef<float> Acceleration = new VariantRef<float>(20.0f);
        public VariantRef<GameObject> SwordVFX = new VariantRef<GameObject>(null);
        public VariantRef<GameObject> SwordExplodeVFX = new VariantRef<GameObject>(null);

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var pos = AbiOwner.WorldPosition;
            pos.y += 4.5f;
            pos.z += 0.5f;
            var direction = (TargetLocation - pos).normalized;
            var funcUnit = FuncUnit.Spawn(pos, direction);
            funcUnit.transform.LookAt(TargetLocation);
            funcUnit.CreateSocketVisual(SwordVFX, "", Vector3.zero, Vector3.one);
            var projectileDistance = funcUnit.AddUnitComponent<ProjectileDistance>();
            funcUnit.onLogic += () =>
            {
                var newPos = Utility.DetectGround(funcUnit.WorldPosition, GameUnitAPI.GetGroundMask());
                if (funcUnit.WorldPosition.y - newPos.y < 0.05f)
                {
                    projectileDistance.ForceFinish();
                }

                projectileDistance.Logic();
            };
            projectileDistance.MaxDistance = MaxDistance;
            projectileDistance.Direction = (TargetLocation - pos).normalized;
            projectileDistance.IgnoreY = false;
            projectileDistance.Speed = Speed;
            projectileDistance.MaxSpeed = 30.0f;
            projectileDistance.Acceleration = Acceleration;
            projectileDistance.FinishOnArrived = true;
            projectileDistance.DetectGround = false;
            projectileDistance.OnFinish += (projectile) =>
            {
                funcUnit.Die();
                VfxAPI.CreateVisualEffect(SwordExplodeVFX, projectileDistance.transform.position);

                OverlapSphereEnemy<CharacterUnit>(funcUnit.WorldPosition, DataBoxAreaAt(ActAbiDataKey.Key0).z,
                    out var ret);
                foreach (var selection in ret)
                {
                    float damage = DataMultipleAt() * AbiOwner.RealPhysicalDamage;
                    DamageParam damageParam = new DamageParam()
                    {
                        DamageValue = damage,
                        Source = AbiOwner,
                        DamageType = DamageType.PhysicalDamage,
                        ValueLevel = ValueLevel.Level1
                    };
                    if (!CharacterUnitAPI.GenericEnemyCondition(AbiOwner, selection))
                        return;
                    selection.TryTakeDamage(damageParam);
                    MovementBuff buff =
                        GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                    float distance = (funcUnit.WorldPosition - selection.WorldPosition).magnitude;
                    buff.SetLifetime(0.5f * Math.Min(1.0f, 5.0f / distance));
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(funcUnit, selection));
                    buff.SetAcceleration(-5.0f);
                    buff.SetMoveSpeed(10)
                        .AddTo(AbiOwner, selection);
                }
            };
            HashSet<CharacterUnit> _Hitted = new();
            projectileDistance.Begin();
        }
    }
}