using System.Collections;
using System.Collections.Generic; // Necesario para List<T>
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("Configuraci�n de Ca�ones")]
    [Tooltip("Arrastra aqu� los Transforms de los puntos de disparo de tus 4 ca�ones.")]
    public Transform[] cannonMuzzles;

    [Header("Configuraci�n de Objetos")]
    [Tooltip("Arrastra aqu� los Prefabs de los objetos que los ca�ones pueden disparar (estrella, bal�n, etc.).")]
    public List<GameObject> objectPrefabs;

    [Tooltip("Etiqueta que tendr�n los objetos lanzados para que el jugador pueda agarrarlos.")]
    public string launchedObjectTag = "ObjetoAgarrable"; // Coincide con tu PlayerControllerAlt

    [Header("Configuraci�n de Disparo")]
    [Tooltip("La fuerza con la que se lanzar�n los objetos.")]
    public float launchForce = 15f;

    [Tooltip("N�mero de objetos a lanzar en una secuencia/ola.")]
    public int objectsPerWave = 5;

    [Tooltip("Tiempo en segundos entre cada disparo de la ola.")]
    public float timeBetweenShots = 2f;

    [Tooltip("Tiempo en segundos antes de que comience la primera ola de disparos.")]
    public float initialDelay = 3f;

    // Variable para controlar si ya se est� ejecutando una ola
    private bool isWaveActive = false;

    void Start()
    {
        // Validaciones iniciales
        if (cannonMuzzles == null || cannonMuzzles.Length == 0)
        {
            Debug.LogError("�Error en CannonController! No se han asignado 'Cannon Muzzles'. Asigna los Transforms de los puntos de disparo.", this);
            enabled = false; // Deshabilitar el script si no hay ca�ones
            return;
        }
        foreach (Transform muzzle in cannonMuzzles)
        {
            if (muzzle == null)
            {
                Debug.LogError("�Error en CannonController! Uno de los 'Cannon Muzzles' no est� asignado (es nulo).", this);
                enabled = false;
                return;
            }
        }

        if (objectPrefabs == null || objectPrefabs.Count == 0)
        {
            Debug.LogError("�Error en CannonController! No se han asignado 'Object Prefabs'. Asigna la lista de prefabs de objetos a disparar.", this);
            enabled = false; // Deshabilitar el script si no hay objetos
            return;
        }
        foreach (GameObject prefab in objectPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("�Error en CannonController! Uno de los 'Object Prefabs' no est� asignado (es nulo).", this);
                enabled = false;
                return;
            }
        }

        // Iniciar la primera ola de disparos despu�s de un retraso inicial
        StartCoroutine(BeginSpawningWaves());
    }

    IEnumerator BeginSpawningWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        // Puedes hacer que esto se repita o se active bajo ciertas condiciones
        // Por ahora, lanzar� una ola.
        StartCoroutine(LaunchObjectWave());
    }

    IEnumerator LaunchObjectWave()
    {
        if (isWaveActive)
        {
            // Si ya hay una ola activa, no iniciar otra.
            // Esto es una salvaguarda simple.
            yield break;
        }
        isWaveActive = true;
        Debug.Log("Iniciando nueva ola de objetos...");

        for (int i = 0; i < objectsPerWave; i++)
        {
            // 1. Seleccionar un ca��n al azar de la lista 'cannonMuzzles'
            int randomCannonIndex = Random.Range(0, cannonMuzzles.Length);
            Transform selectedCannonMuzzle = cannonMuzzles[randomCannonIndex];

            // 2. Seleccionar un objeto al azar de la lista 'objectPrefabs'
            int randomObjectIndex = Random.Range(0, objectPrefabs.Count);
            GameObject objectToLaunchPrefab = objectPrefabs[randomObjectIndex];

            // 3. Instanciar (crear) el objeto en la posici�n y rotaci�n del ca��n seleccionado
            // La rotaci�n del ca��n determinar� la direcci�n inicial del disparo.
            GameObject launchedObject = Instantiate(objectToLaunchPrefab, selectedCannonMuzzle.position, selectedCannonMuzzle.rotation);

            // Asignar la etiqueta para que el jugador pueda agarrarlo
            launchedObject.tag = launchedObjectTag;

            // 4. Obtener el Rigidbody del objeto instanciado
            Rigidbody rb = launchedObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Aplicar una fuerza al Rigidbody para lanzar el objeto
                // La fuerza se aplica en la direcci�n 'hacia adelante' del ca��n (muzzle.forward)
                rb.AddForce(selectedCannonMuzzle.forward * launchForce, ForceMode.Impulse);
                Debug.Log($"Ca��n {selectedCannonMuzzle.name} dispar� {launchedObject.name} con fuerza {launchForce}");
            }
            else
            {
                Debug.LogWarning($"El prefab '{objectToLaunchPrefab.name}' no tiene un componente Rigidbody. No se puede aplicar fuerza de lanzamiento.", launchedObject);
            }

            // 5. Esperar el tiempo definido antes del siguiente disparo
            yield return new WaitForSeconds(timeBetweenShots);
        }

        Debug.Log("Ola de objetos completada.");
        isWaveActive = false;

        // Opcional: Si quieres que las olas se repitan autom�ticamente, puedes llamar a la corrutina de nuevo aqu�.
        // StartCoroutine(LaunchObjectWave()); 
        // O podr�as tener un delay m�s largo entre olas:
        // yield return new WaitForSeconds(delayBetweenWaves);
        // StartCoroutine(LaunchObjectWave());
    }

    // Puedes a�adir un m�todo p�blico para iniciar una ola desde otro script si lo necesitas
    public void TriggerNewWave()
    {
        if (!isWaveActive)
        {
            StartCoroutine(LaunchObjectWave());
        }
        else
        {
            Debug.Log("No se puede iniciar una nueva ola, ya hay una activa.");
        }
    }
}
