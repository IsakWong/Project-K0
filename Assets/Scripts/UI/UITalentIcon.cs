using System.Collections;
using System.Collections.Generic;
using K1.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITalentIcon : UIIcon
{
    public Image mIcon;
    public Image mBG;
    private TalentNode _talent;

    public TalentNode AttachTalent
    {
        get => _talent;
        set
        {
            _talent = value;
            mTelantTitle.text = _talent.Config.TalentName;
            mTelantDesc.text = _talent.Config.TalentDesc;
            mIcon.sprite = _talent.Config.TalentIcon;
        }
    }

    public TextMeshProUGUI mTelantTitle;
    public TextMeshProUGUI mTelantDesc;

    protected override void OnShowTip()
    {
        mTipOrigin.SetContent(_talent.Config.TalentName, _talent.Config.TalentDesc);
        base.OnShowTip();
    }
}