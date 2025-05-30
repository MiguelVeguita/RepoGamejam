using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerAlt : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Punto vacío hijo del jugador donde se sostendrá el objeto.")]
    [SerializeField] private Transform holdPoint;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Mirada (Mouse)")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookUpAngle = 80f;
    [SerializeField] private float minLookDownAngle = -80f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Agarre de Objetos")]
    [Tooltip("La etiqueta que deben tener los objetos para ser agarrables.")]
    [SerializeField] private string grabbableTag; // Cambia esto si usas otra tag
    [SerializeField] private float dropForce = 5f;

    [Header("Chequeo de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // --- Componentes y variables internas ---
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private GameObject heldObject = null;
    private Rigidbody heldObjectRb = null;
    // private Collider heldObjectCollider = null; // No lo usaremos directamente por ahora

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        if (holdPoint == null)
        {
            Debug.LogError("Hold Point no está asignado en el PlayerController. Por favor, crea un objeto vacío hijo del jugador y asígnalo.", this);
            enabled = false;
        }
    }

    // --- MÉTODOS PÚBLICOS PARA EL INPUT ---
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
        if (context.performed && isGrounded && !isDashing)
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

    public void OnInteract(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            
                DropObject();
            
        }
    }

    // --- LÓGICA DE COLISIÓN PARA AGARRAR ---
    private void OnCollisionEnter(Collision collision)
    {
        // Solo agarramos si no tenemos ya un objeto y el objeto tiene la tag correcta
        if (heldObject == null && collision.gameObject.CompareTag(grabbableTag))
        {
            Debug.Log("Objeto agarrable colisionado: " + collision.gameObject.name);
            heldObject = collision.gameObject;
            heldObjectRb = heldObject.GetComponent<Rigidbody>();

            if (heldObjectRb != null)
            {
                heldObjectRb.isKinematic = true; // Hacerlo kinemático para que no le afecte la física mientras se lleva
            }

            // Emparentar y posicionar en el holdPoint
            heldObject.transform.SetParent(holdPoint);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity; // O una rotación específica si prefieres
        }
    }

    // --- LÓGICA PRINCIPAL ---
    void Update()
    {
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

    // --- MÉTODOS DE MOVIMIENTO Y ACCIONES ---
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

    private IEnumerator PerformDash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        Vector3 dashDirection = transform.forward;
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
        }
        float originalYVelocity = rb.linearVelocity.y;
        rb.linearVelocity = dashDirection * dashForce + Vector3.up * originalYVelocity;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null); // Quitarlo como hijo del holdPoint

        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false; // Devolverle la física
            // Aplicar una pequeña fuerza hacia adelante o simplemente dejarlo caer
            heldObjectRb.AddForce(cameraTransform.forward * dropForce, ForceMode.VelocityChange);
        }

        Debug.Log("Objeto soltado: " + heldObject.name);
        heldObject = null;
        heldObjectRb = null;
    }

    // Opcional: Dibujar Gizmos para debug
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null) { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius); }
        // No es necesario dibujar el rayo de agarre con este método
    }
}
