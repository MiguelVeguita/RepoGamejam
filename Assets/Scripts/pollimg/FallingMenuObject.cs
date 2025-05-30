using UnityEngine;

public class FallingMenuObject : PoolObject // Aseg�rate que herede de PoolObject
{
    [Tooltip("Velocidad a la que el objeto caer� en unidades del mundo por segundo.")]
    public float fallSpeed = 2f;

    private Transform objectTransform; // Para objetos 3D
    private float worldBottomYBoundary; // L�mite inferior en coordenadas del mundo

    void Awake()
    {
        objectTransform = transform; // Obtener la referencia al Transform del objeto
    }

    /// <summary>
    /// Configura el l�mite inferior en el espacio del mundo para este objeto.
    /// Se llama cuando el objeto es activado por el MenuVisualEffectsManager.
    /// </summary>
    public void SetupWorldBoundary(float bottomY)
    {
        worldBottomYBoundary = bottomY;
    }

    void Update()
    {
        // Solo mover si el objeto est� activo
        if (gameObject.activeInHierarchy && objectTransform != null)
        {
            // Movimiento hacia abajo en el espacio del mundo
            objectTransform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            // La comprobaci�n de si ha salido del l�mite y la devoluci�n al pool
            // la manejar� completamente MenuVisualEffectsManager.cs.
            // Si quisieras que el objeto se auto-desactive (sin que el manager lo haga expl�citamente):
            // if (objectTransform.position.y < worldBottomYBoundary)
            // {
            //     // Esto podr�a ser problem�tico si el manager tambi�n intenta devolverlo.
            //     // Es mejor que el manager controle la devoluci�n.
            // }
        }
    }

    // Sobrescribir los m�todos de PoolObject si necesitas l�gica espec�fica al activar/desactivar
    public override void OnActivate()
    {
        base.OnActivate(); // Llama a la implementaci�n base (SetActive(true))
        // Ejemplo: Resetear rotaci�n o alguna propiedad espec�fica del objeto 3D
        // objectTransform.rotation = Quaternion.identity;
        // Debug.Log(gameObject.name + " activado desde el pool (3D).");
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate(); // Llama a la implementaci�n base (SetActive(false))
        // Debug.Log(gameObject.name + " devuelto al pool (3D).");
    }
}
