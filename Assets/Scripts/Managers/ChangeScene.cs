using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject panelCine, MenuPrincipal, controles, opciones;

    [Header("Control de Escena y Efectos")]
    [Tooltip("Arrastra aquí el GameObject que tiene el script PostProcessController.")]
    [SerializeField] private PostProcessController postProcessController;

    [Header("Managers Visuales")] // Nuevo Header
    [Tooltip("Arrastra aquí el GameObject que tiene el script MenuVisualEffectsManager.")]
    [SerializeField] private MenuVisualEffectsManager menuVisualEffectsManager; // Referencia al manager de efectos

    void Start()
    {
        MenuPrincipal.SetActive(true);
        panelCine.SetActive(false);
        controles.SetActive(false);
        opciones.SetActive(false);

        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(false);
        }

        // Asegurarse que los efectos de menú estén activos al inicio si deben estarlo
        if (menuVisualEffectsManager != null)
        {
            menuVisualEffectsManager.ActivateFallingObjects();
        }
    }

    // ... (LoadScene, LoadAsync, LoaderSceneAsync y sus corutinas se mantienen igual que antes) ...
    public void LoadScene(string nameScene)
    {
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(true);
        }
        SceneGlobalManager.Instance.LoadScene(nameScene);
    }

    public void LoadAsync(string nameScene)
    {
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(true);
        }
        else
        {
            Debug.LogWarning("PostProcessController no asignado en ChangeScene. No se controlará el Vignette.");
        }
        StartCoroutine(DoLoadAsyncWithVignetteControl(nameScene));
    }

    private IEnumerator DoLoadAsyncWithVignetteControl(string nameScene)
    {
        // yield return null; 
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(false);
        }
        SceneGlobalManager.Instance.LoadSceneAsync(nameScene);
        yield break;
    }

    public void LoaderSceneAsync(string targetSceneName)
    {
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(true);
        }
        else
        {
            Debug.LogWarning("PostProcessController no asignado en ChangeScene. No se controlará el Vignette.");
        }
        StartCoroutine(DoLoaderSceneAsyncWithVignetteControl(targetSceneName));
    }

    private IEnumerator DoLoaderSceneAsyncWithVignetteControl(string targetSceneName)
    {
        // yield return null;
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(false);
        }
        SceneGlobalManager.Instance.LoaderScene(targetSceneName);
        yield break;
    }

    public void IniciarCine()
    {
        panelCine.SetActive(true);
        MenuPrincipal.SetActive(false);
        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(true);
        }
        else
        {
            Debug.LogWarning("PostProcessController no asignado en ChangeScene. No se controlará el Vignette.");
        }
        // Detener los efectos de caída de objetos
        if (menuVisualEffectsManager != null)
        {
            menuVisualEffectsManager.DeactivateFallingObjects(true); // true para limpiar objetos activos
        }
    }

    public void IniciarControles()
    {
        controles.SetActive(true);
        MenuPrincipal.SetActive(false);
        // Quizás también quieras detener los efectos aquí si la pantalla de controles es "limpia"
        // if (menuVisualEffectsManager != null)
        // {
        //     menuVisualEffectsManager.DeactivateFallingObjects(true);
        // }
    }

    public void IrMenuinicio()
    {
        controles.SetActive(false);
        MenuPrincipal.SetActive(true);
        opciones.SetActive(false);
        if (panelCine.activeSelf)
        {
            panelCine.SetActive(false);
        }

        // Reactivar los efectos de caída de objetos al volver al menú principal
        if (menuVisualEffectsManager != null)
        {
            menuVisualEffectsManager.ActivateFallingObjects();
        }
    }

    public void IrOpciones()
    {
        opciones.SetActive(true);
        MenuPrincipal.SetActive(false);
        // Quizás también quieras detener los efectos aquí
        // if (menuVisualEffectsManager != null)
        // {
        //     menuVisualEffectsManager.DeactivateFallingObjects(true);
        // }
    }

    public void Exit()
    {
        SceneGlobalManager.Instance.QuitGame();
    }
}
