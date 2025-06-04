using UnityEngine;
using System.Collections; // Necesario para las Coroutines

public class ChangeScene : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject panelCine, MenuPrincipal, controles, opciones;

    [Header("Control de Escena y Efectos")]
    [Tooltip("Arrastra aquí el GameObject que tiene el script PostProcessController.")]
    [SerializeField] private PostProcessController postProcessController; // Referencia al controlador de post-procesado

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
    }

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
        // Opcional: Descomenta la siguiente línea si quieres asegurar que el vignette
        // sea visible por al menos un frame.
        // yield return null; 
        // yield return new WaitForEndOfFrame();

        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(false);
        }
        SceneGlobalManager.Instance.LoadSceneAsync(nameScene);
        yield break; // <--- AÑADIDO: Indica que la corutina ha terminado.
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
        // Opcional: yield return null;

        if (postProcessController != null)
        {
            postProcessController.SetVignetteActive(false);
        }
        SceneGlobalManager.Instance.LoaderScene(targetSceneName);
        yield break; // <--- AÑADIDO: Indica que la corutina ha terminado.
    }

    public void IniciarCine()
    {
        panelCine.SetActive(true);
        MenuPrincipal.SetActive(false);
    }

    public void IniciarControles()
    {
        controles.SetActive(true);
        MenuPrincipal.SetActive(false);
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
    }

    public void IrOpciones()
    {
        opciones.SetActive(true);
        MenuPrincipal.SetActive(false);
    }

    public void Exit()
    {
        SceneGlobalManager.Instance.QuitGame();
    }
}
