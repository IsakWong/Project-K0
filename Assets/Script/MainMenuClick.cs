using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuClick : MonoBehaviour
{
    public Camera[] TargetCameras;


    public Camera _target;
    public Camera MainCamera;

    // Use this for initialization
    void Start()
    {
    }

    private Vector3 _v;

    // Update is called once per frame
    void Update()
    {
        if (_target != null)
        {
            Vector3 _targetPos = this._target.transform.position;
            Vector3 _currentPos = MainCamera.transform.position;
            MainCamera.transform.position = Vector3.SmoothDamp(_currentPos, _targetPos, ref _v, 0.5f, 6f);
        }
    }

    public void OnStartClick()
    {
        _target = TargetCameras[1];
        var start = transform.parent.GetChild(1);
        start.GetComponent<Animator>().SetBool("move_in", true);
        start.GetComponent<Animator>().SetBool("move_out", false);
        GetComponent<Animator>().SetBool("move_out", true);
        GetComponent<Animator>().SetBool("move_in", false);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }

    public void OnHelpClick()
    {
        _target = TargetCameras[2];
        var help = transform.parent.GetChild(2);
        help.GetComponent<Animator>().SetBool("move_in", true);
        help.GetComponent<Animator>().SetBool("move_out", false);
        GetComponent<Animator>().SetBool("move_out", true);
        GetComponent<Animator>().SetBool("move_in", false);
    }
}