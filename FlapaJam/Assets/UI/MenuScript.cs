using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuCanvas; // Drag MainMenuCanvas here

    void Start()
    {
        Time.timeScale = 0f; // Pauses game at start
        mainMenuCanvas.SetActive(true); // Ensures menu is visible
        Debug.Log("MainMenu started - Game should be paused");
    }

    public void NewGame()
    {
        mainMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("New Game clicked");
    }

    public void ContinueGame()
    {
        mainMenuCanvas.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Continue Game clicked");
    }

    public void Options()
    {
        Debug.Log("Options clicked");
    }
}