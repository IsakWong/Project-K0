using System;
using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

    public class PlayerModule : KModule
    {
        public ControllerBase LocalPlayerController;
        public T GetLocalPlayerController<T>() where T : ControllerBase
        {
            return LocalPlayerController as T;
        }

        public PlayerInput GetLocalPlayerInput()
        {
            return LocalPlayerController.GetLocalPlayerInput();
        }
        
        public void SwitchInputMode(string inputMode)
        {
            LocalPlayerController.GetLocalPlayerInput().SwitchCurrentActionMap(inputMode);
        }

        public void FixedUpdate()
        {
            
        }
    }
