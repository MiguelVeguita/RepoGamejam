using UnityEngine;
using System; // Para Action

public class GrabObjects : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] GameObject point_ref; // Punto desde donde se calcula la trayectoria y se lanza
    [SerializeField] string[] grabbableTags = { "star", "ship", "moon", "alien", "saturn" };

    [Header("Throwing Mechanics")]
    [SerializeField] float baseLaunchSpeed = 15f; // Velocidad base del lanzamiento
    [SerializeField] float minLaunchAngle = 10f;  // Ángulo mínimo de lanzamiento (hacia arriba desde la horizontal)
    [SerializeField] float maxLaunchAngle = 75f;  // Ángulo máximo de lanzamiento
    [SerializeField] float aimChargeRate = 1f;    // Qué tan rápido se carga el ángulo/altura (unidades normalizadas por segundo)
    [SerializeField] int trajectorySegments = 30; // Número de puntos para el LineRenderer
    [SerializeField] Material trajectoryMaterial; // Material para el LineRenderer (opcional, puedes crear uno simple)
    [SerializeField] float trajectoryLineWidth = 0.1f;

    private GameObject currentTargetObject;
    private Rigidbody rb_currentTargetObject;

    private GameObject heldObject;
    private Rigidbody rb_heldObject;
    private bool isHoldingObject = false;

    // Nuevas variables para el apuntado
    private LineRenderer trajectoryLine;
    private bool isActivelyAiming = false; // True mientras RMB está presionado
    private float currentAimChargeNormalized = 0f; // Valor de 0 a 1 para la carga del ángulo

    void Awake()
    {
        // Configurar el LineRenderer para la trayectoria
        trajectoryLine = gameObject.AddComponent<LineRenderer>();
        trajectoryLine.positionCount = trajectorySegments;
        trajectoryLine.startWidth = trajectoryLineWidth;
        trajectoryLine.endWidth = trajectoryLineWidth;
        if (trajectoryMaterial != null)
        {
            trajectoryLine.material = trajectoryMaterial;
        }
        else
        {
            // Crear un material simple por defecto si no se asigna uno
            var defaultMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            trajectoryLine.material = defaultMaterial; // O usa un shader Unlit/Color
            trajectoryLine.startColor = Color.yellow; // Ejemplo
            trajectoryLine.endColor = Color.red;     // Ejemplo
        }
        trajectoryLine.enabled = false;
    }


    // Ya no nos suscribimos a PlayerControllerAlt.OnGrab/OnThrow de esta manera,
    // PlayerControllerAlt llamará a métodos públicos de esta clase directamente.
    // void OnEnable() { ... }
    // void OnDisable() { ... }

    private void OnTriggerEnter(Collider other)
    {
        if (isHoldingObject) return;

        foreach (string tag in grabbableTags)
        {
            if (other.gameObject.CompareTag(tag))
            {
                currentTargetObject = other.gameObject;
                rb_currentTargetObject = currentTargetObject.GetComponent<Rigidbody>();
                Debug.Log($"GrabObjects: {currentTargetObject.name} en rango para agarrar.");
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHoldingObject && currentTargetObject == other.gameObject)
        {
            Debug.Log($"GrabObjects: {currentTargetObject.name} salió del rango.");
            currentTargetObject = null;
            rb_currentTargetObject = null;
        }
    }

    // Llamado por PlayerControllerAlt cuando se presiona 'E'
    public void ProcessGrabDropKey()
    {
        if (isHoldingObject)
        {
            ForceDrop(); // Si está sosteniendo, suelta el objeto
        }
        else
        {
            TryGrab(); // Si no, intenta agarrar
        }
    }

    // Llamado por PlayerControllerAlt cuando se presiona RMB
    public void StartAiming()
    {
        if (isHoldingObject)
        {
            isActivelyAiming = true;
            currentAimChargeNormalized = 0f; // Reiniciar carga al empezar a apuntar
            trajectoryLine.enabled = true;
            Debug.Log("GrabObjects: Empezando a apuntar.");
        }
    }

    // Llamado por PlayerControllerAlt cuando se suelta RMB
    public void StopAiming()
    {
        if (isHoldingObject && isActivelyAiming)
        {
            isActivelyAiming = false;
            // La trayectoria se queda visible con la última carga hasta que se lance o se cancele.
            // Si quieres que desaparezca al soltar RMB: trajectoryLine.enabled = false;
            Debug.Log($"GrabObjects: Dejó de apuntar activamente. Carga final: {currentAimChargeNormalized}");
        }
    }

    // Llamado desde PlayerControllerAlt.Update si isActivelyAiming es true
    public void UpdateAimCharge(float deltaTime)
    {
        if (isHoldingObject && isActivelyAiming)
        {
            currentAimChargeNormalized = Mathf.Clamp01(currentAimChargeNormalized + aimChargeRate * deltaTime);
            DrawTrajectory();
        }
    }

    public bool IsAimingActive()
    {
        return isActivelyAiming;
    }

    // Llamado por PlayerControllerAlt cuando se presiona LMB
    public void ProcessThrowKey()
    {
        if (isHoldingObject) // Solo lanza si está sosteniendo algo
        {
            // No necesariamente necesita estar en "isActivelyAiming" para lanzar,
            // podría lanzar con la última carga almacenada o una carga por defecto si no se apuntó.
            // Por ahora, lanzará con la currentAimChargeNormalized (que será 0 si no se usó RMB).
            PerformThrow();
        }
        else
        {
            Debug.Log("GrabObjects: Intento de lanzamiento (LMB), pero no se está sosteniendo nada.");
        }
    }

    private void TryGrab()
    {
        if (currentTargetObject == null)
        {
            Debug.Log("GrabObjects: Intento de agarre (E), pero no hay currentTargetObject en rango.");
            return;
        }

        if (rb_currentTargetObject == null)
        {
            rb_currentTargetObject = currentTargetObject.GetComponent<Rigidbody>();
            if (rb_currentTargetObject == null)
            {
                Debug.LogError($"GrabObjects: {currentTargetObject.name} no tiene Rigidbody. No se puede agarrar.");
                return;
            }
        }

        heldObject = currentTargetObject;
        rb_heldObject = rb_currentTargetObject;
        isHoldingObject = true;

        heldObject.transform.SetParent(point_ref.transform);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
        rb_heldObject.isKinematic = true;

        Debug.Log($"GrabObjects: Objeto agarrado - {heldObject.name}");
        PlayerControllerAlt.OnGrab?.Invoke(); // Notificar que se agarró un objeto

        currentTargetObject = null;
        rb_currentTargetObject = null;
    }

    private void PerformThrow()
    {
        if (!isHoldingObject || heldObject == null) return;

        Debug.Log($"GrabObjects: Lanzando objeto {heldObject.name} con carga {currentAimChargeNormalized}");
        trajectoryLine.enabled = false;
        isActivelyAiming = false; // Ya no está apuntando activamente después de lanzar

        float launchAngle = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, currentAimChargeNormalized);
        Vector3 launchVelocity = CalculateLaunchVelocity(launchAngle, baseLaunchSpeed, point_ref.transform);

        heldObject.transform.SetParent(null);
        rb_heldObject.isKinematic = false;
        rb_heldObject.AddForce(launchVelocity, ForceMode.VelocityChange); // VelocityChange es bueno para consistencia

        PlayerControllerAlt.OnThrow?.Invoke(); // Notificar que se lanzó un objeto

        heldObject = null;
        rb_heldObject = null;
        isHoldingObject = false;
        currentAimChargeNormalized = 0f; // Resetear carga
    }

    private void ForceDrop()
    {
        if (heldObject != null)
        {
            Debug.Log($"GrabObjects: Soltando (drop) {heldObject.name}");
            trajectoryLine.enabled = false;
            isActivelyAiming = false;

            heldObject.transform.SetParent(null);
            if (rb_heldObject != null)
            {
                rb_heldObject.isKinematic = false;
                // Opcional: aplicar una pequeña fuerza hacia abajo o ninguna
                // rb_heldObject.AddForce(Vector3.down * 2f, ForceMode.Impulse);
            }
            PlayerControllerAlt.OnThrow?.Invoke(); // Notificar que se soltó un objeto
        }
        heldObject = null;
        rb_heldObject = null;
        isHoldingObject = false;
        currentAimChargeNormalized = 0f;
    }

    private Vector3 CalculateLaunchVelocity(float angle, float speed, Transform launchPoint)
    {
        // Rotar el vector 'forward' del punto de lanzamiento hacia arriba por 'angle' grados
        // alrededor del eje 'right' del punto de lanzamiento.
        Vector3 direction = Quaternion.AngleAxis(-angle, launchPoint.right) * launchPoint.forward;
        return direction.normalized * speed;
    }

    private void DrawTrajectory()
    {
        if (!isHoldingObject || !trajectoryLine.enabled)
        {
            if (trajectoryLine.enabled) trajectoryLine.enabled = false; // Asegurarse que esté apagado si no debe dibujarse
            return;
        }

        float currentActualLaunchAngle = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, currentAimChargeNormalized);
        Vector3 launchVel = CalculateLaunchVelocity(currentActualLaunchAngle, baseLaunchSpeed, point_ref.transform);

        trajectoryLine.positionCount = trajectorySegments;
        Vector3 currentSimPos = point_ref.transform.position;
        Vector3 currentSimVel = launchVel;
        float timeStep = 0.1f; // Ajusta este valor para la "densidad" de la línea

        for (int i = 0; i < trajectorySegments; i++)
        {
            trajectoryLine.SetPosition(i, currentSimPos);
            currentSimVel += Physics.gravity * timeStep; // Aplicar gravedad
            currentSimPos += currentSimVel * timeStep;   // Mover a la siguiente posición
        }
    }
}