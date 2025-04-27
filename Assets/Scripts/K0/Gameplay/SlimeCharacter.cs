using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using K1.Gameplay;
using Obi;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SlimeCharacter : MonoBehaviour, IAbsorbSource
{
    // private Animator animator
    public Rigidbody SlimeRigidBody;
    public ObiSoftbody Softbody;
    
    public AudioSource Walk;
    public AudioClip GrabSFX;
    public AudioClip GrabFinishSFX;
    public List<AudioClip> JumpSFX = new List<AudioClip>();   
    public List<AudioClip> OnGroundSFX = new List<AudioClip>();
    public Vector3 InputMove;
    public ObiEmitter Emitter;
    private Sequence seq;

    public void SpitOut(InputAction.CallbackContext context)
    {
        if(context.canceled)
        {
            Emitter.speed = 0.0f;
        }
        else
        {
            Emitter.speed = 2.5f;
        }
        if (context.performed)
        {
            if (Current != null)
            {
                Current.Released(this);
                Current = null;
            }
        }
    }

    public static T[] Overlap<T>(Vector3 center, float radius, int mask) where T : class
    {
        var colliders = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Collide);
        List<T> result = new List<T>(); 
        foreach (var c in colliders)
        {
            var item = c.gameObject.GetComponent<InteractableItem>();
            if (item != null)
            {
                var t = item as T;
                if (t is not null)
                {
                    result.Add(t);
                }
            }
        }
        return result.ToArray();
    }
    
    public float InteractionRange = 3.0f;

    private AudioSource grab;
    public void Interaction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            var colliders = Overlap<InteractableItem>(transform.position, InteractionRange, 1 << LayerMask.NameToLayer("Interactable"));
            foreach (var it in colliders)
            {
                if (!it.IsAbsorbed())
                {
                    seq = DOTween.Sequence();
                    grab = KGameCore.SystemAt<AudioModule>().PlayAudio(GrabSFX, 1.0f, false, loop:true);
                    
                    seq.Append(it.GetTransform().DOShakeRotation(1f, 10));
                    seq.SetLoops(-1);
                    seq.Play();
                }
                return;
            }
        }
        if(context.canceled)
        {
            if(grab != null)
                Destroy(grab.gameObject);
            if (seq != null && seq.IsActive())
            {
                seq.Kill();
                seq = null;
            }
            var colliders = Overlap<IAbsorbTarget>(transform.position, InteractionRange, 1 << LayerMask.NameToLayer("Interactable"));
            foreach (var item in colliders)
            {
                if (!item.IsAbsorbed())
                {
                    Absorb(item);
                    KGameCore.SystemAt<AudioModule>().PlayAudio(GrabFinishSFX, 1.0f);
                }
                return;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        SlimeRigidBody = GetComponent<Rigidbody>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (isGrounded && jumpable)
        {
            KGameCore.SystemAt<AudioModule>().PlayAudioAtPosition(JumpSFX.RandomAccess(), transform.position, volume:0.5f);
            SlimeRigidBody.AddForce(new Vector3(0, 5.0f, 0), ForceMode.VelocityChange);
        }
    }

    private bool isGrounded;
    public float moveSpeed = 5.0f;
    public float airControlFactor = 0.5f;

    private void HandleMovement()
    {
        var v = InputMove;
        
        Vector3 moveDirection = InputMove;
        if(InputMove.magnitude > 0.1f)
        {
            Walk.UnPause();
        }
        else
        {
            Walk.Pause();
        }

        // 应用移动力
        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airControlFactor;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = SlimeRigidBody.velocity.y; // 保持原有Y轴速度

        // 使用VelocityChange进行更精确的速度控制
        Vector3 velocityDelta = targetVelocity - SlimeRigidBody.velocity;
        velocityDelta.y = 0; // Y轴由跳跃单独处理
        SlimeRigidBody.AddForce(velocityDelta, ForceMode.VelocityChange);
        
    }

    public float rotationSpeed;

    private void HandleRotation()
    {
        if (InputMove.magnitude > 0.1f)
        {
            // 获取摄像头方向的平面旋转
            Quaternion targetRotation = Quaternion.LookRotation(InputMove);

            // 平滑旋转
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
    private bool DetectGround()
    {
        return Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 0.3f,
            1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    }

    private float airTime = 0;
    private bool jumpable = true;
    private void FixedUpdate()
    {
        bool _isGrounded = DetectGround();
        
        if(!isGrounded && _isGrounded)
        {
            jumpable = false;
            float oldResistance = Softbody.deformationResistance;
            Softbody.deformationResistance = 0.1f;
            KGameCore.SystemAt<AudioModule>().PlayAudioAtPosition(OnGroundSFX.RandomAccess(), transform.position);
            DOTween.To(() => Softbody.deformationResistance, x => Softbody.deformationResistance = x, oldResistance, 0.3f).OnComplete(() =>
            {
                jumpable = true;
            });
            Softbody.deformationResistance = 0.1f;
        }
        if (!isGrounded)
        {
            airTime+= Time.fixedDeltaTime;
        }
        if(isGrounded)
        {
            airTime = 0;
        }
        HandleMovement();
        HandleRotation();
        isGrounded = _isGrounded;
    }

    private IAbsorbTarget Current;

    public void Absorb(IAbsorbTarget target)
    {
        if (Current != null)
            Current.Released(this);
        Current = target;
        if (Current != null)
            Current.GetAbsorbed(this);
    }

    public void OnTargetAbsorbed(IAbsorbTarget target)
    {
    }

    public Component GetComponent()
    {
        return this;
    }
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    public Transform GetTransform()
    {
        return transform;
    }
}