using UnityEngine;
using TMPro; // Usar esto para TextMeshPro. Si usas UI Text, cambia a 'using UnityEngine.UI;'

public class ControladorRelojUI : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El componente de texto donde se mostrará el reloj.")]
    // Si usas UI.Text, cambia 'TextMeshProUGUI' por 'Text'
    [SerializeField] private TextMeshProUGUI textoDelReloj;
    [SerializeField] private TextMeshProUGUI textoDelRelojVictoria;

    [Tooltip("Arrastra aquí el objeto de la escena que tiene el script ControladorBalanza.")]
    [SerializeField] private ControladorBalanza controladorBalanza;

    [Header("Configuración del Reloj")]
    [Tooltip("La hora a la que empieza la jornada (ej. 9 para 9:00 AM).")]
    [SerializeField] private float horaInicioJornada = 9f;

    [Tooltip("La cantidad de horas que dura la jornada.")]
    [SerializeField] private float duracionJornadaEnHoras = 8f;

    private bool juegoTerminado = false;

    void Awake()
    {
        // Seguridad: Si no hay texto asignado, lo busca en el mismo objeto.
        if (textoDelReloj == null)
        {
            textoDelReloj = GetComponent<TextMeshProUGUI>(); // O GetComponent<Text>();
        }

        // Seguridad: Si no se asignó el controlador, avisar.
        if (controladorBalanza == null)
        {
            Debug.LogError("¡ERROR! No se ha asignado la referencia al ControladorBalanza en el ControladorRelojUI.");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos para saber cuándo mostrar mensajes de victoria/derrota
        ControladorBalanza.OnVictoriaAlcanzada += MostrarMensajeVictoria;
        ControladorBalanza.OnEquilibrioPerdido += MostrarMensajeDerrota;
    }

    private void OnDisable()
    {
        ControladorBalanza.OnVictoriaAlcanzada -= MostrarMensajeVictoria;
        ControladorBalanza.OnEquilibrioPerdido -= MostrarMensajeDerrota;
    }

    void Update()
    {
        if (juegoTerminado || controladorBalanza == null) return;

        // 1. Obtener el progreso del temporizador (de 0.0 a 1.0)
        float progreso = controladorBalanza.TiempoTranscurrido / controladorBalanza.DuracionTotal;
        progreso = Mathf.Clamp01(progreso);

        // 2. Calcular la hora actual en la jornada ficticia
        float horasTranscurridasFicticias = progreso * duracionJornadaEnHoras;
        float horaActualFicticia = horaInicioJornada + horasTranscurridasFicticias;

        // 3. Convertir la hora a formato HH:MM
        int horas = (int)horaActualFicticia;
        int minutos = (int)((horaActualFicticia - horas) * 60);

        // --- ¡AQUÍ ESTÁ LA MAGIA! ---
        // Usamos el operador de módulo (%) para que la hora se reinicie a 0 después de 23.
        horas = horas % 24; // <<-- LÍNEA MODIFICADA/AÑADIDA

        // 4. Actualizar el texto del reloj
        textoDelReloj.text = string.Format("{0:00}:{1:00}", horas, minutos);
    }

    private void MostrarMensajeVictoria(float puntajeFinal) // <<-- MODIFICADO: ahora recibe un float
    {
        juegoTerminado = true;

        // Creamos un nuevo mensaje que incluye el puntaje.
        // Usamos (int) para mostrarlo como un número entero.
        // El \n crea un salto de línea.
        textoDelRelojVictoria.text = $"¡JORNADA COMPLETADA!\nPesos distribuidos: {(int)puntajeFinal}"; // <<-- LÍNEA MODIFICADA

        textoDelReloj.color = Color.green;
        Debug.Log($"UI: Mostrando mensaje de victoria con puntaje {puntajeFinal}.");
    }

    private void MostrarMensajeDerrota()
    {
        juegoTerminado = true;
        textoDelReloj.text = "¡JORNADA FALLIDA!";
        textoDelReloj.color = Color.red;
        Debug.Log("UI: Mostrando mensaje de derrota.");
    }
}
