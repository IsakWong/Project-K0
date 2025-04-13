using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_SwordSlash_Ground_Fast : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnAbiBegin += (abi) => { };
            OnActionActingBegin += () =>
            {
                var pos = AbiOwner.WorldPosition;
                pos += TargetDirectionNoY * 0.5f;
                var direction = TargetDirectionNoY;
                var funcUnit = FuncUnit.Spawn(pos, direction);
                if (ActionIdx == 0)
                {
                    funcUnit.CreateSocketVisual(SwordProjectileVFX, "", Vector3.zero, Vector3.one);
                }
                else
                {
                    funcUnit.CreateSocketVisual(SwordVertcialProjectileVFX, "", Vector3.zero, Vector3.one);
                }

                var projectileDistance = funcUnit.AddUnitComponent<ProjectileDistance>();
                projectileDistance.MaxDistance = MaxDistance;
                projectileDistance.Direction = (TargetLocation - pos).normalized;
                projectileDistance.IgnoreY = false;
                projectileDistance.Speed = Speed;
                projectileDistance.MaxSpeed = 30.0f;
                projectileDistance.Acceleration = Acceleration;
                projectileDistance.FinishOnArrived = false;
                projectileDistance.DetectGround = true;
                projectileDistance.CollideLayerMask = GameUnitAPI.GetCharacterLayerMask();
                projectileDistance.OnFinish += (prject) => { funcUnit.Die(); };
                Dictionary<CharacterUnit, int> hitCounts = new();
                var timer = AddTimer(0.03f, () =>
                {
                    OverlapSphereEnemy<CharacterUnit>(funcUnit.WorldPosition, DataBoxAreaAt().z, out var ret);
                    foreach (var selection in ret)
                    {
                        float damage = DataMultipleAt() * AbiOwner.RealPhysicalDamage;
                        DamageParam damageParam = new DamageParam()
                        {
                            DamageValue = damage * 0.5f,
                            Source = AbiOwner,
                            DamageType = DamageType.PhysicalDamage,
                            ValueLevel = ValueLevel.Level1
                        };
                        if (!CharacterUnitAPI.GenericEnemyCondition(AbiOwner, selection))
                            return;
                        if (!hitCounts.ContainsKey(selection))
                        {
                            hitCounts.Add(selection, 1);
                        }
                        else
                        {
                            hitCounts[selection]++;
                        }

                        if (hitCounts[selection] == 1)
                        {
                            MovementBuff buff =
                                GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                            float distance = (funcUnit.WorldPosition - selection.WorldPosition).magnitude;
                            buff.SetLifetime(0.2f);
                            buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(funcUnit, selection));
                            buff.SetAcceleration(-2.5f);
                            buff.SetMoveSpeed(5)
                                .AddTo(AbiOwner, selection);
                        }

                        if (hitCounts[selection] < 4 && selection.TryTakeDamage(damageParam))
                        {
                        }
                    }
                }, -1).Start();
                funcUnit.OnUnitDie += () => { timer.Stop(); };
                projectileDistance.Begin();
            };
        }

        public VariantRef<float> Speed = new VariantRef<float>(5.0f);
        public VariantRef<float> MaxDistance = new VariantRef<float>(30.0f);
        public VariantRef<float> Acceleration = new VariantRef<float>(20.0f);
        public VariantRef<GameObject> SwordProjectileVFX = new VariantRef<GameObject>();
        public VariantRef<GameObject> SwordVertcialProjectileVFX = new VariantRef<GameObject>();
    }
}