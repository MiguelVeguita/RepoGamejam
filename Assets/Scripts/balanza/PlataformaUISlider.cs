using UnityEngine;
using UnityEngine.UI; // ¡Muy importante incluir esto para poder usar Sliders!

public class PlataformaUISlider : MonoBehaviour
{
    [Header("Referencias UI")]
    // Arrastra tu componente Slider aquí desde el editor de Unity
    [Tooltip("El Slider de la UI que mostrará la inclinación.")]
    public Slider inclinacionSlider;

    [Header("Configuración de Inclinación")]
    [Tooltip("El ángulo máximo que la plataforma se inclinará. Este valor corresponderá a 0 y 100 en el slider.")]
    [SerializeField] private float maxAnguloInclinacion = 45f;

    [Tooltip("Elige sobre qué eje (X o Z) se inclina tu plataforma.")]
    [SerializeField] private Axis EjeDeInclinacion = Axis.Z;

    // Un enum para que sea fácil elegir el eje en el inspector.
    public enum Axis { X, Z }

    void Update()
    {
        // Seguridad: si no hemos asignado el slider, no hacemos nada para evitar errores.
        if (inclinacionSlider == null)
        {
            return;
        }

        // 1. OBTENER EL ÁNGULO ACTUAL DE LA PLATAFORMA
        float anguloActual = 0f;
        Vector3 rotacionActual = transform.eulerAngles;

        switch (EjeDeInclinacion)
        {
            case Axis.X:
                anguloActual = rotacionActual.x;
                break;
            case Axis.Z:
                anguloActual = rotacionActual.z;
                break;
        }

        // 2. CORREGIR EL RANGO DEL ÁNGULO
        if (anguloActual > 180f)
        {
            anguloActual -= 360f;
        }

        // 3. MAPEAR EL ÁNGULO AL VALOR DEL SLIDER (0-100)
        // --- ¡AQUÍ ESTÁ EL CAMBIO! ---
        // Invertimos el signo de 'anguloActual' para que el slider se mueva en la dirección opuesta.
        float valorNormalizado = Mathf.Clamp(-anguloActual / maxAnguloInclinacion, -1f, 1f);

        float valorSlider = 50f + (valorNormalizado * 50f);

        // 4. ASIGNAR EL VALOR FINAL AL SLIDER
        inclinacionSlider.value = valorSlider;
    }
}