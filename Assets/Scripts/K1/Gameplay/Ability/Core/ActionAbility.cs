using System;
using System.Collections.Generic;
using FSM;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace K1.Gameplay
{
    public enum AbiStateId
    {
        None,
        Idle,
        Acting
    }

    public class ActionAbilityResult
    {
        public bool IsCastBegin = false;
        public bool IsCastFailed = false;
        public bool IsCastBreak = false;
        public bool IsCasting = false;
        public bool IsActingBegin = false;
        public bool IsAbiBeginFailed = false;
        public bool IsAbiBegin = false;
        public bool IsAbiEnd = false;
        public bool ActingBreak = false;
    }

    // 一个带有施法动作触发的简易技能
    public class ActionAbility : AbilityBase
    {
        public Action<ActionAbility> OnAbiBegin;

        public Action OnActionCastBegin;
        public Action OnActionCasting;
        public Action OnActionCastBreak;
        public Action OnActionCastEnd;
        public Action OnActionActingBegin;
        public Action OnActionActing;
        public Action OnActionActingBreak;
        public Action OnActionActingEnd;


        public Action<ActionAbility> OnAbiActing;
        public Action<ActionAbility> OnAbiEnd;

        public StateMachine<AbiStateId> FSM = new StateMachine<AbiStateId>();
        public bool HasIndicator = false;

        public bool AutoApplyAnimator = true;

        #region Config

        public bool HighPiority = false;

        public AbilityIndicator Indicator;

        public ActionAbilityConfig ActionConfig
        {
            get { return Config as ActionAbilityConfig; }
        }


        public GameObject DataVisualAt(ActAbiDataKey key = ActAbiDataKey.Key0)
        {
            if (ActionConfig.mDataVisualEffect.ContainsKey(key))
                return ActionConfig.mDataVisualEffect[key];
            return new Variant(GameplayConfig.Instance().DefaultBox.gameObject);
        }

        public Vector3 DataBoxAreaAt(ActAbiDataKey key = ActAbiDataKey.Key0)
        {
            if (ActionConfig.mDataBoxArea.ContainsKey(key))
                return ActionConfig.mDataBoxArea[key];
            return Vector3.one;
        }

        public float DataActingTimeAt(int key = 0)
        {
            if (key >= ActionConfig.ActionCastConfig.Count)
            {
                return 0.0f;
            }

            return ActionConfig.ActionCastConfig[key].CastingTime;
        }

        public BuffConfig DataBuffAt(ActAbiDataKey key = ActAbiDataKey.Key0)
        {
            return ActionConfig.mDataBuff[key];
        }

        public AudioClip DataAudioAt(ActAbiDataKey key = ActAbiDataKey.Key0)
        {
            if (ActionConfig.Audios.ContainsKey(key))
                return ActionConfig.Audios[key].RandomAccess();
            return null;
        }

        public float DataCastPointAt(int key = 0)
        {
            if (key >= ActionConfig.ActionCastConfig.Count)
            {
                return 0.0f;
            }

            return ActionConfig.ActionCastConfig[key].CastPoint;
        }

        public float DataBackswingAt(int key = 0)
        {
            if (key >= ActionConfig.ActionCastConfig.Count)
            {
                return 0.0f;
            }

            return ActionConfig.ActionCastConfig[key].CastBackswingTime;
        }

        public float DataMultipleAt(ActAbiDataKey key = ActAbiDataKey.Key0)
        {
            if (ActionConfig.DamageMultiple.ContainsKey(key))
                return ActionConfig.DamageMultiple[key];
            return 0.0f;
        }

        public AnimatorParamConfig DataAnimatorParamAt(int key = 0)
        {
            if (key >= ActionConfig.AnimatorParam.Count)
            {
                return null;
            }

            return ActionConfig.AnimatorParam[key];
        }

        #endregion


        protected void ApplyAnimatorParam(int key = 0, Animator animator = null)
        {
            var variant = DataAnimatorParamAt(key);
        }

        protected bool NeedFaceTarget = true;
        public int ActionIdx = 0;
        private bool _castBegin = false;

        public override void OnLogic(float logicTime)
        {
            base.OnLogic(logicTime);
            if (_pendingState != FSM.ActiveStateName)
            {
                var newState = _pendingState;
                _pendingState = AbiStateId.None;
                if (newState != AbiStateId.None)
                {
                    if (_pendingCallback != null)
                        _pendingCallback(true);
                    FSM.RequestStateChange(newState);
                }
            }

            FSM.OnLogic();
        }

        #region Casting相关

        public enum ActionCastType
        {
            NoTarget,
            GameUnit,
            Location
        }

        public ActionCastType CastType;

        private AbiStateId _pendingState = AbiStateId.None;
        private Action<bool> _pendingCallback;

        public ActionAbilityResult PendingActing()
        {
            var result = new ActionAbilityResult();
            CastResult = result;
            PendingState(AbiStateId.Acting, (val) =>
            {
                if (!val)
                {
                    CastResult.IsAbiBeginFailed = true;
                }
            });
            return result;
        }

        public void PendingState(AbiStateId id, Action<bool> callback)
        {
            Debug.Assert(_pendingState != id);
//            Debug.LogError($"{ActionConfig.Prototype} (Pending: {_pendingState} {FSM.ActiveStateName})-> {id}");
            _pendingState = id;
            if (_pendingCallback != null)
                _pendingCallback(false);
            _pendingCallback = callback;
        }

        // 施法目标遮罩
        public int CastTargetLayerMask = 0;

        public float CustomCastPointOnce = -1.0f;

        // 自定义释放持续时间
        public float CustomActingTimeOnce = -1.0f;


        public float CustomBackswingTimeOnce = -1.0f;

        protected SoloActionParam soloActionParam;

        #endregion

        private ActionAbilityResult CastResult;

        public bool CanBegin
        {
            get
            {
                if (FSM.ActiveStateName == AbiStateId.Idle && _begin)
                    Debug.Assert(false);
                if (_begin)
                    return false;
                if (AbiOwner.IsSoloActing || AbiOwner.IsStun || AbiOwner.IsDead || AbiOwner.IsHit)
                    return false;
                if (IsCoolingDown)
                    return false;
                if (FSM.ActiveStateName != AbiStateId.Idle)
                    return false;
                return true;
            }
        }

        public ActionAbilityResult BeginAtTargetUnit(GameUnit unit)
        {
            if (CanBegin)
            {
                DirectionToTarget = (unit.transform.position - OwnerLocation).normalized;
                mTargetUnit = unit;
                if (FSM.ActiveStateName == AbiStateId.Idle && _pendingState != AbiStateId.Acting)
                {
                    Debug.Assert(CastResult == null);
                    var result = PendingActing();
                    return result;
                }
            }

            return null;
        }

        public ActionAbilityResult BeginNoTarget()
        {
            if (CanBegin)
            {
                DirectionToTarget = AbiOwner.transform.forward;
                DirectionToTarget.Normalize();
                if (FSM.ActiveStateName == AbiStateId.Idle && _pendingState != AbiStateId.Acting)
                {
                    Debug.Assert(CastResult == null);
                    var result = PendingActing();
                    return result;
                }
            }

            return null;
        }

        public ActionAbilityResult BeginAtLocation(Vector3 location)
        {
            if (CanBegin)
            {
                if (location == Vector3.zero)
                    return null;
                targetLocation = location;
                DirectionToTarget = TargetLocation - OwnerLocation;
                DirectionToTarget.Normalize();
                if (FSM.ActiveStateName == AbiStateId.Idle && _pendingState != AbiStateId.Acting)
                {
                    Debug.Assert(CastResult == null);
                    var result = PendingActing();
                    return result;
                }
            }

            return null;
        }


        public bool CanEnd
        {
            get { return FSM.ActiveStateName == AbiStateId.Acting; }
        }

        public virtual void ForceEnd()
        {
            _forceEnd = true;
        }

        private bool _forceEnd = false;


        private bool _actingBreak = false;
        protected bool EndWhenActionExit = true;

        private StateBase<AbiStateId> idleState;
        private StateBase<AbiStateId> preActingState;
        private StateBase<AbiStateId> actingState;

        public override void Init()
        {
            base.Init();
            mCooldownTime = ActionConfig.mCooldownTime;
            HasIndicator = ActionConfig.mHasIndicatorDefault;
            abilityType = AbilityType.ActionAbility;
            idleState = new State<AbiStateId>((state) =>
                {
                    if (_begin == true)
                        Debug.Assert(false);
                }, (state) => { },
                (state) => { });
            actingState = new State<AbiStateId>((state) => { AbiBegin(); }, (state) =>
                {
                    if (_castBegin)
                    {
                        ActionCastBegin();
                        OnApplyAnimator();
                        _castBegin = false;
                    }

                    if (_castBreak)
                    {
                        ActionCastBreak();
                        _casting = false;
                        _actionExit = true;
                        _castBreak = false;
                    }

                    if (_casting)
                    {
                        AbiOwner.CharEvent.OnAbilityCasting?.Invoke(AbiOwner, this);
                        OnActionCasting?.Invoke();
                    }

                    if (_castEnd)
                    {
                        _casting = false;
                        ActionCastEnd();
                        _castEnd = false;
                    }

                    if (_actingBegin)
                    {
                        CastResult.IsActingBegin = true;
                        ActionActBegin();
                        _actingBegin = false;
                    }

                    if (_actingActing)
                    {
                        ActionActing();
                    }

                    if (_actingBreak)
                    {
                        ActionActBreak();
                        _actionExit = true;
                        _actingBreak = false;
                    }

                    if (_actingEnd)
                    {
                        ActionActingEnd();
                        _actingEnd = false;
                    }

                    if (_actionExit)
                    {
                        soloActionResult = null;
                        _actionExit = false;
                        ActionExit();
                        if (EndWhenActionExit)
                            _forceEnd = true;
                    }

                    if (_forceEnd)
                    {
                        PendingState(AbiStateId.Idle, null);
                        _forceEnd = false;
                    }
                },
                (state) => { AbiEnd(); });
            FSM.AddState(AbiStateId.Idle, idleState);
            FSM.AddState(AbiStateId.Acting, actingState);
            FSM.SetStartState(AbiStateId.Idle);
            FSM.Init();
            if (ActionConfig.FacingTargetWhenCast)
            {
                OnActionCasting += () =>
                {
                    CharacterUnit target = null;
                    OverlapSphereEnemy<CharacterUnit>(AbiOwner.WorldPosition, 5, out var ret, false);
                    foreach (var selection in ret)
                    {
                        target = selection;
                        break;
                    }

                    if (target != null)
                    {
                        DirectionToTarget = AbiOwner.DirectionToTarget(target);

                        AbiOwner.RotateTowards(AbiOwner.DirectionToTarget(target), 720 * KTime.scaleDeltaTime);
                    }
                };
            }
        }

        protected virtual void OnApplyAnimator()
        {
            if (AutoApplyAnimator)
                ApplyAnimatorParam(ActionIdx);
        }

        protected SoloActionResult soloActionResult = null;

        protected virtual void DoAction()
        {
            Debug.Assert(soloActionResult is null);
            float actingTime = DataActingTimeAt(ActionIdx);
            float castPoint = DataCastPointAt(ActionIdx);
            float backswingTime = DataBackswingAt(ActionIdx);
            if (CustomCastPointOnce > 0)
                castPoint = CustomCastPointOnce;
            if (CustomActingTimeOnce > 0)
                actingTime = CustomActingTimeOnce;
            if (CustomBackswingTimeOnce > 0)
                backswingTime = CustomBackswingTimeOnce;
            if (!ActionConfig.ActionCastConfig.SafeAccess(ActionIdx, out var config))
            {
            }


            switch (CastType)
            {
                case ActionCastType.NoTarget:
                case ActionCastType.GameUnit:
                case ActionCastType.Location:
                    if (castPoint >= 0)
                    {
                        soloActionParam = new SoloActionParam()
                        {
                            ActionName = $"{mAbiName}/{ActionIdx}",
                            FaceDirection = DirectionToTarget,
                            NeedFaceTarget = NeedFaceTarget,
                            CastTime = castPoint,
                            ActionTime = actingTime,
                            BackswingTime = backswingTime,
                            CastEndureLevel = config.CastEndureLevel,
                            ActingEndureLevel = config.ActingEndureLevel
                        };
                        soloActionParam.AnimatorParam = DataAnimatorParamAt(ActionIdx);
                        soloActionParam.OnActionBegin = param =>
                        {
                            soloActionResult = param;
                            soloActionResult.OnCastBegin = param =>
                            {
                                CastResult.IsCastBegin = true;
                                _castBegin = true;
                            };
                            soloActionResult.OnCastBreak = param =>
                            {
                                CastResult.IsCastBreak = true;
                                _castBreak = true;
                            };
                            soloActionResult.OnCasting = param =>
                            {
                                CastResult.IsCasting = true;
                                _casting = true;
                            };
                            soloActionResult.OnCastEnd = param =>
                            {
                                _casting = false;
                                _castEnd = true;
                            };
                            soloActionResult.OnActingBegin = param => { _actingBegin = true; };
                            soloActionResult.OnActingLogic = param => { _actingActing = true; };
                            soloActionResult.OnActingBreak = param =>
                            {
                                _actingBreak = true;
                                _actingActing = false;
                            };
                            soloActionResult.OnActingEnd = param =>
                            {
                                _actingActing = false;
                                _actingEnd = true;
                            };
                            soloActionResult.OnActionExit = param => { _actionExit = true; };
                            soloActionParam = null;
                        };
                        soloActionParam.OnActionBeginFailed = () =>
                        {
                            //未成功进入SoloAction
                            CastResult.IsCastFailed = true;
                            PendingState(AbiStateId.Idle, (val) => { });
                            soloActionParam = null;
                        };
                        if (!AbiOwner.SoloAction(soloActionParam))
                        {
                            CastResult.IsCastFailed = true;
                            PendingState(AbiStateId.Idle, (val) => { });
                            soloActionParam = null;
                        }
                    }

                    break;
            }
        }

        private bool _begin = false;

        protected virtual void AbiBegin()
        {
            Debug.Assert(_begin == false);
            Debug.Assert(_end == true);
            Debug.Assert(CastResult != null);
            AbiOwner.CurrentActionAbi = this;
            _begin = true;
            _end = false;
            if (ActionConfig.ManaCost.SafeAccess(0, out var manaCost))
            {
                AbiOwner.CostMana(manaCost);
            }

            BeginCoolDown();
            OnAbiBegin?.Invoke(this);
            DoAction();
        }

        protected bool _end = true;

        protected virtual void AbiEnd()
        {
            Debug.Assert(_end == false);
            Debug.Assert(_begin == true);
            Debug.Assert(CastResult != null);
            AbiOwner.CurrentActionAbi = null;
            CastResult.IsAbiEnd = true;
            _begin = false;
            _end = true;
            CustomActingTimeOnce = -1;
            CustomBackswingTimeOnce = -1;


            _castBegin = false;
            _castBreak = false;
            _actingBreak = false;
            _castEnd = false;
            _actingBegin = false;
            _forceEnd = false;
            _actionExit = false;
            CastResult = null;

            AbiOwner.Log($"{this.GetType().Name} Acting -> None");

            OnAbiEnd?.Invoke(this);
        }


        protected virtual void AbiActing()
        {
            OnAbiActing?.Invoke(this);
        }

        protected virtual void ActionCastBegin()
        {
            OnActionCastBegin?.Invoke();

            if (ActionConfig.CastOwnerLocationVFX && AutoVFX)
            {
                VfxAPI.CreateVisualEffect(ActionConfig.CastOwnerLocationVFX.gameObject, AbiOwner.WorldPosition,
                    TargetDirectionNoY);
            }

            if (ActionConfig.CastTargetLocationVFX && AutoVFX)
            {
                VfxAPI.CreateVisualEffect(ActionConfig.CastTargetLocationVFX.gameObject, TargetLocation,
                    TargetDirectionNoY);
            }

            if (ActionConfig.CastAudio.Count > 0)
            {
                AbiOwner.PlayAudioByChannel(ActionConfig.CastAudio.RandomAccess(), CharacterAudioChannel.VO_Channel0,
                    false);
            }
        }

        private bool _castBreak = false;
        private bool _casting = false;

        protected virtual void ActionCastBreak()
        {
            OnActionCastBreak?.Invoke();
        }

        private bool _castEnd = false;
        private bool _actingEnd = false;

        protected virtual void ActionCastEnd()
        {
            OnActionCastEnd?.Invoke();
            AbiOwner.CharEvent.OnAbilityCasting?.Invoke(AbiOwner, this);
        }

        private bool _actingBegin = false;
        public bool AutoVFX = true;

        protected virtual void ActionActBreak()
        {
            OnActionActingBreak?.Invoke();
        }

        protected virtual void ActionActBegin()
        {
            OnActionActingBegin?.Invoke();

            if (ActionConfig.ActingOwnerLocationVFX && AutoVFX)
            {
                VfxAPI.CreateVisualEffect(ActionConfig.ActingOwnerLocationVFX.gameObject, AbiOwner.WorldPosition,
                    TargetDirectionNoY);
            }

            if (ActionConfig.ActingTargetLocationVFX && AutoVFX)
            {
                VfxAPI.CreateVisualEffect(ActionConfig.ActingTargetLocationVFX.gameObject, TargetLocation,
                    TargetDirectionNoY);
            }
        }

        private bool _actingActing = false;

        protected virtual void ActionActing()
        {
            OnActionActing?.Invoke();
        }

        protected virtual void ActionActingEnd()
        {
            OnActionActingEnd?.Invoke();
        }

        private bool _actionExit = false;

        protected virtual void ActionExit()
        {
        }

        public virtual bool IsIdle()
        {
            return FSM.ActiveStateName == AbiStateId.Idle;
        }

        public virtual bool IsActing()
        {
            return FSM.ActiveStateName == AbiStateId.Acting;
        }

        public Vfx WarningBox(Vector3 startPos, Vector3 size, Vector3 direction, float time,
            ValueLevel level = ValueLevel.Level2)
        {
            var position = startPos;
            return AbilityAPI.CreateSqureWarning(position, direction, size, time, level);
        }

        public Vfx WarningCircle(Vector3 startPos, Vector3 size, float time, ValueLevel level = ValueLevel.Level2)
        {
            var position = startPos;
            return AbilityAPI.CreateCicleWarning(position, size, time, level);
        }

        public void OverlapBox<T>(Vector3 pos, Vector3 halfSize, Vector3 direction, out List<T> list,
            bool debugDraw = true) where T : GameUnit
        {
            GameUnitAPI.GetGameUnitInBox(pos, halfSize, Quaternion.LookRotation(direction),
                GameUnitAPI.GetCharacterLayerMask(), out list,
                (t) => { return AbiOwner.IsEnemy(t as CharacterUnit) && t.IsAlive; }, debugDraw: debugDraw);
        }

        public CharacterUnit GetLockEnemy(float range = 6.0f)
        {
            if (TargetUnit)
                return TargetUnit as CharacterUnit;
            GameUnitAPI.GetGameUnitInSphere<CharacterUnit>(AbiOwner.WorldPosition, range,
                GameUnitAPI.GetCharacterLayerMask(), out var list,
                (t) => { return AbiOwner.IsEnemy(t as CharacterUnit) && t.IsAlive; });
            CharacterUnit result = null;
            list.SafeAccess(0, out result);
            return result;
        }

        public void OverlapSphereEnemy<T>(Vector3 pos, float size, out List<T> list, bool debugDraw = true)
            where T : GameUnit
        {
            GameUnitAPI.GetGameUnitInSphere(pos, size, GameUnitAPI.GetCharacterLayerMask(), out list,
                (t) => { return AbiOwner.IsEnemy(t as CharacterUnit) && t.IsAlive; }, debugDraw: debugDraw);
        }
    }
}