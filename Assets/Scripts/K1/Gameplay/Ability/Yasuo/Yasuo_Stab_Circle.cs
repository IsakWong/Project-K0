using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Stab_Circle : ActionAbility
    {
        public VariantRef<GameObject> BladeVFX = new(null);
        HashSet<CharacterUnit> _hitted = new();

        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;

            OnActionActing += () =>
            {
                var damgeCenterPosition = AbiOwner.WorldPosition;
                damgeCenterPosition = TargetDirectionNoY * DataBoxAreaAt().z + damgeCenterPosition;
                float damage = DataMultipleAt() * AbiOwner.RealPhysicalDamage;

                OverlapSphereEnemy<CharacterUnit>(damgeCenterPosition, DataBoxAreaAt().z, out var ret);
                foreach (var selection in ret)
                {
                    if (_hitted.Contains(selection))
                        continue;
                    _hitted.Add(selection);
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = damage,
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
                        .SetLifetime(0.2f) as MovementBuff;
                    buff.AddTo(AbiOwner, selection);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                        .SetMoveSpeed(5);
                    lastHit.Add(selection);
                }
            };
            OnActionActingBegin += () =>
            {
                _hitted.Clear();
                var position = AbiOwner.transform.position;
                position = TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key0).z + position;
                var effect = VfxAPI.CreateVisualEffect(BladeVFX, position, TargetDirectionNoY);

                lastHit = new List<CharacterUnit>();
                bool isParry = false;
            };
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();

            var position = AbiOwner.transform.position;
            position = TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key0).z + position;
        }

        public List<CharacterUnit> lastHit;
        private MovementBuff mOwnerMovement = null;


        // ReSharper disable Unity.PerformanceAnalysis
    }
}