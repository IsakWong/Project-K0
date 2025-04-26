using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using DG.Tweening;
using cakeslice;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-300)]
public class CameraModule : KModule
{
    // Start is called before the first frame update
    public CinemachineCamera ThirdPersonCamera;
    public ThirdPersonCameraController ThirdPersonCameraController;

    public Light EnvDirectionalLight;
    public CinemachineBrain Brain;

    public CinemachineInputAxisController Input;

    public bool EnableFOV = true;
    public bool EnableShake = true;
    
    public void EnableInput(bool val)
    {
        //Input.enabled = val;
    }

    public void ThirdPersonFollow(GameObject followTarget, GameObject lookTarget)
    {
        ThirdPersonCamera.gameObject.SetActive(true);
        ThirdPersonCamera.Follow = followTarget.transform;
        ThirdPersonCameraController.FromTarget = lookTarget.transform;
        foreach (var cam in Cameras)
        {
            cam.gameObject.SetActive(false);
        }
    }

    public HashSet<CinemachineCamera> Cameras = new();


    public CinemachineCamera ActiveCamera
    {
        get { return Brain.ActiveVirtualCamera as CinemachineCamera; }
    }

    public PostProcessVolume PostProcessVolume;

    // Shake Preset
    public NoiseSettings mHighShake;
    public NoiseSettings m6DShake;
    public NoiseSettings mStunShake;
    public NoiseSettings ZShake;

    //Camera Main
    public CinemachineBrain mCameraBrain;

    private Sequence shakeSeq;
    private Sequence postSeq;

    public void UpdateCamera()
    {
        ThirdPersonCameraController.UpdateCamera();
        Brain.ManualUpdate();
    }

    protected new void Awake()
    {
        base.Awake();
        foreach (var cam in Cameras)
        {
            cam.gameObject.SetActive(false);
        }

        //Input.gameObject.SetActive(false);
    }

    public void Start()
    {
//         EnvDirectionalLight = GameObject.Find("Directional Light").GetComponent<Light>();
    }

    private Sequence fivSeq;

    public void FieldView(float fov, float time1, float duration, float time)
    {
        if(EnableFOV == false)
            return;
        if (fivSeq != null)
        {
            fivSeq.Kill();
        }

        var old = ActiveCamera.Lens.FieldOfView;
        fivSeq = DOTween.Sequence();
        fivSeq.Append(DOTween.To(() => ActiveCamera.Lens.FieldOfView, v => ActiveCamera.Lens.FieldOfView = v, fov,
            time1));
        fivSeq.AppendInterval(duration);
        fivSeq.AppendCallback(() =>
        {
            DOTween.To(() => ActiveCamera.Lens.FieldOfView, v => ActiveCamera.Lens.FieldOfView = v, old, time);
        });
        fivSeq.Play();
    }

    public void PostProcess(float time, float duration)
    {
        if (postSeq != null)
        {
            postSeq.Kill();
        }

        postSeq = DOTween.Sequence();
        postSeq.Append(DOTween.To(() => PostProcessVolume.weight, v => PostProcessVolume.weight = v, 1.0f, time));
        postSeq.AppendInterval(duration);
        postSeq.AppendCallback(() =>
        {
            DOTween.To(() => PostProcessVolume.weight, v => PostProcessVolume.weight = v, 0.0f, time);
            shakeSeq.AppendInterval(time);
        });
        postSeq.Play();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    public float ShakeAmplitudeGain = 1.0f;
    public float ShakeFrequencyGain = 1.0f;

    public void ShakeCamera(float time, NoiseSettings settings = null)
    {
        if(EnableShake == false)
            return;
        if (settings == null)
            settings = m6DShake;

        var p = ActiveCamera.gameObject.GetComponent<CinemachineBasicMultiChannelPerlin>();
        p.NoiseProfile = settings;
        if (shakeSeq != null)
        {
            shakeSeq.Kill();
        }
        p.AmplitudeGain = ShakeAmplitudeGain;
        p.FrequencyGain = ShakeFrequencyGain;
        shakeSeq = DOTween.Sequence();
        shakeSeq.AppendInterval(time);
        shakeSeq.AppendCallback(() =>
        {
            p.AmplitudeGain = 0;
            p.FrequencyGain = 0;
        });
    }

    public void SetBlend(CinemachineBlendDefinition.Styles style, float duration)
    {
        Brain.DefaultBlend.Style = style;
        Brain.DefaultBlend.Time = duration;
    }

    public void PushCamera(CinemachineCamera camera, bool active = true, float duration = -1.0f)
    {
        if (camera == null)
            return;
        if (!Cameras.Contains(camera))
            Cameras.Add(camera);
        //camera.transform.SetParent(this.transform, true);
        foreach (var cam in Cameras)
        {
            if (camera == cam)
                cam.gameObject.SetActive(true);
            else
                cam.gameObject.SetActive(false);
        }

        if (duration > 0)
        {
            KGameCore.Instance.Timers.AddTimer(duration, () =>
            {
                Cameras.Remove(camera);
                GameObject.Destroy(camera.gameObject);
            });
        }
    }

    public void PlayCameraPreset()
    {
    }
}