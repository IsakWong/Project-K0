using UnityEngine;
using UnityEngine.InputSystem;

namespace K1.Gameplay
{
    public class TopdownPlayerController : K1PlayerController
    {
        

        public void OnLeftPress(InputAction.CallbackContext context)
        {
            
        }
        public void OnLeftMouseClick(InputAction.CallbackContext context)
        {
            OnLeftPress(context);
        }

        protected virtual void InitInput()
        {
            mInput.actions.FindAction("Left").started += OnLeftMouseClick;
            mInput.actions.FindAction("Right").started += OnRightMouseClick;
        }
        
        protected override void InitAbi()
        {
            // foreach (var action in mInput.actions.FindActionMap("Battle").actions)
            // {
            //     AbilityKey key;
            //     if (AbilityKey.TryParse(action.name, out key))
            //     {
            //         action.canceled += (delegate(InputAction.CallbackContext context) { OnAbiRelease(context, key); });
            //         action.started += (delegate(InputAction.CallbackContext context) { OnAbiPress(context, key); });
            //     }
            // }
        }
        
        public void OnRightMouseClick(InputAction.CallbackContext context)
        {

            Vector2 mousePosition = Mouse.current.position.ReadValue();

            var ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit result;
            int _targetLayer = GameUnitAPI.GetGroundMask();
            if (Physics.Raycast(ray, out result, 100, _targetLayer))
            {
                PushCommand(new MoveCommand()
                {
                    TargetLocation = result.point,
                    Unit = mControlCharacter,
                });
                var gameobj = GameObject.Instantiate(KGameCore.SystemAt<HUDModule>().mMoveIndicator);
                var indicator = gameobj.GetComponent<Indicator>();
                indicator.mHighlightSelection = false;
                indicator.ShowAndDestroy(0.3f);
                var pos = result.point;
                pos.y += 0.05f;
                gameobj.transform.position = pos;
                if (mControlCharacter.CanWalk())
                {
                    var audio = mControlCharacter.mCharacterConfig.CharacterAudio.RandomAccess();
                    if (PlayAudio(audio))
                    {
                        if (mControlCharacter.mCharacterConfig.CharacterAudioTitle.TryGetValue(audio, out var value))
                            UIManager.Instance.GetUI<UIMainPanel>().ShowSubtitle2(mControlCharacter.mCharacterConfig.Name, value,
                                audio.length);
                    }
                }
            }
        }


        public virtual void OnAbiPress(InputAction.CallbackContext context, AbilityKey index)
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

            switch (abi.CastType)
            {
                case ActionAbility.ActionCastType.Location:
                    BeginAbilityAtMouse(abi, mousePosition);
                    break;
                case ActionAbility.ActionCastType.GameUnit:
                    BeginAbilityAtMouse(abi, mousePosition);
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

    }
}