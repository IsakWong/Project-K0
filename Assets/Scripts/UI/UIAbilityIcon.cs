using System;
using System.Collections;
using System.Collections.Generic;
using DTT.Utils.Extensions;
using K1.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIAbilityIcon : UIIcon
{
    public Image mIcon;
    public Image mMask;

    public TextMeshProUGUI Key;

    private AbilityBase _ability;
    public AbilityConfig Config;

    public AbilityBase mAbility
    {
        get { return _ability; }
        set
        {
            _ability = value;
            if (_ability != null)
            {
                mIcon.sprite = _ability.Config.mSprite;
                Config = _ability.Config;

                switch (_ability.Config.mDefaultAbiKey)
                {
                    case AbilityKey.Ability_1:
                        Key.text = "1";
                        break;
                    case AbilityKey.Ability_2:
                        Key.text = "2";
                        break;
                    case AbilityKey.Ability_3:
                        Key.text = "3";
                        break;
                    case AbilityKey.Ability_4:
                        Key.text = "4";
                        break;
                    case AbilityKey.Ability_Q:
                        Key.text = "Q";
                        break;
                    case AbilityKey.Ability_W:
                        Key.text = "W";
                        break;
                    case AbilityKey.Ability_E:
                        Key.text = "E";
                        break;
                    case AbilityKey.Ability_R:
                        Key.text = "R";
                        break;
                }
            }
        }
    }

    public void SetPercent(float value)
    {
        mMask.fillAmount = value;
    }

    protected override void OnShowTip()
    {
        mTipOrigin.SetContent(Config.mName, Config.mDescription);
        base.OnShowTip();
    }
}

public enum IconTipDirection
{
    Top,
    Bottom,
    Left,
    Right
}

public class UIIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UITip mTipOrigin;
    public Vector3 mOffset;

    public UITip TipOrigin
    {
        get => mTipOrigin;
    }

    public IconTipDirection mTipDirection = IconTipDirection.Top;

    protected virtual void OnShowTip()
    {
        TipOrigin.gameObject.SetActive(true);
        var rect = transform as RectTransform;
        var worldCorners = new Vector3[4];
        rect.GetWorldCorners(worldCorners);
        var pos = worldCorners[0] + worldCorners[2];
        pos = pos * 0.5f;
        switch (mTipDirection)
        {
            case IconTipDirection.Top:
                pos.y += rect.rect.height;
                break;
            case IconTipDirection.Bottom:
                pos.y -= rect.rect.height;
                break;
            case IconTipDirection.Left:
                pos.x -= rect.rect.width;
                break;
            case IconTipDirection.Right:
                pos.x += rect.rect.width;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, pos);
        RectTransform rectTransform = TipOrigin.transform.parent as RectTransform; /* your RectTransform */
        ;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out localPoint))
        {
            localPoint.x += mOffset.x;
            localPoint.y = localPoint.y + mOffset.y;
            TipOrigin.Move(localPoint);
        }

        TipOrigin.Show();
    }

    protected virtual void OnHideTip()
    {
        TipOrigin.Hide();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHideTip();
    }
}