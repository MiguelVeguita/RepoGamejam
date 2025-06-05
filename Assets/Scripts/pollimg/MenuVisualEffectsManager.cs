using UnityEngine;
using System.Collections.Generic; // Necesario para List
using System.Collections; // Necesario para Coroutines

public class MenuVisualEffectsManager : MonoBehaviour
{
    [Header("Configuraci�n del Pool de Objetos")]
    [Tooltip("Arrastra aqu� GameObjects que tengan el script StaticObjectPooling<FallingMenuObject> configurado para tus prefabs 3D.")]
    public List<StaticObjectPooling<FallingMenuObject>> objectPools;

    [Header("Configuraci�n de Aparici�n")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 2.0f;
    public float baseFallSpeed = 2f;

    [Header("L�mites de Aparici�n y Desaparici�n (Espacio del Mundo)")]
    public float spawnYPosition = 10f;
    public float spawnAreaWidth = 20f;
    public float spawnAreaDepth = 10f;
    public float despawnYPosition = -10f;

    [Tooltip("Opcional: Si los objetos deben ser hijos de este Transform para organizar la jerarqu�a.")]
    public Transform spawnedObjectsParent;

    private Coroutine _spawnCoroutine;
    private bool _isSpawningActive = true; // Controla si el efecto est� activo

    void Start()
    {
        if (!ValidatePools())
        {
            enabled = false;
            return;
        }

        if (spawnedObjectsParent == null)
        {
            Debug.LogWarning("MenuVisualEffectsManager: 'Spawned Objects Parent' no asignado. Los objetos ser�n hijos del GameObject del Pool respectivo o de este manager si el pool lo permite.", this);
        }

        if (_isSpawningActive)
        {
            _spawnCoroutine = StartCoroutine(SpawnObjectsRoutine());
        }
    }

    bool ValidatePools()
    {
        if (objectPools == null || objectPools.Count == 0)
        {
            Debug.LogError("MenuVisualEffectsManager: No se han asignado pools de objetos (objectPools).", this);
            return false;
        }
        for (int i = 0; i < objectPools.Count; i++)
        {
            if (objectPools[i] == null)
            {
                Debug.LogError($"MenuVisualEffectsManager: El pool de objetos en el �ndice {i} es nulo.", this);
                return false;
            }
        }
        return true;
    }

    IEnumerator SpawnObjectsRoutine()
    {
        // Esta corutina ahora se detendr� si _isSpawningActive es false,
        // porque la corutina entera ser� detenida por StopCoroutine.
        // El while(true) est� bien aqu�.
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Doble chequeo por si la corutina no se detuvo a tiempo externamente
            // O si se quiere un control m�s granular dentro del loop (aunque detener la corutina es m�s limpio).
            if (!_isSpawningActive) yield break;

            SpawnSingleObject();
        }
    }

    void SpawnSingleObject()
    {
        if (objectPools.Count == 0) return;

        int poolIndex = Random.Range(0, objectPools.Count);
        StaticObjectPooling<FallingMenuObject> selectedPool = objectPools[poolIndex];

        if (selectedPool == null)
        {
            Debug.LogWarning("MenuVisualEffectsManager: El pool seleccionado es nulo.", this);
            return;
        }

        FallingMenuObject spawnedObject = selectedPool.GetObject();

        if (spawnedObject != null)
        {
            float spawnX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            float spawnZ = Random.Range(-spawnAreaDepth / 2f, spawnAreaDepth / 2f);
            spawnedObject.transform.position = new Vector3(spawnX, spawnYPosition, spawnZ);
            spawnedObject.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

            if (spawnedObjectsParent != null)
            {
                spawnedObject.transform.SetParent(spawnedObjectsParent, true);
            }
            // Si no hay spawnedObjectsParent, el objeto permanecer� como hijo del pool (comportamiento t�pico del pool).

            spawnedObject.fallSpeed = baseFallSpeed;
            spawnedObject.SetupWorldBoundary(despawnYPosition); // Aseg�rate que FallingMenuObject tenga este m�todo
        }
    }

    void Update()
    {
        // Solo procesar el despawn si el efecto est� activo, para evitar errores si los objetos se limpian de otra forma.
        if (!_isSpawningActive) return;

        for (int i = 0; i < objectPools.Count; i++)
        {
            StaticObjectPooling<FallingMenuObject> currentPool = objectPools[i];
            if (currentPool == null) continue;

            // IMPORTANTE: La l�gica para encontrar objetos a despawnear necesita ser consistente
            // con c�mo se manejan los padres de los objetos (spawnedObjectsParent vs hijos del pool).
            // El c�digo original itera sobre currentPool.transform.childCount.
            // Esto asume que los objetos activos (o al menos los que este Update debe revisar)
            // son hijos del transform del pool. Si `spawnedObjectsParent` se usa y los objetos
            // se mueven all�, este Update no los encontrar�a.
            // Para este ejemplo, mantendr� la l�gica original del Update,
            // pero esto podr�a necesitar ajuste basado en c�mo funciona tu `StaticObjectPooling`
            // y el emparentamiento.

            int childCount = currentPool.transform.childCount;
            for (int j = childCount - 1; j >= 0; j--) // Iterar hacia atr�s es m�s seguro si se eliminan elementos
            {
                Transform childTransform = currentPool.transform.GetChild(j);
                if (childTransform.gameObject.activeInHierarchy)
                {
                    FallingMenuObject fmo = childTransform.GetComponent<FallingMenuObject>();
                    if (fmo != null)
                    {
                        if (fmo.transform.position.y < despawnYPosition)
                        {
                            currentPool.ReturnObject(fmo);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Activa el efecto de ca�da de objetos.
    /// </summary>
    public void ActivateFallingObjects()
    {
        if (!_isSpawningActive)
        {
            _isSpawningActive = true;
            // Validar que el componente y el GameObject est�n activos antes de iniciar la corutina
            if (this.gameObject.activeInHierarchy && this.enabled)
            {
                _spawnCoroutine = StartCoroutine(SpawnObjectsRoutine());
                Debug.Log("Efecto de ca�da de objetos ACTIVADO.");
            }
            else
            {
                Debug.LogWarning("MenuVisualEffectsManager est� inactivo, no se pueden activar los efectos de ca�da.", this);
            }
        }
    }

    /// <summary>
    /// Desactiva el efecto de ca�da de objetos.
    /// </summary>
    /// <param name="returnActiveObjectsToPool">Si es true, todos los objetos activos visibles ser�n devueltos al pool.</param>
    public void DeactivateFallingObjects(bool returnActiveObjectsToPool = true)
    {
        if (_isSpawningActive)
        {
            _isSpawningActive = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
            Debug.Log("Efecto de ca�da de objetos DESACTIVADO.");

            if (returnActiveObjectsToPool)
            {
                ReturnAllActiveObjectsToPool();
            }
        }
    }

    private void ReturnAllActiveObjectsToPool()
    {
        Debug.Log("Devolviendo objetos activos al pool...");
        // Esta funci�n asume que los objetos activos son hijos de `spawnedObjectsParent` si est� asignado,
        // o que necesitan ser encontrados de alguna manera si no (por ejemplo, iterando hijos de los pools,
        // lo cual es complejo si se re-emparentan).
        // La forma m�s simple y consistente con tu `Update` es iterar los hijos de cada pool.
        // Esto asume que `ReturnObject` en tu pool puede manejar un objeto sin importar su padre actual,
        // o que los objetos activos son de hecho hijos del transform del pool.

        for (int i = 0; i < objectPools.Count; i++)
        {
            StaticObjectPooling<FallingMenuObject> currentPool = objectPools[i];
            if (currentPool == null || currentPool.transform == null) continue;

            List<FallingMenuObject> objectsToReturnFromThisPool = new List<FallingMenuObject>();
            // Iterar sobre los hijos del transform del pool.
            // Esto es consistente con tu bucle Update para despawn.
            for (int j = 0; j < currentPool.transform.childCount; j++)
            {
                Transform childTransform = currentPool.transform.GetChild(j);
                // Es crucial que GetObject() del pool active el GameObject y ReturnObject() lo desactive.
                if (childTransform.gameObject.activeInHierarchy)
                {
                    FallingMenuObject fmo = childTransform.GetComponent<FallingMenuObject>();
                    if (fmo != null) // Asegurarse de que es uno de nuestros objetos
                    {
                        objectsToReturnFromThisPool.Add(fmo);
                    }
                }
            }
            // Devolver los objetos recolectados para este pool
            foreach (FallingMenuObject fmo in objectsToReturnFromThisPool)
            {
                currentPool.ReturnObject(fmo); // El pool se encarga de desactivarlo y manejarlo.
            }
        }
    }

    // Es una buena pr�ctica detener las corutinas si el objeto se desactiva o destruye
    void OnDisable()
    {
        // No necesariamente queremos cambiar _isSpawningActive aqu�, solo detener la corutina
        // para que no siga corriendo si el objeto se desactiva.
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }
}
