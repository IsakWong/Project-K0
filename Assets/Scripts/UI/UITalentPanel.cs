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

public class UITalentPanel : UIPanel
{
    private TalentNodeConfig LeftConfig;
    private TalentNodeConfig RightConfig;
    public UITalentNode Left;
    public UITalentNode Right;
    public Action<TalentNodeConfig> OnTalentChoose;

    public void SetTalent(TalentNodeConfig left, TalentNodeConfig right)
    {
        LeftConfig = left;
        RightConfig = right;
        Left.SetTalentNode(left);
        Right.SetTalentNode(right);
    }

    public void OnLeftChoose()
    {
        HidePanel(0.5f);
        OnTalentChoose?.Invoke(LeftConfig);
    }

    public void OnRightChoose()
    {
        HidePanel(0.5f);
        OnTalentChoose?.Invoke(RightConfig);
    }
}