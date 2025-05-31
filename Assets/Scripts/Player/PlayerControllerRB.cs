using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System; // Necesario para Action y Func

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
    [SerializeField] private Transform groundCheck; // Origen del Raycast para chequear suelo
    // groundCheckRadius ya no se usará para CheckSphere, sino groundCheckDistance para Raycast
    [SerializeField] private float groundCheckDistance = 0.3f; // Distancia del Raycast
    [SerializeField] private LayerMask groundLayer;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    // --- NUEVAS VARIABLES PARA DESLIZAMIENTO ---
    [Header("Deslizamiento en Pendientes")]
    [SerializeField] private float minSlopeAngleToSlide = 30f; // Ángulo mínimo para empezar a deslizar
    [SerializeField] private float slideAcceleration = 8f;     // Fuerza de aceleración del deslizamiento
    // --- FIN NUEVAS VARIABLES PARA DESLIZAMIENTO ---

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    // --- NUEVAS VARIABLES DE ESTADO PARA DESLIZAMIENTO ---
    private Vector3 groundNormal; // Normal de la superficie del suelo
    private bool isSliding = false;   // Indica si el jugador está deslizando actualmente
    // --- FIN NUEVAS VARIABLES DE ESTADO PARA DESLIZAMIENTO ---

    // Variables de tu sistema de agarre (no se modifican)
    public static Action OnGrab;
    public static Action OnThrow;
    public delegate void GrabFunc();
    private GrabFunc grabFunc;
    public bool isPressed = false;
    bool grabbed = false;
    bool ongrab = false;
    public GameObject object_ref2;
    public static Func<GameObject> objectState;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // --- AÑADIDO: Inicializar groundNormal ---
        groundNormal = Vector3.up;
        // --- FIN AÑADIDO ---
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
        // --- MODIFICADO: Añadida la condición !isSliding ---
        if (context.performed && isGrounded && !isDashing && !isSliding)
        {
            HandleJump();
        }
        // --- FIN MODIFICADO ---
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && dashCooldownTimer <= 0f)
        {
            StartCoroutine(PerformDash());
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
        // --- MODIFICADO: CheckGround ahora también actualiza el estado de deslizamiento ---
        CheckGroundAndUpdateSlidingState();
        // --- FIN MODIFICADO ---

        // --- NUEVA LÓGICA: Si está deslizando, aplicar fuerza de deslizamiento ---
        if (isSliding)
        {
            HandleSliding();
        }
        // --- FIN NUEVA LÓGICA ---

        // --- MODIFICADO: Añadida la condición !isSliding para HandleMovement ---
        if (!isDashing && !isSliding)
        {
            HandleMovement();
        }
        // --- FIN MODIFICADO ---
    }

    // Tu método Getpressed (no se modifica)
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
        // Considera si el dash debería funcionar diferente si estás en el aire o deslizando
        // Por ahora, lo dejamos igual, pero si estás deslizando, el dash podría ser menos efectivo o cancelarlo.
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    // --- MÉTODO CheckGround MODIFICADO ---
    private void CheckGroundAndUpdateSlidingState()
    {
        RaycastHit hit;
        // Usamos el transform 'groundCheck' como el origen del rayo.
        // El rayo se lanza un poco hacia arriba del 'groundCheck.position' para evitar que empiece dentro del suelo.
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
            groundNormal = Vector3.up; // Si no está en el suelo, no hay normal de suelo y no desliza.
        }
    }
    // --- FIN MÉTODO CheckGround MODIFICADO ---

    private void HandleMovement() // Este método ahora solo se llama si !isDashing y !isSliding
    {
        Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.Normalize();

        Vector3 targetVelocity = moveDirection * moveSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;
    }

    // --- NUEVO MÉTODO: HandleSliding ---
    private void HandleSliding()
    {
        // Calcular la dirección del deslizamiento basada en la normal del suelo
        Vector3 slideDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

        // Calcular un factor de deslizamiento basado en qué tan empinada es la pendiente
        // más allá del ángulo mínimo para deslizar.
        float currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
        float slideRatio = (currentSlopeAngle - minSlopeAngleToSlide) / (90f - minSlopeAngleToSlide);
        slideRatio = Mathf.Clamp01(slideRatio); // Asegurar que esté entre 0 y 1

        // Aplicar la fuerza de deslizamiento.
        // ForceMode.Acceleration ignora la masa, aplicando una aceleración constante.
        rb.AddForce(slideDirection * slideAcceleration * slideRatio, ForceMode.Acceleration);

        // Opcional: Podrías querer reducir el control del jugador sobre el movimiento horizontal
        // mientras desliza, pero como HandleMovement() ya no se llama, esto ya sucede.
        // Si quisieras que el jugador tenga *algo* de influencia mientras desliza,
        // podrías aplicar una porción de su input aquí, o modificar HandleMovement.
    }
    // --- FIN NUEVO MÉTODO: HandleSliding ---

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
            Gizmos.color = Color.green; // Color del rayo de chequeo de suelo
            // Dibuja el rayo desde un poco arriba del groundCheck para que se vea mejor
            Vector3 rayGizmoOrigin = groundCheck.position + Vector3.up * 0.05f;
            Gizmos.DrawLine(rayGizmoOrigin, rayGizmoOrigin + Vector3.down * (groundCheckDistance + 0.05f));

            // Si está en el suelo, dibuja la normal del suelo
            if (isGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(groundCheck.position, groundCheck.position + groundNormal * 1f);
            }
        }
    }
}
