using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace K1.Gameplay
{
    public class Lux_Attack : ActionAbility
    {
        public int ComboIndex = 0;
        KTimer _comboTimer;
        bool dashAtack = false;

        public override void Init()
        {
            CastType = ActionCastType.Location;
            base.Init();
            OnAbiBegin += (ab) =>
            {
                var dashBuff = AbiOwner.GetBuff<Lux_Dash_Buff>();
                if (dashBuff != null)
                {
                    ActionIdx = 4;
                    dashAtack = true;
                    AbiOwner.RemoveBuff(dashBuff);
                }
                else
                {
                    ActionIdx = ComboIndex;
                }
            };
            OnActionCastBegin += () =>
            {
                if (_comboTimer != null)
                {
                    _comboTimer.Stop();
                    _comboTimer = null;
                }

                if (ComboIndex == 3)
                    KGameCore.SystemAt<CameraModule>().FieldView(90, DataCastPointAt(3), 0, 0.1f);
            };
            OnActionCastBreak += () => { Reset(); };
            OnActionActingEnd += () =>
            {
                ComboIndex++;
                if (ComboIndex == 4)
                    ComboIndex = 0;
            };

            OnAbiEnd += (abi) =>
            {
                dashAtack = false;
                StartTimer();
            };
        }

        public void Reset()
        {
            if (_comboTimer != null)
            {
                _comboTimer.Stop();
                _comboTimer = null;
            }

            ComboIndex = 0;
        }

        public void StartTimer()
        {
            if (_comboTimer == null)
            {
                _comboTimer = AddTimer(1.5f, () => { ComboIndex = 0; });
            }

            if (_comboTimer.IsRunning)
            {
                _comboTimer.Reset();
            }
            else
            {
                _comboTimer.Start();
            }
        }

        public void AttackMultiple(GameObject vfx, ActAbiDataKey key, int loops, float duration)
        {
            var position = AbiOwner.WorldPosition;
            var direction = TargetDirectionNoY;
            VfxAPI.CreateVisualEffect(vfx,
                position, direction);
            position += TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key1).z;
            Action damageCB = () =>
            {
                bool hitted = false;
                OverlapBox<CharacterUnit>(position, DataBoxAreaAt(ActAbiDataKey.Key1), TargetDirectionNoY,
                    out var result);
                foreach (var selection in result)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageType = DamageType.MagicDamage,
                        DamageValue = DataMultipleAt(ActAbiDataKey.Key1) * AbiOwner.RealMagicDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level1,
                    };
                    hitted = selection.TryTakeDamage(param);
                }

                if (hitted)
                {
                    AbiOwner.SetAnimatorSpeed(0.1f, 0.10f);
                    KGameCore.SystemAt<CameraModule>().ShakeCamera(0.1f);
                    // KGameCore.SystemAt<AudioModule>().PlayAudio(DataAudioAt(ActAbiDataKey.Key0));
                }
            };
            damageCB();
            AddTimer(duration, () => { damageCB(); }, loops);
        }

        public VariantRef<GameObject> VerticalVFX = new VariantRef<GameObject>();
        public VariantRef<GameObject> CircleVFX = new VariantRef<GameObject>();
        public VariantRef<GameObject> PrickVFX = new VariantRef<GameObject>();
        public VariantRef<GameObject> CircleVFX2 = new VariantRef<GameObject>();
        public VariantRef<GameObject> FarAttackVfx = new VariantRef<GameObject>();

        public void AttackFar()
        {
            var func = FuncUnit.Spawn(AbiOwner.WorldPosition, TargetDirectionNoY);
            func.CreateSocketVisual(FarAttackVfx, "", Vector3.zero, Vector3.one);
            var distance = func.AddUnitComponent<ProjectileDistance>();
            distance.Speed = 29.0f;
            distance.Acceleration = 10.0f;
            distance.Direction = TargetDirectionNoY;
            distance.Begin();
            distance.OnFinish += (a) => { func.Die(); };
            HashSet<CharacterUnit> _hit = new HashSet<CharacterUnit>();
            func.onLogic += () =>
            {
                var pos = func.WorldPosition;
                pos.z += DataBoxAreaAt().z * 0.5f;
                OverlapBox<CharacterUnit>(pos, DataBoxAreaAt(), TargetDirectionNoY, out var ret);
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
                    if (_hit.Contains(selection))
                        continue;

                    _hit.Add(selection);
                    MovementBuff buff =
                        GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                    float distance = (func.WorldPosition - selection.WorldPosition).magnitude;
                    buff.SetLifetime(0.2f);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(func, selection));
                    buff.SetAcceleration(-5.0f);
                    buff.SetMoveSpeed(10)
                        .AddTo(AbiOwner, selection);
                }
            };
        }

        protected override void ActionActBegin()
        {
            var postion = AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.RightHand);
            var dashBuff = AbiOwner.GetBuff<Lux_Dash_Buff>();
            if (dashAtack)
            {
                AttackHigh(ActAbiDataKey.Key4, AbiOwner.GetSocketWorldPosition(BuiltinCharacterSocket.Weapon),
                    TargetDirectionNoY);
                dashAtack = false;
            }
            else
            {
                switch (ComboIndex)
                {
                    case 0:
                        AttackNormal(CircleVFX, ActAbiDataKey.Key0, AbiOwner.WorldPosition, TargetDirectionNoY);
                        break;
                    case 1:
                        AttackMultiple(VerticalVFX, ActAbiDataKey.Key1, 1, 0.4f);
                        break;
                    case 2:
                        AttackNormal(PrickVFX, ActAbiDataKey.Key2, AbiOwner.WorldPosition, TargetDirectionNoY,
                            ValueLevel.Level2, false);
                        break;
                    case 3:
                        KGameCore.SystemAt<CameraModule>()
                            .ShakeCamera(0.5f, KGameCore.SystemAt<CameraModule>().mHighShake);
                        AttackNormal(CircleVFX2, ActAbiDataKey.Key3, AbiOwner.WorldPosition, TargetDirectionNoY,
                            ValueLevel.Level2);
                        break;
                }
            }
        }

        public void AttackNormal(GameObject Vfx, ActAbiDataKey key, Vector3 postion, Vector3 direction,
            ValueLevel level = ValueLevel.Level0, bool isCircle = true)
        {
            ActAbiDataKey newKey = key;
            VfxAPI.CreateVisualEffect(Vfx,
                postion, direction);
            postion += direction * DataBoxAreaAt(key).z;
            bool hitted = false;
            List<CharacterUnit> result;
            if (isCircle)
            {
                OverlapSphereEnemy<CharacterUnit>(postion, DataBoxAreaAt(key).z, out result);
            }
            else
            {
                OverlapBox<CharacterUnit>(postion, DataBoxAreaAt(key), TargetDirectionNoY, out result);
            }

            foreach (var selection in result)
            {
                float hitValue = 40;
                if (level == ValueLevel.Level2)
                    hitValue = 100.0f;
                DamageParam param = new DamageParam()
                {
                    DamageType = DamageType.MagicDamage,
                    DamageValue = DataMultipleAt(key) * AbiOwner.RealMagicDamage,
                    Source = AbiOwner,
                    ValueLevel = level,
                    HitValue = hitValue
                };
                hitted = selection.TryTakeDamage(param);
            }

            if (hitted)
            {
                AbiOwner.SetAnimatorSpeed(0.1f, 0.1f);
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.1f);
                //KGameCore.SystemAt<AudioModule>().PlayAudio(DataAudioAt(ActAbiDataKey.Key0));
            }
        }

        public VariantRef<float> ImpactStunTime = new(1.5f);
        public VariantRef<float> ImpactMoveTime = new(0.2f);
        public VariantRef<GameObject> Attack_ImpactVFX = new();

        public void AttackHigh(ActAbiDataKey key, Vector3 postion, Vector3 diection)
        {
            VfxAPI.CreateVisualEffect(Attack_ImpactVFX,
                postion, diection);

            GameUnitAPI.OverlapGameUnitInBox<EnvUnit>(postion + diection * DataBoxAreaAt(key).z,
                DataBoxAreaAt(key),
                Quaternion.LookRotation(diection),
                GameUnitAPI.GetEnvLayerMask(),
                (envUnit) =>
                {
                    var rigibody = envUnit.GetComponent<Rigidbody>();
                    if (rigibody)
                        rigibody.AddForce(TargetDirectionNoY * 10, ForceMode.Impulse);
                });
            bool hitted = false;
            OverlapBox<CharacterUnit>(postion + diection * DataBoxAreaAt(ActAbiDataKey.Key3).z,
                DataBoxAreaAt(key),
                diection, out var result);
            foreach (var selection in result)
            {
                DamageParam param = new DamageParam()
                {
                    DamageType = DamageType.MagicDamage,
                    Source = AbiOwner,
                    DamageValue = DataMultipleAt() * AbiOwner.RealMagicDamage,
                    ValueLevel = ValueLevel.Level2,
                };
                if (selection.TryTakeDamage(param))
                {
                    var stun = GameplayConfig.Instance().CreateStunBuff() as StunBuff;
                    stun.StunLevel = ValueLevel.Level2;
                    stun.SetLifetime(ImpactStunTime);
                    stun.AddTo(AbiOwner, selection);

                    float moveDistance = DataBoxAreaAt(key).z -
                                         GameUnitAPI.DistanceBetweenGameUnit(selection, AbiOwner);
                    hitted = true;
                    if (moveDistance < 0)
                        continue;

                    var movementBuff = GameplayConfig.Instance().DefaultRepellMovement.CreateBuff() as MovementBuff;
                    float moveSpeed = moveDistance / 0.2f;
                    moveSpeed = Mathf.Min(15, moveSpeed);
                    movementBuff.SetDirection(TargetDirectionNoY)
                        .SetMoveSpeed(moveSpeed)
                        .SetLifetime(ImpactMoveTime)
                        .AddTo(AbiOwner, selection);
                }
            }

            KGameCore.SystemAt<CameraModule>().ShakeCamera(0.3f, KGameCore.SystemAt<CameraModule>().mHighShake);
            if (hitted)
            {
                AbiOwner.SetAnimatorSpeed(0.0f, 0.21f);
                //KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);
            }
        }
    }
}