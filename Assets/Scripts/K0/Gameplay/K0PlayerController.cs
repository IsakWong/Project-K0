using System.Collections;
using System.Collections.Generic;
using K1.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class K0PlayerController : ControllerBase, IAbsorbSource
{
    public float MaxWalSpeed = 5.0f;
    public float MaxRotateDegree = 360;
    public PlayerInput Input;

    public bool isshadow;
    private Rigidbody rigidBody;

    private float time = 0.0f;
    // private Animator animator;

    public Transform Center;

    private bool isRotate = false;

    private Vector3 velocity;

    private void Move(Vector2 move)
    {
        var s1 = move * MaxWalSpeed;
        velocity = rigidBody.velocity;
        velocity.x = s1.x;
        velocity.z = s1.y;
        //转向
        var cameraFoward = Camera.main.transform.rotation * velocity;
        cameraFoward.y = 0;
        velocity.x = cameraFoward.x;
        velocity.z = cameraFoward.z;
    }

    void Interation(InputAction.CallbackContext context)
    {
        if(waterCount > 0)
        {
            var colliders2 = Physics.OverlapSphere(transform.position, 1.0f, 1 << LayerMask.NameToLayer("Seed"),
                QueryTriggerInteraction.Collide);
            foreach (var collider in colliders2)
            {
                var item = collider.gameObject.GetComponent<Seed>();
                if (item is Seed)
                {
                    var seed = item as Seed;
                    if(seed.seedState == Seed.SeedState.OnGround)
                        seed.Grow();
                    return;
                }
            }
        }
        if (Current != null)
        {
            Current.Released(this);
            Current = null;
        }
        else
        {
            var colliders = Physics.OverlapSphere(transform.position, 1.0f, 1 << LayerMask.NameToLayer("Interactable"),
                QueryTriggerInteraction.Collide);
            foreach (var collider in colliders)
            {
                var item = collider.gameObject.GetComponent<InteractableItem>();
                if (item is IAbsorbTarget)
                {
                    var interactableItem = item as IAbsorbTarget;
                    if(!interactableItem.IsAbsorbed())
                        Absorb(interactableItem);
                    return;
                }
            }
            
        }
    }

    // Use this for initialization
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Input.actions.FindAction("Move").performed += ctx => Move(ctx.ReadValue<Vector2>());
        Input.actions.FindAction("Move").canceled += ctx => Move(ctx.ReadValue<Vector2>());
        Input.actions.FindAction("Jump").performed += Jump;
        Input.actions.FindAction("Interaction").performed += Interation;


        //animator = GetComponent<Animator>();
        // rigidbody = GetComponent<Rigidbody>();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (DetectGround())
        {
            rigidBody.AddForce(new Vector3(0, 5.0f, 0), ForceMode.VelocityChange);
            // animator.SetTrigger("Jump");
        }
    }

    private float _water;

    public float waterCount
    {
        get => _water;
        set
        {
            _water = value;
            float scale = _water / 5.0f + 1.0f;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private bool DetectGround()
    {
        return Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 1.0f,
            1 << LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore);
    }

    private void FixedUpdate()
    {
        var v = velocity;
        v.y = rigidBody.velocity.y;
        rigidBody.velocity = v;
        if (velocity.magnitude != 0)
        {
            RotateTowards(velocity, MaxRotateDegree * Time.fixedDeltaTime, true);
        }
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

    public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
    {
        Vector3 characterUp = transform.up;

        if (updateYawOnly)
            worldDirection = worldDirection.projectedOnPlane(characterUp);

        if (worldDirection == Vector3.zero)
            return;
        Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);
        transform.localRotation =
            Quaternion.RotateTowards(transform.localRotation, targetRotation, maxDegreesDelta);
    }
    public void OnTargetAbsorbed(IAbsorbTarget target)
    {
        if (target is Water)
        {
            var w = target as Water;
            waterCount = w.WaterAmount;
        }
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