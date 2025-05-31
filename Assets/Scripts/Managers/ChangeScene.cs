using UnityEngine;

public class ChangeScene: MonoBehaviour
{
    [SerializeField] private GameObject panelCine,MenuPrincipal,controles,opciones;
    void Start()
    {
        MenuPrincipal.SetActive(true);
        panelCine.SetActive(false);
        controles.SetActive(false);
    }
    void Update()
    {
        
    }
    public void LoadScene(string nameScene)
    {
        SceneGlobalManager.Instance.LoadScene(nameScene);
    }
    public void LoadAsync(string nameScene) 
    { 
     SceneGlobalManager.Instance.LoadSceneAsync(nameScene);
    }
    public void LoaderSceneAsync(string target)
    {
        SceneGlobalManager.Instance.LoaderScene(target);
    }
    public void IniciarCine()
    {
        panelCine.SetActive (true);
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
