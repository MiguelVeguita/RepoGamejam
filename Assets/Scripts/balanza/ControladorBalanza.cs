using System;
using UnityEngine;

public class ControladorBalanza : MonoBehaviour
{
    [Header("Pesos de las Cajas")]
    public float pesoCajaA = 0f;
    public float pesoCajaB = 0f;

    [Header("Configuración de la Balanza")]
    [SerializeField] private float sensibilidadInclinacion = 5f;
    [SerializeField] private float anguloMaximoPeligro = 30f;
    [SerializeField] private float velocidadRotacionSuave = 5f;
    [SerializeField] private Vector3 ejeDeRotacionLocal = Vector3.forward;
    [SerializeField] private GameObject HUD;

    [Header("Condición de Victoria por Tiempo")] // <<-- SECCIÓN MODIFICADA
    [Tooltip("El tiempo en segundos reales que el jugador debe sobrevivir para ganar.")]
    [SerializeField] private float duracionParaGanar = 30f; // <<-- NUEVA VARIABLE

    // <<-- Variables públicas para que el UI pueda leer el estado del temporizador -->>
    public float TiempoTranscurrido { get; private set; } = 0f;
    public float DuracionTotal => duracionParaGanar; // Forma corta de exponer una variable privada

    private Quaternion rotacionInicial;
    private float anguloInclinacionActual = 0f;
    private bool victoriaAlcanzada = false;
    private bool juegoTerminado = false; // <<-- NUEVO: Para detener el timer si se pierde

    public static event Action OnEquilibrioPerdido;
    public static event System.Action<float> OnVictoriaAlcanzada;

    void Start()
    {
        rotacionInicial = transform.localRotation;
        ejeDeRotacionLocal.Normalize();
        HUD.SetActive(true);
    }

    private void OnEnable()
    {
        Boxes.OnACollisionWeight += AnadirPesoACajaA;
        Boxes.OnBCollisionWeight += AnadirPesoACajaB;
        OnEquilibrioPerdido += DetenerJuego; // <<-- NUEVO: Suscribirse a su propio evento
    }

    private void OnDisable()
    {
        Boxes.OnACollisionWeight -= AnadirPesoACajaA;
        Boxes.OnBCollisionWeight -= AnadirPesoACajaB;
        OnEquilibrioPerdido -= DetenerJuego; // <<-- NUEVO
    }

    void Update()
    {
        // Si el juego ha terminado (por victoria o derrota), no hacemos nada más.
        if (juegoTerminado)
        {
            HUD.SetActive(false);
            return;
        }

        // --- LÓGICA DE LA BALANZA (SIN CAMBIOS) ---
        float diferenciaDePeso = pesoCajaA - pesoCajaB;
        float anguloInclinacionObjetivo = diferenciaDePeso * sensibilidadInclinacion;
        Quaternion rotacionObjetivo = rotacionInicial * Quaternion.AngleAxis(anguloInclinacionObjetivo, ejeDeRotacionLocal);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionObjetivo, Time.deltaTime * velocidadRotacionSuave);
        anguloInclinacionActual = anguloInclinacionObjetivo;

        // --- LÓGICA DE PÉRDIDA (SIN CAMBIOS) ---
        if (Mathf.Abs(anguloInclinacionActual) > anguloMaximoPeligro)
        {
            Debug.LogWarning("¡EQUILIBRIO PERDIDO! La balanza se inclinó demasiado.");
            OnEquilibrioPerdido?.Invoke();
            HUD.SetActive(false);
        }

        // --- NUEVA LÓGICA DE VICTORIA POR TIEMPO ---
        TiempoTranscurrido += Time.deltaTime;
        if (TiempoTranscurrido >= duracionParaGanar)
        {
            victoriaAlcanzada = true;
            juegoTerminado = true;
            Debug.Log("¡VICTORIA! Se ha sobrevivido durante " + duracionParaGanar + " segundos.");

            // Calcula el peso total
            float pesoTotal = pesoCajaA + pesoCajaB;

            // Invoca el evento y "envía" el pesoTotal junto a él
            OnVictoriaAlcanzada?.Invoke(pesoTotal); // <<-- MODIFICADO
            HUD.SetActive(false);
            // Desactivar la lógica de añadir peso para que se quede estable
            enabled = false;
        }
    }

    // <<-- NUEVO MÉTODO -->>
    private void DetenerJuego()
    {
        juegoTerminado = true;
        // Opcional: Desactivar el script para congelar la balanza en su estado de caída
        // enabled = false;
    }

    // Ya no comprueban la victoria, solo acumulan peso
    public void AnadirPesoACajaA(int cantidad)
    {
        pesoCajaA += cantidad;
        Debug.Log($"Añadido {cantidad} a Caja A. Nuevo peso: {pesoCajaA}");
    }

    public void AnadirPesoACajaB(int cantidad)
    {
        pesoCajaB += cantidad;
        Debug.Log($"Añadido {cantidad} a Caja B. Nuevo peso: {pesoCajaB}");
    }
    // (Opcional) Método para resetear la balanza
    public void ResetearBalanza()
    {
        pesoCajaA = 0f;
        pesoCajaB = 0f;
        transform.localRotation = rotacionInicial;
        anguloInclinacionActual = 0f;
        victoriaAlcanzada = false;
        enabled = true; // Reactivar el script si fue desactivado
        Debug.Log("Balanza reseteada.");
    }

    // Dibujar Gizmos en el editor para visualizar el eje de rotación y los límites (opcional)
    void OnDrawGizmosSelected()
    {
        if (ejeDeRotacionLocal == Vector3.zero) return;

        Gizmos.color = Color.red;
        // Dibuja una línea para el eje de rotación
        Gizmos.DrawLine(transform.position - transform.TransformDirection(ejeDeRotacionLocal) * 1f, transform.position + transform.TransformDirection(ejeDeRotacionLocal) * 1f);

        // Visualizar los ángulos de peligro (aproximado)
        Gizmos.color = Color.yellow;
        Quaternion rotPeligroPos = rotacionInicial * Quaternion.AngleAxis(anguloMaximoPeligro, ejeDeRotacionLocal);
        Quaternion rotPeligroNeg = rotacionInicial * Quaternion.AngleAxis(-anguloMaximoPeligro, ejeDeRotacionLocal);

        // Vector que representa el "arriba" de la plataforma de la balanza
        Vector3 upDeLaPlataforma = Vector3.up; // Asume que la plataforma es horizontal cuando está equilibrada
                                               // Si tu modelo de balanza tiene una plataforma que no es 'up', ajusta este vector.

        Gizmos.DrawRay(transform.position, transform.TransformDirection(rotPeligroPos * upDeLaPlataforma) * 2f);
        Gizmos.DrawRay(transform.position, transform.TransformDirection(rotPeligroNeg * upDeLaPlataforma) * 2f);
    }
}
