using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothIn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    private Vector3 speed = new Vector3();
	// Update is called once per frame
	void Update ()
	{
	    GetComponent<RectTransform>().anchoredPosition = Vector3.SmoothDamp(GetComponent<RectTransform>().anchoredPosition, new Vector3(0, 0, 0), ref speed, 0.3f);
	}
}
