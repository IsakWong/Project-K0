using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using K1.Gameplay;
using Obi;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerControllerK0 : ControllerBase
{
    private PlayerInput Input;
    public SlimeCharacter Slime;

    public override bool IsLocalPlayerController()
    {
        return true;
    } 
    
    private void Awake()
    {
        Input = KGameCore.SystemAt<PlayerModule>().GetLocalPlayerInput();
    }

    private void Move(Vector2 move)
    {
        Slime.InputMove.x = move.x;
        Slime.InputMove.z = move.y;        
    }


    void SpitOut(InputAction.CallbackContext context)
    {
        Slime.SpitOut(context);
    }

    void Interaction(InputAction.CallbackContext context)
    {
        Slime.Interaction(context);
    }

    // Use this for initialization
    void Start()
    {
        Input.actions.FindAction("Move").performed += ctx => Move(ctx.ReadValue<Vector2>());
        Input.actions.FindAction("Move").canceled += ctx => Move(Vector2.zero);
        Input.actions.FindAction("Jump").performed += Jump;
        Input.actions.FindAction("Jump").started += Jump;
        Input.actions.FindAction("Jump").canceled += Jump;
        Input.actions.FindAction("Interaction").performed += Interaction;
        Input.actions.FindAction("Interaction").canceled += Interaction;
        Input.actions.FindAction("Release").performed += SpitOut;
        Input.actions.FindAction("Release").canceled += SpitOut;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        Slime.Jump(context);
    }
    
}