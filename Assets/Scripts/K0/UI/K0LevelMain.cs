using System;
using K0;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class K0UILevelMain : UIPanel
{
    public GameObject Menu;
    public Text SeedCount;

    private void OnBack(InputAction.CallbackContext ctx)
    {
        if(!Menu.activeInHierarchy)
            Menu.GetComponent<UIAnimBase>().Show(0.5f);
        else
            Menu.GetComponent<UIAnimBase>().Hide(0.5f);
        

    }
    private void Start()
    {
        var controller = KGameCore.SystemAt<PlayerModule>().LocalPlayerController as PlayerControllerK0;
        var actionMap = controller.GetLocalPlayerInput().actions.FindActionMap("UI");
        actionMap.FindAction("Menu").performed += OnBack;
        var gameMode = KGameCore.Instance.CurrentGameMode as K0GameMode;
        gameMode.OnSeedGrowEvent += () =>
        {
            SeedCount.text = gameMode.GrowSeedCount.ToString();
        };
    }

}