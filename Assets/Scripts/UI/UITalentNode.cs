using System.Collections;
using System.Collections.Generic;
using K1.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class UITalentNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI Title;
    public UIAbilityIcon Icon;
    public TextMeshProUGUI Content;
    public RenderTexture RT;
    public Image VideoImage;
    public VideoPlayer VideoPlayer;
    private TalentNodeConfig TalentNode;
    private RectTransform rectTransform;


    public void SetTalentNode(TalentNodeConfig node)
    {
        TalentNode = node;
        Content.text = TalentNode.TalentDesc;
        Title.text = TalentNode.TalentName;
        VideoPlayer.clip = node.Video;
        var newRT = RenderTexture.Instantiate(RT);
        VideoPlayer.targetTexture = newRT;
        VideoPlayer.Prepare();
        VideoPlayer.Pause();
        VideoImage.material.SetTexture("_MainTex", newRT);
        rectTransform = VideoImage.gameObject.GetComponent<RectTransform>();
        Icon.mIcon.sprite = TalentNode.TalentIcon;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rectTransform.sizeDelta =
            new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * RT.height / RT.width);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        VideoPlayer.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        VideoPlayer.Pause();
    }
}