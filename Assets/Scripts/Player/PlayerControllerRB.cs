// PlayerControllerAlt.cs

using UnityEngine;
using UnityEngine.InputSystem; // Asegúrate de tenerlo para Mouse.current
using System.Collections;
using System;

public class PlayerControllerAlt : MonoBehaviour
{
    [Header("Referencias")]
    // [SerializeField] private Transform cameraTransform; // Puede que ya no lo necesites para la rotación de cámara FPS
    [SerializeField] private Camera mainCamera; // << NUEVO: Asigna tu cámara principal aquí
    [SerializeField] private Animator animator;
    [SerializeField] private GrabObjects grabber;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float rotationSpeed = 720f; // Velocidad de rotación normal
    [SerializeField] private float mouseAimRotationSpeedFactor = 1.5f; // Factor para rotación más rápida con mouse

    // ... (resto de tus variables de Header sin cambios significativos) ...
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

    // Los eventos OnGrab y OnThrow siguen siendo útiles para que GrabObjects notifique al PlayerController
    // o a otros sistemas sobre el estado del agarre.
    public static Action OnGrab;
    public static Action OnThrow;
    public bool isPressed = false; // Revisar uso

    private int moveSpeedAnimHash;

    public static event Action OnLose;
    public static event Action<int> OnGrabSound;
    public static event Action<int> OnThrowSound;


    // << NUEVOS MÉTODOS PÚBLICOS ESTÁTICOS PARA INVOCAR EVENTOS >>
    public static void TriggerGrabSoundEvent(int soundId)
    {
        OnGrabSound?.Invoke(soundId);
    }

    public static void TriggerThrowSoundEvent(int soundId)
    {
        OnThrowSound?.Invoke(soundId);
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundNormal = Vector3.up;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError("Animator no encontrado en el Player o sus hijos.");
        else moveSpeedAnimHash = Animator.StringToHash("MovementSpeed");

        if (grabber == null)
        {
            Debug.LogError("Referencia a GrabObjects no asignada en PlayerControllerAlt.");
        }

        // Asignar cámara principal si no está en el inspector
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("Cámara principal no encontrada. Por favor, asígnala en PlayerControllerAlt.");
        }

        // Para la vista top-down, considera si quieres que el cursor sea visible siempre o solo al apuntar
        // Cursor.lockState = CursorLockMode.Confined; // Podría ser útil para que no se salga de la ventana
        // Cursor.visible = true; // O gestionarlo dinámicamente
    }

    // ... OnMove, OnLook (si se usa para algo), OnJump, OnDash ...
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // Ya no se usa para rotar al jugador estilo FPS.
        // Podría usarse para un cursor libre si fuera necesario.
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
    public void OnGrabDropInput(InputAction.CallbackContext context)
    {
        if (context.performed && grabber != null)
        {
            // El sonido de agarre ahora se invoca desde GrabObjects.TryGrab()
            grabber.ProcessGrabDropKey();
        }
    }
    public void OnAimInput(InputAction.CallbackContext context)
    {
        if (grabber == null) return;

        if (context.started)
        {
            grabber.StartAiming();
        }
        else if (context.canceled)
        {
            grabber.StopAiming();
        }
    }
    public void OnThrowInput(InputAction.CallbackContext context)
    {
        if (context.performed && grabber != null)
        {
            grabber.ProcessThrowKey(); // El sonido de lanzamiento ahora se invoca desde GrabObjects.PerformThrow()
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("lose")) // Usar CompareTag es más eficiente
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

        if (grabber != null && grabber.IsHoldingObject()) // Cambiado a IsHoldingObject para el cursor
        {
            // Opcional: Gestionar visibilidad del cursor
            // Cursor.visible = false; // O mostrar un retículo personalizado
            // Cursor.lockState = CursorLockMode.Confined; // Para que el mouse no se salga de la pantalla
            if (grabber.IsAimingActive()) // Si está activamente cargando (RMB presionado)
            {
                grabber.UpdateAimCharge(Time.deltaTime);
            }
        }
        // else
        // {
        // Opcional: Restaurar visibilidad del cursor
        // Cursor.visible = true;
        // Cursor.lockState = CursorLockMode.None;
        // }
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
            HandleMovementAndRotation(); // <<-- Aquí se aplicará la nueva lógica
        }
    }

    // ... GetPressed, PerformDash, CheckGroundAndUpdateSlidingState, HandleSliding, HandleJump, OnDrawGizmosSelected ...
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


    // << MÉTODO CENTRAL MODIFICADO >>
    private void HandleMovementAndRotation()
    {
        // --- Obtener Input de Movimiento (Teclas A,W,S,D) ---
        Vector3 worldMoveInput = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        Quaternion targetRotation;

        // --- Decidir Modo de Rotación ---
        bool playerIsHolding = grabber != null && grabber.IsHoldingObject();

        if (playerIsHolding) // Modo: Sosteniendo objeto -> Rotar con el mouse
        {
            if (mainCamera == null) return; // Seguridad

            Ray mouseRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Crear un plano a la altura del jugador para la intersección del rayo
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(mouseRay, out float distanceToPlane))
            {
                Vector3 mouseWorldPoint = mouseRay.GetPoint(distanceToPlane);
                Vector3 directionToMouse = (mouseWorldPoint - transform.position);
                directionToMouse.y = 0f; // Asegurar que la rotación sea solo en el plano XZ

                if (directionToMouse.sqrMagnitude > 0.01f) // Evitar rotar a Vector3.zero
                {
                    targetRotation = Quaternion.LookRotation(directionToMouse.normalized);
                    // Rotar un poco más rápido para que se sienta responsivo al mouse
                    rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * mouseAimRotationSpeedFactor * Time.fixedDeltaTime);
                    currentMoveDirection = directionToMouse.normalized; // Actualizar la dirección encarada
                }
            }
            // Si el rayo no impacta el plano (raro en top-down), no se actualiza la rotación por mouse.
        }
        else // Modo: Normal -> Rotar con las teclas de movimiento
        {
            if (worldMoveInput != Vector3.zero)
            {
                currentMoveDirection = worldMoveInput; // Guardar para el dash, etc.
                targetRotation = Quaternion.LookRotation(currentMoveDirection, Vector3.up);
                rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        // --- Aplicar Movimiento (Traslación basada en Teclas A,W,S,D) ---
        // El personaje se mueve en la dirección de las teclas, pero su cuerpo encara el mouse (si sostiene objeto) o la dirección de teclas.
        Vector3 targetVelocity = worldMoveInput * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z); // Preservar Y para saltos/gravedad

        // --- Animación ---
        if (animator != null)
        {
            float animationSpeed = Mathf.Clamp01(worldMoveInput.magnitude);
            animator.SetFloat(moveSpeedAnimHash, animationSpeed);
            // Podrías añadir un booleano al animator para "IsAiming" o "IsHolding"
            // animator.SetBool("IsHoldingObject", playerIsHolding);
        }
    }
}
