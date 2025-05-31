using UnityEngine;

public class FallingMenuObject : PoolObject // Asegúrate que herede de PoolObject
{
    [Tooltip("Velocidad a la que el objeto caerá en unidades del mundo por segundo.")]
    public float fallSpeed = 2f;

    private Transform objectTransform; // Para objetos 3D
    private Rigidbody rb; // Referencia al Rigidbody
    private float worldBottomYBoundary; // Límite inferior en coordenadas del mundo

    void Awake()
    {
        objectTransform = transform; // Obtener la referencia al Transform del objeto
        rb = GetComponent<Rigidbody>(); // Obtener el Rigidbody
    }

    /// <summary>
    /// Configura el límite inferior en el espacio del mundo para este objeto.
    /// Se llama cuando el objeto es activado por el MenuVisualEffectsManager.
    /// </summary>
    public void SetupWorldBoundary(float bottomY)
    {
        worldBottomYBoundary = bottomY;
    }

    void Update()
    {
        if (gameObject.activeInHierarchy && objectTransform != null)
        {
            // Si el Rigidbody NO es kinemático, la gravedad debería moverlo.
            // Si es kinemático o no tiene gravedad, lo movemos manualmente.
            if (rb != null && rb.isKinematic || rb == null || (rb !=null && !rb.useGravity) )
            {
                 objectTransform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            }
            // Si usa gravedad y no es kinemático, el motor de física se encarga de la caída.
            // La comprobación de límites la hará el manager.
        }
    }

    public override void OnActivate()
    {
        base.OnActivate(); // Llama a la implementación base (SetActive(true))
        
        // Resetear estado para "respawn" como si fuera nuevo
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Detener cualquier movimiento anterior
            rb.angularVelocity = Vector3.zero; // Detener cualquier rotación anterior
        }
        // Aquí puedes resetear otras propiedades específicas si es necesario
        // objectTransform.rotation = Quaternion.identity; // O una rotación inicial específica
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate(); // Llama a la implementación base (SetActive(false))
    }
}
