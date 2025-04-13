using System.Collections.Generic;
using DG.Tweening;
using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameMode1 : GameModeBattle
{
    public CharacterUnit mYasuo;

    private UITalentPanel TalentUI;

    public CinemachineSequencerCamera PerfomCamera;

    [TextArea] public string leftTopTip;

    public AudioClip mBassAudio;

    public GameObject BossUIPrefab;
    public GameObject MainUIPrefab;
    public GameObject TalentUIPrefab;

    public AudioClip LuxTalk;
    private int TalentCount = 0;

    public void InitTalentChoose()
    {
        // TalentUI = UIManager.Instance.ShowUI<UITalentPanel>(TalentUIPrefab);
        // TalentUI.ShowPanel(0.5f, -1f);
        // TalentUI.SetTalent(Lux.mCharacterConfig.mTalentConfigs[0], Lux.mCharacterConfig.mTalentConfigs[1]);
        // TalentUI.OnTalentChoose += config =>
        // {
        //     var talent = config.CreateTalent();
        //     Lux.AddTalent(talent);
        //     if (TalentCount == 1)
        //     {
        //         TalentUI.HidePanel(0.5f);
        //         return;
        //     }
        //     TalentCount++;
        //     TalentUI.ShowPanel(0.5f, -1f);
        //     TalentUI.SetTalent(Lux.mCharacterConfig.mTalentConfigs[2], Lux.mCharacterConfig.mTalentConfigs[3]);
        //     
        // };
    }


    public GameObject StarObject;
    public GameObject ExplodeObject;
    public CinemachineCamera StarCamera;
    public CinemachineCamera BeginCamera;

    public override void OnModeBegin()
    {
        //base.OnModeBegin();
        var seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            UIManager.Instance.GetUI<UIMenuPanel>().HidePanel(0.5f, true);
            KGameCore.SystemAt<CameraModule>().SetBlend(CinemachineBlendDefinition.Styles.EaseIn, 0.5f);
            KGameCore.SystemAt<CameraModule>().PushCamera(BeginCamera, true, -1.0f);


            KGameCore.SystemAt<AudioModule>().PlayAudio(mBassAudio);
            ThunderEffect.PlayThunderLoop();

            KGameCore.SystemAt<AudioModule>()
                .AttachAudioListener(KGameCore.SystemAt<CameraModule>().mCameraBrain.gameObject);
            KGameCore.SystemAt<CameraModule>().SetBlend(CinemachineBlendDefinition.Styles.Cut, 0.0f);
            KGameCore.SystemAt<CameraModule>().PushCamera(StarCamera, true, 4.6f);
            StarObject.gameObject.SetActive(true);
        });
        seq.AppendInterval(4.6f);
        seq.AppendCallback(() =>
        {
            KGameCore.SystemAt<CameraModule>().Brain.DefaultBlend =
                new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseIn, 0.5f);
            Lux.gameObject.SetActive(true);
            KGameCore.SystemAt<AudioModule>()
                .AttachAudioListener(KGameCore.SystemAt<CameraModule>().mCameraBrain.gameObject);
            KGameCore.SystemAt<AudioModule>().PlayAudio(LuxTalk);
            BeginBoss();
            var bossUI = UIManager.Instance.ShowUI<UIBossPanel>(BossUIPrefab);
            bossUI.ShowPanel(0.5f, 2.0f);
            bossUI.gameObject.SetActive(true);
        });
        /*seq.InsertCallback(6.0f, () =>
        {
            Lux.mAnimator.SetTrigger("Lux_Act1");
        });
        seq.InsertCallback(12.0f, () =>
        {
            Lux.mAnimator.SetTrigger("Lux_Act2");
        });*/
        seq.Play();

        bool[] played = new bool[3];
        Lux.CharEvent.OnDie += (u) =>
        {
            UIManager.Instance.GetUI<UIMainPanel>().HidePanel(0.5f);
            UIManager.Instance.ShowUI<UIMainMenuPanel>();
        };
        mYasuo.CharEvent.OnHealthChange += delegate(CharacterUnit unit)
        {
            if (unit.HealthPercent < 0.9f && played[0] == false)
            {
                played[0] = true;
                return;
            }

            if (unit.HealthPercent < 0.6f && played[1] == false)
            {
                played[1] = true;
                return;
            }

            if (unit.HealthPercent < 0.3f && played[2] == false)
            {
                played[2] = true;
                return;
            }
        };
    }

    public void Restart()
    {
        MainUI = UIManager.Instance.ShowUI<UIMainPanel>(MainUIPrefab.gameObject);
        
        Begin();
    }

    public void Failed()
    {
    }

    public void Win()
    {
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }
}