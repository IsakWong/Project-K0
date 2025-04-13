using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using EasyCharacterMovement;
using Framework.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace K1.Gameplay
{
    public enum BuiltinCharacterSocket
    {
        Origin,
        Head,
        Overhead,
        Body,
        FollowTarget,
        Weapon = 32,
        WeaponDeflect,
        RightHand = 64,
        LeftHand,
        RightFoot = 96,
        LeftFoot,
        None = 1024,
    }

    public enum DamageType
    {
        None,
        MagicDamage,
        PhysicalDamage,
        TrueDamage
    }

    public enum ValueLevel
    {
        LevelNone = 0, // 0级  默认时候处于0级
        Level0, // 0级  默认时候处于0级
        Level1, // 1级  可以防御0/1级
        Level2, // 2级  可以防御0/1/2级
        LevelMax, // Max
    }
    // 当 Damage/Hit Level 大于 Defence/Endure Level时才会生效
    // LevelMax 的防御可以 无效所有攻击

    public class HitParam
    {
        public HitParam(CharacterUnit source)
        {
            Source = source;
        }

        public ValueLevel ValueLevel = ValueLevel.Level0;
        public float HitTime = 0.2f;
        public string HitAnimTrigger = "Hit";
        public bool PlayAnimation = true;
        public CharacterUnit Source = null;
    }

    public class DamageParam
    {
        public float DamageValue = 0.0f;
        public CharacterUnit Source;

        public DamageType DamageType = DamageType.MagicDamage;
        public ValueLevel ValueLevel = 0;

        public float HitValue = 20.0f;
        public Vfx DamagePrefab;
        public bool PlayHurtAudio = true;

        public DamageParam Clone()
        {
            var ret = new DamageParam();
            ret.DamageValue = DamageValue;
            ret.Source = Source;
            ret.DamageType = DamageType;
            ret.ValueLevel = ValueLevel;
            ret.HitValue = HitValue;
            ret.DamagePrefab = DamagePrefab;
            ret.PlayHurtAudio = PlayHurtAudio;
            return ret;
        }
    }

    public class ParryParam
    {
        public CharacterUnit PerriedUnit;
    }

    public enum CharacterAudioChannel
    {
        VO_Channel0,
        VO_Channel1,
        VO_Channel2,
        Weapon_Channel0,
        Weapon_Channel1,
        Weapon_Channel2,
    }

    public partial class CharacterUnit : GameUnit
    {
        public GameObject Root;
        public OverlapDetecter overlapDetecter;

        public struct BuiltinCharacterEvent
        {
            public Action<CharacterUnit> OnHealthChange;
            public Action<CharacterUnit> OnManaChange;
            public Action<CharacterUnit> OnStatusChange;
            public Action<CharacterUnit, CharacterUnit> OnEndure;
            public Action<CharacterUnit> OnDie;
            public Action<CharacterUnit, CharacterUnit> OnBreakAction;
            public Action<CharacterUnit, ParryParam> OnParry;
            public Action<CharacterUnit, CharacterUnit, DamageParam> OnHitOthers;
            public Action<CharacterUnit, CharacterUnit, DamageParam> OnTakeDamage;
            public Action<CharacterUnit, CharacterUnit, float> OnGetStuned;
            public Action<CharacterUnit, CharacterUnit, float> OnDeflect;
            public Action<CharacterUnit, CharacterUnit, float> OnStunOthers;
            public Action<CharacterUnit, HitParam> OnHit;
            public Action<CharacterUnit, ActionAbility> OnAbilityCasting;
        }

        public Transform MeshTransform;
        public PositionConstraint PositionConstraint;

        public BuiltinCharacterEvent CharEvent = new();
        public CharacterConfig mCharacterConfig;
        public float mAttackRange = 1;
        public int mPlayerID = 0;
        public NavMeshAgent mAgent;
        public Animator mAnimator;
        public Rigidbody mRigidbody;
        public Vector3 mMoveVelocity = Vector3.zero;

        [FormerlySerializedAs("mForceVelocity")]
        public Vector3 ForceVelocity = Vector3.zero;

        public Vector3 mGravityVelocity = Vector3.zero;
        public Vector3 mFootOffset;
        public bool mIsHero = false;

        public int PlayerID
        {
            get => mPlayerID;
        }

        public SoloActionCharacterState ActionState
        {
            get => FSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
        }

        public void EnableCollision(bool val)
        {
            mRigidbody.detectCollisions = val;
        }

        public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
        {
            Vector3 characterUp = transform.up;

            if (updateYawOnly)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
        }

        public void Log(string str)
        {
            gameplaySys.Log($"<{mCharacterConfig.Name}> {str}");
        }

        public SkinnedMeshRenderer mRenderer;
        public int MaterialIdx = 0;

        private Vector3 _walkTarget;
        private Vector3 _actionDirection = Vector3.forward;

        private Dictionary<CharacterAudioChannel, List<AudioSource>> channelAudios = new();

        public ActionAbility CurrentActionAbi = null;

        public T GetCurrentAbi<T>() where T : ActionAbility
        {
            return CurrentActionAbi as T;
        }

        public void StopChannelAudio(CharacterAudioChannel channel = CharacterAudioChannel.VO_Channel0)
        {
            List<AudioSource> audios;
            if (!channelAudios.ContainsKey(channel))
            {
                channelAudios[channel] = new List<AudioSource>();
            }

            audios = channelAudios[channel];
            foreach (var audioSource in audios)
            {
                Destroy(audioSource.gameObject);
            }

            audios.Clear();
        }

        public void PlayAudioByChannel(AudioClip clip,
            CharacterAudioChannel channel = CharacterAudioChannel.VO_Channel0, bool stopOthers = true,
            bool canStop = true)
        {
            List<AudioSource> audios;
            if (!channelAudios.ContainsKey(channel))
            {
                channelAudios[channel] = new List<AudioSource>();
            }

            audios = channelAudios[channel];
            if (stopOthers)
            {
                foreach (var audioSource in audios)
                {
                    Destroy(audioSource.gameObject);
                }

                audios.Clear();
            }

            var source = KGameCore.SystemAt<AudioModule>().PlayAudioAtPosition(clip, WorldPosition, false);
            audios.Add(source);
            TimerManager.AddTimer(clip.length + 0.1f, () =>
            {
                if (audios.Contains(source))
                {
                    audios.Remove(source);
                    Destroy(source.gameObject);
                }
            }, 1);
        }

        public Vector3 mActionDirection
        {
            get => _actionDirection;
            set
            {
                _actionDirection = value;
                if (_actionDirection == Vector3.zero)
                {
                    _actionDirection = transform.forward;
                }

                _actionDirection.Normalize();
            }
        }

        public Vector3 WalkTargetPosition
        {
            get => _walkTarget;
        }

        [ShowNonSerializedProperty("状态")] protected CharacterFsm mFSM;

        public CharacterFsm FSM
        {
            get { return mFSM; }
        }

        public Dictionary<CharacterStateID, Action<bool>> pendingStateQueue = new();

        private void ProcessPendingStates()
        {
            CharacterStateID newStateID = CharacterStateID.None;
            if (pendingStateQueue.ContainsKey(CharacterStateID.DeathState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.DeathState);
                }

                _pendingState = CharacterStateID.DeathState;
            }
            else if (pendingStateQueue.ContainsKey(CharacterStateID.StunState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.StunState);
                }

                _pendingState = CharacterStateID.StunState;
            }
            else if (pendingStateQueue.ContainsKey(CharacterStateID.HitState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.HitState);
                }

                _pendingState = CharacterStateID.HitState;
            }
            else if (pendingStateQueue.ContainsKey(CharacterStateID.ActionState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.ActionState);
                }

                _pendingState = CharacterStateID.ActionState;
            }
            else if (pendingStateQueue.ContainsKey(CharacterStateID.WalkState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.WalkState);
                }

                _pendingState = CharacterStateID.WalkState;
            }
            else if (pendingStateQueue.ContainsKey(CharacterStateID.IdleState))
            {
                foreach (var it2 in pendingStateQueue)
                {
                    it2.Value?.Invoke(it2.Key == CharacterStateID.IdleState);
                }

                _pendingState = CharacterStateID.IdleState;
            }

            pendingStateQueue.Clear();
        }

        public CharacterStateID pendingState;

        public void PendingState(CharacterStateID stateID, Action<bool> callback = null)
        {
            if (pendingStateQueue.ContainsKey(stateID))
            {
                return;
            }

            if (stateID == _pendingState)
                return;
            pendingStateQueue[stateID] = callback;
        }

        protected void _DoSwitchState()
        {
            ProcessPendingStates();
            if (_pendingState == CharacterStateID.None)
                return;
            var newState = _pendingState;
            _pendingState = CharacterStateID.None;
            if (CurrentState == CharacterStateID.ActionState)
            {
                if (newState == CharacterStateID.DeathState || newState == CharacterStateID.StunState ||
                    newState == CharacterStateID.HitState)
                {
                    var solo = mFSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
                    solo.BreakAction(null);
                }
            }

            if (newState != CurrentState)
            {
                mFSM.RequestStateChange(newState, true);
            }

            _pendingState = CharacterStateID.None;
        }

        private float _hitTimer = 0.0f;

        // Start is called before the first frame update
        public override void OnLogic()
        {
            base.OnLogic();
            Debug.Assert(transform.eulerAngles.x < 1 && transform.eulerAngles.z < 1);
            ForceVelocity = Vector3.zero;
            lastManaTimer += KTime.scaleDeltaTime;
            _hitTimer += KTime.scaleDeltaTime;
            if (_hitTimer > 3.0f)
            {
            }

            if (lastManaTimer > 1.5f)
            {
                float recoverSpeed = lastManaTimer < 5.0f ? lastManaTimer : 5.0f;
                float mana = mRealProperty[CharacterProperty.Mana];
                mana += 20 * recoverSpeed * KTime.scaleDeltaTime;
                if (mana > mOriginProperty[CharacterProperty.Mana])
                    mana = mOriginProperty[CharacterProperty.Mana];
                mRealProperty[CharacterProperty.Mana] = mana;
                CharEvent.OnManaChange?.Invoke(this);
            }

            var unit = this;
            foreach (var buff in _toAddBuff)
            {
                buff.BuffAdd();
                BuffList.Add(buff);
                OnBuffAdd?.Invoke(buff);
            }

            _toAddBuff.Clear();
            List<Buffbase> toRemoveBuff = new List<Buffbase>();
            foreach (var buf in unit.Buffs)
            {
                buf.Logic(KTime.scaleDeltaTime);
                if (buf._isFinish || buf.ForceRemove)
                {
                    if (buf.ForceRemove)
                        buf.BuffBreak();
                    toRemoveBuff.Add(buf);
                }
            }

            foreach (var buf in toRemoveBuff)
            {
                buf.BuffEnd();
                OnBuffRemove?.Invoke(buf);
                buf.BuffOwner.BuffList.Remove(buf);
            }

            foreach (var abi in abiList)
            {
                abi.OnLogic(KTime.scaleDeltaTime);
            }

            if (IsAlive)
            {
                switch (_pendingState)
                {
                    case CharacterStateID.WalkState:
                        NavMesh.CalculatePath(WorldPosition, WalkTargetPosition, NavMesh.AllAreas, path);
                        break;
                    case CharacterStateID.ActionState:
                        break;
                }
            }

            _DoSwitchState();
            FSM.OnLogic();

            if (EnableGravity)
            {
                if (characterMovement.isGrounded)
                {
                    mGravityVelocity = Vector3.zero;
                }
                else
                {
                    mGravityVelocity += Physics.gravity * KTime.scaleDeltaTime * 1.5f;
                }
            }
            else
            {
                mGravityVelocity = Vector3.zero;
            }

            characterMovement.velocity = mMoveVelocity + mGravityVelocity + ForceVelocity;
            var old = transform.eulerAngles.x;

            characterMovement.Move(KTime.scaleDeltaTime);

            mMoveVelocity = Vector3.zero;
            _pendingActionParam = null;
            _pendingHitted = false;
            _pendingSoloAction = false;
            if (IsInState(CharacterStateID.HitState) || IsInState(CharacterStateID.DeathState) ||
                IsInState(CharacterStateID.StunState) || IsInState(CharacterStateID.ActionState))
            {
            }
            else
            {
                if (isHitted)
                {
                    SetAnimatorTrigger("Hit");
                    PlayAudioByChannel(mCharacterConfig.TakeDamageAudio.RandomAccess(),
                        CharacterAudioChannel.VO_Channel0);
                }
            }

            isHitted = false;
        }

        public bool EnableGravity = true;
        private float lastManaTimer;

        public bool RecoverMana(float val)
        {
            float mana = RealPropertyAt(CharacterProperty.Mana);
            mana += val;
            mRealProperty[CharacterProperty.Mana] = mana;
            CharEvent.OnManaChange?.Invoke(this);
            lastManaTimer = 0.0f;
            return true;
        }

        public void CostMana(float val)
        {
            float mana = RealPropertyAt(CharacterProperty.Mana);
            mana -= val;
            mRealProperty[CharacterProperty.Mana] = mana;
            CharEvent.OnManaChange?.Invoke(this);
            lastManaTimer = 0.0f;
        }

        public void LookAt(Vector3 position)
        {
            Vector3 direct = position - WorldPosition;
            direct.y = 0;
            direct.Normalize();
            transform.rotation = Quaternion.LookRotation(direct);
        }

        public ActionAbility ActingAbi
        {
            get
            {
                if (mFSM.ActiveStateName != CharacterStateID.ActionState)
                    return null;
                var soloAction = mFSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
                return soloAction.CastParam.SourceAbi;
            }
        }

        public bool IsSoloActing
        {
            get { return mFSM.ActiveStateName == CharacterStateID.ActionState; }
        }

        public bool IsHit
        {
            get { return mFSM.ActiveStateName == CharacterStateID.HitState; }
        }

        public bool IsStun
        {
            get { return mFSM.ActiveStateName == CharacterStateID.StunState; }
        }

        public bool IsDead
        {
            get { return mFSM.ActiveStateName == CharacterStateID.DeathState; }
        }

        public bool IsAlive
        {
            get { return mFSM.ActiveStateName != CharacterStateID.DeathState; }
        }

        public bool IsPendingWalking
        {
            get { return _pendingState == CharacterStateID.WalkState; }
        }

        public bool IsWalking
        {
            get { return CurrentState == CharacterStateID.WalkState; }
        }

        public bool IsIdle
        {
            get { return CurrentState == CharacterStateID.IdleState; }
        }

        public bool IsEnemy(CharacterUnit target)
        {
            return CharacterUnitAPI.IsEnemy(this, target);
        }

        #region Movement

        private NavMeshPath path;


        private CharacterMovement _characterMovement;

        public CharacterMovement characterMovement
        {
            get
            {
                if (_characterMovement == null)
                    _characterMovement = GetComponent<CharacterMovement>();
                return _characterMovement;
            }
        }

        #endregion

        public override void OnSpawn()
        {
            gameObject.layer = LayerMask.NameToLayer("CharacterUnit");
            gameplaySys.Characters.Add(this);
            UnityDestroyDelay = -1;
            EnableOnLogic = true;
            foreach (var abiConfig in mCharacterConfig.OriginAbilityList)
            {
                if (abiConfig && abiConfig.Prototype.As() != null)
                {
                    var abi = abiConfig.CreateAbi();
                    AddAbility(abi);
                }
            }

            mAttackRange = mCharacterConfig.AttackRange;
            InitProperty();
            foreach (var config in mCharacterConfig.TalentConfigs)
            {
                AddTalent(config.CreateTalent());
            }

            _currentHealth = mOriginMaxHealth;
            gameObject.layer = GameUnitAPI.GetCharacterLayer();
            mRigidbody = GetComponent<Rigidbody>();
            if (!mAnimator)
                mAnimator = GetComponent<Animator>();
            mFSM = new CharacterFsm(this);
            mFSM.Init();
            path = new NavMeshPath();
        }

        public override void OnUnitInactive()
        {
            base.OnUnitInactive();
            KGameCore.Instance.RequireModule<HUDModule>().UnRegisterHelathBarHUD(this);
            
        }

        public override void OnUnitActive()
        {
            base.OnUnitActive();
            KGameCore.Instance.RequireModule<HUDModule>().RegisterCharacterHUD(this, Vector3.up * 2f);
        }

        #region Acting动作相关

        private Action<bool> _pendingCallback;
        private CharacterStateID _pendingState = CharacterStateID.None;

        public bool CanStun
        {
            get { return true; }
        }

        public bool Defelct(CharacterUnit deflectSource, float time = 0.5f)
        {
            if (!IsAlive)
                return false;
            if (IsInState(CharacterStateID.StunState))
                return false;
            if (IsInState(CharacterStateID.HitState))
                return false;
            PendingState(CharacterStateID.DeflectState, (bool val) =>
            {
                if (val)
                {
                    var state = mFSM.GetState(CharacterStateID.DeflectState) as DeflectState;
                    state.DeflectTime = time;
                }
                else
                {
                }
            });
            return true;
        }

        public bool Stun(CharacterUnit stunSource = null, float time = -1)
        {
            if (IsInState(CharacterStateID.DeathState))
                return false;
            if (!IsAlive)
                return false;
            if (!CanStun)
            {
                CharEvent.OnEndure?.Invoke(this, stunSource);
                gameplaySys.AnyEvent.OnEndure?.Invoke(this, stunSource);
                return false;
            }

            if (IsInState(CharacterStateID.DeathState))
                return false;
            if (IsInState(CharacterStateID.StunState))
            {
                var stun = FSM.GetState(CharacterStateID.StunState) as StunCharacterState;
                if (time > stun.StunTime)
                    stun.StunTime = time;
                return true;
            }

            PendingState(CharacterStateID.StunState, (bool val) =>
            {
                if (val)
                {
                    gameplaySys.AnyEvent.OnGetStuned?.Invoke(this, stunSource, time);
                    var state = mFSM.GetState(CharacterStateID.StunState) as StunCharacterState;
                    state.StunTime = time;
                }
            });
            return true;
        }

        public bool IsInState(CharacterStateID state)
        {
            return CurrentState == state || pendingStateQueue.ContainsKey(state);
        }

        CharacterStateID CurrentState
        {
            get { return mFSM.ActiveStateName; }
        }

        public void Idle(string idleAnimator = "Idle")
        {
            if (IsInState(CharacterStateID.DeathState))
                return;
            PendingState(CharacterStateID.IdleState, (val) =>
            {
                if (val)
                {
                    var state = FSM.GetState(CharacterStateID.IdleState) as IdleCharacterState;
                    state.IdleAnimator = idleAnimator;
                }
            });
        }

        public void ExitState()
        {
            PendingState(CharacterStateID.IdleState, (val) =>
            {
                if (val)
                {
                    var state = FSM.GetState(CharacterStateID.IdleState) as IdleCharacterState;
                    state.IdleAnimator = "Idle";
                }
            });
        }

        private bool _pendingHitted = false;
        private float _pendingHitTime = 0.0f;

        private float _hittedValue = 0.0f;
        public float HittedValueMax = 100.0f;

        public void GetHitted(HitParam param)
        {
            if (IsInState(CharacterStateID.DeathState) || IsInState(CharacterStateID.StunState))
                return;
            if (IsInState(CharacterStateID.HitState))
                return;

            PendingState(CharacterStateID.HitState, (val) =>
            {
                if (val)
                {
                    var state = mFSM.GetState(CharacterStateID.HitState) as HitCharacterState;
                    CharEvent.OnHit?.Invoke(param.Source, param);
                    state.HitRecoverTime = param.HitTime;
                    state.PlayHitAnim = param.PlayAnimation;
                    state.HitAnimation = "Hitted";
                }
            });
        }

        public void RemoveBuffOfType<T>(bool removeDebuff, bool removeBuff) where T : Buffbase
        {
            foreach (var buff in BuffList)
            {
                T movementBuff = buff as T;
                if (movementBuff != null)
                {
                    if (removeDebuff && movementBuff.IsDebuff)
                    {
                        RemoveBuff(buff);
                    }

                    if (removeBuff && !movementBuff.IsDebuff)
                    {
                        RemoveBuff(buff);
                    }
                }
            }
        }

        private CharacterUnit _breakActionSource = null;

        public bool BreakAction(CharacterUnit source = null)
        {
            if (source == null)
                source = this;
            var state = FSM.GetState(CharacterStateID.IdleState) as IdleCharacterState;
            state.IdleAnimator = "Idle";
            _pendingState = CharacterStateID.IdleState;
            _breakActionSource = source;
            return false;
        }

        private bool _pendingSoloAction = false;
        private SoloActionParam _pendingActionParam;

        public bool SoloAction(SoloActionParam param)
        {
            if (IsInState(CharacterStateID.DeathState))
                return false;
            if (IsInState(CharacterStateID.ActionState))
                return false;
            if (IsInState(CharacterStateID.StunState))
                return false;
            if (IsInState(CharacterStateID.HitState))
                return false;
            if (IsInState(CharacterStateID.ActionState))
                return false;
            PendingState(CharacterStateID.ActionState, (bool val) =>
            {
                if (val)
                {
                    var state = mFSM.GetState(CharacterStateID.ActionState) as SoloActionCharacterState;
                    state.CastParam = param;
                    state.CastResult = new SoloActionResult();
                    param.OnActionBegin.Invoke(state.CastResult);
                }
                else
                {
                    param.OnActionBeginFailed.Invoke();
                }
            });
            return true;
        }

        public void DirectWalk(Vector3 direction, Vector3 faceDirection)
        {
            if (IsInState(CharacterStateID.DeathState) || IsInState(CharacterStateID.ActionState) ||
                IsInState(CharacterStateID.HitState) || IsInState(CharacterStateID.StunState) ||
                IsInState(CharacterStateID.DeflectState))
                return;

            var walkState = FSM.GetState(CharacterStateID.WalkState) as WalkCharacterState;
            if (mFSM.ActiveStateName == CharacterStateID.WalkState)
            {
                walkState.DirectMove = true;
                walkState.MoveTargetDirection = direction;
                walkState.FaceTargetDirection = faceDirection;
                return;
            }

            if (!IsInState(CharacterStateID.IdleState))
                return;
            if (!CanWalk())
                return;
            PendingState(CharacterStateID.WalkState, (bool val) =>
            {
                if (val)
                {
                    var state = mFSM.GetState(CharacterStateID.WalkState) as WalkCharacterState;
                    state.DirectMove = true;
                    state.MoveTargetDirection = direction;
                    state.FaceTargetDirection = faceDirection;
                }
            });
        }

        public void WalkTowards(Vector3 target)
        {
            if (IsInState(CharacterStateID.DeathState))
                return;
            if (mFSM.ActiveStateName != CharacterStateID.WalkState &&
                mFSM.ActiveStateName != CharacterStateID.IdleState)
                return;
            if (IsInState(CharacterStateID.ActionState))
                return;
            if (IsInState(CharacterStateID.HitState))
                return;
            if (GameUnitAPI.DistanceBetweenPosition(WorldPosition, target) < 0.1f)
                return;
            if (!CanWalk())
                return;
            var walkState = FSM.GetState(CharacterStateID.WalkState) as WalkCharacterState;
            PendingState(CharacterStateID.WalkState, (bool val) =>
            {
                if (val)
                {
                    var newLocation = DetectFloor(target + Vector3.up * 0.5f);
                    var state = mFSM.GetState(CharacterStateID.WalkState) as WalkCharacterState;
                    state.DirectMove = false;
                    _walkTarget = newLocation;
                }
            });
            // mFireballAbility.Run();
        }

        public bool CanWalk()
        {
            foreach (var buf in BuffList)
            {
                StuckBuff stuck = buf as StuckBuff;
                if (stuck != null)
                    return false;
                StunBuff stun = buf as StunBuff;
                if (stun != null)
                    return false;
            }

            return true;
        }

        public EndureBuff GetEndureBuff()
        {
            EndureBuff ret = null;
            foreach (var buff in BuffList)
            {
                if (buff is EndureBuff)
                {
                    var it = buff as EndureBuff;
                    if (ret == null)
                        ret = it;
                    else
                    {
                        if (it.EndureLevel > ret.EndureLevel)
                            ret = it;
                    }
                }
            }

            return ret;
        }

        public DefensiveBuff GetDefenceBuff()
        {
            DefensiveBuff ret = null;
            foreach (var buff in BuffList)
            {
                if (buff is DefensiveBuff)
                {
                    var it = buff as DefensiveBuff;
                    if (ret == null)
                        ret = it;
                    else
                    {
                        if (it.DefenceLevel > ret.DefenceLevel)
                            ret = it;
                    }
                }
            }

            return ret;
        }

        ValueLevel GetEndureLevel()
        {
            var buff = GetEndureBuff();
            if (buff != null)
                return buff.EndureLevel;
            return ValueLevel.Level0;
        }

        ValueLevel GetDefenceLevel()
        {
            var buff = GetDefenceBuff();
            if (buff != null)
                return buff.DefenceLevel;
            return ValueLevel.Level0;
        }

        #endregion

        private Sequence hitSeq = null;

        public bool TryGetHitted(HitParam param)
        {
            Assert.IsTrue(param.Source);
            if (GetBuff<InvincibleBuff>() != null)
                return false;

            bool endured = false;
            var endureBuff = GetEndureBuff();
            if (endureBuff != null && param.ValueLevel <= endureBuff.EndureLevel)
            {
                endured = true;
                endureBuff.OnEndure();
            }

            if (!endured)
            {
                GetHitted(param);
            }

            return endured;
        }

        public bool TryTakeDamage(DamageParam param)
        {
            Assert.IsTrue(param.Source);
            if (GetBuff<InvincibleBuff>() != null)
            {
                GetBuff<InvincibleBuff>().OnInvicibleEffect(param.Source);
                GetBuff<InvincibleBuff>().OnGetHitted?.Invoke();
                return false;
            }

            bool defenced = false;
            var defenceBuff = GetDefenceBuff();
            if (defenceBuff != null && param.ValueLevel <= defenceBuff.DefenceLevel)
            {
                defenceBuff.OnDefence(param);
                param.DamageValue = param.DamageValue * 0.5f;
                defenced = true;
            }

            return TakeDamage(param) && !defenced;
        }

        public bool TakeHealing(float value)
        {
            if (_currentHealth <= 0)
                return false;
            if (_currentHealth + value > mOriginMaxHealth)
            {
                _currentHealth = mOriginMaxHealth;
            }
            else
            {
                _currentHealth = _currentHealth + value;
            }

            CharEvent.OnHealthChange?.Invoke(this);
            gameplaySys.AnyEvent.OnHealthChange?.Invoke(this);
            return true;
        }

        public bool TakeDamage(DamageParam param)
        {
            if (param.DamageValue <= 0)
                return false;
            if (_currentHealth <= 0)
                return false;
            float damage = param.DamageValue;
            damage = TakeDamageMultiple * damage;

            if (damage > FixedDefenceValue)
                damage = damage - FixedDefenceValue;

            if (_currentHealth <= damage)
            {
                _currentHealth = 0;
            }
            else
            {
                _currentHealth -= damage;
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // if (param.PlayHurtAudio)
                //     KGameCore.SystemAt<AudioModule>().PlayAudioAtPosition(mCharacterConfig.TakeDamageAudio.RandomAccess(), WorldPosition);
                _hittedValue += param.HitValue;
                if (_hittedValue >= HittedValueMax)
                {
                    _hittedValue = 0;
                    if (param.ValueLevel >= ValueLevel.Level2)
                    {
                        HitParam hitParam = new HitParam(param.Source)
                        {
                            ValueLevel = param.ValueLevel,
                            PlayAnimation = true
                        };
                        hitParam.HitTime = 1.0f;
                        TryGetHitted(hitParam);
                    }
                }

                PlayHitAnim("Hit");
                PlayHitColorAnim(0.2f);
            }

            CharEvent.OnTakeDamage?.Invoke(this, param.Source, param);
            gameplaySys.AnyEvent.OnTakeDamage?.Invoke(this, param.Source, param);

            CharEvent.OnHealthChange?.Invoke(this);
            gameplaySys.AnyEvent.OnHealthChange?.Invoke(this);

            VfxAPI.CreateVisualEffect(GameplayConfig.Instance().BloodEffect,
                GetSocketWorldPosition(BuiltinCharacterSocket.Body), Vector3.forward);
            return true;
        }

        public void SetAnimatorTrigger(string key)
        {
            mAnimator.SetTrigger(key);
            gameplay.Log($"{mCharacterConfig.Name} Animator Trigger: {key}");
        }

        public void SetAnimatorBool(string key, bool val)
        {
            mAnimator.SetBool(key, val);
            //gameplay.Log($"Animator {key}");
        }

        public void SetAnimatorFloat(string key, float val)
        {
            mAnimator.SetFloat(key, val);
            //gameplay.Log($"Animator {key}");
        }

        private bool isHitted;

        public void PlayHitAnim(string HitTriiger)
        {
            if (IsDead)
                return;
            isHitted = true;
            //StopChannelAudio(CharacterAudioChannel.VO_Channel0);
            //StopChannelAudio(CharacterAudioChannel.Weapon_Channel0);
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (mFSM != null && mFSM.ActiveStateName == CharacterStateID.WalkState)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(this.WalkTargetPosition, 0.2f);
            }

            var foot = DetectFloor();
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(foot, 0.1f);
            var endurelevel = GetEndureLevel();
            var defenceLevel = GetDefenceLevel();
            var pos = WorldPosition;
            pos.y += 2.5f;
            Handles.Label(pos, $"Defence Level: {defenceLevel}, Endure Level: {endurelevel}");
        }
#endif
        public void PlayHitColorAnim(float deltaTime = 0.2f)
        {
            if (hitSeq != null && hitSeq.IsPlaying())
            {
                hitSeq.Kill();
                hitSeq.Kill();
            }

            hitSeq = DOTween.Sequence();
            mRenderer.materials[MaterialIdx].SetColor("_Color", Color.white);
            hitSeq.Append(mRenderer.materials[MaterialIdx]
                .DOColor(new Color(1.0f, 0.5f, 0.5f, 1), "_Color", deltaTime));
            hitSeq.Append(mRenderer.materials[MaterialIdx].DOColor(Color.white, "_Color", 0.1f));
            hitSeq.Play();
        }

        public Vector3 DetectFloor(Vector3 position)
        {
            var hitinfos = Physics.RaycastAll(position, Vector3.down, 10, GameUnitAPI.GetGroundMask());

            if (hitinfos == null)
                return transform.position;
            if (hitinfos.Length > 0)
            {
                Vector3 foot = hitinfos[0].point;

                foreach (var it in hitinfos)
                {
                    if (it.point.y > foot.y)
                    {
                        foot.y = it.point.y;
                    }
                }

                return foot;
            }

            return transform.position;
        }

        public Vector3 DetectFloor()
        {
            return DetectFloor(WorldPosition + Vector3.up * 0.2f);
        }

        public Vector3 GetFloorPosition(Vector3 position)
        {
            return DetectFloor(position + Vector3.up * 0.4f);
        }

        void Update()
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
        }


        #region Socket

        [SerializedDictionary("Socket Name", "Socket Transform")]
        public SerializedDictionary<BuiltinCharacterSocket, Transform> mBuiltinSocket;

        [SerializedDictionary("Socket Name", "Socket Transform")]
        public SerializedDictionary<string, Transform> mCustomSocket;


        public Transform GetSocketTransform(BuiltinCharacterSocket name)
        {
            if (mBuiltinSocket.ContainsKey(name))
                return mBuiltinSocket[name];
            return transform;
        }

        public void SetAnimatorSpeed(float val, float time)
        {
            mAnimator.speed = val;
            KGameCore.Instance.Timers.AddTimer(time, () => { mAnimator.speed = 1.0f; }).Start();
        }

        public Vector3 GetSocketWorldPosition(BuiltinCharacterSocket name)
        {
            if (mBuiltinSocket.ContainsKey(name))
                return mBuiltinSocket[name].position;
            return transform.position;
        }

        public Quaternion GetSocketWorldRotation(BuiltinCharacterSocket name)
        {
            if (mBuiltinSocket.ContainsKey(name))
                return mBuiltinSocket[name].rotation;
            return transform.rotation;
        }

        public Quaternion GetSocketWorldRotation(string name)
        {
            BuiltinCharacterSocket socket = BuiltinCharacterSocket.None;
            if (BuiltinCharacterSocket.TryParse(name, out socket) && mBuiltinSocket.ContainsKey(socket))
                return mBuiltinSocket[socket].rotation;
            if (mCustomSocket.ContainsKey(name))
                return mCustomSocket[name].rotation;
            return transform.rotation;
        }

        public override Vector3 GetSocketWorldPosition(string name)
        {
            BuiltinCharacterSocket socket = BuiltinCharacterSocket.None;
            if (BuiltinCharacterSocket.TryParse(name, out socket) && mBuiltinSocket.ContainsKey(socket))
                return mBuiltinSocket[socket].position;
            if (mCustomSocket.ContainsKey(name))
                return mCustomSocket[name].position;
            return transform.position;
        }

        public override Transform GetSocketTransform(string name)
        {
            BuiltinCharacterSocket socket;
            if (BuiltinCharacterSocket.TryParse(name, out socket))
            {
                if (mBuiltinSocket.ContainsKey(socket))
                    return mBuiltinSocket[socket];
            }
            else
            {
                if (mCustomSocket.ContainsKey(name))
                    return mCustomSocket[name];
            }

            return transform;
        }

        #endregion
    }
}