using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace K1.Gameplay
{
    public class K1PlayerController : ControllerBase
    {
        public Action<K1PlayerController> OnAbiCooldownWarning;
        public PlayerInput mInput;
        public CharacterUnit mControlCharacter;

        // Start is called before the first frame update
        protected void Start()
        {
            mInput = GetComponent<PlayerInput>();
            InitInput();
            InitAbi();
        }

        protected void OnEnable()
        {
            base.OnEnable();
            KGameCore.RequireSystem<GameplayModule>().RegisterController(this, true);
        }

        protected void OnDisable()
        {
            base.OnDisable();
            KGameCore.RequireSystem<GameplayModule>().RegisterController(this, false);
        }
        
        protected virtual void InitAbi()
        {
            foreach (var action in mInput.actions.FindActionMap("Battle").actions)
            {
                AbilityKey key;
                if (AbilityKey.TryParse(action.name, out key))
                {
                    action.canceled += (delegate(InputAction.CallbackContext context) { OnAbiRelease(context, key); });
                    action.started += (delegate(InputAction.CallbackContext context) { OnAbiPress(context, key); });
                }
            }
        }
        
        protected virtual void InitInput()
        {
        }
        
        public virtual void OnAbiRelease(InputAction.CallbackContext context, AbilityKey index)
        {
            throw new NotImplementedException();
        }

        public virtual void OnAbiPress(InputAction.CallbackContext context, AbilityKey index)
        {
            throw new NotImplementedException();
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
                else
                    Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public bool BeginAbility(ActionAbility abi, Vector3 walkDirection, Vector3 direction)
        {
            var targetLocation = abi.CorrectTargetLocation(walkDirection, Quaternion.LookRotation(direction));
            switch (abi.CastType)
            {
                case ActionAbility.ActionCastType.Location:
                {
                    PushCommand(new BeginAbilityCmd()
                    {
                        Abi = abi,
                        Unit = mControlCharacter,
                        TargetLocation = targetLocation
                    });
                    break;
                }
            }

            return true;
        }

        public bool BeginAbilityAtMouse(ActionAbility abi, Vector2 mousePosition)
        {
            switch (abi.CastType)
            {
                case ActionAbility.ActionCastType.Location:
                {
                    var ray = Camera.main.ScreenPointToRay(mousePosition);
                    RaycastHit result;
                    int _targetLayer = GameUnitAPI.GetGroundMask();
                    if (Physics.Raycast(ray, out result, 100, _targetLayer))
                    {
                        var location = new Vector3(result.point.x, result.point.y, result.point.z);
                        PushCommand(new BeginAbilityCmd()
                        {
                            Abi = abi,
                            Unit = mControlCharacter,
                            TargetLocation = location
                        });
                    }

                    break;
                }
                case ActionAbility.ActionCastType.GameUnit:
                {
                    var ray = Camera.main.ScreenPointToRay(mousePosition);
                    RaycastHit result;
                    int targetLayer = abi.CastTargetLayerMask;
                    if (Physics.Raycast(ray, out result, 100, targetLayer))
                    {
                        var target = result.collider.gameObject.GetComponent<GameUnit>();
                        PushCommand(new BeginAbilityCmd()
                        {
                            Abi = abi,
                            Unit = mControlCharacter,
                            TargetUnit = target
                        });
                    }

                    break;
                }
            }

            return true;
        }

        private bool mAudioPlayable = true;
        private AudioSource _audioSource;

        public void FixedUpdate()
        {
            
        }

        public AudioSource audioSource
        {
            get
            {
                if (_audioSource)
                    return _audioSource;
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                    _audioSource = gameObject.AddComponent<AudioSource>();
                return _audioSource;
            }
        }

        public bool PlayAudio(AudioClip clip, bool force = false)
        {
            if (mControlCharacter == null)
                return false;
            if (mControlCharacter.IsDead)
                return false;
            bool playable = false;
            if (force)
            {
                playable = true;
            }
            else
            {
                playable = !audioSource.isPlaying && mAudioPlayable;
            }

            if (playable && clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                mAudioPlayable = false;

                KTimer timer = KGameCore.GlobalTimers.AddTimer(audioSource.clip.length + Random.Range(2, 6),
                    () => { mAudioPlayable = true; });
                timer.Start();
                return true;
            }

            return false;
        }

    }
}