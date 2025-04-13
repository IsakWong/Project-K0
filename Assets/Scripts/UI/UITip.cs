using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class UITip : MonoBehaviour
{
    public TextMeshProUGUI mText;
    public TextMeshProUGUI mTitle;
    public CanvasGroup mRoot;
    public bool mDestroyAfterHide = false;
    public UIAnimFadeMove mUIAnim;

    // Start is called before the first frame update
    private void Start()
    {
    }

    public void Move(Vector2 anchoredPosition)
    {
        mUIAnim.SetTargetAnchoredPosition(anchoredPosition);
    }

    public void SetContent(string title, string text)
    {
        mTitle.text = title;
        mText.text = text;
    }

    public void Show(float delta = 0.3f, float lifeTime = -1f)
    {
        mUIAnim.Show(delta, lifeTime);
    }

    public void Hide(float delta = 0.3f)
    {
        mUIAnim.Hide(delta);
    }
}