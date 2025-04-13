using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform FromTarget;
    public Transform LockTarget;
    public CinemachineOrbitalFollow Follow;
    public float Yaw;
    public float Pitch;
    public Vector3 Offset;

    // Start is called before the first frame update
    void Start()
    {
        Follow = GetComponent<CinemachineOrbitalFollow>();
    }

    public void UpdateCamera()
    {
        if (!FromTarget)
            return;

        var offset = transform.TransformVector(Offset);
        var lookAt = FromTarget.transform.position + offset;
        var direction = lookAt - transform.position;
        Follow.HorizontalAxis.Value = Yaw;
        Follow.VerticalAxis.Value = Pitch;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    // Update is called once per frame
    void LateUpdate()
    {
    }

    private void OnDrawGizmos()
    {
        if (!FromTarget)
            return;
        var offset = transform.TransformVector(Offset);
        var lookAt = FromTarget.transform.position + offset;
        Gizmos.DrawCube(lookAt, Vector3.one);
    }
}