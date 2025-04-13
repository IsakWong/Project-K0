using System.Collections.Generic;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Lux_Attack2 : ActionAbility
    {
        public ObjectVariantRef<GameObject> Vfx0 = new();
        public ObjectVariantRef<GameObject> VfxMissile = new();
        public VariantRef<float> ProjectileSpeed = new(7.0f);
        public VariantRef<float> ProjectileAcceleration = new(10.0f);

        public VariantRef<float> DamageDelta = new(0.05f);
        public VariantRef<float> MoveSpeed = new(10.0f);
        public VariantRef<GameObject> Attack_FarVFX = new();

        public void AttackFar(Vector3 postion, Vector3 direction)
        {
            postion = postion + direction * 2.0f;
            var funcUnit = FuncUnit.Spawn(postion, direction);
            var hitCounts = new Dictionary<CharacterUnit, int>();
            funcUnit.AddTimer(DamageDelta, () =>
            {
                bool hitted = false;
                OverlapSphereEnemy<CharacterUnit>(funcUnit.WorldPosition, DataBoxAreaAt(ActAbiDataKey.Key2).z,
                    out var ret);
                foreach (var selection in ret)
                {
                    if (!hitCounts.ContainsKey(selection))
                    {
                        hitCounts.Add(selection, 1);
                    }
                    else
                    {
                        hitCounts[selection]++;
                    }

                    DamageParam param = new DamageParam()
                    {
                        Source = AbiOwner,
                        DamageType = DamageType.MagicDamage,
                        HitValue = 20.0f,
                        ValueLevel = ValueLevel.Level1,
                        DamageValue = DataMultipleAt(ActAbiDataKey.Key2) * AbiOwner.RealMagicDamage
                    };
                    if (hitCounts[selection] == 1)
                    {
                        var movementBuff = CharacterUnitAPI.CreateMovementBuff();
                        float moveSpeed = 6;
                        moveSpeed = Mathf.Min(15, moveSpeed);
                        movementBuff.SetDirection((TargetDirectionNoY).normalized)
                            .SetMoveSpeed(moveSpeed)
                            .SetLifetime(0.4f)
                            .AddTo(AbiOwner, selection);
                    }

                    if (hitCounts[selection] < 4)
                    {
                        selection.TryTakeDamage(param);
                        hitted = true;
                    }
                }

                if (hitted)
                    KGameCore.SystemAt<CameraModule>().ShakeCamera(DamageDelta);
            }, -1).Start();

            var move = funcUnit.AddUnitComponent<ProjectileDistance>();
            move.Speed = MoveSpeed;
            move.Direction = TargetDirectionNoY;
            var ve = VfxAPI.CreateVisualEffect(Attack_FarVFX, postion, direction);

            funcUnit.Lifetime = ve.mLifeTime;
            move.Begin();
        }

        public override void Init()
        {
            CastType = ActionCastType.Location;
            CastDistance = 20.0f;
            base.Init();
            OnAbiBegin += (abi) =>
            {
                var dashBuff = AbiOwner.GetBuff<Lux_Dash_Buff>();
                if (dashBuff != null)
                {
                    ActionIdx = 2;
                    AbiOwner.RemoveBuff(dashBuff);
                }
            };
            OnAbiEnd += (act) => { ActionIdx = (ActionIdx + 1) % 2; };
            OnActionActingBegin += () =>
            {
                if (ActionIdx == 2)
                {
                    AttackFar(AbiOwner.WorldPosition, TargetDirectionNoY);
                }
                else
                {
                    var postion = AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon);

                    var func = FuncUnit.Spawn(postion, Vector3.forward);
                    VfxAPI.CreateVisualEffect(Vfx0.As(), AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon),
                        AbiOwner.transform.forward);
                    func.CreateSocketVisual(VfxMissile, "", Vector3.zero, Vector3.one);
                    var distance = func.AddUnitComponent<ProjectileDistance>();
                    //distance.Direction = GameUnitAPI.DirectionBetweenUnit(func, source);
                    distance.Mode = MoveMode.Linear;
                    distance.Direction = (TargetLocation - func.WorldPosition).normalized;
                    //distance.MaxSpeed = 20.0f;
                    distance.Acceleration = ProjectileAcceleration;
                    distance.CollideRange = 0.5f;
                    distance.MaxDistance = 20.0f;
                    distance.IgnoreY = true;
                    distance.Speed = ProjectileSpeed;
                    distance.CollideLayerMask = GameUnitAPI.GetCharacterLayerMask() | GameUnitAPI.GetEnvLayerMask() |
                                                GameUnitAPI.GetGroundMask();
                    distance.OnCollideUnit += (selection) =>
                    {
                        var charUnit = selection as CharacterUnit;
                        if (charUnit && !charUnit.IsEnemy(AbiOwner) && charUnit.IsAlive)
                            return;
                        func.Die();
                        if (charUnit)
                        {
                            DamageParam param = new DamageParam()
                            {
                                DamageType = DamageType.MagicDamage,
                                DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                                Source = AbiOwner,
                                ValueLevel = ValueLevel.Level1,
                            };
                            selection.GetComponent<CharacterUnit>().TryTakeDamage(param);
                        }
                    };
                    distance.OnFinish += (u) => { func.Die(); };
                    distance.Begin();
                }
            };
        }
    }
}