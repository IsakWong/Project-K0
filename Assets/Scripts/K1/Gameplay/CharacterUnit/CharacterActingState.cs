using System;
using EasyCharacterMovement;
using FSM;
using UnityEngine;

namespace K1.Gameplay
{
    public class SoloActionResult
    {
        // 此次动作的回调
        public Action<SoloActionParam> OnCastBegin;
        public Action<SoloActionParam> OnCasting;
        public Action<SoloActionParam> OnCastBreak;
        public Action<SoloActionParam> OnCastEnd;

        public Action<SoloActionParam> OnActingBegin;
        public Action<SoloActionParam> OnActingLogic;
        public Action<SoloActionParam> OnActingBreak;
        public Action<SoloActionParam> OnActingEnd;

        public Action<SoloActionParam> OnBackswingBegin;
        public Action<SoloActionParam> OnBackswingEnd;

        public Action<SoloActionParam> OnActionExit;
        public bool EventEnable = true;
    }

    public class SoloActionParam
    {
        public string ActionName = "";

        public float CastTime = 0.0f;
        public ValueLevel CastEndureLevel = ValueLevel.LevelNone;
        public ValueLevel ActingEndureLevel = ValueLevel.LevelNone;
        public float ActionTime = 0.0f;
        public float BackswingTime = 0.0f;

        public AnimatorParamConfig AnimatorParam = new AnimatorParamConfig();


        //需要面朝方向
        public bool NeedFaceTarget = false;
        public Vector3 FaceDirection = Vector3.forward;

        // 触发动作的技能
        public ActionAbility SourceAbi;

        public Action<SoloActionResult> OnActionBegin;
        public Action OnActionBeginFailed;
    }

    public enum ActionSubID
    {
        None,
        ActionState_Facing,
        ActionState_Casting, //动作前摇，可以被一些特殊操作主动打断
        ActionState_Acting, //动作释放过程，无法主动打断
        ActionState_BackswingRecovery, //后摇恢复，可以主动打断
    }

    public class SoloActionCharacterState : HierarchicalCharacterState
    {
        public SoloActionParam CastParam = new SoloActionParam();
        public SoloActionResult CastResult = new SoloActionResult();

        private float _castRemainTime = 0;
        private float _actingRemainTime = 0;
        private float _backswingRecoverTime = 0;
        private bool _acting = false;
        private bool _casting = false;
        private bool _break = false;
        private bool _backswing = false;
        private bool _facingTarget = false;

        protected void ApplyAnimator(AnimatorParamConfig variant)
        {
            Animator animator = Character.mAnimator;

            switch (variant.VarValue.mType)
            {
                case VariantType.None:
                    Character.SetAnimatorTrigger(variant.VarName);
                    break;
                case VariantType.Bool:
                    Character.SetAnimatorBool(variant.VarName, variant.VarValue.Get<bool>());
                    break;
                case VariantType.Enum:
                    break;
                case VariantType.Int:
                    animator.SetInteger(variant.VarName, variant.VarValue.Get<int>());
                    break;
                case VariantType.Float:
                    animator.SetFloat(variant.VarName, variant.VarValue.Get<float>());
                    break;
                case VariantType.Double:
                    animator.SetFloat(variant.VarName, variant.VarValue.Get<float>());
                    break;
                case VariantType.String:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float CastPercent
        {
            get
            {
                if (CastParam is not null && CastParam.CastTime != 0)
                    return 1 - _castRemainTime / CastParam.CastTime;
                return 0;
            }
        }

        protected void SubState_Facing_Logic(State<ActionSubID, string> state)
        {
            if (CastParam.NeedFaceTarget)
            {
                Vector3 direction = CastParam.FaceDirection;
                direction.y = 0;
                direction.Normalize();
                var deltaDir = Character.transform.forward - direction;
                deltaDir.y = 0;
                _facingTarget = deltaDir.magnitude < 0.01f;

                if (!_facingTarget)
                {
                    RotateTowards(direction, 960 * KTime.scaleDeltaTime, true);
                }
                else
                {
                    Character.transform.rotation = Quaternion.LookRotation(direction);
                    RequestStateChange(ActionSubID.ActionState_Casting);
                }
            }
            else
            {
                RequestStateChange(ActionSubID.ActionState_Casting);
            }
        }


        protected void SubState_Casting_Logic(State<ActionSubID, string> state)
        {
            if (_casting)
            {
                if (_castRemainTime > 0)
                {
                    _castRemainTime -= KTime.scaleDeltaTime;
                    CastResult.OnCasting?.Invoke(CastParam);
                }
                else
                {
                    _casting = false;
                    _acting = true;
                    RequestStateChange(ActionSubID.ActionState_Acting);
                }
            }
        }

        protected void SubState_Acting_Logic(State<ActionSubID, string> state)
        {
            if (_acting)
            {
                if (_actingRemainTime > 0)
                {
                    _actingRemainTime -= KTime.scaleDeltaTime;
                    CastResult.OnActingLogic?.Invoke(CastParam);
                }
                else
                {
                    CastResult.OnActingEnd?.Invoke(CastParam);
                    RequestStateChange(ActionSubID.ActionState_BackswingRecovery);
                }
            }
        }

        protected void SubState_Backswing_Enter(State<ActionSubID, string> state)
        {
            _backswing = true;
            _backswingRecoverTime = CastParam.BackswingTime;
            if (CastResult.EventEnable)
                CastResult.OnBackswingBegin?.Invoke(CastParam);
        }

        protected void SubState_Backswing_Logic(State<ActionSubID, string> state)
        {
            if (_backswing)
            {
                if (_backswingRecoverTime > 0)
                {
                    _backswingRecoverTime -= KTime.scaleDeltaTime;
                }
                else
                {
                    Character.Idle();
                    _backswing = false;
                    if (CastResult.EventEnable)
                        CastResult.OnBackswingEnd?.Invoke(CastParam);
                }
            }
        }

        State<ActionSubID> facingState;
        State<ActionSubID> castingState;
        State<ActionSubID> actingState;
        State<ActionSubID> backswingState;
        State<ActionSubID> breakState;

        public class BreakTransition : Transition<ActionSubID>
        {
            public BreakTransition(
                ActionSubID from,
                ActionSubID to,
                Func<Transition<ActionSubID>, bool> condition = null,
                bool forceInstantly = false,
                Func<ActionSubID, ActionSubID, bool> shouldTransition = null) : base(from, to, condition,
                forceInstantly)
            {
                _shouldTransition = shouldTransition;
            }

            Func<ActionSubID, ActionSubID, bool> _shouldTransition;

            public override bool ShouldTransition()
            {
                if (_shouldTransition != null)
                    return _shouldTransition.Invoke(from, to);
                return true;
            }
        }

        public SoloActionCharacterState()
        {
            facingState = new State<ActionSubID>(
                onLogic: SubState_Facing_Logic);

            EndureBuff castEndureBuff = null;
            castingState = new State<ActionSubID>(
                (state) =>
                {
                    Character.Log($"Casting {CastParam.ActionName}，持续 {CastParam.CastTime} s");
                    if (Character.PositionConstraint)
                        Character.PositionConstraint.weight = 0.5f;
                    if (CastParam.CastEndureLevel != ValueLevel.LevelNone)
                    {
                        castEndureBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
                        castEndureBuff.EndureLevel = CastParam.CastEndureLevel;
                        castEndureBuff.SetLifetime(999);
                        Character.AddBuff(castEndureBuff);
                    }

                    ApplyAnimator(CastParam.AnimatorParam);
                    CastResult.OnCastBegin?.Invoke(CastParam);
                    _castRemainTime = CastParam.CastTime;
                    _casting = true;
                },
                SubState_Casting_Logic, (state) =>
                {
                    if (_break)
                        CastResult.OnCastBreak?.Invoke(CastParam);
                    CastResult.OnCastEnd?.Invoke(CastParam);
                    if (castEndureBuff != null)
                    {
                        castEndureBuff.BuffOwner.RemoveBuff(castEndureBuff);
                        castEndureBuff = null;
                    }
                });

            EndureBuff actingEndureBuff = null;
            actingState = new State<ActionSubID>(
                (state) =>
                {
                    Character.Log($"Acting {CastParam.ActionName}，持续 {CastParam.ActionTime} s");

                    if (CastParam.ActingEndureLevel != ValueLevel.LevelNone)
                    {
                        actingEndureBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
                        actingEndureBuff.EndureLevel = CastParam.ActingEndureLevel;
                        actingEndureBuff.SetLifetime(999);
                        Character.AddBuff(actingEndureBuff);
                    }

                    _actingRemainTime = CastParam.ActionTime;
                    _acting = true;
                    CastResult.OnActingBegin?.Invoke(CastParam);
                },
                SubState_Acting_Logic,
                (state) =>
                {
                    if (actingEndureBuff != null)
                    {
                        actingEndureBuff.BuffOwner.RemoveBuff(actingEndureBuff);
                        actingEndureBuff = null;
                    }

                    if (_break)
                        CastResult.OnActingBreak?.Invoke(CastParam);
                    CastResult.OnActingEnd?.Invoke(CastParam);
                });
            backswingState = new State<ActionSubID>(
                SubState_Backswing_Enter,
                SubState_Backswing_Logic);

            AddState(ActionSubID.ActionState_Facing, facingState);
            AddState(ActionSubID.ActionState_Casting, castingState);
            AddState(ActionSubID.ActionState_Acting, actingState);
            AddState(ActionSubID.ActionState_BackswingRecovery, backswingState);
            SetStartState(ActionSubID.ActionState_Facing);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _casting = false;
            _break = false;
            _facingTarget = false;
            _backswing = false;
            _acting = true;
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
        {
            Vector3 characterUp = Character.transform.up;

            if (updateYawOnly)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            Character.transform.rotation =
                Quaternion.RotateTowards(Character.transform.rotation, targetRotation, maxDegreesDelta);
        }


        public override void OnLogic()
        {
            base.OnLogic();
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Character.PositionConstraint)
                Character.PositionConstraint.weight = 0.0f;
            Character.CharEvent.OnStatusChange?.Invoke(Character);
            CastResult.OnActionExit?.Invoke(CastParam);
        }

        public void BreakAction(CharacterUnit _breakActionSource)
        {
            _break = true;
            if (ActiveStateName == ActionSubID.ActionState_Casting)
            {
                _casting = false;
            }

            if (ActiveStateName == ActionSubID.ActionState_Acting)
            {
                _acting = false;
            }
        }
    }
}