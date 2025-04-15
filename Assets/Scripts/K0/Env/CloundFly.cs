using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloundFly : MonoBehaviour
{
    private Vector3 _oldPosition;

    public float y = 0.1f;
    public float x = 0f;

    private Vector3 _targetPosition;

    public float SmoothTime = 1f;
    public float MaxSpeed = 1.5f;

    private bool up = true;

    // Use this for initialization
    void Start()
    {
        _oldPosition = transform.position;
        _targetPosition = _oldPosition;
        _targetPosition.y += y;
        _targetPosition.x += x;
    }

    private Vector3 _upSpeed;

    private Vector3 _downSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.position =
            Vector3.SmoothDamp(transform.position, _targetPosition, ref _upSpeed, SmoothTime, MaxSpeed);
        if (Vector3.Magnitude(transform.position - _targetPosition) < 0.01)
        {
            transform.position = _oldPosition;
        }
    }
}