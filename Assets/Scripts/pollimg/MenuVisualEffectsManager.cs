using UnityEngine;
using System.Collections.Generic; // Necesario para List
using System.Collections; // Necesario para Coroutines

public class MenuVisualEffectsManager : MonoBehaviour
{
    [Header("Configuraci�n del Pool de Objetos")]
    [Tooltip("Arrastra aqu� GameObjects que tengan el script StaticObjectPooling<FallingMenuObject> configurado para tus prefabs 3D.")]
    public List<StaticObjectPooling<FallingMenuObject>> objectPools;

    [Header("Configuraci�n de Aparici�n")]
    [Tooltip("Tiempo m�nimo entre la aparici�n de nuevos objetos.")]
    public float minSpawnInterval = 0.5f;
    [Tooltip("Tiempo m�ximo entre la aparici�n de nuevos objetos.")]
    public float maxSpawnInterval = 2.0f;
    [Tooltip("Velocidad de ca�da base para los objetos (puede ser sobreescrita por el FallingMenuObject).")]
    public float baseFallSpeed = 2f; // Ajusta esta velocidad para el espacio 3D

    [Header("L�mites de Aparici�n y Desaparici�n (Espacio del Mundo)")]
    [Tooltip("Posici�n Y en el mundo donde aparecer�n los objetos (ej. encima de la vista de c�mara).")]
    public float spawnYPosition = 10f;
    [Tooltip("Ancho del �rea de aparici�n en el eje X (centrado en X=0).")]
    public float spawnAreaWidth = 20f;
    [Tooltip("Profundidad del �rea de aparici�n en el eje Z (si es relevante, centrado en Z=0).")]
    public float spawnAreaDepth = 10f; // Puedes poner 0 si es un efecto 2.5D
    [Tooltip("Posici�n Y en el mundo donde los objetos se consideran 'fuera de pantalla' y se devuelven al pool (ej. debajo de la vista de c�mara).")]
    public float despawnYPosition = -10f;

    [Tooltip("Opcional: Si los objetos deben ser hijos de este Transform para organizar la jerarqu�a.")]
    public Transform spawnedObjectsParent;


    void Start()
    {
        // --- Validaciones Iniciales ---
        if (objectPools == null || objectPools.Count == 0)
        {
            Debug.LogError("MenuVisualEffectsManager: No se han asignado pools de objetos (objectPools). Asigna la lista en el Inspector.", this);
            enabled = false;
            return;
        }
        for (int i = 0; i < objectPools.Count; i++)
        {
            if (objectPools[i] == null)
            {
                Debug.LogError($"MenuVisualEffectsManager: El pool de objetos en el �ndice {i} de la lista 'objectPools' es nulo. Asigna un pool v�lido.", this);
                enabled = false;
                return;
            }
        }

        if (spawnedObjectsParent == null)
        {
            // Si no se asigna un padre, se puede usar el transform de este mismo GameObject
            // o instanciarlos en la ra�z de la escena (lo cual no es ideal para organizaci�n).
            // Para este ejemplo, si no hay padre asignado, los objetos se instancian en la ra�z
            // pero el pool los hace hijos de su propio transform.
            Debug.LogWarning("MenuVisualEffectsManager: 'Spawned Objects Parent' no asignado. Los objetos ser�n hijos del GameObject del Pool respectivo.", this);
        }

        StartCoroutine(SpawnObjectsRoutine());
    }

    IEnumerator SpawnObjectsRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
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
            Debug.LogWarning("MenuVisualEffectsManager: El pool seleccionado para instanciar es nulo. Saltando este spawn.");
            return;
        }

        FallingMenuObject spawnedObject = selectedPool.GetObject();

        if (spawnedObject != null)
        {
            // Configurar el objeto 3D
            float spawnX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            float spawnZ = Random.Range(-spawnAreaDepth / 2f, spawnAreaDepth / 2f);

            spawnedObject.transform.position = new Vector3(spawnX, spawnYPosition, spawnZ);
            // La rotaci�n inicial podr�a ser la del prefab o una aleatoria
            spawnedObject.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));


            // Asignar padre si est� especificado
            if (spawnedObjectsParent != null)
            {
                spawnedObject.transform.SetParent(spawnedObjectsParent, true); // true para mantener la posici�n del mundo
            }
            // Nota: El script StaticObjectPooling ya hace que el objeto sea hijo del transform del pool.
            // Si quieres un padre diferente, aseg�rate de que esta l�gica no entre en conflicto.
            // Lo m�s simple es que el pool los instancie como hijos suyos, y luego aqu� los re-emparentes si es necesario,
            // o que modifiques el pool para que acepte un padre al instanciar.
            // Por ahora, el pool los har� hijos suyos. Si `spawnedObjectsParent` est� asignado,
            // se re-emparentar�n aqu� despu�s de ser activados por el pool.

            spawnedObject.fallSpeed = baseFallSpeed;
            spawnedObject.SetupWorldBoundary(despawnYPosition);
        }
    }

    void Update()
    {
        for (int i = 0; i < objectPools.Count; i++)
        {
            StaticObjectPooling<FallingMenuObject> currentPool = objectPools[i];
            if (currentPool == null) continue;

            int childCount = currentPool.transform.childCount;
            for (int j = 0; j < childCount; j++)
            {
                Transform childTransform = currentPool.transform.GetChild(j);
                if (childTransform.gameObject.activeInHierarchy)
                {
                    FallingMenuObject fmo = childTransform.GetComponent<FallingMenuObject>();
                    if (fmo != null)
                    {
                        // Comprobar si el objeto 3D ha ca�do por debajo del l�mite de desaparici�n
                        if (fmo.transform.position.y < despawnYPosition)
                        {
                            currentPool.ReturnObject(fmo);
                        }
                    }
                }
            }
        }
    }
}
