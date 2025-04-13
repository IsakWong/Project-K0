using System;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using Framework.Debug;
using Framework.Foundation;
using FSM;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace K1.Gameplay.AI
{
    public class K1BehaviorTreeBuilder : BehaviorTreeBuilder
    {
        public K1BehaviorTreeBuilder(GameObject owner) : base(owner)
        {
        }
    }

    public class AICharacterBehaviorTreeBlackboard
    {
        
    }

    public class AICharacterBehaviorTreeBuilder : K1BehaviorTreeBuilder
    {
        public AICharacterController OwnerController;
        public float TickDelta;

        /// <summary>
        /// distance为负的时候是在目标到Owner的中间
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Vector3 LocationToTarget(float distance)
        {
            return TargetUnitLocation + -DirectionToTarget * distance;
        }

        public bool TargetAngleInRange(float val)
        {
            var angle = TargetAngle;
            if (angle < val && angle > 360 - val)
                return true;
            return false;
        }

        public float TargetAngle
        {
            get
            {
                var rotation = Quaternion.FromToRotation(ControlCharacter.transform.forward, DirectionToTarget);
                return rotation.eulerAngles.y;
            }
        }

        public CharacterUnit ControlCharacter
        {
            get => OwnerController.ControlCharacter;
        }

        public AICharacterBehaviorTreeBuilder(GameObject owner) : base(owner)
        {
        }


        private Dictionary<string, int> conditionCounts = new();

        public AICharacterBehaviorTreeBuilder ConditionCountLess(string tag, int maxCount)
        {
            Condition(() =>
            {
                if (!conditionCounts.ContainsKey(tag))
                    conditionCounts[tag] = maxCount;
                if (conditionCounts[tag] > 0)
                {
                    conditionCounts[tag]--;
                    return true;
                }

                return false;
            });
            return this;
        }

        public AICharacterBehaviorTreeBuilder ConditionHealthPercentLessThan(float percent)
        {
            Condition(() => { return ControlCharacter.HealthPercent < percent; });
            return this;
        }

        private Dictionary<int, float> conditionTimer = new();

        int TimerCount = 0;

        public AICharacterBehaviorTreeBuilder ConditionDo(Func<bool> condition, Func<bool, TaskStatus> then)
        {
            Do(() =>
            {
                if (condition != null)
                    return then(condition());
                return TaskStatus.Success;
            });
            return this;
        }

        public AICharacterBehaviorTreeBuilder ConditionCooldown(float cooldDown)
        {
            Do($"条件:间隔大于{cooldDown}",() =>
            {
                int Idx = TimerCount;
                if (!conditionTimer.ContainsKey(Idx))
                {
                    conditionTimer[Idx] = Time.realtimeSinceStartup;
                    return TaskStatus.Success;
                }

                float last = conditionTimer[Idx];
                if (Time.realtimeSinceStartup - last > cooldDown)
                {
                    conditionTimer[Idx] = Time.realtimeSinceStartup;
                    return TaskStatus.Success;
                }

                return TaskStatus.Failure;
            });
            TimerCount++;
            return this;
        }

        private float _PlayAnimRemainTime = 0.0f;

        public AICharacterBehaviorTreeBuilder PlayAnim(string trigger, float time = -1.0f)
        {
            Do("PlayAnim", () =>
            {
                _PlayAnimRemainTime -= TickDelta;
                if (_PlayAnimRemainTime < 0)
                    return TaskStatus.Success;
                return TaskStatus.Continue;
            }, () =>
            {
                ControlCharacter.SetAnimatorTrigger(trigger);
                _PlayAnimRemainTime = time;
            }, () => { _PlayAnimRemainTime = 0.0f; });
            return this;
        }

        bool _soloAction = false;
        public SoloActionParam SoloActionParam;
        public SoloActionResult SoloActionResult;

        public AICharacterBehaviorTreeBuilder SoloAction(Action setParam = null)
        {
            using (new ScopeSequence(this))
            {
                Facing();
                Do("SoloAction", () =>
                    {
                        if (_soloAction)
                            return TaskStatus.Continue;
                        else
                            return TaskStatus.Success;
                    }
                    , () =>
                    {
                        setParam?.Invoke();
                        _soloAction = true;
                        ControlCharacter.SoloAction(SoloActionParam);
                    }, () => { _soloAction = false; });
            }


            return this;
        }

        public Vector3 DirectionToTarget
        {
            get => (TargetUnitLocation - ControlCharacter.WorldPosition).normalized;
        }

        public float DistanceToTarget
        {
            get => (TargetUnitLocation - ControlCharacter.WorldPosition).magnitude;
        }

        public Vector3 Walk_TargetLocation;

        private NavMeshPath path = new NavMeshPath();

        void DrawPath(NavMeshPath path)
        {
            if (path.corners.Length < 2)
                return;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
            }
        }

        private BehaviourTreeEvent _highPriority = BehaviourTreeEvent.None;

        public void ConditionHighPriority(BehaviourTreeEvent val)
        {
            Condition(() =>
            {
                if (_highPriority == val)
                {
                    _highPriority = BehaviourTreeEvent.None;
                    return true;
                }

                return _highPriority == val;
            });
        }

        public void PushHighPriorityNode(BehaviourTreeEvent node)
        {
            _highPriority = node;
        }

        public TaskStatus ConditionInvoke(BehaviourTreeEvent node, Action callback)
        {
            if (_highPriority == node)
            {
                callback?.Invoke();
                _highPriority = BehaviourTreeEvent.None;
            }

            return TaskStatus.Success;
        }

        public float DirectWalk_Angle = 90;
        public float DirectWalk_Time;
        private float _directWalkTimer = 0.0f;

        public AICharacterBehaviorTreeBuilder DirectWalk(Action setParam = null)
        {
            Yasuo_DefenceBuff defenceBuff = null;
            EndureBuff endureBuff = null;
            bool IsHitted = false;
            Action<CharacterUnit, CharacterUnit, DamageParam> hitAction = (unit, characterUnit, arg3) =>
            {
                IsHitted = true;
            };
            Sequence("Walk")
                .Do(() =>
                {
                    if (_directWalkTimer > DirectWalk_Time)
                    {
                        return TaskStatus.Success;
                    }


                    _directWalkTimer += TickDelta;
                    var targetDirection = MathUtility.RotateDirectionY(DirectionToTarget, DirectWalk_Angle);
                    ControlCharacter.DirectWalk(targetDirection, DirectionToTarget);
                    if (IsHitted)
                        return TaskStatus.Failure;
                    if (ControlCharacter.IsStun)
                        return TaskStatus.Failure;
                    if (ControlCharacter.IsHit)
                        return TaskStatus.Failure;
                    return TaskStatus.Continue;
                }, () =>
                {
                    _directWalkTimer = 0;
                    setParam?.Invoke();
                    ControlCharacter.CharEvent.OnTakeDamage += hitAction;
                    // endureBuff = GameplayConfig.Instance().DefaultEndureBuff.CreateBuff() as EndureBuff;
                    // endureBuff.SetLifetime(999);
                    // endureBuff.AddTo(ControlCharacter, ControlCharacter);
                    IsHitted = false;
                    defenceBuff =
                        ControlCharacter.GetAbility<Yasuo_Defense>().DefenceBuff.As().CreateBuff() as Yasuo_DefenceBuff;
                    defenceBuff.SetLifetime(999);
                    defenceBuff.AddTo(ControlCharacter, ControlCharacter);
                }, () =>
                {
                    IsHitted = false;
                    ControlCharacter.Idle("Idle1");
                    ControlCharacter.CharEvent.OnTakeDamage -= hitAction;
                    ControlCharacter.RemoveBuff(defenceBuff);
//                    ControlCharacter.RemoveBuff(endureBuff);
                });
            End();
            return this;
        }

        public AICharacterBehaviorTreeBuilder Walk(Action setParam = null)
        {
            Do("移动", () =>
            {
                if ((ControlCharacter.WorldPosition - Walk_TargetLocation).magnitude < 0.1f)
                {
                    return TaskStatus.Success;
                }

                if (!ControlCharacter.IsWalking)
                    ControlCharacter.WalkTowards(Walk_TargetLocation);
                return TaskStatus.Continue;
            }, () =>
            {
                setParam?.Invoke();
                ControlCharacter.WalkTowards(Walk_TargetLocation);
            }, () => { });
            return this;
        }

        public AICharacterBehaviorTreeBuilder WalkToUnit(float distance = 1.5f, Action start = null, Action exit = null)
        {
            Do($"移动至目标{distance}处",() =>
            {
                if (ControlCharacter.IsDead)
                    return TaskStatus.Failure;
                if ((ControlCharacter.WorldPosition - TargetUnitLocation).magnitude < distance)
                    return TaskStatus.Success;
                ControlCharacter.WalkTowards(TargetUnitLocation);
                return TaskStatus.Continue;
            }, start, exit);
            return this;
        }

        public float WalkAround_Range = 1.0f;
        public float WalkAround_Internal = 1.0f;

        public AICharacterBehaviorTreeBuilder WalkAround()
        {
            Sequence("闲逛移动")
                .Do("移动",() =>
                {
                    var direction = Random.onUnitSphere;
                    direction.y = 0;
                    var delta = direction * WalkAround_Range;
                    var target = ControlCharacter.WorldPosition + delta;
                    ControlCharacter.WalkTowards(target);
                    return TaskStatus.Success;
                })
                .WaitTime(WalkAround_Internal)
                .End();
            return this;
        }

        public Vector3 CustomCastLocation;


        public float Wait_Time = 0.0f;
        float _tickCounter = 0;

        public AICharacterBehaviorTreeBuilder WaitInDuration(Action setParam = null)
        {
            Do($"等待 {TickDelta}s",() =>
            {
                _tickCounter -= TickDelta;
                if (_tickCounter < 0)
                    return TaskStatus.Success;
                return TaskStatus.Continue;
            }, () =>
            {
                setParam?.Invoke();
                _tickCounter = Wait_Time;
            });
            return this;
        }

        public AICharacterBehaviorTreeBuilder If(Func<bool> condition, Action thenAction = null,
            Action elseAction = null)
        {
            using (new ScopeSelector(this, "If"))
            {
                using (new ScopeSequence(this, "Then"))
                {
                    Condition(condition);
                    ReturnSuccess();
                    using (new ScopeSequence(this, "ThenAction"))
                    {
                        thenAction?.Invoke();
                    }

                    End();
                }

                using (new ScopeSequence(this, "Else"))
                {
                    ReturnSuccess();
                    using (new ScopeSequence(this, "ThenAction"))
                    {
                        elseAction?.Invoke();
                    }

                    End();
                }
            }

            return this;
        }

        public AICharacterBehaviorTreeBuilder IfDistanceBetween(float min, float max, Action thenAction = null,
            Action elseAction = null)
        {
            Selector("If");
            Sequence();
            ConditionDistanceIn(min, max);
            ReturnSuccess();
            Sequence("Then");
            thenAction?.Invoke();
            End();
            End();
            End();

            ReturnSuccess();
            Sequence("Else");
            elseAction?.Invoke();
            End();
            End();

            End();
            return this;
        }

        public AICharacterBehaviorTreeBuilder DistanceInCircle(float distance, float range)
        {
            Do($"条件: 处于{distance - range} - {distance + range}之间", () =>
            {
                var dis = (ControlCharacter.WorldPosition - TargetUnit.WorldPosition).magnitude;
                if (dis > distance - range && dis < distance + range)
                    return TaskStatus.Success;
                return TaskStatus.Failure;
            });
            return this;
        }

        public AICharacterBehaviorTreeBuilder ConditionDistanceIn(float min, float max)
        {
            Do($"条件: 处于{min} - {max}之间", () =>
            {
                var distance = (ControlCharacter.WorldPosition - TargetUnit.WorldPosition).magnitude;
                if (distance > min && distance < max)
                    return TaskStatus.Success;
                return TaskStatus.Failure;
            });
            return this;
        }

        public AICharacterBehaviorTreeBuilder IsTargetCastAbility<T>() where T : ActionAbility
        {
            Do(() =>
            {
                var abi = ControlCharacter.GetAbility<T>();
                if (abi == null)
                    return TaskStatus.Failure;
                if (abi.IsActing())
                    return TaskStatus.Success;
                return TaskStatus.Failure;
            });
            return this;
        }


        public ActionAbility TargetUnitActingAbi
        {
            get { return TargetUnit.ActingAbi; }
        }

        public AICharacterBehaviorTreeBuilder IsTargetActing()
        {
            Do(System.Reflection.MethodBase.GetCurrentMethod().Name, () =>
            {
                if (TargetUnit.IsSoloActing)
                    return TaskStatus.Success;
                return TaskStatus.Failure;
            });
            return this;
        }

        public class CastResult
        {
            public bool isEnded = false;
            public bool isBreak = false;
        }


        public ActionAbilityResult _castResult = null;
        public ActionAbilityResult LastCastResult = null;
        ActionAbility _currentCastAbi = null;

        public ActionAbility LastCastAbi = null;

        public AICharacterBehaviorTreeBuilder Facing()
        {
            Do(() =>
            {
                var idleState = ControlCharacter.FSM.GetState(CharacterStateID.IdleState) as IdleCharacterState;
                if (idleState.TargetDirection == Vector3.zero)
                    return TaskStatus.Success;
                else
                    return TaskStatus.Continue;
            }, () =>
            {
                var idleState = ControlCharacter.FSM.GetState(CharacterStateID.IdleState) as IdleCharacterState;
                idleState.TargetDirection = DirectionToTarget;
                ControlCharacter.Idle();
            }, () => { });
            return this;
        }

        public AudioClip PlayAudio_Clip;

        public AICharacterBehaviorTreeBuilder PlayAudio(Action setParam, float cooldown)
        {
            Sequence("播放音频序列");
            ConditionCooldown(cooldown);
            Do("音频", () => { return TaskStatus.Success; }, () =>
            {
                setParam?.Invoke();
                if (PlayAudio_Clip)
                    KGameCore.SystemAt<AudioModule>().PlayAudio(PlayAudio_Clip);
            });
            End();
            return this;
        }

        public AICharacterBehaviorTreeBuilder CastAbi<T>(Action<T> setParam = null,
            bool untilSuccess = false,
            CastType type = CastType.TargetUnit) where T : ActionAbility
        {
            CastAbiByName(typeof(T).Name, setParam, untilSuccess, type);
            return this;
        }

        public AICharacterBehaviorTreeBuilder CastAbiByName<T>(string name, Action<T> setParam = null,
            bool untilSuccess = false, CastType type = CastType.TargetUnit) where T : ActionAbility
        {
            Sequence($"释放技能:{name}序列");
            Do("等待释放技能", () =>
            {
                if (_castResult != null)
                    return TaskStatus.Continue;
                var abi = ControlCharacter.GetAbility(name);
                LastCastResult = _castResult;
                _castResult = null;
                _currentCastAbi = abi;
                return TaskStatus.Success;
            }, () => { }, () => { });
            Do($"释放技能中{name}", () =>
                {
                    var abi = ControlCharacter.GetAbility(name);
                    if (_castResult == null)
                    {
                        abi = _currentCastAbi;
                        if (abi == null)
                            return TaskStatus.Failure;
                        if (untilSuccess && abi.IsCoolingDown)
                            return TaskStatus.Continue;
                        if (!_currentCastAbi.CanBegin)
                            return TaskStatus.Continue;
                        if (_currentCastAbi.IsActing())
                            return TaskStatus.Continue;
                        if (ControlCharacter.IsSoloActing)
                            return TaskStatus.Continue;
                        setParam?.Invoke(abi as T);

                        KGizmos.Instance.DrawGizmos(() =>
                        {
#if UNITY_EDITOR
                            if (type == CastType.TargetUnit)
                            {
                                Handles.Label(TargetUnit.WorldPosition, $"CastAbi Unit <{abi.GetType().Name}>");
                            }

                            if (type == CastType.CustomLocation)
                            {
                                Handles.Label(CustomCastLocation, $"CastAbi Location<{abi.GetType().Name}>");
                            }
#endif
                        }, 1.0f);

                        if (abi.CastType == ActionAbility.ActionCastType.GameUnit)
                        {
                            _castResult = abi.BeginAtTargetUnit(TargetUnit);
                        }

                        if (abi.CastType == ActionAbility.ActionCastType.Location)
                        {
                            if (type == CastType.TargetUnit)
                                _castResult = abi.BeginAtLocation(TargetUnit.WorldPosition);
                            else if (type == CastType.CustomLocation)
                                _castResult = abi.BeginAtLocation(CustomCastLocation);
                        }

                        if (abi.CastType == ActionAbility.ActionCastType.NoTarget)
                        {
                            _castResult = abi.BeginNoTarget();
                        }
                    }

                    if (_castResult != null)
                    {
                        if (_castResult.IsCastFailed)
                            return TaskStatus.Failure;
                        if (_castResult.IsAbiEnd)
                            return TaskStatus.Success;
                        return TaskStatus.Continue;
                    }

                    return TaskStatus.Failure;
                }
                , () => { }, () =>
                {
                    _castResult = null;
                    _currentCastAbi = null;
                });
            End();
            return this;
        }

        public void Chasing(float chasingDistance = 2.0f, float maxRange = -1.0f)
        {
            Sequence("WalkChasing");
            Condition(() =>
            {
                if (maxRange < 0)
                    return true;
                return DistanceToTarget < maxRange;
            });
            Do("WalkChasing", () =>
            {
                if (DistanceToTarget < chasingDistance)
                    return TaskStatus.Success;
                if (!ControlCharacter.IsWalking)
                    ControlCharacter.WalkTowards(TargetUnitLocation);
                return TaskStatus.Continue;
            });
            End();
        }

        private Vector3 _detectLocation;

        public float Chasing_Angle = 0;
        public float Chasing_Offset = 1.5f;
        public float Chasing_MaxRange = 10;
        public float Chasing_FinishRange = 2;

        public Vector3 Chasing_TargetPos;

        // 追逐状态
        public AICharacterBehaviorTreeBuilder Chasing(Action setParam = null)
        {
            Do(System.Reflection.MethodBase.GetCurrentMethod().Name, () =>
            {
                var direction = (ControlCharacter.transform.position - TargetUnit.transform.position).normalized;
                Chasing_TargetPos = TargetUnit.transform.position + direction * Chasing_Offset;
                var distance = (ControlCharacter.transform.position - Chasing_TargetPos).magnitude;
                if (distance < Chasing_FinishRange)
                    return TaskStatus.Success;
                else if (distance > Chasing_MaxRange)
                    return TaskStatus.Failure;
                if (!ControlCharacter.IsWalking)
                    ControlCharacter.WalkTowards(Chasing_TargetPos);
                return TaskStatus.Continue;
            }, () => { setParam?.Invoke(); });
            return this;
        }

        #region DetectEnemy

        private CharacterUnit _TargetUnit;

        public CharacterUnit TargetUnit
        {
            get => _TargetUnit;
        }

        public Vector3 TargetUnitLocation
        {
            get => _TargetUnit.WorldPosition;
        }

        public List<CharacterUnit> RangeUnits = new List<CharacterUnit>();
        public float DetectEnemy_Range = 40.0f;

        public AICharacterBehaviorTreeBuilder DetectEnemy()
        {
            Do(System.Reflection.MethodBase.GetCurrentMethod().Name,
                () => { return _TargetUnit != null ? TaskStatus.Success : TaskStatus.Failure; }, () =>
                {
                    CharacterUnit target;
                    var range = DetectEnemy_Range;
                    RangeUnits.Clear();
                    var detectLocation = ControlCharacter.WorldPosition;
                    CharacterUnitAPI.ForCharacterInSphere(detectLocation, DetectEnemy_Range,
                        (CharacterUnit selection) =>
                        {
                            if (CharacterUnitAPI.GenericEnemyCondition(ControlCharacter, selection))
                            {
                                RangeUnits.Add(selection);
                            }
                        });
                    if (RangeUnits.Count > 0)
                    {
                        target = RangeUnits.Count > 0 ? RangeUnits[0] : null;
                    }
                    else
                    {
                        target = null;
                    }

                    if (_TargetUnit != target)
                    {
                        OwnerController.OnTargetChange(_TargetUnit, target);
                    }

                    _TargetUnit = target;
                }, () => { });
            return this;
        }

        #endregion

        private float DetectActing_Range = 5.0f;

        public AICharacterBehaviorTreeBuilder DetectActing(Action setParam = null)
        {
            Do("DetectActing", () =>
            {
                if (TargetUnit.IsSoloActing && TargetUnit.IsAlive && DistanceToTarget <= DetectActing_Range)
                    return TaskStatus.Success;
                return TaskStatus.Continue;
            });
            return this;
        }
    }

    public enum CastType
    {
        CustomLocation,
        TargetUnit,
    }
}