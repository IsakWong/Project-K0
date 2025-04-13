using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Wind : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();
        }

        public VariantRef<float> WindSpeed = new(10);
        public VariantRef<float> MaxDistance = new(30);
        public VariantRef<float> Acceleration = new(5);
        public VariantRef<float> MinSpeed = new(10);
        public VariantRef<GameObject> WindProjectileVFX = new(null);

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var funcUnit = FuncUnit.Spawn(AbiOwner.WorldPosition, Vector3.forward);
            funcUnit.CreateSocketVisual(WindProjectileVFX, "", Vector3.zero, Vector3.one);
            var projectileDistance = funcUnit.AddUnitComponent<ProjectileDistance>();
            projectileDistance.MaxDistance = MaxDistance;
            projectileDistance.Direction = TargetDirectionNoY;
            projectileDistance.Speed = WindSpeed;
            projectileDistance.MinSpeed = MinSpeed;
            projectileDistance.Acceleration = Acceleration;
            projectileDistance.FinishOnArrived = true;
            projectileDistance.DetectGround = true;
            projectileDistance.TargetPosition = AbiOwner.WorldPosition + MaxDistance * TargetDirectionNoY;
            HashSet<CharacterUnit> _Hitted = new();
            projectileDistance.Begin();
            var timer = funcUnit.AddTimer(0.30f, () => { _Hitted.Clear(); }, -1);
            timer.Start();
            projectileDistance.OnFinish += (projectile) => { funcUnit.Die(); };
            var hitCounts = new Dictionary<CharacterUnit, int>();
            funcUnit.onLogic += () =>
            {
                OverlapSphereEnemy<CharacterUnit>(funcUnit.WorldPosition, DataBoxAreaAt().z, out var ret);
                foreach (var selection in ret)
                {
                    float damage = AbiOwner.RealPhysicalDamage * 0.5f;
                    if (_Hitted.Contains(selection))
                        return;
                    DamageParam damageParam = new DamageParam()
                    {
                        DamageValue = damage,
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

                    if (hitCounts[selection] < 4 && selection.TryTakeDamage(damageParam))
                    {
                        MovementBuff buff =
                            GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                        buff.SetLifetime(0.15f);
                        buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection));
                        buff.SetMoveSpeed(8)
                            .AddTo(AbiOwner, selection);
                        _Hitted.Add(selection);
                        KGameCore.SystemAt<AudioModule>().PlayAudio(DataAudioAt(ActAbiDataKey.Key0));
                    }
                }
            };
        }
    }
}