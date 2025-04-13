using System.Collections.Generic;
using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_Attack : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            //AutoApplyAnimator = false;
        }

        private MovementBuff _ownerMovement = null;

        int loops = 3;

        private HashSet<CharacterUnit> _Hitted = new HashSet<CharacterUnit>();
        private bool hitted = false;
        private Vector3 lastPos;
        private Quaternion lastRotation;

        protected override void ActionActing()
        {
            base.ActionActing();
            foreach (var selection in AbiOwner.overlapDetecter.OverlapUnits)
            {
                if (!_Hitted.Contains(selection))
                {
                    DamageParam damageParam = new DamageParam()
                    {
                        DamageValue = DataMultipleAt() * AbiOwner.RealPhysicalDamage,
                        Source = AbiOwner,
                        DamageType = DamageType.PhysicalDamage,
                        ValueLevel = ValueLevel.Level1,
                    };
                    if (selection.TryTakeDamage(damageParam))
                    {
                        MovementBuff buff =
                            GameplayConfig.Instance().DefaultMovement.CreateBuff() as
                                MovementBuff;
                        buff.SetLifetime(0.1f);
                        buff.SetDirection(GameUnitAPI.DirectionBetweenUnit(AbiOwner, selection));
                        buff.SetMoveSpeed(5)
                            .AddTo(AbiOwner, selection);
                        KGameCore.SystemAt<AudioModule>().PlayAudio(DataAudioAt(ActAbiDataKey.Key0));
                    }

                    _Hitted.Add(selection);
                    hitted = true;
                }
            }

            if (hitted)
            {
                //AbiOwner.SetAnimatorSpeed(0.1f, 0.1f);
                KGameCore.SystemAt<CameraModule>().ShakeCamera(0.15f);
            }

            if (ActionIdx == 2)
                ActionIdx = 0;
            else
                ActionIdx++;
        }

        // ReSharper disable Unity.PerformanceAnalysis

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            var startPos = AbiOwner.transform.position;
            var direction = TargetDirectionNoY;
            hitted = false;
            _Hitted.Clear();
            VfxAPI.CreateVisualEffect(DataVisualAt((ActAbiDataKey)ActionIdx), AbiOwner.WorldPosition, direction);
        }
    }
}