using DamageNumbersPro;
using TMPro;
using UnityEngine;

public class DamageHUD : MonoBehaviour
{
    public RectTransform mText;

    public TextMeshProUGUI mTextMeshPro;

    // Start is called before the first frame update
    public DamageNumber mGUI;

    public void SetText(string val)
    {
        mGUI.text = val;
    }

    protected new void Start()
    {
        //Sequence sequence = DOTween.Sequence();
        //float start = mText.transform.position.y;
        //float startX = mText.transform.position.x;
        //mText.transform.localScale = Vector3.one * 2f;
        //sequence.Insert(0, mText.DOMoveX(startX + Random.Range(-0.5f, 0.5f), 0.2f));
        //sequence.Append(mText.DOMoveY(start + Random.Range(0.2f, 0.5f), 0.2f));
        //{
        //    var scale = mText.DOScale(Vector3.one, 0.2f);
        //    scale.SetEase(Ease.InOutBounce);
        //    sequence.Insert(0, scale);
        //}
        //sequence.Append(mText.DOMoveY(start + 0.4f, 0.5f));
        //sequence.SetLoops(1);
        //{
        //    var scale = mText.DOScale(Vector3.zero, 0.3f);
        //    scale.SetEase(Ease.InOutBounce);
        //    sequence.Insert(1.5f, scale);
        //}
        //sequence.Play();
        //Destroy(gameObject, 2);
    }
}