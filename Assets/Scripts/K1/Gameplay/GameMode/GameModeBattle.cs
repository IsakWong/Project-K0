using System.Collections.Generic;
using DG.Tweening;
using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameModeBattle : GameModeK1
{
    public CharacterUnit Yasuo;
    public CharacterUnit Lux;

    protected UIMainPanel MainUI;

    public EnvThunder ThunderEffect;
    private int Combo = 0;
    private KTimer timer;

    public void Begin()
    {
        var seq = DOTween.Sequence();

        MainUI = UIManager.Instance.ShowUI<UIMainPanel>();
        //MainUI.ShowPanel(0.25f, -1f);
        MainUI.HidePanel(0.25f);
        seq.InsertCallback(1.0f, () =>
        {
            if (ThunderEffect != null)
                ThunderEffect.PlayThunderLoop();
        });

        BeginBoss();
        KGameCore.SystemAt<CameraModule>().EnableInput(true);

        KGameCore.SystemAt<GameplayModule>().BeginGame();

        Yasuo.CharEvent.OnDie += (unit) => { MainUI.ShowSubtitle2("系统", "挑战失败！", 5.0f); };
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
        timer = KGameCore.Instance.Timers.AddTimer(3.0f, () => { Combo = 0; });
        KGameCore.SystemAt<GameplayModule>().AnyEvent.OnTakeDamage += (source, target, param) =>
        {
            Combo++;
            timer.Reset();
            MainUI.PopupText($"Combo X {Combo}");
        };
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