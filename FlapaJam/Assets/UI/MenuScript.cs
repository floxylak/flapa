using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuCanvas; // Drag MainMenuCanvas here
    public Camera menuCamera;         // Drag MenuCamera here
    public Camera playerCamera;       // Drag PlayerCamera here

    void Start()
    {
        mainMenuCanvas.SetActive(true);
        menuCamera.enabled = true;
        playerCamera.enabled = false;

        // Force cursor on
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("MainMenu Start - Cursor forced unlocked and visible");
    }

    public void NewGame()
    {
        mainMenuCanvas.SetActive(false);
        menuCamera.enabled = false;
        playerCamera.enabled = true;

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("New Game - Cursor locked and hidden");
    }

    public void ContinueGame()
    {
        mainMenuCanvas.SetActive(false);
        menuCamera.enabled = false;
        playerCamera.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Continue Game - Cursor locked and hidden");
    }

    public void Options()
    {
        Debug.Log("Options clicked");
    }
}