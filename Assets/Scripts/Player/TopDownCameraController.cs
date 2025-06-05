using UnityEngine;

public class TopDownCameraController : MonoBehaviour
{
    public Transform playerTarget; // Arrastra tu objeto Jugador aquí
    public Vector3 offset = new Vector3(0f, 15f, -5f); // Distancia y ángulo de la cámara respecto al jugador
    public float smoothSpeed = 10f; // Velocidad de suavizado del seguimiento

    void LateUpdate()
    {
        if (playerTarget == null)
        {
            Debug.LogWarning("Target del jugador no asignado a la cámara TopDown.");
            return;
        }

        Vector3 desiredPosition = playerTarget.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Hacer que la cámara siempre mire al jugador o un punto ligeramente delante de él
        // O una rotación fija si es una vista cenital pura.
        // transform.LookAt(playerTarget.position); // Para que siga al jugador

        // Para una vista cenital estricta (directamente desde arriba):
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        // Si quieres que la cámara tenga un yaw (rotación Y) fijo o que rote con algo más, ajústalo aquí.
        // Por ejemplo, si el offset tiene un componente Z, LookAt(playerTarget) creará un ángulo.
        // Si el offset es (0, 15, 0) y la rotación es (90,0,0), es una vista directamente desde arriba.
    }
}