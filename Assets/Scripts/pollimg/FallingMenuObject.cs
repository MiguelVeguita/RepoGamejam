using UnityEngine;

public class FallingMenuObject : PoolObject // Asegúrate que herede de PoolObject
{
    [Tooltip("Velocidad a la que el objeto caerá en unidades del mundo por segundo.")]
    public float fallSpeed = 2f;

    private Transform objectTransform; // Para objetos 3D
    private float worldBottomYBoundary; // Límite inferior en coordenadas del mundo

    void Awake()
    {
        objectTransform = transform; // Obtener la referencia al Transform del objeto
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
        // Solo mover si el objeto está activo
        if (gameObject.activeInHierarchy && objectTransform != null)
        {
            // Movimiento hacia abajo en el espacio del mundo
            objectTransform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            // La comprobación de si ha salido del límite y la devolución al pool
            // la manejará completamente MenuVisualEffectsManager.cs.
            // Si quisieras que el objeto se auto-desactive (sin que el manager lo haga explícitamente):
            // if (objectTransform.position.y < worldBottomYBoundary)
            // {
            //     // Esto podría ser problemático si el manager también intenta devolverlo.
            //     // Es mejor que el manager controle la devolución.
            // }
        }
    }

    // Sobrescribir los métodos de PoolObject si necesitas lógica específica al activar/desactivar
    public override void OnActivate()
    {
        base.OnActivate(); // Llama a la implementación base (SetActive(true))
        // Ejemplo: Resetear rotación o alguna propiedad específica del objeto 3D
        // objectTransform.rotation = Quaternion.identity;
        // Debug.Log(gameObject.name + " activado desde el pool (3D).");
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate(); // Llama a la implementación base (SetActive(false))
        // Debug.Log(gameObject.name + " devuelto al pool (3D).");
    }
}
