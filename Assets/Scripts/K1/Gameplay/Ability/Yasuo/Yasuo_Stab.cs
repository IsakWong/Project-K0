using System.Collections.Generic;
using DG.Tweening;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Stab : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            OnAbiBegin += (abi) => { ActionIdx = Random.Range(0, 2); };
            OnActionActingBegin += () =>
            {
                var startPos = AbiOwner.transform.position;
                var direction = TargetDirectionNoY;
                if (WithMovement)
                {
                    var ownerMovement = GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                    ownerMovement.SetDirection(direction);
                    ownerMovement.SetMoveSpeed(MathUtility.CalculateSpeed(MovementDistance, 0.5f));
                    ownerMovement.SetAcceleration(-MathUtility.CaclulateAcc(MovementDistance, 0.5f));
                    ownerMovement.SetLifetime(0.5f);
                    ownerMovement.AddTo(AbiOwner, AbiOwner);
                }

                Trigger(AbiOwner.transform, startPos, direction);
            };
            OnActionCastBegin += () =>
            {
                var startPos = AbiOwner.transform.position;
                var direction = TargetDirectionNoY;
            };
            OnActionActingEnd += () => { };
            OnAbiEnd += (abi) => { };
            OnActionActing += () => { };
        }

        public bool WithMovement = true;

        private MovementBuff mOwnerMovement = null;

        public VariantRef<GameObject> BladeVFX = new();

        public float MovementDistance = 5.0f;

        public void Trigger(Transform damageTransform, Vector3 startPos, Vector3 direction)
        {
            Vfx warning = null;
            var pos = direction * DataBoxAreaAt(ActAbiDataKey.Key0).z + startPos;
            warning = WarningBox(pos, DataBoxAreaAt(ActAbiDataKey.Key0), direction, 999);

            var damageCenterPosition = startPos;
            float damage = DataMultipleAt(ActAbiDataKey.Key0) * AbiOwner.RealPhysicalDamage;
            HashSet<CharacterUnit> _hitted = new();
            damageCenterPosition = direction * DataBoxAreaAt(ActAbiDataKey.Key0).z + damageCenterPosition;

            var visualEffect = BladeVFX;
            var effect = AbiOwner.CreateSocketVisual(visualEffect, BuiltinCharacterSocket.Origin.ToString(),
                Vector3.zero, Vector3.one);
            AddTimer(0.03f, () =>
            {
                damageCenterPosition = direction * DataBoxAreaAt().z + damageTransform.position;

                if (warning)
                {
                    warning.transform.position = damageCenterPosition;
                }

                OverlapBox<CharacterUnit>(damageCenterPosition, DataBoxAreaAt(ActAbiDataKey.Key0),
                    direction, out var ret);
                foreach (var selection in ret)
                {
                    if (_hitted.Contains(selection))
                        continue;
                    DamageParam damageParam = new DamageParam()
                    {
                        DamageValue = damage,
                        Source = AbiOwner,
                        DamageType = DamageType.PhysicalDamage,
                        ValueLevel = ValueLevel.Level2
                    };
                    _hitted.Add(selection);
                    selection.TryTakeDamage(damageParam);
                    var stun = GameplayConfig.Instance().CreateStunBuff();
                    stun.StunLevel = ValueLevel.LevelMax;
                    stun.SetLifetime(1.0f);
                    stun.AddTo(AbiOwner, selection);
                    MovementBuff buff =
                        GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                    buff.SetLifetime(0.1f);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection));
                    buff.SetMoveSpeed(5)
                        .AddTo(AbiOwner, selection);
                }
            }, (int)(DataActingTimeAt(0) / 0.03f));
            AddTimer(DataActingTimeAt(), () =>
            {
                if (warning)
                {
                    warning.Die();
                    warning = null;
                }
            });
        }
    }
}