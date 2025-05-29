using UnityEngine;

public class TestCode : MonoBehaviour
{
    [SerializeField] GameObject point_ref; // El punto donde se sostiene el objeto
    [SerializeField] float throwForce = 10f; // Fuerza con la que se lanzar� el objeto

    private GameObject object_ref;
    private Rigidbody objectRigidbody; // Para guardar la referencia al Rigidbody del objeto agarrado

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (point_ref == null)
        {
            Debug.LogError("Point_ref no est� asignado en el Inspector.");
            // Considera deshabilitar el script si point_ref es esencial
            // this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Podr�as a�adir l�gica aqu� si es necesario, por ejemplo,
        // para mantener el objeto en su sitio si la f�sica lo mueve un poco
        // mientras est� agarrado, aunque SetParent y localPosition = zero
        // deber�an ser bastante estables.
    }

    private void OnEnable()
    {
        // Podr�as suscribir eventos aqu� si fuera necesario al activar el objeto
    }

    private void OnDisable()
    {
        // Es buena pr�ctica desuscribir eventos aqu� para evitar errores
        // si el objeto se desactiva mientras est� suscrito.
        if (object_ref != null) // Si al desactivarse a�n tiene un objeto "detectado"
        {
            PlayerControllerAlt.OnGrab -= GrabObject;
            // Considera si tambi�n deber�a soltar el objeto
            // DropObject();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "object")
        {
            // Solo tomar un nuevo objeto si no tenemos uno ya, o si es una l�gica espec�fica
            if (object_ref == null)
            {
                object_ref = collision.gameObject;
                objectRigidbody = object_ref.GetComponent<Rigidbody>(); // Obtener el Rigidbody

                if (objectRigidbody == null)
                {
                    Debug.LogWarning("El objeto " + object_ref.name + " no tiene un Rigidbody. No se podr� lanzar con fuerza.");
                }

                Debug.Log("Objeto detectado: " + object_ref.name);
                PlayerControllerAlt.OnGrab += GrabObject; // Usar += para ser m�s robusto
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Solo procesar si el objeto que sale es el que tenemos referenciado
        if (object_ref != null && collision.gameObject == object_ref)
        {
            Debug.Log("El objeto " + object_ref.name + " ha salido de la colisi�n.");
            PlayerControllerAlt.OnGrab -= GrabObject;

            // No llames a DropObject() aqu� directamente si OnGrab es lo que realmente suelta.
            // DropObject() deber�a ser llamado por la acci�n de "soltar" del jugador,
            // o si el objeto se aleja demasiado, etc.
            // Si OnGrab es el evento que dispara el agarre/suelte,
            // entonces la l�gica de soltar (incluido el lanzamiento)
            // deber�a estar en un m�todo que se llame cuando el jugador decida soltar.

            // Por ahora, si la intenci�n es que al salir de la colisi�n se suelte Y se lance:
            // DropAndThrowObject(); // Necesitar�amos un m�todo que haga ambas cosas
            // O, si el evento OnGrab es quien maneja el "soltar", entonces
            // el c�digo de PlayerControllerAlt que invoca OnGrab (cuando se suelta)
            // es quien deber�a llamar a DropObject o similar.

            // Para simplificar, y asumiendo que quieres que se suelte al salir del trigger:
            // Si tienes un objeto y sales de su colisi�n, lo sueltas (y potencialmente lo lanzas).
            // Esto podr�a no ser lo ideal si el jugador a�n quiere sostenerlo.
            // La l�gica de soltar/lanzar deber�a estar m�s ligada a una acci�n del jugador.
            // Pero si es lo que quieres:
            if (PlayerControllerAlt.OnGrab == null) // Si ya no hay nadie suscrito para agarrar (o sea, se solt�)
            {
                // DropObject(); // Llamar�as a tu m�todo de soltar/lanzar
            }
            // Limpiamos la referencia si el objeto que sali� es el que ten�amos.
            // object_ref = null;
            // objectRigidbody = null;
            // Esto es importante: si sales de la colisi�n, ya no deber�as poder agarrarlo
            // mediante el evento OnGrab que se suscribi� en OnCollisionEnter para ESTE objeto.
            // La desuscripci�n ya se hizo.
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

        // Importante: Si el objeto tiene un Rigidbody, querr�s hacerlo cinem�tico
        // mientras est� agarrado para que no reaccione a la f�sica de forma extra�a.
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
            objectRigidbody.detectCollisions = false; // Opcional: desactivar colisiones mientras est� agarrado
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

        // 2. Reactivar la f�sica en el Rigidbody (si lo tiene)
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
            objectRigidbody.detectCollisions = true; // Reactivar colisiones

            // Calcular la direcci�n del lanzamiento.
            // Usaremos la direcci�n hacia adelante del 'point_ref' (o de la c�mara del jugador).
            Vector3 throwDirection = point_ref.transform.forward;
            // Si point_ref es hijo de la c�mara y est� orientado con ella, esto funcionar� bien.
            // Si no, podr�as querer usar Camera.main.transform.forward

            objectRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            Debug.Log("Objeto " + object_ref.name + " lanzado con fuerza: " + throwForce + " en direcci�n " + throwDirection);
        }
        else
        {
            Debug.LogWarning("El objeto " + object_ref.name + " fue soltado pero no tiene Rigidbody para lanzar.");
        }

        // 3. Limpiar las referencias
        // Es importante desuscribir aqu� tambi�n si no se hizo en OnCollisionExit
        // o si la l�gica de OnCollisionExit no siempre se ejecuta antes de soltar.
        PlayerControllerAlt.OnGrab -= GrabObject; // Asegurarse de que est� desuscrito

        object_ref = null;
        objectRigidbody = null;
    }

    // M�todo original de DropObject, ahora modificado para llamar a DropAndThrowObject
    // o para simplemente soltar sin lanzar si as� lo prefieres.
    public void DropObject() // Este es el que probablemente llamas desde OnCollisionExit o un input de "soltar"
    {
        // Aqu� decides si al "soltar" siempre se lanza, o si hay dos acciones separadas.
        // Por ahora, haremos que DropObject llame a la funci�n de lanzar.
        DropAndThrowObject();
    }
}
