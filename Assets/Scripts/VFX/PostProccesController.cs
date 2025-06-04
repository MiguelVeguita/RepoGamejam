using UnityEngine;
using UnityEngine.Rendering; // Necesario para Volume
using UnityEngine.Rendering.Universal; // Necesario para los efectos específicos de URP como Vignette

public class PostProcessController : MonoBehaviour
{
    [Header("Configuración de Post-Procesamiento")]
    [Tooltip("Arrastra aquí el GameObject que contiene tu componente Volume global o principal.")]
    public Volume postProcessVolume;

    private Vignette vignetteEffect; // Referencia al efecto Vignette

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("Error: No se ha asignado un 'Post Process Volume' en el Inspector.", this);
            enabled = false; // Desactivar este script si no hay Volume asignado
            return;
        }

        // Intentamos obtener el efecto Vignette del profile del Volume.
        // Usar 'postProcessVolume.profile' crea una instancia del perfil en tiempo de ejecución si es necesario,
        // lo que evita modificar el asset original directamente.
        if (postProcessVolume.profile.TryGet<Vignette>(out vignetteEffect))
        {
            // ¡Efecto Vignette encontrado!
            // Puedes establecer un estado inicial si lo deseas, por ejemplo:
            // vignetteEffect.active = false; // Empezar con el efecto desactivado
        }
        else
        {
            Debug.LogWarning("Advertencia: El efecto 'Vignette' no se encontró en el Profile del Volume asignado. Asegúrate de haberlo añadido.", this);
        }
    }

    /// <summary>
    /// Activa o desactiva el efecto Vignette.
    /// </summary>
    /// <param name="isActive">True para activar, False para desactivar.</param>
    public void SetVignetteActive(bool isActive)
    {
        if (vignetteEffect != null)
        {
            vignetteEffect.active = isActive;
            Debug.Log("Efecto Vignette " + (isActive ? "activado." : "desactivado."));
        }
        else
        {
            Debug.LogWarning("No se puede modificar el Vignette porque no se encontró en el Profile.", this);
        }
    }

    // --- Ejemplo de uso: Activar/desactivar con una tecla ---
    void Update()
    {
        // Por ejemplo, presiona la tecla 'V' para alternar el Vignette
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (vignetteEffect != null)
            {
                SetVignetteActive(!vignetteEffect.active); // Alterna el estado actual
            }
        }
    }
}