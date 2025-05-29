using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; 


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
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f; 
    [SerializeField] private float dashDuration = 0.2f; 
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        if (context.performed && isGrounded)
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
    void Update()
    {
        // Actualizamos el temporizador del cooldown del dash cada frame
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    void FixedUpdate()
    {
        CheckGround();
       
        if (!isDashing)
        {
            HandleMovement();
        }
    }

   
    void LateUpdate()
    {
        HandleLook();
    }
    // --- CORRUTINA DEL DASH ---
    private IEnumerator PerformDash()
    {
        // Iniciar estado de Dash
        isDashing = true;
        dashCooldownTimer = dashCooldown; // Iniciar el cooldown

        // La dirección del dash es hacia donde mira el jugador
        Vector3 dashDirection = transform.forward;

        // Aplicamos una fuerza instantánea que ignora la masa del objeto.
        // Es ideal para un dash que se sienta rápido y consistente.
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

        // Esperamos a que termine la duración del dash
        yield return new WaitForSeconds(dashDuration);

        // Opcional: Frenar bruscamente después del dash
        // rb.velocity = new Vector3(0, rb.velocity.y, 0);

        // Finalizar estado de Dash
        isDashing = false;
    }
    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y; 
        rb.linearVelocity = targetVelocity;
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
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
