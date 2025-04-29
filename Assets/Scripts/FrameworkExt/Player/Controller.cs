using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerBase : MonoBehaviour
{
    private CommandQueue controllerCmdQueue = new CommandQueue();

    public void PushCommand(ICommand cmd)
    {
        if (this.enabled)
            controllerCmdQueue.PushCmd(cmd);
    }

    protected void OnEnable()
    {
        controllerCmdQueue.Queue.Clear();
        //KGameCore.RequireSystem<GameplayModule>().RegisterController(this, true);
    }

    protected void OnDisable()
    {
        controllerCmdQueue.Queue.Clear();
        //KGameCore.RequireSystem<GameplayModule>().RegisterController(this, false);
    }

    public virtual void OnLogic()
    {
        controllerCmdQueue.ProcessOnce();
    }

    public virtual bool IsLocalPlayerController()
    {
        return false;
    }

    public virtual PlayerInput GetLocalPlayerInput()
    {
        if (!IsLocalPlayerController())
            return null;
        return GetComponent<PlayerInput>();
    }
}