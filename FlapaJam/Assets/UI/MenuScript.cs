using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public Camera menuCamera;
    public Camera playerCamera;

    void Start()
    {
        mainMenuCanvas.SetActive(true);
        menuCamera.enabled = true;
        playerCamera.enabled = false;
        Debug.Log("MainMenu Start - Menu camera on");
    }

    void Update()
    {
        // Check if Enter key is pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        mainMenuCanvas.SetActive(false);
        menuCamera.enabled = false;
        playerCamera.enabled = true;
        Debug.Log("Game Started - Player camera on");
    }

    // Keeping these methods in case you want to use them later
    public void NewGame()
    {
        StartGame();
    }

    public void ContinueGame()
    {
        StartGame();
    }

    public void Options()
    {
        Debug.Log("Options clicked");
    }
}