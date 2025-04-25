using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class K0PlayerController : ControllerBase, IAbsorbSource
{
    public PlayerInput Input;

    private float time = 0.0f;
    // private Animator animator;
    private bool isRotate = false;
    private Rigidbody rigidBody;

    private Vector3 input;

    private void Move(Vector2 move)
    {
        input.x = move.x;
        input.z = move.y;
    }

    private Sequence seq;

    void SpitOut(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Current != null)
            {
                Current.Released(this);
                Current = null;
            }
        }
    }

    T[] Overlap<T>(Vector3 center, float radius, int mask) where T : class
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
    
    void Interaction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            var colliders = Overlap<InteractableItem>(transform.position, InteractionRange, 1 << LayerMask.NameToLayer("Interactable"));
            foreach (var it in colliders)
            {
                if (!it.IsAbsorbed())
                {
                    seq = DOTween.Sequence();
                    seq.Append(it.GetTransform().DOShakeRotation(1f, 10));
                    seq.SetLoops(-1);
                    seq.Play();
                }
                return;
            }
        }
        if(context.canceled)
        {
            if (seq != null && seq.IsActive())
            {
                seq.Kill();
                seq = null;
            }
            var colliders = Overlap<IAbsorbTarget>(transform.position, InteractionRange, 1 << LayerMask.NameToLayer("Interactable"));
            foreach (var item in colliders)
            {
                if(!item.IsAbsorbed())
                    Absorb(item);
                return;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Input.actions.FindAction("Move").performed += ctx => Move(ctx.ReadValue<Vector2>());
        Input.actions.FindAction("Move").canceled += ctx => Move(Vector2.zero);
        Input.actions.FindAction("Jump").performed += Jump;
        Input.actions.FindAction("Interaction").performed += Interaction;
        Input.actions.FindAction("Interaction").canceled += Interaction;
        Input.actions.FindAction("Release").performed += SpitOut;
        Input.actions.FindAction("Release").canceled += SpitOut;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        rigidBody.AddForce(new Vector3(0, 5.0f, 0), ForceMode.VelocityChange);
    }

    private bool isGrounded;
    public float moveSpeed = 5.0f;
    public float airControlFactor = 0.5f;

    private void HandleMovement()
    {
        var v = input;
        
        Vector3 moveDirection = input;

        // 应用移动力
        float currentSpeed = isGrounded ? moveSpeed : moveSpeed * airControlFactor;
        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rigidBody.velocity.y; // 保持原有Y轴速度

        // 使用VelocityChange进行更精确的速度控制
        Vector3 velocityDelta = targetVelocity - rigidBody.velocity;
        velocityDelta.y = 0; // Y轴由跳跃单独处理
        rigidBody.AddForce(velocityDelta, ForceMode.VelocityChange);
        
    }

    public float rotationSpeed;

    private void HandleRotation()
    {
        if (input.magnitude > 0.1f)
        {
            // 获取摄像头方向的平面旋转
            Quaternion targetRotation = Quaternion.LookRotation(input);

            // 平滑旋转
            rigidBody.rotation = Quaternion.RotateTowards(
                rigidBody.rotation, 
                targetRotation, 
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
    private bool DetectGround()
    {
        return Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1.0f,
            1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    }

    private void FixedUpdate()
    {
        isGrounded = DetectGround();
        HandleMovement();
        HandleRotation();

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