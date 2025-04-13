using K1.Gameplay;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace K3
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class K3PlayerController : ControllerBase
    {
        [Header("Movement Settings")] [SerializeField]
        private float moveSpeed = 5f;

        public PlayerInput mInput;

        private InputAction moveXAction;
        private InputAction moveZAction;

        public Rigidbody2D rb;
        private Vector2 currentInput;


        private void OnEnable()
        {
            mInput.SwitchCurrentActionMap("Default");
            moveXAction = mInput.actions.FindAction("MoveX");
            moveZAction = mInput.actions.FindAction("MoveZ");
            moveXAction.performed += HandleXInput;
            moveXAction.canceled += HandleXInput;
            moveZAction.performed += HandleZInput;
            moveZAction.canceled += HandleZInput;
        }

        private void OnDisable()
        {
            moveXAction.Disable();
            moveZAction.Disable();

            moveXAction.performed -= HandleXInput;
            moveXAction.canceled -= HandleXInput;
            moveZAction.performed -= HandleZInput;
            moveZAction.canceled -= HandleZInput;
        }

        private void HandleXInput(InputAction.CallbackContext context)
        {
            currentInput.x = context.ReadValue<float>();
        }

        private void HandleZInput(InputAction.CallbackContext context)
        {
            currentInput.y = context.ReadValue<float>();
        }

        
        private void CalculateTargetVelocity()
        {
            // 计算目标速度（输入方向 * 移动速度）
            targetVelocity = currentInput.normalized * moveSpeed;
        }

        private void Update()
        {
            CalculateTargetVelocity();
        }
        
        private void FixedUpdate()
        {
            MoveCharacter();
        }
        
        [SerializeField] private float inputSmoothing = 0.1f;
        private Vector2 smoothInputVelocity;
        
        
        [Header("Inertia Settings")]
        [SerializeField] private float deceleration = 5f;    // 减速度系数
        [SerializeField] private float velocitySmoothing = 0.05f; // 速度平滑时间

        private Vector2 targetVelocity;
        private Vector2 currentVelocityRef;
        private void MoveCharacter()
        {
            currentInput = Vector2.SmoothDamp(
                currentInput, 
                new Vector2(
                    moveXAction.ReadValue<float>(),
                    moveZAction.ReadValue<float>()
                ),
                ref smoothInputVelocity, 
                inputSmoothing
            );
            
            
            if (currentInput.magnitude > 0.1f)
            {
                rb.velocity = Vector2.SmoothDamp(
                    rb.velocity,
                    targetVelocity,
                    ref currentVelocityRef,
                    velocitySmoothing
                );
            }
            else
            {
                // 应用指数衰减阻尼公式：v = v * (1 - deceleration * Time.deltaTime)
                rb.velocity *= Mathf.Clamp01(1 - deceleration * Time.fixedDeltaTime);
            
                // 当速度小于阈值时直接归零
                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }
    }
}