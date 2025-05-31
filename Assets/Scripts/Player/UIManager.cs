using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public GameObject gameOverScreen; // Asigna esto en el Inspector

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
    }
    private void OnDisable()
    {
        Boxes.OnACollisionWeight -= WeightAType;
        Boxes.OnBCollisionWeight -= WeightBType;
        PlayerControllerAlt.OnLose -= ShowGameOverScreen;
    }
    private void Update()
    {
        AweightText.text = ("weight " + Aweight);
        BweightText.text = ("weight " + Bweight);
    }
    public void WeightAType(int weight)
    {
        Aweight += weight;
    }
    public void WeightBType(int weight)
    {
        Bweight += weight;
    }

}