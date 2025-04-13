using System;
using System.Collections;
using System.Collections.Generic;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using DamageNumbersPro;
using DG.Tweening;
using K1.Gameplay;
using K1.UI;
using Mopsicus.InfiniteScroll;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMainPanel : UIPanel
{
    public Image mBG;
    public Image mFG;
    public UITip mLeftTopTip;
    public UITip mTopTip;
    public UITip mSubtitleTip;
    public UITip mSubtitleTip2;
    public RectTransform TargetLockHUD;

    public UICharacterIcon mHeroIcon;
    public Image mNextHeroImg;
    public TextMeshProUGUI mCurrentHeroName;
    public DamageNumber mTextPrefab;
    public DamageNumberGUI ComboPrefab;
    public GameObject mAbiGroup;
    public GameObject mAbiOrigin;
    public GameObject TalentGroupRoot;
    public GameObject TalentOrigin;

    public GameObject mBuffGroup;
    public GameObject mBuffOrigin;

    public CharacterUnit Boss;

    public UICharaterUltPanel mCharacterUltPanel;

    public void PopupText(string val)
    {
        var rect = ComboPrefab.transform as RectTransform;
        ComboPrefab.SpawnUIText(transform, rect.anchoredPosition, val);
    }

    private CharacterUnit mCurrentDisplayCharacterUnit;

    public InfiniteScroll mScroll;

    public Image mHitImage;
    private Sequence _hitSeq;

    public void ShowDownTip(string title, string text, float lifetime)
    {
        KGameCore.SystemAt<AudioModule>().PlayAudio(KGameCore.SystemAt<AudioModule>().mWarningAudioClip);
        mSubtitleTip.SetContent($@"{title}: ", text);
        mSubtitleTip.Show(0.1f, lifetime);
    }

    public void ShowSubtitle2(string title, string text, float lifetime)
    {
        mSubtitleTip2.SetContent($@"{title}: ", text);
        mSubtitleTip2.Show(0.1f, lifetime);
    }

    public void PlayHitUI()
    {
        if (_hitSeq != null)
        {
            if (_hitSeq.IsPlaying())
                return;
        }

        var targetColor = mHitImage.color;
        targetColor.a = 0.15f;
        _hitSeq = DOTween.Sequence();
        _hitSeq.Append(mHitImage.DOColor(targetColor, 0.1f));
        _hitSeq.AppendInterval(0.1f);
        var newColor = mHitImage.color;
        newColor.a = 0;
        _hitSeq.Append(mHitImage.DOColor(newColor, 0.1f));
        var color = mHitImage.color;
        color.a = 0;
        mHitImage.color = color;
        _hitSeq.Play();
    }

    public RectTransform bossHealth;
    public RectTransform characterHealth;
    public RectTransform characterMana;

    public void ShowBossHealth(CharacterUnit unit)
    {
        Boss = unit;
        float bossHealthWidth = bossHealth.sizeDelta.x;
        Boss.CharEvent.OnHealthChange += delegate(CharacterUnit unit)
        {
            bossHealth.DOScaleX(unit.HealthPercent, 0.5f);
        };
    }

    void Awake()
    {
        base.Awake();
        KGameCore.SystemAt<PlayerModule>().GetLocalPlayerController<K1PlayerController>().OnAbiCooldownWarning += (controller) =>
        {
            ShowDownTip("系统", "技能冷却中", 1.0f);
        };
    }

    private void LateUpdate()
    {
        if (KGameCore.SystemAt<CameraModule>().ThirdPersonCameraController.LockTarget != null)
        {
            TargetLockHUD.gameObject.SetActive(true);
            var pos = KGameCore.SystemAt<CameraModule>().ThirdPersonCameraController.LockTarget.position;
            var screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
            Vector2 anchoredPosition = TargetLockHUD.parent.transform.InverseTransformPoint(screenPos);
            TargetLockHUD.anchoredPosition = anchoredPosition;
        }
        else
        {
            TargetLockHUD.gameObject.SetActive(false);
        }

        if (!mCurrentDisplayCharacterUnit)
            return;
        var buffs = mCurrentDisplayCharacterUnit.Buffs;
        foreach (var abilityIcon in _abilityIcons)
        {
            abilityIcon.SetPercent(abilityIcon.mAbility.CooldownPercent);
        }

        foreach (var buffIcon in _buffIcons)
        {
            buffIcon.SetPercent(buffIcon.Buff.Percent);
        }
    }

    void RefreshBuff(CharacterUnit unit)
    {
        {
            var count = mBuffGroup.transform.childCount;
            var buffs = unit.Buffs;
            for (int i = 2; i < count; i++)
            {
                GameObject.Destroy(mBuffGroup.transform.GetChild(i).gameObject);
            }

            _buffIcons.Clear();
            foreach (var abi in buffs)
            {
                if (abi.BuffConfig is null)
                {
                    continue;
                }

                if (abi.BuffConfig.mBuffIcon == null)
                    continue;
                var newAbiObj = GameObject.Instantiate(mBuffOrigin);
                var buffIcon = newAbiObj.GetComponent<UIBuffIcon>();
                newAbiObj.SetActive(true);
                newAbiObj.transform.SetParent(mBuffGroup.transform);
                buffIcon.SetBuff(abi);
                buffIcon.SetPercent(abi.Percent);
                _buffIcons.Add(buffIcon);
            }
        }
    }

    void OnCurrentCharacterBuffChanged(Buffbase buf)
    {
        if (buf is not null)
        {
            RefreshBuff(mCurrentDisplayCharacterUnit);
        }
    }

    private List<UIAbilityIcon> _abilityIcons = new();
    private List<UIBuffIcon> _buffIcons = new();
    public float Radius = 200;

    public void RefreshTalentGroup(CharacterUnit unit)
    {
        TalentOrigin.gameObject.SetActive(false);
        var abis = unit.mTalentNodes;
        if (TalentGroupRoot.transform.childCount != abis.Count)
        {
            for (int i = 0; i < TalentGroupRoot.transform.childCount; i++)
            {
                GameObject.Destroy(TalentGroupRoot.transform.GetChild(i).gameObject);
            }
        }

        foreach (var abi in abis)
        {
            var newAbiObj = GameObject.Instantiate(TalentOrigin);
            var abiIcon = newAbiObj.GetComponent<UITalentIconSmall>();
            newAbiObj.transform.SetParent(TalentGroupRoot.transform);
            newAbiObj.SetActive(true);
            abiIcon.AttachTalent = abi;
        }
    }

    public void RefreshMiddleBottomAbi(CharacterUnit unit)
    {
        var abis = unit.Abilities;
        for (int i = 2; i < mAbiGroup.transform.childCount; i++)
        {
            GameObject.Destroy(mAbiGroup.transform.GetChild(i).gameObject);
        }

        _abilityIcons.Clear();
        var count = 0;
        foreach (var abi in abis)
        {
            if (abi.Config.AbiUIType == AbilityUIType.MiddleBottom)
                count++;
        }

        var idx = 0;
        var delta = Mathf.PI / (count - 1);
        foreach (var abi in abis)
        {
            if (abi.Config.AbiUIType != AbilityUIType.MiddleBottom)
            {
                continue;
            }

            var newAbiObj = GameObject.Instantiate(mAbiOrigin);
            var abiIcon = newAbiObj.GetComponent<UIAbilityIcon>();
            newAbiObj.transform.SetParent(mAbiGroup.transform);
            var rect = newAbiObj.transform as RectTransform;
            rect.anchoredPosition = new Vector2(Mathf.Cos(-Mathf.PI + idx * delta) * Radius,
                -Mathf.Sin(-Mathf.PI + idx * delta) * Radius);
            _abilityIcons.Add(abiIcon);
            newAbiObj.SetActive(true);
            abiIcon.mAbility = abi;
            abiIcon.SetPercent(0);
            idx++;
        }
    }

    public void RefreshAvatar(K1PlayerController controller)
    {
        if (mCurrentDisplayCharacterUnit)
        {
            mCurrentDisplayCharacterUnit.CharEvent.OnHealthChange += (unit) => { };
            mCurrentDisplayCharacterUnit.OnBuffAdd -= OnCurrentCharacterBuffChanged;
            mCurrentDisplayCharacterUnit.OnBuffRemove -= OnCurrentCharacterBuffChanged;
        }

        RefreshMiddleBottomAbi(controller.mControlCharacter);
        RefreshTalentGroup(controller.mControlCharacter);
        //mNextHeroImg.sprite = controller.NextCharacterUnit.mCharacterConfig.Avatar;
        mCurrentDisplayCharacterUnit = controller.mControlCharacter;
        mCurrentDisplayCharacterUnit.CharEvent.OnHealthChange += delegate(CharacterUnit unit)
        {
            characterHealth.DOScaleX(unit.HealthPercent, 0.5f);
        };
        mCurrentDisplayCharacterUnit.CharEvent.OnManaChange += delegate(CharacterUnit unit)
        {
            characterMana.DOScaleY(unit.ManaPercent, 0.2f);
        };
        if (mCurrentDisplayCharacterUnit)
        {
            mCurrentDisplayCharacterUnit.OnBuffAdd += OnCurrentCharacterBuffChanged;
            mCurrentDisplayCharacterUnit.OnBuffRemove += OnCurrentCharacterBuffChanged;
            //mCurrentHeroName.text = mCurrentDisplayCharacterUnit.mCharacterConfig.Name;
            mHeroIcon.mCharacter = mCurrentDisplayCharacterUnit;
        }
    }
}