using UnityEngine;
using UnityEngine.UI; // Necesario para Image (aunque no se use directamente en este script)
using TMPro;          // Necesario para TextMeshProUGUI (aunque no se use directamente en este script)
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using System.Collections.Generic; // Necesario para List<T>

// La clase StoryPanel ya no es necesaria, ya que usaremos GameObjects directamente.

public class StorySequenceManager : MonoBehaviour
{
    [Header("Configuración de Paneles")]
    [Tooltip("Arrastra aquí los GameObjects de los paneles de historia en orden secuencial. Cada GameObject debe tener su propia Imagen y Texto.")]
    public List<GameObject> storyPanelObjects; // Cambiado de List<StoryPanel> a List<GameObject>

    [Header("Referencias de UI")]
    // displayImage y displayText ya no son necesarios aquí,
    // ya que cada panel GameObject manejará sus propios componentes visuales.

    [Tooltip("El GameObject del botón 'Siguiente' (con una flecha, por ejemplo).")]
    [SerializeField] private GameObject nextButton;

    [Tooltip("El GameObject del botón 'Jugar Gameplay' que aparece al final.")]
    [SerializeField] private GameObject playGameplayButton;

    [Header("Configuración de Gameplay")]
    [Tooltip("El nombre de la escena de gameplay a cargar.")]
    [SerializeField] private string gameplaySceneName = "GameplayScene"; // ¡Cambia esto al nombre de tu escena!

    private int currentPanelIndex = 0;

    void Start()
    {
        // Validaciones iniciales
        if (storyPanelObjects == null || storyPanelObjects.Count == 0)
        {
            Debug.LogError("StorySequenceManager: No se han asignado paneles de historia (Story Panel Objects). Por favor, arrastra tus GameObjects de panel a la lista.", this);
            enabled = false;
            return;
        }
        // Validar que todos los GameObjects en la lista estén asignados
        for (int i = 0; i < storyPanelObjects.Count; i++)
        {
            if (storyPanelObjects[i] == null)
            {
                Debug.LogError($"StorySequenceManager: El panel de historia en el índice {i} no está asignado.", this);
                enabled = false;
                return;
            }
        }

        if (nextButton == null || playGameplayButton == null)
        {
            Debug.LogError("StorySequenceManager: Faltan referencias a los botones 'Siguiente' o 'Jugar Gameplay'. Por favor, asígnalas en el Inspector.", this);
            enabled = false;
            return;
        }
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            Debug.LogError("StorySequenceManager: No se ha especificado un nombre de escena para 'Gameplay Scene Name'.", this);
            enabled = false;
            return;
        }

        // Asegurarse de que el botón de "Jugar Gameplay" esté oculto al inicio
        // y todos los paneles de historia también, excepto el primero.
        playGameplayButton.SetActive(false);
        nextButton.SetActive(true);

        // Desactivar todos los paneles al inicio
        foreach (GameObject panel in storyPanelObjects)
        {
            panel.SetActive(false);
        }

        // Mostrar el primer panel
        ShowPanel(currentPanelIndex);
    }

    /// <summary>
    /// Muestra el panel de historia especificado por el índice,
    /// desactivando los demás.
    /// </summary>
    private void ShowPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= storyPanelObjects.Count)
        {
            Debug.LogError($"StorySequenceManager: Índice de panel fuera de rango: {panelIndex}", this);
            return;
        }

        // Desactivar todos los paneles
        for (int i = 0; i < storyPanelObjects.Count; i++)
        {
            if (storyPanelObjects[i] != null) // Comprobación extra
            {
                storyPanelObjects[i].SetActive(false);
            }
        }

        // Activar el panel actual
        if (storyPanelObjects[panelIndex] != null)
        {
            storyPanelObjects[panelIndex].SetActive(true);
        }


        // Comprobar si es el último panel
        if (panelIndex == storyPanelObjects.Count - 1)
        {
            // Es el último panel: ocultar "Siguiente", mostrar "Jugar Gameplay"
            nextButton.SetActive(false);
            playGameplayButton.SetActive(true);
        }
        else
        {
            // No es el último panel: mostrar "Siguiente", ocultar "Jugar Gameplay"
            nextButton.SetActive(true);
            playGameplayButton.SetActive(false);
        }
    }

    /// <summary>
    /// Llamado por el botón "Siguiente". Avanza al siguiente panel.
    /// </summary>
    public void OnNextPanelClicked()
    {
        currentPanelIndex++;
        if (currentPanelIndex < storyPanelObjects.Count)
        {
            ShowPanel(currentPanelIndex);
        }
        // La lógica en ShowPanel maneja el cambio de botón en el último panel.
    }

    /// <summary>
    /// Llamado por el botón "Jugar Gameplay". Carga la escena de gameplay.
    /// </summary>
    public void OnPlayGameplayClicked()
    {
        Debug.Log($"Cargando escena de gameplay: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }
}
