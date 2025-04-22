using System;
using DG.Tweening;
using UnityEngine;

public interface IAbsorbSource
{
    void Absorb(IAbsorbTarget target);
    void OnTargetAbsorbed(IAbsorbTarget target);
    
    T GetComponent<T>();
    GameObject GetGameObject();
    Transform GetTransform();
} 

public interface IAbsorbTarget
{
    void GetAbsorbed(IAbsorbSource absorbSource);
    void Released(IAbsorbSource absorbSource);
    
    bool IsAbsorbed();
    Transform GetTransform();

}
public class InteractableItem : MonoBehaviour, IAbsorbTarget
{
    public void Interaction()
    {
        
    }
    protected Transform ObtainerCenter;
    protected IAbsorbSource _absorbSource = null;
    protected IAbsorbSource _realAbsorbSource = null;
    
    
    private bool isAborbing = false;
    private bool isAborbed = false;
    
    private Vector3 oldScale = Vector3.one;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void GetAbsorbed(IAbsorbSource absorbSource)
    {
        if (isAborbing || isAborbed)
            return;
        isAborbing = true;
        GetComponent<Collider>().isTrigger = true;
        var rigidbody = GetComponent<Rigidbody>();
        if(rigidbody)
            GetComponent<Rigidbody>().isKinematic = true;
        ObtainerCenter = absorbSource.GetTransform();
        
        var seq = DOTween.Sequence();
        var t = DOTween.To(
            () => transform.position - ObtainerCenter.position, // Value getter
            x => transform.position = x + ObtainerCenter.position, // Value setter
            Vector3.zero, 
            0.5f);
        oldScale = transform.localScale;
        seq.Insert(0.0f, t);
        var scale = transform.DOScale(Vector3.zero, 0.4f)
            .SetEase(Ease.InQuart);
        seq.Insert(0.5f, scale);
        seq.Play();
        seq.AppendCallback(() =>
        {
            isAborbing = false;
            isAborbed = true;
            gameObject.SetActive(false);
            absorbSource.OnTargetAbsorbed(this);
        });
        t.SetTarget(transform);
        _absorbSource = absorbSource;
    }
    
    public void AddForce()
    {
        
    }
    
    public bool IsAbsorbed()
    {
        return _absorbSource != null && _realAbsorbSource != null;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public void Released(IAbsorbSource absorbSource)
    {
        gameObject.SetActive(true);
        transform.position = absorbSource.GetTransform().position;
        
        var scale = transform.DOScale(oldScale, 0.5f)
            .SetEase(Ease.InQuart);
        scale.Play();
        GetComponent<Collider>().isTrigger = false;
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.isKinematic = false;
            rigidbody.WakeUp();
            if(_realAbsorbSource != null)
                rigidbody.velocity = _realAbsorbSource.GetTransform().forward * 3;
        }else
        {
            transform.DOMove(transform.position + _realAbsorbSource.GetTransform().forward * 2, 0.5f);            
        }
        _realAbsorbSource = null;
        _absorbSource = null;
    }

}