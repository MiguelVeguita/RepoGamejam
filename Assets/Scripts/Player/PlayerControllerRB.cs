using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System; 

public class PlayerControllerAlt : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Mirada (Mouse)")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookUpAngle = 80f;
    [SerializeField] private float minLookDownAngle = -80f;

    [Header("Chequeo de Suelo")]
    [SerializeField] private Transform groundCheck;
    
    [SerializeField] private float groundCheckDistance = 0.3f; 
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Deslizamiento en Pendientes")]
    [SerializeField] private float minSlopeAngleToSlide = 30f; 
    [SerializeField] private float slideAcceleration = 8f;   

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

   
    private Vector3 groundNormal; 
    private bool isSliding = false;  

    public static Action OnGrab;
    public static Action OnThrow;
    public delegate void GrabFunc();
    private GrabFunc grabFunc;
    public bool isPressed = false;
    bool grabbed = false;
    bool ongrab = false;
    public GameObject object_ref2;
    public static Func<GameObject> objectState;


    public static event Action OnLose;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        groundNormal = Vector3.up;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {

        if (context.performed && isGrounded && !isDashing && !isSliding)
        {
            HandleJump();
        }

    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && dashCooldownTimer <= 0f)
        {
            StartCoroutine(PerformDash());
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "lose")
        {
            Debug.Log("perderXD");
            OnLose?.Invoke();

        }
    }
    public void OnGrabAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            grabbed = !grabbed;
            if (grabbed == true)
            {
                OnGrab?.Invoke();
            }
            else
            {
                OnThrow?.Invoke();
            }
            Debug.Log("funcionaxd");
        }
    }

    void Update()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
       
        CheckGroundAndUpdateSlidingState();

        if (isSliding)
        {
            HandleSliding();
        }
      
        if (!isDashing && !isSliding)
        {
            HandleMovement();
        }

    }

 
    public bool Getpressed()
    {
        return isPressed;
    }

    void LateUpdate()
    {
        HandleLook();
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        Vector3 dashDirection = transform.forward;
      
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

  
    private void CheckGroundAndUpdateSlidingState()
    {
        RaycastHit hit;
      
        Vector3 rayOrigin = groundCheck.position + Vector3.up * 0.05f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance + 0.05f, groundLayer))
        {
            isGrounded = true;
            groundNormal = hit.normal;

            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            if (slopeAngle > minSlopeAngleToSlide)
            {
                isSliding = true;
            }
            else
            {
                isSliding = false;
            }
        }
        else
        {
            isGrounded = false;
            isSliding = false;
            groundNormal = Vector3.up;
        }
    }
 

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }


    private void HandleSliding()
    {

        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        float currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        float slideRatio = (currentSlopeAngle - minSlopeAngleToSlide) / (90f - minSlopeAngleToSlide);
        slideRatio = Mathf.Clamp01(slideRatio); 
        rb.AddForce(slideDirection * slideAcceleration * slideRatio, ForceMode.Acceleration);
    }


    private void HandleLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookDownAngle, maxLookUpAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void HandleJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green; 
            Vector3 rayGizmoOrigin = groundCheck.position + Vector3.up * 0.05f;
            Gizmos.DrawLine(rayGizmoOrigin, rayGizmoOrigin + Vector3.down * (groundCheckDistance + 0.05f));
            if (isGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + groundNormal * 1f);
            }
        }
    }
}
