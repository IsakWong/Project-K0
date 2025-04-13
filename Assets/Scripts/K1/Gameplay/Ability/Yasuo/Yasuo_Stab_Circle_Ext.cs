using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace K1.Gameplay
{
    public class Yasuo_Stab_Circle_Ext : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
        }

        private GameObject castAudio;

        protected override void ActionCastBreak()
        {
            if (castAudio != null)
                GameObject.Destroy(castAudio);
        }

        protected override void AbiBegin()
        {
            base.AbiBegin();
            EndureBuff endureBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
            endureBuff.SetLifetime(CustomCastPointOnce);
            endureBuff.AddTo(AbiOwner, AbiOwner);

            KGameCore.SystemAt<HUDModule>().AbiTextPrefab.SpawnText(AbiOwner.WorldPosition, "领域展开:空间斩！");

            //KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.5f, DataActingTimeAt());
            KGameCore.SystemAt<CameraModule>().PostProcess(0.1f, 0.6f);

            var projectilePrefab = DataVisualAt(ActAbiDataKey.Key0);
            var position = AbiOwner.transform.position;
            position = TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key0).z + position;
            var effect = VfxAPI.CreateVisualEffect(projectilePrefab, position, TargetDirectionNoY);
        }

        protected override void ActionExit()
        {
            base.ActionExit();
        }

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var damgeCenterPosition = AbiOwner.transform.position;
            bool isParry = false;

            damgeCenterPosition = TargetDirectionNoY * DataBoxAreaAt(ActAbiDataKey.Key0).x + damgeCenterPosition;
            float damage = DataMultipleAt(ActAbiDataKey.Key0) * AbiOwner.RealPhysicalDamage;
            AddTimer(0.1f, () =>
            {
                HashSet<CharacterUnit> seletions = new();
                OverlapSphereEnemy<CharacterUnit>(damgeCenterPosition, DataBoxAreaAt(ActAbiDataKey.Key0).x,
                    out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = damage,
                        DamageType = DamageType.PhysicalDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level0,
                    };

                    if (selection.TryTakeDamage(param))
                    {
                        MovementBuff buff = CharacterUnitAPI.CreateMovementBuff()
                            .SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection))
                            .SetMoveSpeed(5.0f)
                            .SetLifetime(0.05f) as MovementBuff;
                        buff.AddTo(AbiOwner, selection);
                    }
                }
            }, 4).Start().OnFinish += () =>
            {
                OverlapSphereEnemy<CharacterUnit>(damgeCenterPosition, DataBoxAreaAt(ActAbiDataKey.Key0).x,
                    out var ret);
                foreach (var selection in ret)
                {
                    DamageParam param = new DamageParam()
                    {
                        DamageValue = damage,
                        DamageType = DamageType.PhysicalDamage,
                        Source = AbiOwner,
                        ValueLevel = ValueLevel.Level2,
                    };

                    if (selection.TryTakeDamage(param))
                    {
                        selection.Stun(AbiOwner, 0.4f);
                    }
                }
            };
        }
    }
}