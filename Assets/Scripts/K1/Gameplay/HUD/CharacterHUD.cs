using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHUD : HUDBase
{
    public RectTransform Background;
    public RectTransform Foreground;

    public Color mEnemyColor;
    public Color mFriendColor;

    public TextMeshProUGUI mStatusText;
    public RectTransform StatusBackground;
    public RectTransform StatusForeground;

    public Image mAvatarImg;

    public void SetPercent(float percent)
    {
        Foreground.sizeDelta = new Vector2(Background.sizeDelta.x * percent, Foreground.sizeDelta.y);
    }

    public void SetStatusPercent(float percent)
    {
        StatusForeground.sizeDelta = new Vector2(StatusBackground.sizeDelta.x * percent, StatusBackground.sizeDelta.y);
        SetStatusVisible(percent >= 0);
    }

    public void SetStatusLabel(string label)
    {
        mStatusText.text = label;
    }

    public void SetAvatarImage(Sprite img)
    {
        mAvatarImg.sprite = img;
    }

    public void SetStatusVisible(bool val)
    {
        StatusForeground.gameObject.SetActive(val);
        StatusBackground.gameObject.SetActive(val);
        mStatusText.gameObject.SetActive(val);
    }

    public void SetColor(Color color)
    {
        var image = Foreground.gameObject.GetComponent<Image>();
        image.color = color;
    }

    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
    }
}