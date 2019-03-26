using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    private Vector3 _oldPosition;

    public float y = 0.1f;
    public float x = 0f;

    private Vector3 _targetPosition;

    public float SmoothTime = 1f;
    public float MaxSpeed = 1.5f;
   
    private bool up = true;
	// Use this for initialization
	void Start ()
	{
	    _oldPosition = transform.localPosition;
	    _targetPosition = _oldPosition;
	    _targetPosition.y += y;

	}

    private Vector3 _upSpeed;

    private Vector3 _downSpeed;
	// Update is called once per frame
	void Update () {
	    if (up)
	    {
		    var mewPos = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _downSpeed, SmoothTime,MaxSpeed);
		    mewPos.x = transform.localPosition.x;
		    mewPos.z = transform.localPosition.z;
		    transform.localPosition = mewPos;
	        if (Vector3.Magnitude(transform.localPosition - _targetPosition) < 0.01)
	        {
                _targetPosition.y -= 2 * y;
                up = false;
	        }
        }
	    else
	    {
	        var mewPos = Vector3.SmoothDamp(transform.localPosition, _targetPosition, ref _downSpeed, SmoothTime,MaxSpeed);
	        mewPos.x = transform.localPosition.x;
	        mewPos.z = transform.localPosition.z;
	        transform.localPosition = mewPos;
	        if (Vector3.Magnitude(transform.localPosition - _targetPosition) < 0.01)
	        {
	            _targetPosition.y += 2 * y;
                up = true;
            }
        }
	}
}
