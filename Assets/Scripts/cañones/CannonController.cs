using System.Collections;
using System.Collections.Generic; // Necesario para List<T>
using UnityEngine;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class CannonController : MonoBehaviour
{
    [Header("Configuración de Cañones")]
    [Tooltip("Arrastra aquí los Transforms de los puntos de disparo de tus 4 cañones.")]
    public Transform[] cannonMuzzles; 

    [Header("Configuración de Objetos")]
    [Tooltip("Arrastra aquí los Prefabs de los objetos que los cañones pueden disparar (estrella, planeta, etc.).")]
    public List<GameObject> objectPrefabs; 

    [Tooltip("Etiqueta que tendrán los objetos lanzados para que el jugador pueda agarrarlos.")]
    public string launchedObjectTag = "ObjetoAgarrable"; 

    [Header("Configuración de Disparo")]
    [Tooltip("La fuerza con la que se lanzarán los objetos.")]
    public float launchForce = 15f; 
    [Tooltip("Número de objetos a lanzar en una secuencia/ola.")]
    public int objectsPerWave = 5; 
    [Tooltip("Tiempo en segundos entre cada disparo de la ola.")]
    public float timeBetweenShots = 2f; 
    [Tooltip("Tiempo en segundos antes de que comience la PRIMERA ola de disparos.")]
    public float initialDelay = 3f; 
    [Tooltip("Tiempo en segundos de espera entre el final de una ola y el inicio de la siguiente.")]
    public float delayBetweenWaves = 5f; 

    private bool isWaveActive = false; 

    void Start()
    {
        
        if (cannonMuzzles == null || cannonMuzzles.Length == 0)
        {
            Debug.LogError("¡Error en CannonController! No se han asignado 'Cannon Muzzles' (puntos de disparo). Asigna los Transforms en el Inspector.", this);
            enabled = false; 
            return;
        }
       
        foreach (Transform muzzle in cannonMuzzles)
        {
            if (muzzle == null)
            {
                Debug.LogError("¡Error en CannonController! Uno de los 'Cannon Muzzles' (puntos de disparo) en la lista es nulo. Revisa las asignaciones.", this);
                enabled = false;
                return;
            }
        }

        if (objectPrefabs == null || objectPrefabs.Count == 0)
        {
            Debug.LogError("¡Error en CannonController! No se han asignado 'Object Prefabs' (prefabs de objetos). Asigna la lista en el Inspector.", this);
            enabled = false;
            return;
        }
        
        foreach (GameObject prefab in objectPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError("¡Error en CannonController! Uno de los 'Object Prefabs' (prefabs de objetos) en la lista es nulo. Revisa las asignaciones.", this);
                enabled = false;
                return;
            }
        }

        StartCoroutine(BeginFirstWave());
    }

    
    IEnumerator BeginFirstWave()
    {
        Debug.Log($"Esperando {initialDelay} segundos para la primera ola...");
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(LaunchObjectWave()); 
    }

   
    IEnumerator LaunchObjectWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("Intento de iniciar una nueva ola mientras una ya está activa. Saliendo.");
            yield break;
        }
        isWaveActive = true; 
        Debug.Log("Iniciando nueva ola de objetos...");

        for (int i = 0; i < objectsPerWave; i++)
        {
            if (cannonMuzzles.Length == 0 || objectPrefabs.Count == 0)
            {
                Debug.LogError("Configuración de cañones u objetos inválida durante el transcurso de la ola. Deteniendo.");
                isWaveActive = false; 
                yield break; 
            }

            int randomCannonIndex = Random.Range(0, cannonMuzzles.Length);
            Transform selectedCannonMuzzle = cannonMuzzles[randomCannonIndex];

            int randomObjectIndex = Random.Range(0, objectPrefabs.Count);
            GameObject objectToLaunchPrefab = objectPrefabs[randomObjectIndex];

            GameObject launchedObject = Instantiate(objectToLaunchPrefab, selectedCannonMuzzle.position, selectedCannonMuzzle.rotation);

            
            Rigidbody rb = launchedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(selectedCannonMuzzle.forward * launchForce, ForceMode.Impulse);
            }
            else
            {
                Debug.LogWarning($"El prefab '{objectToLaunchPrefab.name}' no tiene un componente Rigidbody. No se puede aplicar fuerza de lanzamiento.", launchedObject);
            }

            yield return new WaitForSeconds(timeBetweenShots);
        }

        Debug.Log("Ola de objetos completada.");
        isWaveActive = false; 

       
        Debug.Log($"Esperando {delayBetweenWaves} segundos para la siguiente ola.");
        yield return new WaitForSeconds(delayBetweenWaves);
        StartCoroutine(LaunchObjectWave()); 
       
    }

    
    public void TriggerNewWave()
    {
        if (!isWaveActive)
        {
            Debug.Log("Disparando una nueva ola manualmente...");
            StopAllCoroutines();
            StartCoroutine(LaunchObjectWave());
        }
        else
        {
            Debug.Log("No se puede iniciar una nueva ola manualmente, ya hay una activa o en espera de ser procesada por el ciclo automático.");
        }
    }
}