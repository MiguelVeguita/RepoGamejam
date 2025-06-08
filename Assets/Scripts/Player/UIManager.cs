using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public GameObject gameOverScreen,victoryscreem; // Asigna esto en el Inspector

    //[SerializeField] TMP_Text text;
    [SerializeField] TMP_Text AweightText;
    [SerializeField] TMP_Text BweightText;

    //[SerializeField] GameObject LosePanel;
    int Aweight;
    int Bweight;
    public void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Debug.Log("Game Over screen displayed!");
        }
    }

    private void OnEnable()
    {
        Boxes.OnACollisionWeight += WeightAType;
        Boxes.OnBCollisionWeight += WeightBType;
        PlayerControllerAlt.OnLose += ShowGameOverScreen;
        Pelotita.OnLosem += ShowGameOverScreen;
        ControladorBalanza.OnVictoriaAlcanzada += victory;
    }
    private void OnDisable()
    {
        Boxes.OnACollisionWeight -= WeightAType;
        Boxes.OnBCollisionWeight -= WeightBType;
        PlayerControllerAlt.OnLose -= ShowGameOverScreen;
        Pelotita.OnLosem -= ShowGameOverScreen;
        ControladorBalanza.OnVictoriaAlcanzada -= victory;

    }
    // <<-- ESTE ES EL MÉTODO QUE FALTABA Y CAUSABA EL ERROR -->>
    public void victory(float finalScore)
    {
        if (victoryscreem != null)
        {
            victoryscreem.SetActive(true); // Activa la pantalla de victoria
        }
    }
    private void Update()
    {
        AweightText.text = ("PESO " + Aweight);
        BweightText.text = ("PESO " + Bweight);
    }
    public void WeightAType(int weight)
    {
        Aweight += weight;
    }
    public void WeightBType(int weight)
    {
        Bweight += weight;
    }
    public void victory()
    {
        if (victoryscreem!= null)
        {
            victoryscreem.SetActive(true);
            Debug.Log("ganaste papeto!");
        }
    }

}