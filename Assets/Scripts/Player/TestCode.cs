using UnityEngine;

public class TestCode : MonoBehaviour
{
    [SerializeField] GameObject point_ref; // El punto donde se sostiene el objeto
    [SerializeField] float throwForce = 10f; // Fuerza con la que se lanzará el objeto

    private GameObject object_ref;
    private Rigidbody objectRigidbody; // Para guardar la referencia al Rigidbody del objeto agarrado

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (point_ref == null)
        {
            Debug.LogError("Point_ref no está asignado en el Inspector.");
            // Considera deshabilitar el script si point_ref es esencial
            // this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Podrías añadir lógica aquí si es necesario, por ejemplo,
        // para mantener el objeto en su sitio si la física lo mueve un poco
        // mientras está agarrado, aunque SetParent y localPosition = zero
        // deberían ser bastante estables.
    }

    private void OnEnable()
    {
        // Podrías suscribir eventos aquí si fuera necesario al activar el objeto
    }

    private void OnDisable()
    {
        // Es buena práctica desuscribir eventos aquí para evitar errores
        // si el objeto se desactiva mientras está suscrito.
        if (object_ref != null) // Si al desactivarse aún tiene un objeto "detectado"
        {
            PlayerControllerAlt.OnGrab -= GrabObject;
            // Considera si también debería soltar el objeto
            // DropObject();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "object")
        {
            // Solo tomar un nuevo objeto si no tenemos uno ya, o si es una lógica específica
            if (object_ref == null)
            {
                object_ref = collision.gameObject;
                objectRigidbody = object_ref.GetComponent<Rigidbody>(); // Obtener el Rigidbody

                if (objectRigidbody == null)
                {
                    Debug.LogWarning("El objeto " + object_ref.name + " no tiene un Rigidbody. No se podrá lanzar con fuerza.");
                }

                Debug.Log("Objeto detectado: " + object_ref.name);
                PlayerControllerAlt.OnGrab += GrabObject; // Usar += para ser más robusto
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Solo procesar si el objeto que sale es el que tenemos referenciado
        if (object_ref != null && collision.gameObject == object_ref)
        {
            Debug.Log("El objeto " + object_ref.name + " ha salido de la colisión.");
            PlayerControllerAlt.OnGrab -= GrabObject;

            // No llames a DropObject() aquí directamente si OnGrab es lo que realmente suelta.
            // DropObject() debería ser llamado por la acción de "soltar" del jugador,
            // o si el objeto se aleja demasiado, etc.
            // Si OnGrab es el evento que dispara el agarre/suelte,
            // entonces la lógica de soltar (incluido el lanzamiento)
            // debería estar en un método que se llame cuando el jugador decida soltar.

            // Por ahora, si la intención es que al salir de la colisión se suelte Y se lance:
            // DropAndThrowObject(); // Necesitaríamos un método que haga ambas cosas
            // O, si el evento OnGrab es quien maneja el "soltar", entonces
            // el código de PlayerControllerAlt que invoca OnGrab (cuando se suelta)
            // es quien debería llamar a DropObject o similar.

            // Para simplificar, y asumiendo que quieres que se suelte al salir del trigger:
            // Si tienes un objeto y sales de su colisión, lo sueltas (y potencialmente lo lanzas).
            // Esto podría no ser lo ideal si el jugador aún quiere sostenerlo.
            // La lógica de soltar/lanzar debería estar más ligada a una acción del jugador.
            // Pero si es lo que quieres:
            if (PlayerControllerAlt.OnGrab == null) // Si ya no hay nadie suscrito para agarrar (o sea, se soltó)
            {
                // DropObject(); // Llamarías a tu método de soltar/lanzar
            }
            // Limpiamos la referencia si el objeto que salió es el que teníamos.
            // object_ref = null;
            // objectRigidbody = null;
            // Esto es importante: si sales de la colisión, ya no deberías poder agarrarlo
            // mediante el evento OnGrab que se suscribió en OnCollisionEnter para ESTE objeto.
            // La desuscripción ya se hizo.
        }
    }

    public void GrabObject()
    {
        if (object_ref == null || point_ref == null)
        {
            Debug.LogWarning("Intentando agarrar pero object_ref o point_ref es null.");
            return;
        }

        object_ref.transform.SetParent(point_ref.transform);
        object_ref.transform.localPosition = Vector3.zero;
        object_ref.transform.localRotation = Quaternion.identity;

        // Importante: Si el objeto tiene un Rigidbody, querrás hacerlo cinemático
        // mientras está agarrado para que no reaccione a la física de forma extraña.
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
            objectRigidbody.detectCollisions = false; // Opcional: desactivar colisiones mientras está agarrado
        }
        Debug.Log(object_ref.name + " agarrado y posicionado.");
    }

    // Renombrado para mayor claridad, o puedes tener un Drop() y un Throw() separados
    public void DropAndThrowObject()
    {
        if (object_ref == null)
        {
            Debug.LogWarning("Intentando soltar/lanzar pero no hay object_ref.");
            return;
        }

        // 1. Quitar el parentesco
        object_ref.transform.SetParent(null);

        // 2. Reactivar la física en el Rigidbody (si lo tiene)
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
            objectRigidbody.detectCollisions = true; // Reactivar colisiones

            // Calcular la dirección del lanzamiento.
            // Usaremos la dirección hacia adelante del 'point_ref' (o de la cámara del jugador).
            Vector3 throwDirection = point_ref.transform.forward;
            // Si point_ref es hijo de la cámara y está orientado con ella, esto funcionará bien.
            // Si no, podrías querer usar Camera.main.transform.forward

            objectRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            Debug.Log("Objeto " + object_ref.name + " lanzado con fuerza: " + throwForce + " en dirección " + throwDirection);
        }
        else
        {
            Debug.LogWarning("El objeto " + object_ref.name + " fue soltado pero no tiene Rigidbody para lanzar.");
        }

        // 3. Limpiar las referencias
        // Es importante desuscribir aquí también si no se hizo en OnCollisionExit
        // o si la lógica de OnCollisionExit no siempre se ejecuta antes de soltar.
        PlayerControllerAlt.OnGrab -= GrabObject; // Asegurarse de que esté desuscrito

        object_ref = null;
        objectRigidbody = null;
    }

    // Método original de DropObject, ahora modificado para llamar a DropAndThrowObject
    // o para simplemente soltar sin lanzar si así lo prefieres.
    public void DropObject() // Este es el que probablemente llamas desde OnCollisionExit o un input de "soltar"
    {
        // Aquí decides si al "soltar" siempre se lanza, o si hay dos acciones separadas.
        // Por ahora, haremos que DropObject llame a la función de lanzar.
        DropAndThrowObject();
    }
}
