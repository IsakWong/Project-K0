using System.Collections;
using System.Collections.Generic;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuffIcon : UIIcon
{
    public Image mIcon;
    public Image mBG;
    private Buffbase _buff;

    public Buffbase Buff
    {
        get => _buff;
    }

    public void SetBuff(Buffbase buf)
    {
        mIcon.sprite = buf.BuffConfig.mBuffIcon;
        _buff = buf;
    }

    public void SetPercent(float value)
    {
        mBG.fillAmount = value;
    }
}