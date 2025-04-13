using System;
using DG.Tweening;
using Framework.Foundation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace K1.Gameplay
{
    public class ThirdPersonPlayerController : K1PlayerController
    {
        private Vector2 _input;
        public Indicator mTargetIndicator;

        private ActionAbility mIndicatorAbi;

        protected AbilityIndicator CurrentIndicator;

        public void ShowIndicator(ActionAbility abi)
        {
            mIndicatorAbi = abi;
            mTargetIndicator.ShowIndicator();
            mTargetIndicator.gameObject.SetActive(true);
            mTargetIndicator.transform.localScale = Vector3.zero;
            var scale = mTargetIndicator.transform.DOScale(Vector3.one * mIndicatorAbi.ActionConfig.mIndicatorRadius,
                0.1f);
            scale.SetEase(Ease.InExpo);
            scale.Play();
        }
        protected new void FixedUpdate()
        {
            UpdateIndicator();
            if (CurrentIndicator != null)
                CurrentIndicator.Logic();
        }
        private void UpdateIndicator()
        {
            if (mIndicatorAbi is not null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, GameUnitAPI.GetGroundMask()))
                {
                    Vector3 pos = hit.point;
                    pos.y += 0.05f;
                    mTargetIndicator.gameObject.transform.position = pos;
                }
            }
        }

        protected override void InitInput()
        {
            //   base.InitInput();
            
            mTargetIndicator = GameObject.Instantiate(KGameCore.SystemAt<HUDModule>().mTargetIndicator)
                .GetComponent<Indicator>();
            mTargetIndicator.gameObject.SetActive(false);
            
            mInput.actions.FindAction("Move").performed += Move;
            mInput.actions.FindAction("Move").canceled += Move;
            mInput.actions.FindAction("LockTarget").performed += LockTarget;
            mInput.actions.FindAction("Mouse").performed += (context => { _input = context.ReadValue<Vector2>(); });
        }

        private Vector3 walkDirection = new Vector3();


        public void Move(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
            {
                walkDirection = Vector3.zero;
            }
            else
            {
                walkDirection.x = context.ReadValue<Vector2>().x;
                walkDirection.z = context.ReadValue<Vector2>().y;
            }
        }

        CharacterUnit LockedTarget = null;

        public void LockTarget(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                var cameraController = KGameCore.SystemAt<CameraModule>().ThirdPersonCameraController;
                if (cameraController.LockTarget != null)
                {
                    cameraController.LockTarget = null;
                    LockedTarget = null;
                }
                else
                {
                    GameUnitAPI.GetGameUnitInSphere<CharacterUnit>(mControlCharacter.WorldPosition, 10.0f,
                        GameUnitAPI.GetCharacterLayerMask(), out var result,
                        (unit) => { return mControlCharacter.IsEnemy(unit); });
                    if (result.Count > 0)
                    {
                        LockedTarget = result[0];
                        locked = false;
                        cameraController.LockTarget = result[0].GetSocketTransform(BuiltinCharacterSocket.Body);
                    }
                }
            }
        }

        public void MoveZ(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
            {
                walkDirection.z = 0;
            }
            else
            {
                walkDirection.z = context.ReadValue<float>();
            }
        }

        public override void OnLogic()
        {
            if (mControlCharacter.IsWalking)
            {
                if (walkDirection.x == 0 && walkDirection.z == 0)
                {
                    PushCommand(new IdleCommand() { Unit = mControlCharacter });
                }
                else
                {
                    var walk = mControlCharacter.FSM.GetState(CharacterStateID.WalkState) as WalkCharacterState;
                    walk.MoveTargetDirection = Camera.main.transform.rotation * walkDirection;
                    walk.FaceTargetDirection = Camera.main.transform.rotation * walkDirection;
                }
            }

            if (mControlCharacter.IsIdle)
            {
                if (walkDirection.x != 0 || walkDirection.z != 0)
                {
                    Vector3 delta = Camera.main.transform.rotation * walkDirection;
                    PushCommand(new WalkCommand()
                        { Unit = mControlCharacter, TargetDirection = delta, FaceDirection = delta });
                }
            }

            base.OnLogic();
        }

        public override void OnAbiPress(InputAction.CallbackContext context, AbilityKey index)
        {
            var abi = mControlCharacter.GetActionByKey(index);
            KGameCore.SystemAt<GameplayModule>().Log($"尝试释放: {abi.mAbiName}");
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (abi.IsCoolingDown)
                OnAbiCooldownWarning?.Invoke(this);
            if (!abi.CheckTriggerable())
            {
                return;
            }

            if (CurrentIndicator == null)
                CurrentIndicator = abi.Indicator;

            if (CurrentIndicator != null)
            {
                CurrentIndicator.Begin(context);
                return;
            }

            Vector3 diretion = Camera.main.transform.forward;
            diretion.Normalize();

            switch (abi.CastType)
            {
                case ActionAbility.ActionCastType.Location:
                    if (LockedTarget)
                    {
                        BeginAbility(abi, walkDirection,
                            (LockedTarget.WorldPosition - mControlCharacter.WorldPosition).normalized);
                    }
                    else
                    {
                        BeginAbility(abi, walkDirection, diretion);
                    }

                    break;
                case ActionAbility.ActionCastType.NoTarget:
                    PushCommand(new BeginAbilityCmd()
                    {
                        Abi = abi,
                        Unit = mControlCharacter,
                    });
                    break;
            }
        }

        public override void OnAbiRelease(InputAction.CallbackContext context, AbilityKey index)
        {
            var abi = mControlCharacter.GetActionByKey(index);
            if (CurrentIndicator == abi.Indicator)
            {
                if (CurrentIndicator != null)
                {
                    CurrentIndicator.End(this, context);
                    CurrentIndicator = null;
                    return;
                }

                CurrentIndicator = null;
            }

            if (!abi.CheckTriggerable())
                return;
            if (abi.EndWhenKeyRelease())
            {
                abi.ForceEnd();
            }
        }

        // cinemachine
        public float _cinemachineTargetYaw;
        public float _cinemachineTargetPitch;

        private const float _threshold = 0.01f;


        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        public float LeftClamp = 30.0f;
        public float RightClamp = 30.0f;

        public float RangeMax;
        public float RangeMin;
        public float Value;
        public float smoothTime = 0.3f;
        private float velocity = 0;
        private bool locked = false;

        // private void OnDrawGizmos()
        // {
        //     // clamp our rotations so our values are limited 360 degrees
        //     var cameraController = KGameCore.SystemAt<CameraModule>().ThirdPersonCameraController;
        //     if (cameraController.LockTarget)
        //     {
        //         
        //         var lockDelta = cameraController.LockTarget.position - cameraController.FromTarget.position;
        //         lockDelta.Normalize();
        //         var left = MathUtility.RotateDirectionY(lockDelta, -LockRange);
        //         var right = MathUtility.RotateDirectionY(lockDelta, LockRange);
        //     
        //         Handles.ArrowHandleCap(
        //             0,
        //             cameraController.FromTarget.position,
        //             Quaternion.LookRotation(left),
        //             2.0f,
        //             EventType.Repaint
        //         );
        //         Handles.ArrowHandleCap(
        //             0,
        //             cameraController.FromTarget.position,
        //             Quaternion.LookRotation(right),
        //             2.0f,
        //             EventType.Repaint
        //         );
        //         RangeMin = MathUtility.GetAngleInXZ(left);
        //         RangeMax = MathUtility.GetAngleInXZ(right);
        //         Handles.Label(cameraController.FromTarget.position, $"{RangeMin}, {RangeMax}");
        //         
        //     }
        // }

        private void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // clamp our rotations so our values are limited 360 degrees
                var cameraController = KGameCore.SystemAt<CameraModule>().ThirdPersonCameraController;
                
                if (_input.sqrMagnitude >= _threshold)
                {
                    _cinemachineTargetPitch -= _input.y * 0.5f;
                    //Don't multiply mouse input by Time.deltaTime;
                    _cinemachineTargetYaw += _input.x * 0.5f;
                }
                _cinemachineTargetPitch = MathUtility.ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                if (cameraController.LockTarget)
                {
                    var lockDelta = cameraController.LockTarget.position - cameraController.FromTarget.position;
                    lockDelta.Normalize();
                    var left = MathUtility.RotateDirectionY(lockDelta, LeftClamp);
                    var right = MathUtility.RotateDirectionY(lockDelta, RightClamp);
                    RangeMin = MathUtility.GetAngleInXZ(left);
                    RangeMax = MathUtility.GetAngleInXZ(right);
                    if (_cinemachineTargetYaw > 360)
                        _cinemachineTargetYaw -= 360;
                    if (_cinemachineTargetYaw < 0)
                        _cinemachineTargetYaw += 360;
                    var newValue = MathUtility.ClampAngle(_cinemachineTargetYaw, RangeMin, RangeMax);
                    if (!locked)
                    {
                        if (Mathf.Abs(_cinemachineTargetYaw - newValue) < 1f)
                            locked = true;
                        _cinemachineTargetYaw = Mathf.SmoothDamp(_cinemachineTargetYaw, newValue, ref velocity, smoothTime);
                    }
                    else
                    {
                        _cinemachineTargetYaw = newValue;
                    }
                    cameraController.Yaw = _cinemachineTargetYaw;
                    cameraController.Pitch = _cinemachineTargetPitch;
                }
                else
                {
                    cameraController.Yaw = _cinemachineTargetYaw;
                    cameraController.Pitch = _cinemachineTargetPitch;
                }
                // if (KGameCore.SystemAt<CameraModule>().ThirdPersonCamera.Follow)
                // {
                //     KGameCore.SystemAt<CameraModule>().ThirdPersonCamera.Follow.transform.rotation = Quaternion.Euler(
                //         _cinemachineTargetPitch * -0.5f + CameraAngleOverride
                //         , _cinemachineTargetYaw, 0.0f);
                // }
            }
            else
            {
                KGameCore.SystemAt<CameraModule>().EnableInput(false);
            }

            _input.x = 0;
            _input.y = 0;
        }
    }
}