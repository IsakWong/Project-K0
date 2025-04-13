using System.Collections.Generic;
using DG.Tweening;
using Framework.Foundation;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Double_Stab_Circle : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        public VariantRef<GameObject> BladeVFX = new(null);

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var func = FuncUnit.Spawn(AbiOwner.transform.position + TargetDirectionNoY * -5.0f, TargetDirectionNoY);
            var YasuoModel = ActionConfig.GameObjectsData[ActAbiDataKey.Key0];

            func.Lifetime = 2.0f;

            YasuoModel = GameObject.Instantiate(YasuoModel);
            var fakeYasuoAnimator = YasuoModel.transform.GetChild(0).GetComponent<Animator>();
            YasuoModel.transform.SetParent(func.transform);
            YasuoModel.transform.localPosition = Vector3.zero;
            YasuoModel.transform.localRotation = Quaternion.identity;
            fakeYasuoAnimator.SetTrigger("Yasuo_Stab_Circle");

            var bladeVFX = BladeVFX;
            var position = AbiOwner.transform.position;
            position = TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key0).z + position;
            func.transform.LookAt(TargetLocation);
            var distance = func.AddUnitComponent<ProjectileDistance>();
            distance.Direction = TargetDirectionNoY;
            distance.Speed = 10.0f;
            distance.Begin();
            AddTimer(0.8f, () =>
            {
                var damgeCenterPosition = func.WorldPosition;
                bool isParry = false;
                damgeCenterPosition += TargetDirectionNoY * DataBoxAreaAt().z;
                float damage = DataMultipleAt() * AbiOwner.RealPhysicalDamage;
                var effect = VfxAPI.CreateVisualEffect(bladeVFX, damgeCenterPosition, TargetDirectionNoY);
                OverlapSphereEnemy<CharacterUnit>(damgeCenterPosition, DataBoxAreaAt().x, out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = damage,
                        DamageType = DamageType.PhysicalDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level2
                    };
                    selection.TryTakeDamage(param);
                    var stun = GameplayConfig.Instance().CreateStunBuff();
                    stun.SetLifetime(1.0f);
                    stun.AddTo(AbiOwner, selection);
                    MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                        .SetLifetime(0.2f) as MovementBuff;
                    buff.AddTo(AbiOwner, selection);
                    buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                        .SetMoveSpeed(5);
                }
            }).Start();
        }
    }
}