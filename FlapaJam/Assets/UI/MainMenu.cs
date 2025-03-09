using UnityEngine;
using UnityEngine.SceneManagement; // For scene loading
using Player; // Import the correct namespace for PlayerMovement 

public class MainMenu : MonoBehaviour
{
    public GameObject menuUI;  // Assign the GameMenuUI in the Inspector
    public GameObject player;  // Assign the Player GameObject in the Inspector

    private PlayerCamera playerCamScript;
    private PlayerMovement playerMovementScript;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("MainMenu: Player object is not assigned in the Inspector!");
            return;
        }

        // Get PlayerCamera and PlayerMovement scripts
        playerCamScript = player.GetComponent<PlayerCamera>();
        playerMovementScript = player.GetComponent<PlayerMovement>();

        if (playerCamScript != null)
            playerCamScript.enabled = false;
        else
            Debug.LogError("MainMenu: PlayerCamera script not found!");

        if (playerMovementScript != null)
            playerMovementScript.enabled = false;
        else
            Debug.LogError("MainMenu: PlayerMovement script not found!");

        // Unlock cursor in the menu
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    public void StartGame()
    {
        Debug.Log("Starting game... Loading pridebunk scene");

        // Hide the menu UI
        if (menuUI != null)
        {
            menuUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MainMenu: menuUI is not assigned in the Inspector!");
        }

        // Load the "pridebunk" scene to ensure correct teleportation
        SceneManager.LoadScene("pridebunk");

        // Find the PlayerCamera by tag "MainCamera"
        GameObject playerCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (playerCameraObj != null)
        {
            Camera mainCamera = playerCameraObj.GetComponent<Camera>();
            if (mainCamera != null)
            {
                mainCamera.enabled = true; // Activate PlayerCamera
            }
            else
            {
                Debug.LogError("MainMenu: No Camera component found on MainCamera!");
            }
        }
        else
        {
            Debug.LogError("MainMenu: No GameObject tagged 'MainCamera' found!");
        }

        // Enable player movement and camera control
        if (playerCamScript != null)
            playerCamScript.enabled = true;

        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Lock cursor for gameplay
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public void OpenOptions()
    {
        Debug.Log("Options menu opened.");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
