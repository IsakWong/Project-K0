using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
///
/// 
/// </summary>
public class VfxAnimFade : VfxAnim
{
    // Start is called before the first frame update

    public bool mFadeLight = false;
    public bool mFadeParticle = false;
    public bool mFadeMesh = false;
    public bool mFakdeSkinMesh = false;

    public bool ScaleMesh = false;

    public GameObject mFadeParent;

    public List<GameObject> mDieObjects = new List<GameObject>();

    void Awake()
    {
        if (mFadeParent is null)
            mFadeParent = gameObject;
    }

    public override void Spawn()
    {
        if (mDieDuration > 0)
        {
            if (ScaleMesh)
            {
                var oldScale = gameObject.transform.localScale;
                gameObject.transform.localScale = Vector3.zero;
                gameObject.transform.DOScale(oldScale, mDieDuration);
            }

            if (mFadeLight)
                StopAllLight(1.0f);
            if (mFadeMesh)
            {
                var meshs = mFadeParent.GetComponentsInChildren<MeshRenderer>();
                foreach (var mesh in meshs)
                {
                    mesh.material.SetFloat("_Alpha", 0.0f);
                    var color = mesh.material.GetColor("_Color");
                    color.a = 0.0f;
                    mesh.material.SetColor("_Color", color);
                }

                StopAllMeshRenderer(1.0f);
            }
        }
    }

    public override void Die()
    {
        foreach (var ob in mDieObjects)
        {
            ob.SetActive(true);
        }

        if (mDieDuration > 0)
        {
            if (mFadeLight)
                StopAllLight(0.0f);
            if (mFadeMesh)
                StopAllMeshRenderer(0.0f);
            if (mFadeParticle)
                StopAllParticle();
            if (mFakdeSkinMesh)
                StopAllSkinMeshRenderer(0.0f);
            if (ScaleMesh)
            {
                gameObject.transform.DOScale(Vector3.zero, mDieDuration);
            }
        }
    }

    protected void StopAllParticle()
    {
        var particles = mFadeParent.GetComponentsInChildren<ParticleSystem>();
        foreach (var partcile in particles)
        {
            partcile.Stop();
        }
    }

    protected void StopAllLight(float target)
    {
        var meshs = mFadeParent.GetComponentsInChildren<Light>();
        foreach (var mesh in meshs)
        {
            mesh.DOIntensity(target, mDieDuration);
        }
    }

    protected void StopAllMeshRenderer(float target)
    {
        var meshs = mFadeParent.GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in meshs)
        {
            mesh.material.DOFloat(target, "_Alpha", mDieDuration);
            var color = mesh.material.GetColor("_Color");
            color.a = target;
            mesh.material.DOColor(color, "_Color", mDieDuration);
        }
    }

    protected void StopAllSkinMeshRenderer(float target)
    {
        var meshs = mFadeParent.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach (var mesh in meshs)
        {
            foreach (var mat in mesh.materials)
            {
                mat.DOFloat(target, "_Alpha", mDieDuration);
                var color = mat.GetColor("_Color");
                color.a = target;
                mat.DOColor(color, "_Color", mDieDuration);
            }
        }
    }
}