using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class PlayerControllerAlt : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private GrabObjects grabber; // << AÑADIDO: Referencia a GrabObjects

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float rotationSpeed = 720f;

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
    private bool isGrounded;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    private Vector3 groundNormal;
    private bool isSliding = false;
    private Vector3 currentMoveDirection = Vector3.forward;

    // Eventos y delegados existentes
    public static Action OnGrab; // Invocado por GrabObjects cuando se agarra un objeto
    public static Action OnThrow; // Invocado por GrabObjects cuando se lanza/suelta un objeto
    public delegate void GrabFunc();
    private GrabFunc grabFunc;
    public bool isPressed = false; // Esta variable parece no usarse consistentemente con OnGrab/OnThrow, revisar su propósito.
    // bool grabbed = false; // El estado de 'grabbed' ahora lo manejará principalmente GrabObjects.
    // bool ongrab = false;
    public GameObject object_ref2;
    public static Func<GameObject> objectState;

    private int moveSpeedAnimHash;

    public static event Action OnLose;
    public static event Action<int> OnGrabSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundNormal = Vector3.up;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError("Animator no encontrado en el Player o sus hijos.");
        else moveSpeedAnimHash = Animator.StringToHash("MovementSpeed");

        // Asegúrate de que 'grabber' esté asignado en el Inspector
        if (grabber == null)
        {
            Debug.LogError("Referencia a GrabObjects no asignada en PlayerControllerAlt.");
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Lógica de Look si es necesaria para la cámara top-down o no se usa
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

    // Este método se llama con la tecla 'E'
    public void OnGrabDropInput(InputAction.CallbackContext context) // Renombrado de OnGrabAction
    {
        if (context.performed && grabber != null)
        {
            OnGrabSound?.Invoke(0); // Suponiendo que el índice 0 es el sonido de agarrar/soltar
            grabber.ProcessGrabDropKey();
        }
    }

    // << NUEVO: Método para la acción de Apuntar (Clic Derecho) >>
    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (grabber == null) return;

        if (context.started) // Botón presionado
        {
            grabber.StartAiming();
        }
        else if (context.canceled) // Botón soltado
        {
            grabber.StopAiming();
        }
    }

    // << NUEVO: Método para la acción de Lanzar (Clic Izquierdo) >>
    public void OnThrowInput(InputAction.CallbackContext context)
    {
        if (context.performed && grabber != null)
        {
            grabber.ProcessThrowKey();
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "lose")
        {
            Debug.Log("perderXD");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            OnLose?.Invoke();
        }
    }

    void Update()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Si GrabObjects necesita actualizar el apuntado continuamente mientras se mantiene presionado RMB:
        if (grabber != null && grabber.IsAimingActive())
        {
            grabber.UpdateAimCharge(Time.deltaTime); // Pasa el tiempo para el incremento
        }
    }

    void FixedUpdate()
    {
        CheckGroundAndUpdateSlidingState();

        if (isSliding)
        {
            HandleSliding();
        }
        else if (!isDashing)
        {
            HandleMovementAndRotation();
        }
    }

    public bool Getpressed() => isPressed;

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        Vector3 dashDirection = (currentMoveDirection != Vector3.zero && moveInput.sqrMagnitude > 0.1f) ? currentMoveDirection : transform.forward;
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
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
            isSliding = slopeAngle > minSlopeAngleToSlide;
        }
        else
        {
            isGrounded = false;
            isSliding = false;
            groundNormal = Vector3.up;
        }
    }

    private void HandleMovementAndRotation()
    {
        Vector3 worldMoveInput = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (worldMoveInput != Vector3.zero)
        {
            currentMoveDirection = worldMoveInput;
            Quaternion targetRotation = Quaternion.LookRotation(currentMoveDirection, Vector3.up);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        Vector3 targetVelocity = worldMoveInput * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        if (animator != null)
        {
            float animationSpeed = Mathf.Clamp01(worldMoveInput.magnitude);
            animator.SetFloat(moveSpeedAnimHash, animationSpeed);
        }
    }

    private void HandleSliding()
    {
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
        float currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        float slideRatio = Mathf.Clamp01((currentSlopeAngle - minSlopeAngleToSlide) / (90f - minSlopeAngleToSlide));
        rb.AddForce(slideDirection * slideAcceleration * slideRatio, ForceMode.Acceleration);
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
