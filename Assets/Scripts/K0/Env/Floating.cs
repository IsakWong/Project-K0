using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float y = 0.1f;

    public float SmoothTime = 1f;
	public Ease EaseType = Ease.InOutQuart;
    private bool up = true;
    private Sequence sequence;
    
	// Use this for initialization
	void Start ()
	{
	    var endPos = transform.localPosition;
	    endPos.y += y;
	    var startPos = transform.localPosition;
	    startPos.y -= y;
	    transform.localPosition = startPos;
	    sequence = DOTween.Sequence();
	    sequence.Append(transform.DOLocalMoveY(endPos.y, SmoothTime).SetEase(EaseType));
	    sequence.Append(transform.DOLocalMoveY(startPos.y, SmoothTime).SetEase(EaseType));
	    sequence.SetLoops(-1);
	    sequence.Play();
	}
	// Update is called once per frame
}
