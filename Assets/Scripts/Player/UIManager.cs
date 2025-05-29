using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverScreen; // Asigna esto en el Inspector

    public void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            Debug.Log("Game Over screen displayed!");
        }
    }
}