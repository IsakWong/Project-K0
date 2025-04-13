using EasyCharacterMovement;
using FSM;
using UnityEngine;

namespace K1.Gameplay
{
    public enum CharacterStateID
    {
        None,
        WalkState,
        ActionState,
        IdleState,
        StunState,
        HitState,
        DeflectState,
        DeathState
    }

    public interface ICharacterState
    {
        public CharacterUnit Character { get; set; }
    }

    public class HierarchicalCharacterState : StateMachine<CharacterStateID, ActionSubID, string>, ICharacterState
    {
        public CharacterUnit _character;

        public CharacterFsm characterFsm
        {
            get { return fsm as CharacterFsm; }
        }

        public CharacterUnit Character
        {
            get => _character;
            set => _character = value;
        }
    }

    public class CharacterState : StateBase<CharacterStateID>, ICharacterState
    {
        public CharacterUnit _character;
        public string Name;

        public CharacterFsm characterFSM
        {
            get { return fsm as CharacterFsm; }
        }

        public CharacterUnit Character
        {
            get => _character;
            set => _character = value;
        }

        public CharacterState() : base(false)
        {
        }

        public override void OnLogic()
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Character.gameplaySys.Log($"[CharacterState]{Character.mCharacterConfig.Name} 进入状态: {Name}");
        }

        public override void OnExit()
        {
            base.OnEnter();
            Character.gameplaySys.Log($"[CharacterState]{Character.mCharacterConfig.Name} 离开状态: {Name}");
        }
    }

    public class IdleCharacterState : CharacterState
    {
        public Vector3 TargetDirection = Vector3.zero;
        public string IdleAnimator = "Idle";

        public IdleCharacterState()
        {
            Name = "待机";
        }

        public override void OnExit()
        {
            base.OnExit();
            Character.mAnimator.ResetTrigger(IdleAnimator);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Character.SetAnimatorTrigger(IdleAnimator);
            Character.SetAnimatorFloat(AnimatorParamConfig.kWalkSpeed, 0);
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
            var floor = Character.DetectFloor();
            Character.transform.position = floor;
            if (TargetDirection != Vector3.zero)
            {
                Vector3 direction = TargetDirection;
                direction.y = 0;
                direction.Normalize();
                var deltaDir = Character.transform.forward - direction;
                deltaDir.y = 0;
                bool _facingTarget = deltaDir.magnitude < 0.01f;
                if (_facingTarget)
                {
                    TargetDirection = Vector3.zero;
                }
                else
                {
                    RotateTowards(TargetDirection, 960 * KTime.scaleDeltaTime, true);
                }
            }
            //var delta = mCharacter.transform.forward - mCharacter.mActionDirection;
            //if (delta.magnitude > 0.01f)
            //{
            //    Quaternion targetRotation = Quaternion.LookRotation(mCharacter.mActionDirection, Vector3.up);
            //    var rotation = mCharacter.transform.rotation;
            //    rotation = Quaternion.RotateTowards(rotation, targetRotation, 10);
            //    mCharacter.transform.rotation = rotation;
            //}
            //else
            //{
            //    mCharacter.transform.rotation = Quaternion.LookRotation(mCharacter.mActionDirection);
            //}
        }
    }

    public class DeflectState : CharacterState
    {
        public DeflectState()
        {
            Name = "格挡";
        }

        public CharacterUnit DeflectSource;
        public float DeflectTime;
        private float _timer;

        public float RemainPercent
        {
            get => DeflectTime != 0 ? _timer / DeflectTime : 0;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0.0f;
            Character.SetAnimatorTrigger(AnimatorParamConfig.kDeflectTrigger);
            Character.gameplaySys.AnyEvent.OnDeflect?.Invoke(Character, DeflectSource, DeflectTime);
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public override void OnExit()
        {
            base.OnExit();
            Character.mAnimator.ResetTrigger(AnimatorParamConfig.kDeflectTrigger);
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public override void OnLogic()
        {
            base.OnLogic();
            if (_timer > DeflectTime)
            {
                Character.Idle();
            }
            else
            {
                _timer += KTime.scaleDeltaTime;
                Character.CharEvent.OnStatusChange?.Invoke(Character);
            }
        }
    }

    public class StunCharacterState : CharacterState
    {
        public StunCharacterState()
        {
            Name = "眩晕";
        }

        public float StunTime;
        private float PassedTime;

        public float RemainPercent
        {
            get => 1 - (StunTime != 0 ? PassedTime / StunTime : 0);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            PassedTime = 0.0f;
            Character.SetAnimatorTrigger(AnimatorParamConfig.kStunTrigger);
            Character.PlayAudioByChannel(Character.mCharacterConfig.StunAudio.RandomAccess(),
                CharacterAudioChannel.VO_Channel0, true);
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public override void OnExit()
        {
            base.OnExit();
            Character.SetAnimatorBool("Stunned", false);
            Character.SetAnimatorTrigger(AnimatorParamConfig.kIdleTrigger);
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }


        public override void OnLogic()
        {
            base.OnLogic();
            if (PassedTime >= StunTime)
            {
                Character.SetAnimatorBool("Stunned", true);
                Character.Idle();
            }
            else
            {
                PassedTime += KTime.scaleDeltaTime;
                Character.CharEvent.OnStatusChange?.Invoke(Character);
            }
        }
    }

    public class HitCharacterState : CharacterState
    {
        public bool PlayHitAnim = false;
        public float HitRecoverTime = 0.0f;
        public string HitAnimation = "Hitted";

        public float RemainPercent
        {
            get => 1 - (HitRecoverTime != 0 ? _timer / HitRecoverTime : 0);
        }

        public HitCharacterState()
        {
            Name = "受击";
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0.0f;
            Character.SetAnimatorTrigger(HitAnimation);
            Character.PlayAudioByChannel(Character.mCharacterConfig.TakeDamageAudio.RandomAccess(),
                CharacterAudioChannel.VO_Channel0, true);
            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        private float _timer = 0.0f;

        public override void OnLogic()
        {
            base.OnLogic();
            if (_timer >= HitRecoverTime)
            {
                Character.Idle();
            }
            else
            {
                _timer += KTime.scaleDeltaTime;
            }

            Character.CharEvent.OnStatusChange?.Invoke(Character);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class DeathCharacterState : CharacterState
    {
        public DeathCharacterState()
        {
            Name = "死亡";
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Character.SetAnimatorTrigger(AnimatorParamConfig.kDieTrigger);
        }

        public override void OnLogic()
        {
            base.OnLogic();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

    public class CharacterFsm : StateMachine<CharacterStateID, CharacterStateID, string>
    {
        // Start is called before the first frame update
        private CharacterUnit mCharacter;

        void AddState(CharacterStateID name, ICharacterState state)
        {
            state.Character = mCharacter;
            if (state is CharacterState)
                base.AddState(name, state as CharacterState);
            if (state is HierarchicalCharacterState)
                base.AddState(name, state as HierarchicalCharacterState);
        }

        public void ToIdle()
        {
            Trigger("Idle");
        }

        public CharacterFsm(CharacterUnit controller)
        {
            mCharacter = controller;

            AddState(CharacterStateID.IdleState, new IdleCharacterState());
            AddState(CharacterStateID.StunState, new StunCharacterState());
            AddState(CharacterStateID.DeflectState, new DeflectState());
            AddState(CharacterStateID.WalkState, new WalkCharacterState());
            AddState(CharacterStateID.HitState, new HitCharacterState());
            var soloAction = new SoloActionCharacterState();
            AddState(CharacterStateID.ActionState, soloAction);
            AddState(CharacterStateID.DeathState, new DeathCharacterState());
            SetStartState(CharacterStateID.IdleState);
        }
    }
}