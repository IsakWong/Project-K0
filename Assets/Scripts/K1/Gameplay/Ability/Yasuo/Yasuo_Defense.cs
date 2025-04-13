using UnityEngine;

namespace K1.Gameplay
{
    public class Yasuo_DefenceBuff : DefensiveBuff
    {
        public override void Init()
        {
            base.Init();
            DefenceLevel = ValueLevel.LevelMax;
        }

        public override void BuffEnd()
        {
            base.BuffEnd();
            //BuffOwner.PlayAnimator("Is_Defence", false);
        }

        public override void Logic(float logicTime)
        {
            base.Logic(logicTime);
            //BuffOwner.PlayAnimator("Is_Defence", true);
        }

        public override void OnDefence(DamageParam param)
        {
            base.OnDefence(param);
            if (BuffOwner.IsSoloActing)
            {
                var state = BuffOwner.FSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
                if (state.ActiveStateName == ActionSubID.ActionState_Acting ||
                    state.ActiveStateName == ActionSubID.ActionState_Casting ||
                    state.ActiveStateName == ActionSubID.ActionState_Facing)
                {
                    return;
                }
            }

            var target = param.Source;

            {
                // var buff = GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                // buff.SetDirection((target.WorldPosition- BuffOwner.WorldPosition).normalized)
                //     .SetMoveSpeed(6)
                //     .SetLifetime(0.3f)
                //     .AddTo(BuffOwner, target);   
            }
            {
                var buff = GameplayConfig.Instance().DefaultMovement.CreateBuff() as MovementBuff;
                buff.SetDirection((BuffOwner.WorldPosition - target.WorldPosition).normalized)
                    .SetMoveSpeed(4)
                    .SetLifetime(0.10f)
                    .AddTo(BuffOwner, BuffOwner);
            }


            KGameCore.SystemAt<CameraModule>().ShakeCamera(0.2f);
        }
    }

    public class Yasuo_Defense : ActionAbility
    {
        public override void Init()
        {
            base.Init();
            CastType = ActionCastType.Location;
            // IsCustomActingTime = true;
            // CustomActingTime = 2.0f;
        }

        public float BuffTime = 3.0f;

        private Buffbase buff;
        public TVariant<BuffConfig> DefenceBuff = new TVariant<BuffConfig>();

        // ReSharper disable Unity.PerformanceAnalysis
        private Vfx Vfx;

        protected override void ActionCastBegin()
        {
            base.ActionCastBegin();
        }

        protected override void ActionActBegin()
        {
            base.ActionActBegin();
            buff = DataBuffAt().CreateBuff();
            buff.SetLifetime(5.0f).AddTo(AbiOwner, AbiOwner);
        }
    }
}