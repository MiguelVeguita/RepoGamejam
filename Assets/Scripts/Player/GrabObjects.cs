// GrabObjects.cs

using UnityEngine;
using System;
using System.Collections;

public class GrabObjects : MonoBehaviour
{
    // ... (tus variables existentes: point_ref, grabbableTags, mec�nicas de lanzamiento, etc.) ...
    [Header("Setup")]
    [SerializeField] GameObject point_ref;
    [SerializeField] string[] grabbableTags = { "star", "ship", "moon", "alien", "saturn" };

    [Header("Throwing Mechanics")]
    [SerializeField] float baseLaunchSpeed = 15f;
    [SerializeField] float minLaunchAngle = 10f;
    [SerializeField] float maxLaunchAngle = 75f;
    [SerializeField] float aimChargeRate = 1f;
    [SerializeField] int trajectorySegments = 30;
    [SerializeField] Material trajectoryMaterial;
    [SerializeField] float trajectoryLineWidth = 0.1f;
    [SerializeField] float trajectoryRetractionTime = 0.2f;
    private Coroutine retractionCoroutine;

    private GameObject currentTargetObject;
    private Rigidbody rb_currentTargetObject;

    private GameObject heldObject;
    private Rigidbody rb_heldObject;
    private bool isHoldingObject = false; // <<-- ESTA ES LA IMPORTANTE

    private LineRenderer trajectoryLine;
    private bool isActivelyAiming = false; // True mientras RMB est� presionado (para cargar y mostrar trayectoria)
    private float currentAimChargeNormalized = 0f;


    void Awake()
    {
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
            var defaultMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            trajectoryLine.material = defaultMaterial;
            trajectoryLine.startColor = Color.yellow;
            trajectoryLine.endColor = Color.red;
        }
        trajectoryLine.enabled = false;
    }

    // ... OnTriggerEnter, OnTriggerExit ... (sin cambios respecto a tu �ltima versi�n)
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
            Debug.Log($"GrabObjects: {currentTargetObject.name} sali� del rango.");
            currentTargetObject = null;
            rb_currentTargetObject = null;
        }
    }


    // << NUEVO M�TODO P�BLICO >>
    /// <summary>
    /// Devuelve true si el jugador est� actualmente sosteniendo un objeto.
    /// </summary>
    public bool IsHoldingObject()
    {
        return isHoldingObject;
    }

    // Este m�todo ya lo ten�as para saber si RMB est� presionado
    public bool IsAimingActive()
    {
        return isActivelyAiming; // Esto es para la carga y visualizaci�n de la trayectoria
    }
    private IEnumerator RetractTrajectory()
    {
        float timer = trajectoryRetractionTime;
        int initialSegments = trajectoryLine.positionCount;

        // Mientras dure el tiempo del efecto
        while (timer > 0f)
        {
            // Reduce el n�mero de puntos de la l�nea de forma proporcional al tiempo
            trajectoryLine.positionCount = Mathf.RoundToInt(Mathf.Lerp(0, initialSegments, timer / trajectoryRetractionTime));
            timer -= Time.deltaTime;
            yield return null; // Espera al siguiente frame
        }

        // Limpieza final
        trajectoryLine.enabled = false;
    }

    // ... ProcessGrabDropKey, StartAiming, StopAiming, UpdateAimCharge, ProcessThrowKey ...
    // ... TryGrab, PerformThrow, ForceDrop, CalculateLaunchVelocity, DrawTrajectory ...
    // (El resto de tus m�todos en GrabObjects.cs de la versi�n anterior se mantienen igual)
    public void ProcessGrabDropKey()
    {
        if (isHoldingObject)
        {
            ForceDrop();
        }
        else
        {
            TryGrab();
        }
    }
    // GrabObjects.cs

    public void StartAiming()
    {
        if (isHoldingObject)
        {
            // Si hay una coroutine de retroceso en marcha, la detenemos
            if (retractionCoroutine != null)
            {
                StopCoroutine(retractionCoroutine);
                retractionCoroutine = null;
            }

            isActivelyAiming = true;
            currentAimChargeNormalized = 0f;
            trajectoryLine.enabled = true;
            // Aseguramos que la l�nea tenga todos sus segmentos al empezar a apuntar
            trajectoryLine.positionCount = trajectorySegments;
            Debug.Log("GrabObjects: Empezando a apuntar.");
        }
    }

    public void StopAiming()
    {
        if (isHoldingObject && isActivelyAiming)
        {
            isActivelyAiming = false;

            // En lugar de ocultar la l�nea directamente, iniciamos la coroutine de retroceso
            retractionCoroutine = StartCoroutine(RetractTrajectory()); // <<-- L�NEA MODIFICADA

            Debug.Log($"GrabObjects: Dej� de apuntar activamente. Carga final: {currentAimChargeNormalized}");
        }
    }
    public void UpdateAimCharge(float deltaTime)
    {
        if (isHoldingObject && isActivelyAiming)
        {
            currentAimChargeNormalized = Mathf.Clamp01(currentAimChargeNormalized + aimChargeRate * deltaTime);
            DrawTrajectory();
        }
    }
    public void ProcessThrowKey()
    {
        if (isHoldingObject)
        {
            PerformThrow();
        }
        else
        {
            Debug.Log("GrabObjects: Intento de lanzamiento (LMB), pero no se est� sosteniendo nada.");
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
        PlayerControllerAlt.OnGrab?.Invoke();
        PlayerControllerAlt.TriggerGrabSoundEvent(0); // <<-- MODIFICADO AQU�

        currentTargetObject = null;
        rb_currentTargetObject = null;
    }

    private void PerformThrow()
    {
        if (!isHoldingObject || heldObject == null) return;

        Debug.Log($"GrabObjects: Lanzando objeto {heldObject.name} con carga {currentAimChargeNormalized}");
        trajectoryLine.enabled = false;
        isActivelyAiming = false;

        float launchAngle = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, currentAimChargeNormalized);
        Vector3 launchVelocity = CalculateLaunchVelocity(launchAngle, baseLaunchSpeed, point_ref.transform);

        heldObject.transform.SetParent(null);
        rb_heldObject.isKinematic = false;
        rb_heldObject.AddForce(launchVelocity, ForceMode.VelocityChange);

        PlayerControllerAlt.OnThrow?.Invoke();
        PlayerControllerAlt.TriggerThrowSoundEvent(4); // <<-- MODIFICADO AQU�
        TrailRenderer trail = heldObject.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.enabled = true;
        }
        heldObject = null;
        rb_heldObject = null;
        isHoldingObject = false;
        currentAimChargeNormalized = 0f;
    }


    private void ForceDrop()
    {
        if (heldObject != null)
        {
            Debug.Log($"GrabObjects: Soltando (drop) {heldObject.name}");
            trajectoryLine.enabled = false; // Ocultar trayectoria al dropear
            isActivelyAiming = false;

            heldObject.transform.SetParent(null);
            if (rb_heldObject != null)
            {
                rb_heldObject.isKinematic = false;
            }
            PlayerControllerAlt.OnThrow?.Invoke(); // Sigue siendo un "throw" en t�rminos de evento
            // Considera si ForceDrop tambi�n deber�a tener un sonido. Podr�a ser el mismo OnThrowSound
            // o uno nuevo espec�fico para "drop". Por ahora, no se a�ade sonido aqu� seg�n el requerimiento.
        }
        heldObject = null;
        rb_heldObject = null;
        isHoldingObject = false; // <--- Importante actualizar estado
        currentAimChargeNormalized = 0f;
    }
    private Vector3 CalculateLaunchVelocity(float angle, float speed, Transform launchPoint)
    {
        // ANTERIOR:
        // Vector3 direction = Quaternion.AngleAxis(-angle, launchPoint.right) * launchPoint.forward;

        // CORRECCI�N:
        // Usamos el transform del jugador (o el del propio GrabObjects si est� en el jugador)
        // para obtener la direcci�n "adelante" correcta.
        Vector3 forwardDirection = transform.forward;
        Vector3 upwardDirection = transform.up;

        // Rotamos el vector "adelante" hacia arriba seg�n el �ngulo de lanzamiento.
        // El eje de rotaci�n ahora es el eje "derecha" del jugador.
        Vector3 direction = Quaternion.AngleAxis(-angle, transform.right) * forwardDirection;

        return direction.normalized * speed;
    }

    private void DrawTrajectory()
    {
        if (!isHoldingObject || !trajectoryLine.enabled) // Solo dibujar si se est� sosteniendo Y la l�nea est� habilitada (por StartAiming)
        {
            if (trajectoryLine.enabled) trajectoryLine.enabled = false;
            return;
        }

        float currentActualLaunchAngle = Mathf.Lerp(minLaunchAngle, maxLaunchAngle, currentAimChargeNormalized);
        Vector3 launchVel = CalculateLaunchVelocity(currentActualLaunchAngle, baseLaunchSpeed, point_ref.transform);

        trajectoryLine.positionCount = trajectorySegments;
        Vector3 currentSimPos = point_ref.transform.position;
        Vector3 currentSimVel = launchVel;
        float timeStep = 0.1f; // Puedes ajustar este valor

        for (int i = 0; i < trajectorySegments; i++)
        {
            trajectoryLine.SetPosition(i, currentSimPos);
            currentSimVel += Physics.gravity * timeStep; // Aplicar gravedad
            currentSimPos += currentSimVel * timeStep;   // Calcular siguiente posici�n
        }
    }
}