using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    public Transform playerTarget; // Arrastra tu objeto Jugador aqu�
    public Vector3 offset = new Vector3(0f, 15f, -5f); // Distancia y �ngulo de la c�mara respecto al jugador
    public float smoothSpeed = 10f; // Velocidad de suavizado del seguimiento

    void LateUpdate()
    {
        if (playerTarget == null)
        {
            Debug.LogWarning("Target del jugador no asignado a la c�mara TopDown.");
            return;
        }

        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Hacer que la c�mara siempre mire al jugador o un punto ligeramente delante de �l
        // O una rotaci�n fija si es una vista cenital pura.
        // transform.LookAt(playerTarget.position); // Para que siga al jugador

        // Para una vista cenital estricta (directamente desde arriba):
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        // Si quieres que la c�mara tenga un yaw (rotaci�n Y) fijo o que rote con algo m�s, aj�stalo aqu�.
        // Por ejemplo, si el offset tiene un componente Z, LookAt(playerTarget) crear� un �ngulo.
        // Si el offset es (0, 15, 0) y la rotaci�n es (90,0,0), es una vista directamente desde arriba.
    }
}