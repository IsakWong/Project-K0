using System;
using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

    public class PlayerModule : KModule
    {
        public PlayerInput PlayerInput;
        public ControllerBase LocalPlayerController;
        public T GetLocalPlayerController<T>() where T : ControllerBase
        {
            return LocalPlayerController as T;
        }
        public void SwitchInputMode(string inputMode)
        {
            PlayerInput.SwitchCurrentActionMap(inputMode);
        }

        public void FixedUpdate()
        {
            
        }
    }
