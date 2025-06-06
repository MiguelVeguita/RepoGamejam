using System;
using UnityEngine;

public class ControladorBalanza : MonoBehaviour
{
    [Header("Pesos de las Cajas")]
    [Tooltip("Peso acumulado en la Caja A (izquierda, por ejemplo). Modifica esto en el Inspector para probar.")]
    public float pesoCajaA = 0f;
    [Tooltip("Peso acumulado en la Caja B (derecha, por ejemplo). Modifica esto en el Inspector para probar.")]
    public float pesoCajaB = 0f;

    [Header("Configuraci�n de la Balanza")]
    [Tooltip("Qu� tan sensible es la balanza a la diferencia de peso. Un valor mayor significa m�s inclinaci�n por unidad de peso.")]
    [SerializeField] private float sensibilidadInclinacion = 5f; // Grados por unidad de peso
    [Tooltip("El �ngulo m�ximo (en grados) que la balanza puede inclinarse antes de que se considere una p�rdida.")]
    [SerializeField] private float anguloMaximoPeligro = 30f;
    [Tooltip("Velocidad con la que la balanza rota hacia su �ngulo objetivo. M�s alto = m�s r�pido.")]
    [SerializeField] private float velocidadRotacionSuave = 5f;
    [Tooltip("El eje local alrededor del cual rotar� la balanza (ej. (0,0,1) para rotar sobre Z).")]
    [SerializeField] private Vector3 ejeDeRotacionLocal = Vector3.forward; // Com�nmente Vector3.forward (eje Z) o Vector3.right (eje X)
    [SerializeField] private float pesoTotalParaGanar = 50f;
    private Quaternion rotacionInicial; // Para referencia, si la balanza no empieza perfectamente horizontal
    private float anguloInclinacionActual = 0f; // Para seguimiento interno

    public static event System.Action OnEquilibrioPerdido; // Evento para notificar la p�rdida
    private bool victoriaAlcanzada = false;
    public static event Action OnVictoriaAlcanzada;
    void Start()
    {
        // Guardar la rotaci�n inicial de la balanza
        rotacionInicial = transform.localRotation;
        // Asegurarse de que el eje de rotaci�n est� normalizado por si acaso
        ejeDeRotacionLocal.Normalize();
    }
    private void OnEnable()
    {
        Boxes.OnACollisionWeight += AnadirPesoACajaA;
        Boxes.OnBCollisionWeight += AnadirPesoACajaB;
    }
    private void OnDisable()
    {
        Boxes.OnACollisionWeight -= AnadirPesoACajaA;
        Boxes.OnBCollisionWeight -= AnadirPesoACajaB;
    }
    void Update()
    {
        if (victoriaAlcanzada) return;

        // 1. Calcular la diferencia de peso
        // Una diferencia positiva inclinar� en una direcci�n, negativa en la otra.
        float diferenciaDePeso = pesoCajaA - pesoCajaB;

        // 2. Calcular el �ngulo de inclinaci�n objetivo basado en la diferencia y la sensibilidad
        // Si pesoCajaA > pesoCajaB, diferenciaDePeso es positivo.
        // Si el ejeDeRotacionLocal es (0,0,1) (forward), un �ngulo positivo rotar� hacia la "izquierda" (antihorario alrededor de Z).
        // Puedes invertir el signo de diferenciaDePeso si la rotaci�n es al rev�s de lo que esperas.
        float anguloInclinacionObjetivo = diferenciaDePeso * sensibilidadInclinacion;

        // 3. Limitar el �ngulo objetivo (opcional, pero puede ser bueno para evitar giros completos si la sensibilidad es muy alta)
        // anguloInclinacionObjetivo = Mathf.Clamp(anguloInclinacionObjetivo, -90f, 90f); // Ejemplo de l�mite a +/- 90 grados

        // 4. Crear la rotaci�n objetivo
        // Se aplica la rotaci�n sobre la rotaci�n inicial, usando el eje local.
        Quaternion rotacionObjetivo = rotacionInicial * Quaternion.AngleAxis(anguloInclinacionObjetivo, ejeDeRotacionLocal);

        // 5. Aplicar la rotaci�n suavemente al transform de la balanza
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotacionObjetivo, Time.deltaTime * velocidadRotacionSuave);

        // Actualizar el �ngulo de inclinaci�n actual para la l�gica de p�rdida
        // Esto se puede obtener de varias maneras. Una forma es medir el �ngulo entre el "up" actual y el "up" inicial.
        // O, m�s simple para este caso, usar el �ngulo objetivo ya que la rotaci�n lo seguir�.
        anguloInclinacionActual = anguloInclinacionObjetivo; // O podr�as calcularlo desde transform.localEulerAngles

        // 6. Comprobar si se ha superado el �ngulo de peligro
        if (Mathf.Abs(anguloInclinacionActual) > anguloMaximoPeligro)
        {
            // �Peligro! La balanza se ha inclinado demasiado.
            Debug.LogWarning("�EQUILIBRIO PERDIDO! La balanza se inclin� demasiado: " + anguloInclinacionActual + " grados.");
            OnEquilibrioPerdido?.Invoke(); // Disparar el evento de p�rdida

            // Aqu� podr�as a�adir l�gica adicional, como desactivar este script para que no siga rotando,
            // o iniciar una animaci�n de ca�da del jugador, etc.
            // Por ahora, solo mostramos un mensaje y disparamos el evento.
           // enabled = false; // Desactivar este script para detener la l�gica de la balanza
        }
    }

    // M�todos p�blicos para que otros scripts puedan a�adir peso a las cajas
    public void AnadirPesoACajaA(int cantidad)
    {
        pesoCajaA += cantidad;
        Debug.Log($"A�adido {cantidad} a Caja A. Nuevo peso: {pesoCajaA}");
        ComprobarCondicionDeVictoria();
    }

    public void AnadirPesoACajaB(int cantidad)
    {
        pesoCajaB += cantidad;
        Debug.Log($"A�adido {cantidad} a Caja B. Nuevo peso: {pesoCajaB}");
        ComprobarCondicionDeVictoria();
    }
    private void ComprobarCondicionDeVictoria()
    {
        // Si ya se ha ganado, no hacer nada.
        if (victoriaAlcanzada) return;

        // Sumar los pesos de ambas cajas
        float pesoTotal = pesoCajaA + pesoCajaB;

        // Comprobar si la suma es igual o mayor a la meta
        if (pesoTotal >= pesoTotalParaGanar)
        {
            victoriaAlcanzada = true; // Marcar que ya se gan�
            Debug.Log("�VICTORIA! Se ha alcanzado el peso total de " + pesoTotalParaGanar);
            OnVictoriaAlcanzada?.Invoke(); // Disparar el evento de victoria

            // Opcional: podr�as desactivar la balanza para que se quede quieta
            // enabled = false; 
        }
    }
    // (Opcional) M�todo para resetear la balanza
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

    // Dibujar Gizmos en el editor para visualizar el eje de rotaci�n y los l�mites (opcional)
    void OnDrawGizmosSelected()
    {
        if (ejeDeRotacionLocal == Vector3.zero) return;

        Gizmos.color = Color.red;
        // Dibuja una l�nea para el eje de rotaci�n
        Gizmos.DrawLine(transform.position - transform.TransformDirection(ejeDeRotacionLocal) * 1f, transform.position + transform.TransformDirection(ejeDeRotacionLocal) * 1f);

        // Visualizar los �ngulos de peligro (aproximado)
        Gizmos.color = Color.yellow;
        Quaternion rotPeligroPos = rotacionInicial * Quaternion.AngleAxis(anguloMaximoPeligro, ejeDeRotacionLocal);
        Quaternion rotPeligroNeg = rotacionInicial * Quaternion.AngleAxis(-anguloMaximoPeligro, ejeDeRotacionLocal);

        // Vector que representa el "arriba" de la plataforma de la balanza
        Vector3 upDeLaPlataforma = Vector3.up; // Asume que la plataforma es horizontal cuando est� equilibrada
                                               // Si tu modelo de balanza tiene una plataforma que no es 'up', ajusta este vector.

        Gizmos.DrawRay(transform.position, transform.TransformDirection(rotPeligroPos * upDeLaPlataforma) * 2f);
        Gizmos.DrawRay(transform.position, transform.TransformDirection(rotPeligroNeg * upDeLaPlataforma) * 2f);
    }
}
