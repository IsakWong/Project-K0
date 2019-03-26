using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _cachedCurrent;

    public static InputManager Current
    {
        get { return _cachedCurrent; }
    }

    public bool IsEnable = true;

    public KeyCode[] InputKeys =
    {
        KeyCode.Escape,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.Mouse2,
        KeyCode.Mouse3,
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };

    public event Action<Vector3> OnAxisChanged;

    public event Action<KeyCode> OnKeyPressed;

    public event Action<KeyCode> OnKeyHold;

    public event Action<KeyCode> OnKeyReleased;
    // Use this for initialization

    private bool[] _isKeyDown;
    private bool[] _isKeyUp;
    private bool[] _isKeyHold;


    void Start()
    {
        _isKeyDown = new bool[InputKeys.Length];
        _isKeyUp = new bool[InputKeys.Length];
        _isKeyHold = new bool[InputKeys.Length];
    }

    private void Awake()
    {
        _cachedCurrent = this;
    }

    void FixedUpdate()
    {
        if (!IsEnable)
            return;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");


        Vector3 forward = Camera.main.transform.TransformDirection(new Vector3(0, 0, 1));
        forward.y = 0;
        Vector3 right = Camera.main.transform.TransformDirection(new Vector3(1, 0, 0));
        right.y = 0;
        forward.Normalize();
        right.Normalize();
        Vector3 move = right * x + forward * y;
        if (OnAxisChanged != null)
        {
            OnAxisChanged(move);
        }

        for (int i = 0; i < InputKeys.Length; i++)
        {
            var key = InputKeys[i];
            if (_isKeyDown[i])
            {
                if (OnKeyPressed != null)
                    OnKeyPressed(key);
                _isKeyDown[i] = false;
            }

            if (_isKeyUp[i])
            {
                if (OnKeyReleased != null)
                    OnKeyReleased(key);
                _isKeyUp[i] = false;
            }

            if (_isKeyHold[i])
            {
                if (OnKeyHold != null)
                    OnKeyHold(key);
                _isKeyHold[i] = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsEnable)
            return;
        for (int i = 0; i < InputKeys.Length; i++)
        {
            var key = InputKeys[i];
            if (Input.GetKeyDown(key))
            {
                _isKeyDown[i] = true;
            }

            if (Input.GetKeyUp(key))
            {
                _isKeyUp[i] = true;
            }

            if (Input.GetKey(key))
            {
                _isKeyHold[i] = true;
            }
        }
    }
}