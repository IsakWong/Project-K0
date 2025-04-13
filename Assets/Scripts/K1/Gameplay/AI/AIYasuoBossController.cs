using System;
using System.Collections.Generic;
using System.Reflection;
using AYellowpaper.SerializedCollections;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using Framework.Foundation;
using K1.Gameplay.AI;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace K1.Gameplay
{
    public enum BehaviourTreeEvent
    {
        None,
        Confrontation,
        Battle,
        Ultra_Lightning,
        Ultra_SpaceSlash,
        Ultra_Rage,
        Dash,
    };

    public class AIYasuoBossController : AICharacterController
    {
        private AICharacterBehaviorTreeBuilder DefaultBuilder;
        public List<AudioClip> YasuoFastStab;

        public SerializedDictionary<string, List<AudioClip>> BossVOs;
        public List<AudioClip> ConfrontationTalkVO;

        private BehaviorTree ConfrontationTree;
        private BehaviorTree NearBattleTree;


        protected void Awake()
        {
            base.Awake();
            ControlCharacter.CharEvent.OnTakeDamage += (hitted, hitter, param) => { };
            SwitchBehaviour(DefaultPattern);
        }

        private float lastTime;
        private ActionAbilityResult _result;

        protected override void OnTargetCasting(CharacterUnit target, ActionAbility abi)
        {
            var attack = abi as Lux_Attack;
            if (_result != null)
            {
                if (_result.IsAbiBegin)
                {
                    lastTime = Time.realtimeSinceStartup;
                    _result = null;
                }

                if (_result.IsAbiBeginFailed)
                    _result = null;
            }

            if (attack != null)
            {
                var state = target.ActionState;
                if (state.CastPercent < 0.7f)
                    return;
                if (CurrentBuilder.DistanceToTarget < 4.0f && Time.realtimeSinceStartup - lastTime > 10.0f)
                {
                    //CurrentBuilder.PushHighPriorityNode(BehaviourTreeEvent.Dash);
                    _result = CurrentBuilder.ControlCharacter.GetAbility<Yasuo_Dash_H>()
                        .BeginAtLocation(CurrentBuilder.TargetUnitLocation);
                }
            }
        }

        public void InitBehaviorIdle()
        {
            CurrentBuilder.DetectEnemy();
            CurrentBuilder.Do(() =>
            {
                if (CurrentBuilder.TargetUnit != null)
                {
                    BossState = AIBossState.Battle;
                    return TaskStatus.Failure;
                }

                CurrentBuilder.WalkAround_Range = 5.0f;
                CurrentBuilder.WalkAround_Internal = 2.0f;
                return TaskStatus.Success;
            });
            CurrentBuilder.WalkAround();
            CurrentBuilder.DetectEnemy();
            CurrentBuilder.Do(() =>
            {
                BossState = AIBossState.Battle;
                return TaskStatus.Success;
            });
        }

        public void InitBehaviorBattle()
        {
            using (new ScopeSequence(CurrentBuilder, "检测敌人"))
            {
                CurrentBuilder.DetectEnemy();
                using (new ScopeSelector(CurrentBuilder, "战斗"))
                {
                    using (new ScopeSequence(CurrentBuilder, "检测血量"))
                    {
                        CurrentBuilder.Do(() =>
                        {
                            if (CurrentBuilder.ControlCharacter.HealthPercent < 0.5f)
                            {
                                BossPhase = AIBossPhase.Rage0;
                            }
                            else
                            {
                                BossPhase = AIBossPhase.Normal;
                            }

                            return TaskStatus.Failure;
                        });
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionHighPriority(BehaviourTreeEvent.Ultra_Lightning);
                        CurrentBuilder.CastAbi<Yasuo_Lightning>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionHighPriority(BehaviourTreeEvent.Ultra_SpaceSlash);
                        CurrentBuilder.CastAbi<Yasuo_Stab_Circle_Ext>();
                    }

                    using (new ScopeSequence(CurrentBuilder, "超远距离"))
                    {
                        CurrentBuilder.ConditionDistanceIn(25, 200.0f);
                        CurrentBuilder.CastAbi<Yasuo_Blink>(
                            (abi) => { CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(8.0f); },
                            type: CastType.CustomLocation);
                        PatternNormal3_1();
                    }

                    using (new ScopeSequence(CurrentBuilder, "近距离战斗"))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, 10.0f);
                        NearBattle();
                    }

                    using (new ScopeSequence(CurrentBuilder, "远距离战斗"))
                    {
                        FarBattle();
                    }
                }
            }
        }

        public void PatternLightning()
        {
            using (new ScopeSequence(CurrentBuilder, "FastDashAndStab"))
            {
                CurrentBuilder.CastAbi<Yasuo_Blink>(
                    (actionAbi) => { CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(15.0f); },
                    type: CastType.CustomLocation);

                using (new ScopeSequence(CurrentBuilder, "FastDashAndStab"))
                {
                    CurrentBuilder.ConditionDistanceIn(4.0f, 999.0f);
                    CurrentBuilder.CastAbi<Yasuo_Dash_Fast>((actionAbi) =>
                    {
                        KGameCore.SystemAt<AudioModule>().PlayAudio(YasuoFastStab.RandomAccess());
                        CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(2.0f);
                    }, type: CastType.CustomLocation);
                }


                using (new ScopeParallel(CurrentBuilder, ""))
                {
                    using (new ScopeSequence(CurrentBuilder, "FastDashAndStab"))
                    {
                        CurrentBuilder.WaitTime(0.15f);
                        CurrentBuilder.Do(() =>
                        {
                            KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.3f, 0.6f);
                            return TaskStatus.Success;
                        });
                    }

                    using (new ScopeSequence(CurrentBuilder, "FastDashAndStab"))
                    {
                        CurrentBuilder.CastAbi<Yasuo_Stab_Ground>((actionAbi) => { });
                    }
                }

                // CurrentBuilder.ConditionCooldown(30.0f);
                CurrentBuilder.CastAbi<Yasuo_Lightning>();
            }
        }

        public void CheckConfrontation(float confrontationDistance = 9.0f)
        {
            CurrentBuilder.ConditionDo(() => { return CurrentBuilder.DistanceToTarget > confrontationDistance; },
                (val) =>
                {
                    if (val)
                    {
                        CurrentBuilder.PushHighPriorityNode(BehaviourTreeEvent.Confrontation);
                        return TaskStatus.Failure;
                    }

                    return TaskStatus.Success;
                });
        }

        public void EnsureDistance(float abiDistance = 3.0f, float dashMinDstiance = 6.0f,
            float confrontationDistance = 9.0f)
        {
            CurrentBuilder.ConditionDo(() => { return CurrentBuilder.DistanceToTarget > confrontationDistance; },
                (val) =>
                {
                    if (val)
                    {
                        CurrentBuilder.PushHighPriorityNode(BehaviourTreeEvent.Confrontation);
                        return TaskStatus.Failure;
                    }

                    return TaskStatus.Success;
                });
            DashOrWalkTo(abiDistance, dashMinDstiance);
        }

        public void PatternNew4()
        {
            using (new ScopeSequence(CurrentBuilder, "PatternAttackStab"))
            {
                EnsureDistance();
                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, 3);
                        CurrentBuilder.CastAbi<Yasuo_Stab_Ground>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.CastAbi<Yasuo_Stab>((abi) =>
                        {
                            Yasuo_Stab stab = abi as Yasuo_Stab;
                            stab.MovementDistance = Mathf.Min(CurrentBuilder.DistanceToTarget, 4.0f);
                        });
                    }
                }

                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, 5);
                        CurrentBuilder.CastAbi<Yasuo_Stab>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CheckConfrontation(6.0f);
                        CurrentBuilder.CastAbi<Yasuo_Wind>();
                    }
                }
            }
        }

        public void PatternNew5()
        {
            using (new ScopeSequence(CurrentBuilder, "PatternAttackStab"))
            {
                EnsureDistance();
                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, 3);
                        CurrentBuilder.CastAbi<Yasuo_Hand_Wave>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.CastAbi<Yasuo_Stab>((abi) =>
                        {
                            Yasuo_Stab stab = abi as Yasuo_Stab;
                            stab.MovementDistance = Mathf.Min(CurrentBuilder.DistanceToTarget, 4.0f);
                        });
                    }
                }

                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, 5);
                        CurrentBuilder.CastAbi<Yasuo_Stab>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CheckConfrontation(6.0f);
                        CurrentBuilder.CastAbi<Yasuo_Wind>();
                    }
                }

                PatternConfrontation();
            }
        }

        public void PatternAttackCombo()
        {
            using (new ScopeSequence(CurrentBuilder, "PatternAttackCombo"))
            {
                EnsureDistance(2.5f);
                CurrentBuilder.CastAbi<Yasuo_Attack>();

                EnsureDistance(2.5f);
                CurrentBuilder.CastAbi<Yasuo_Attack>();

                CheckConfrontation(6.0f);
                CurrentBuilder.CastAbi<Yasuo_Stab>((abi) =>
                {
                    Yasuo_Stab stab = abi as Yasuo_Stab;
                    stab.MovementDistance = Mathf.Max(CurrentBuilder.DistanceToTarget, 6.0f);
                });

                EnsureDistance(2.5f);
                CurrentBuilder.CastAbi<Yasuo_Attack>();
            }
        }

        public void PatternDashStab()
        {
            using (new ScopeSequence(CurrentBuilder, ""))
            {
                CurrentBuilder.CastAbi<Yasuo_Blink>(
                    (actionAbi) => { CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(-15.0f); },
                    type: CastType.CustomLocation);
                using (new ScopeParallel(CurrentBuilder, ""))
                {
                    using (new ScopeSequence(CurrentBuilder, "DashAndStab"))
                    {
                        CurrentBuilder.WaitTime(0.70f);
                        CurrentBuilder.Do(() =>
                        {
                            KGameCore.SystemAt<GameplayModule>().PushTimeScale(0.3f, 0.5f);
                            return TaskStatus.Success;
                        });
                    }

                    using (new ScopeSequence(CurrentBuilder, "DashAndStab"))
                    {
                        CurrentBuilder.CastAbi<Yasuo_Dash_Stab>(
                            (actionAbi) =>
                            {
                                CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(1.5f);
                            },
                            type: CastType.CustomLocation);
                    }
                }
            }
        }

        private int PatternCount = 5;

        public void PatternCooldownConfrontation()
        {
            using (new ScopeSequence(CurrentBuilder, "CooldownConfrontation"))
            {
                CurrentBuilder.ConditionCooldown(5.0f);
                PatternConfrontation();
                PatternToNear();
            }
        }

        public void NearBattle()
        {
            using (new ScopeSelector(CurrentBuilder))
            {
                // 中距离阶段
                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.Condition(() => { return CurrentBuilder.DistanceToTarget > 4.0f; });
                    using (new ScopeSelector(CurrentBuilder))
                    {
                        PatternCooldownConfrontation();
                        PatternToNear();
                        using (new ScopeSequence(CurrentBuilder, ""))
                        {
                            CurrentBuilder.CastAbi<Yasuo_Act>((abi) =>
                            {
                                Yasuo_Act act = abi as Yasuo_Act;
                                act.ActionIdx = Random.Range(0, 3);
                            });
                            PatternToNear();
                        }
                    }
                }

                //近距离战斗
                using (new ScopeRandomSelector(CurrentBuilder, "Near"))
                {
                    PatternNew1();
                    PatternNew2();
                    PatternNew3();
                    PatternNew4();
                    PatternNew5();
                    PatternBlinkSwordSlash();
                    // Utrla
                    PatternLightning();
                    //概率返回对峙阶段
                    PatternToConfrontation();
                }
            }
        }

        public void PatternToNear()
        {
            DashOrWalkTo(2.0f);
        }

        public void PatternToConfrontation()
        {
            CurrentBuilder.Sequence();
            {
                CurrentBuilder.RandomChance(1, 2);
                CurrentBuilder.ConditionDistanceIn(0.0f, 5.0f);
                DashOrWalkTo(7.0f, skipWhenNear: false);
                PatternConfrontation();
            }
            CurrentBuilder.End();
        }

        public void PatternBlinkSwordSlash()
        {
            using (new ScopeSequence(CurrentBuilder, "ComboBlinkStab"))
            {
                CurrentBuilder.ConditionCooldown(15.0f);
                int Count = 0;
                int MaxCount = 3;
                CurrentBuilder.Do(() =>
                {
                    Count = 0;
                    MaxCount = Random.Range(1, 3);
                    return TaskStatus.Success;
                });
                CurrentBuilder.RepeatUntilFailure();
                CurrentBuilder.Sequence();
                CurrentBuilder.Condition(() => { return Count <= MaxCount; });
                CurrentBuilder.CastAbi<Yasuo_Blink>((abi) =>
                {
                    var direction = MathUtility.RandomDirection();
                    direction.y = 0;
                    direction.Normalize();
                    var blinkAbi = abi as Yasuo_Blink;
                    blinkAbi.ImmediateCast = true;
                    Count++;
                    CurrentBuilder.CustomCastLocation =
                        CurrentBuilder.TargetUnitLocation + direction * Random.Range(10.0f, 15.0f);
                }, type: CastType.CustomLocation);
                using (new ScopeRandomSelector(CurrentBuilder, "FarAttack"))
                {
                    CurrentBuilder.CastAbi<Yasuo_SwordSlash>();
                    CurrentBuilder.CastAbi<Yasuo_SwordSlash_Ground>();
                }

                CurrentBuilder.End();

                CurrentBuilder.End();
            }
        }

        public void Talk()
        {
            CurrentBuilder.ReturnSuccess();
            using (new ScopeSequence(CurrentBuilder, "语音"))
            {
                CurrentBuilder.ConditionCooldown(10.0f);
                CurrentBuilder.Do(() =>
                {
                    var audio = BossVOs["TalkShort"].RandomAccess();
                    CurrentBuilder.ControlCharacter.PlayAudioByChannel(audio, CharacterAudioChannel.VO_Channel0);
                    return TaskStatus.Failure;
                });
            }

            CurrentBuilder.End();
        }

        public void PatternConfrontationOnce(bool talk = true)
        {
            using (new ScopeSequence(CurrentBuilder, "对峙(1次)"))
            {
                if (talk)
                {
                    Talk();
                }

                CurrentBuilder.DirectWalk(() =>
                {
                    CurrentBuilder.DirectWalk_Angle = Random.Range(0, 2) == 0 ? 90.0f : -90.0f;
                    CurrentBuilder.DirectWalk_Time = Random.Range(1.0f, 2.0f);
                });
            }
        }

        public void PatternConfrontation(float walkTimeMax = 2.0f, Action battle = null)
        {
            using (new ScopeSequence(CurrentBuilder, ""))
            {
                Talk();
                CurrentBuilder.DirectWalk(() =>
                {
                    CurrentBuilder.DirectWalk_Time = Random.Range(0.5f, walkTimeMax);
                    CurrentBuilder.DirectWalk_Angle = 90;
                });
                CurrentBuilder.DirectWalk(() =>
                {
                    CurrentBuilder.DirectWalk_Time = Random.Range(0.5f, walkTimeMax);
                    CurrentBuilder.DirectWalk_Angle = -90;
                });
            }
        }

        public void _PatternDoubleStab()
        {
            CurrentBuilder.CastAbi<Yasuo_Double_Stab>();
            using (new ScopeSelector(CurrentBuilder))
            {
                CurrentBuilder.CastAbi<Yasuo_Stab>((abi) =>
                {
                    Yasuo_Stab stab = abi as Yasuo_Stab;
                    stab.MovementDistance = Mathf.Max(CurrentBuilder.DistanceToTarget, 5.0f);
                });
                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.CastAbi<Yasuo_Hand_Wave>();
                    CurrentBuilder.CastAbi<Yasuo_SwordSlash>();
                }
            }
        }

        public void Dash()
        {
            CurrentBuilder.Do(() => { return CurrentBuilder.ConditionInvoke(BehaviourTreeEvent.Dash, () => { }); });
        }

        public void PatternMiss()
        {
            using (new ScopeSequence(CurrentBuilder, "Pattern1"))
            {
                Dash();
            }
        }

        public void PatternNew1()
        {
            using (new ScopeSequence(CurrentBuilder, "Pattern1"))
            {
                Talk();
                CurrentBuilder.ReturnSuccess();
                PatternConfrontationOnce();
                CurrentBuilder.End();
                EnsureDistance(1.5f);
                CurrentBuilder.CastAbi<Yasuo_Attack>();
                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0.0f, 2f);
                        CurrentBuilder.CastAbi<Yasuo_Attack>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.CastAbi<Yasuo_Stab>();
                    }
                }

                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0.0f, 4f);
                        CurrentBuilder.CastAbi<Yasuo_Stab_Phantom>();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        DashOrWalkTo(1.5f, 5f, true);
                        CurrentBuilder.CastAbi<Yasuo_Stab_Circle>();
                    }
                }
            }
        }

        public void PatternNew2()
        {
            using (new ScopeSequence(CurrentBuilder, "Pattern1"))
            {
                EnsureDistance(6.0f);
                CurrentBuilder.ReturnSuccess();
                PatternConfrontationOnce();
                CurrentBuilder.End();
                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(5.0f, 999.0f);
                        CurrentBuilder.CastAbi<Yasuo_Dash>((abi) =>
                        {
                            Yasuo_Dash dash = abi as Yasuo_Dash;
                            dash.SpeedOnce = 30.0f;
                            CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(1.5f);
                        }, type: CastType.CustomLocation);
                        CurrentBuilder.CastAbi<Yasuo_Stab_Ground>();
                    }

                    CurrentBuilder.CastAbi<Yasuo_Stab_Ground>();
                }

                using (new ScopeSelector(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ConditionDistanceIn(0.0f, 4f);
                        _PatternDoubleStab();
                    }

                    using (new ScopeSequence(CurrentBuilder))
                    {
                        CurrentBuilder.ReturnSuccess();
                        PatternConfrontationOnce();
                        CurrentBuilder.End();
                        EnsureDistance(4.0f);
                        CurrentBuilder.CastAbi<Yasuo_Dash>((abi) =>
                        {
                            Yasuo_Dash dash = abi as Yasuo_Dash;
                            dash.SpeedOnce = 30.0f;
                            CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(1.5f);
                        }, type: CastType.CustomLocation);
                        _PatternDoubleStab();
                    }
                }
            }
        }

        public void PatternDetectShield()
        {
            using (new ScopeSelector(CurrentBuilder))
            {
                using (new ScopeParallel(CurrentBuilder))
                {
                    using (new ScopeSequence(CurrentBuilder, "PatternDetect"))
                    {
                        EnsureDistance(2.0f);
                        PatternAttackCombo();
                    }

                    using (new ScopeSequence(CurrentBuilder, "PatternDetect"))
                    {
                        CurrentBuilder.Do(() => { return TaskStatus.Continue; }, () => { }, () => { });
                    }
                }
            }
        }


        public void PatternNormal3_1()
        {
            CurrentBuilder.CastAbi<Yasuo_SwordSlash_Ground_Fast>((ab) =>
            {
                var sword = ab as Yasuo_SwordSlash_Ground_Fast;
                sword.ActionIdx = 0;
            });
            CurrentBuilder.CastAbi<Yasuo_SwordSlash_Ground_Fast>((ab) =>
            {
                var sword = ab as Yasuo_SwordSlash_Ground_Fast;
                sword.ActionIdx = 1;
            });
            CurrentBuilder.CastAbi<Yasuo_SwordSlash_Ground>((abi) =>
            {
                var swordAbi = abi as Yasuo_SwordSlash_Ground;
                swordAbi.ActionIdx = 1;
            });
            DashOrWalkTo(4.0f, 6.0f);
            using (new ScopeSelector(CurrentBuilder))
            {
                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.CastAbi<Yasuo_Stab_Phantom>();
                }

                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.CastAbi<Yasuo_Stab>();
                }
            }

            CurrentBuilder.ConditionDistanceIn(0.0f, 5.0f);
            CurrentBuilder.CastAbi<Yasuo_Hand_Wave>();
            DashOrWalkTo(8.0f);
            CurrentBuilder.CastAbi<Yasuo_Dash_Stab>();
            CurrentBuilder.CastAbi<Yasuo_Double_Stab>();
        }

        public void PatternNew3()
        {
            using (new ScopeSelector(CurrentBuilder))
            {
                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.ConditionDistanceIn(4.0f, 20.0f);
                    CurrentBuilder.ReturnSuccess();
                    PatternConfrontationOnce();
                    CurrentBuilder.End();
                    PatternNormal3_1();
                }

                using (new ScopeSequence(CurrentBuilder))
                {
                    CurrentBuilder.CastAbi<Yasuo_Blink>((abi) =>
                    {
                        abi.ImmediateCast = true;
                        CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(10.0f);
                    }, type: CastType.CustomLocation);
                    PatternNormal3_1();
                }
            }
        }

        private void FarBattle()
        {
            using (new ScopeRandomSelector(CurrentBuilder, "Far"))
            {
                using (new ScopeSequence(CurrentBuilder, "Far1"))
                {
                    CurrentBuilder.ConditionCooldown(7.0f);
                    PatternConfrontationOnce();
                }

                using (new ScopeSequence(CurrentBuilder, "Far1"))
                {
                    CurrentBuilder.ConditionCooldown(5.0f);
                    PatternConfrontationOnce();
                    PatternNormal3_1();
                }

                using (new ScopeSequence(CurrentBuilder, "Far1"))
                {
                    CurrentBuilder.CastAbi<Yasuo_Dash>(
                        (abi) =>
                        {
                            CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(Random.Range(5, 10));
                        }, type: CastType.CustomLocation);
                }
            }
        }

        public void DefenceWalk(float distanceToTarget = 2.0f)
        {
            Yasuo_DefenceBuff defenceBuff = null;
            CurrentBuilder.WalkToUnit(distanceToTarget, () =>
            {
                defenceBuff =
                    ControlCharacter.GetAbility<Yasuo_Defense>().DefenceBuff.As().CreateBuff() as Yasuo_DefenceBuff;
                defenceBuff.SetLifetime(-1);
                defenceBuff.AddTo(ControlCharacter, ControlCharacter);
            }, () => { ControlCharacter.RemoveBuff(defenceBuff); });
        }

        public override void OnLogic()
        {
            base.OnLogic();
            _result = null;
        }

        public void DashOrWalkTo(float distanceToTarget = 2.0f, float dashRange = 8.0f, bool skipWhenNear = true)
        {
            using (new ScopeSelector(CurrentBuilder, "DashOrWalkTo"))
            {
                if (skipWhenNear)
                {
                    using (new ScopeSequence(CurrentBuilder, "Near"))
                    {
                        CurrentBuilder.ConditionDistanceIn(0, distanceToTarget);
                    }
                }

                using (new ScopeSequence(CurrentBuilder, "Walk"))
                {
                    CurrentBuilder.Condition(() =>
                    {
                        var targetLocation = CurrentBuilder.LocationToTarget(distanceToTarget);
                        if ((targetLocation - ControlCharacter.WorldPosition).magnitude < dashRange)
                        {
                            return true;
                        }

                        return false;
                    });
                    CurrentBuilder.WalkToUnit(distanceToTarget);
                }

                using (new ScopeSequence(CurrentBuilder, "Dash"))
                {
                    CurrentBuilder.CastAbi<Yasuo_Dash>((dash) =>
                    {
                        dash.CustomBackswingTimeOnce = 0.01f;
//                       Debug.LogWarning((CurrentBuilder.CustomCastLocation - ControlCharacter.WorldPosition).magnitude);
                        CurrentBuilder.CustomCastLocation = CurrentBuilder.LocationToTarget(distanceToTarget);
                    }, type: CastType.CustomLocation);
                }

                CurrentBuilder.Do(() => { return TaskStatus.Success; });
            }
        }
    }
}