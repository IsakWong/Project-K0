using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using K1.Gameplay;
using K1.UI;
using Mopsicus.InfiniteScroll;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class UIMenuPanel : UIPanel
{
    public CinemachineCamera MenuCamera;
    public CinemachineCamera BossCamera;
    public CinemachineCamera AboutCamera;
    public UIAnimFadeMove MainMenuGroup;

    [FormerlySerializedAs("mMenuAnim2")] public UIAnimFadeMove BossAnim;
    [FormerlySerializedAs("mHeroAnim")] public UIAnimFadeMove AbiAnim;
    public UIAnimFadeMove AboutAnim;

    public InfiniteScroll mAbiGroup;

    public CharacterConfig mLux;


    public Action<string> ButtonClick;


    // Start is called before the first frame update
    void Start()
    {
        KGameCore.SystemAt<CameraModule>().PushCamera(MenuCamera, true, -1.0f);
        KGameCore.SystemAt<CameraModule>().SetBlend(CinemachineBlendDefinition.Styles.EaseIn, 0.5f);
        MainMenuGroup.Show(1.0f);
        mAbiGroup.OnHeight += index =>
        {
            var rect = mAbiGroup.Prefab.transform as RectTransform;
            return (int)(rect.sizeDelta.y);
        };
        mAbiGroup.Prefab.gameObject.SetActive(false);
        mAbiGroup.OnFill += (i, o) =>
        {
            o.gameObject.SetActive(true);
            var abiConfig = mLux.OriginAbilityList[i];
            var abiIcon = o.GetComponentInChildren<UIAbilityIcon>();
            var title = o.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            title.text = abiConfig.mName;
            abiIcon.Config = abiConfig;
            abiIcon.mIcon.sprite = abiConfig.mSprite;
        };
    }

    private List<UIAbilityIcon> _abilityIcons = new();

    public void ShowAbility(CharacterConfig config)
    {
        mAbiGroup.InitData(config.OriginAbilityList.Count);
    }

    public GameMode1 BattleGameMode;

    public void OnBeginClick()
    {
        KGameCore.SystemAt<CameraModule>().PushCamera(BossCamera, true, -1.0f);
        Yasuo.SetAnimatorTrigger("Yasuo_Idle0");
        KGameCore.SystemAt<AudioModule>().PlayAudio(Vos.RandomAccess());
        MainMenuGroup.Hide(0.5f);
        BossAnim.Show(0.5f);
        ButtonClick?.Invoke("Yasuo");
        ;
    }

    public void OnViewMore()
    {
    }

    public void OnViewAbility()
    {
        AbiAnim.Show(0.5f);
        ShowAbility(mLux);
    }

    public void OnAboutClick()
    {
        KGameCore.SystemAt<CameraModule>().PushCamera(AboutCamera, true, -1.0f);
        ButtonClick?.Invoke("About");
        MainMenuGroup.Hide(0.5f);
        AboutAnim.Show(0.5f);
    }

    public CharacterUnit Yasuo;
    public List<AudioClip> Vos;

    public void OnYasuoClick()
    {
        KGameCore.SystemAt<AudioModule>().SwitchBgm(null);
        KGameCore.Instance.SwitchGameMode(BattleGameMode);
        HidePanel(0.5f);
    }


    public void OnBackClick()
    {
        KGameCore.SystemAt<CameraModule>().PushCamera(MenuCamera, true, -1.0f);
        ButtonClick?.Invoke("MainMenu");
        ;
        MainMenuGroup.Show(0.5f);
        AboutAnim.Hide(0.5f);
        BossAnim.Hide(0.5f);
        AbiAnim.Hide(0.5f);
    }

    public void OnSettingsClick()
    {
        ButtonClick?.Invoke("Settings");
    }

    public void OnExitClick()
    {
        ButtonClick?.Invoke("Exit");
        Application.Quit();
    }
}