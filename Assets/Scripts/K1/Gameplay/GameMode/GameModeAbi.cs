using System.Collections.Generic;
using DG.Tweening;
using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameModeAbi : GameMode
{
    public CharacterUnit Yasuo;
    public CharacterUnit Lux;

    protected UIMainPanel MainUI;

    private int Combo = 0;
    private KTimer timer;

    public void Begin()
    {
        var seq = DOTween.Sequence();

        MainUI = UIManager.Instance.ShowUI<UIMainPanel>();
        MainUI.HidePanel(0.5f);
        MainUI.ShowPanel(0.25f, -1f);
        BeginBoss();
        //KGameCore.SystemAt<CameraModule>().EnableInput(true);
        KGameCore.SystemAt<GameplayModule>().BeginGame();

        seq.Play();
    }

    public List<CharacterUnit> mControlPlayers;
    public K1PlayerController LocalPlayerController;

    protected void BeginBoss()
    {
        KGameCore.SystemAt<PlayerModule>().SwitchInputMode("Battle");
        KGameCore.SystemAt<AudioModule>()
            .AttachAudioListener(KGameCore.SystemAt<CameraModule>().mCameraBrain.gameObject);
        KGameCore.SystemAt<CameraModule>()
            .ThirdPersonFollow(Lux.GetSocketTransform(BuiltinCharacterSocket.FollowTarget).gameObject, Lux.Root);
        Lux.gameObject.SetActive(true);
        KGameCore.SystemAt<GameplayModule>().BeginGame();
        MainUI.RefreshAvatar(LocalPlayerController);
        MainUI.ShowBossHealth(Yasuo);

        // timer = KGameCore.Instance.Timers.AddTimer(3.0f, () =>
        // {
        //     Combo = 0;
        // });
        // KGameCore.SystemAt<GameplayModule>().AnyEvent.OnGetHitted += (source, target, param) =>
        // {
        //     Combo++;
        //     timer.Reset();
        //     MainUI.PopupText($"Combo X {Combo}");
        // };
    }

    public override void OnModeBegin()
    {
        base.OnModeBegin();
        Begin();
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }
}