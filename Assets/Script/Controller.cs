using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour, IObtainer
{
    public float walk_speed;
    public float rotation_speed;
    public PlayerInput Input;

    public bool isshadow;
    private Rigidbody rigidBody;

    private float time = 0.0f;
    // private Animator animator;

    private Vector3 direction;
    private Vector3 v = Vector3.zero;
    private float m_TurnAmount = 0;
    public Transform Center;

    private bool isRotate = false;

    private Vector3 velocity;
    private void Move(Vector2 move)
    {
        var s1 = move * walk_speed;
        velocity = rigidBody.velocity;
        velocity.x = s1.x;
        velocity.z = s1.y;
        //转向
        var cameraFoward = Camera.main.transform.rotation * velocity;
        cameraFoward.y = 0;
        velocity.x = cameraFoward.x;
        velocity.z = cameraFoward.z;

        direction = -transform.InverseTransformDirection(move);
        m_TurnAmount = Mathf.Atan2(direction.x, direction.z);

        if (direction.magnitude != 0)
        {
            isRotate = true;
        }
        else
        {
            isRotate = false;
        }
    }

    void Interation(InputAction.CallbackContext context)
    {
        if (Current != null)
        {
            Current.Released(this);
            Current = null;
        }
        else
        {
            var colliders = Physics.OverlapSphere(transform.position, 1.0f, 1 << LayerMask.NameToLayer("Interactable"), QueryTriggerInteraction.Collide);
            foreach (var collider in colliders)
            {
                var item = collider.gameObject.GetComponent<InteractableItem>();
                if(item is Seed)
                {
                    var seed = item as Seed;         
                    Obtain(seed);
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
        Input.actions.FindAction("Interaction").performed += Interation;

        
        //animator = GetComponent<Animator>();
        // rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rigidBody != null)
            rigidBody.WakeUp();
    }

    // Update is called once per frame
    void Update()
    {
        rigidBody.velocity = velocity;
        if (isRotate)
        {
            Vector3 d_w = this.transform.TransformDirection(new Vector3(0, 0, 1));
            Vector3 temp = Vector3.SmoothDamp(d_w, direction, ref v, 1f);


            this.transform.Rotate(0, m_TurnAmount * rotation_speed * Time.deltaTime * 50.0f, 0);
        }


        time += Time.deltaTime;

        if (time > 2.0f)
        {
            // m_rigidbody.AddForce(new Vector3(0, 300.0f, 0));
            // Debug.Log("jump");

            time = 0;
        }
    }

    private IObtainTarget Current;
    public void Obtain(IObtainTarget target)
    {
        if(Current != null)
            Current.Released(this);
        Current = target;
        if(Current != null)
            Current.GetObtained(this);
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