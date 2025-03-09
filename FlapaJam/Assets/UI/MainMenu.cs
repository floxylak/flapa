using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [Header("UI Documents")]
    public UIDocument uiDocument;         // Main menu UI Document
    public VisualTreeAsset optionsMenuUXML; // Options Menu UXML

    [Header("Player References")]
    public PlayerPauseManager pauseManager; // Reference to PlayerPauseManager
    public GameObject playerCamera;      // Player's actual game camera
    public GameObject menuCamera;        // Menu camera that shows the UI

    private VisualElement root;
    private VisualElement mainMenu;
    private VisualElement optionsMenu;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("MainMenu: UIDocument is not assigned!");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("MainMenu: Root VisualElement is null!");
            return;
        }

        // Get main menu container
        mainMenu = root.Q<VisualElement>("main-menu");
        if (mainMenu == null)
        {
            Debug.LogError("MainMenu: 'main-menu' element not found!");
            return;
        }

        // Get buttons
        Button newGameButton = mainMenu.Q<Button>("new-game-btn");
        Button optionsButton = mainMenu.Q<Button>("option-btn");
        Button quitButton = mainMenu.Q<Button>("quit-btn");

        if (newGameButton != null) newGameButton.clicked += StartGame;
        if (optionsButton != null) optionsButton.clicked += OpenOptionsMenu;
        if (quitButton != null) quitButton.clicked += QuitGame;

        // Ensure Options Menu is hidden
        HideOptionsMenu();

        // Pause the game initially
        if (pauseManager != null)
        {
            pauseManager.Freeze();
        }

        // Ensure the correct camera is enabled
        if (menuCamera != null) menuCamera.SetActive(true);
        if (playerCamera != null) playerCamera.SetActive(false);
    }

    private void StartGame()
    {
        Debug.Log("MainMenu: Starting Game!");

        // Unfreeze player controls
        if (pauseManager != null)
        {
            pauseManager.Unfreeze();
        }

        // Switch cameras
        if (menuCamera != null) menuCamera.SetActive(false);
        if (playerCamera != null) playerCamera.SetActive(true);

        // Hide UI
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    private void OpenOptionsMenu()
    {
        Debug.Log("MainMenu: Opening Options Menu!");

        if (optionsMenuUXML == null)
        {
            Debug.LogError("MainMenu: OptionsMenu UXML is not assigned!");
            return;
        }

        // Hide main menu
        mainMenu.style.display = DisplayStyle.None;

        // Create and add Options Menu
        optionsMenu = optionsMenuUXML.Instantiate();
        root.Add(optionsMenu);

        // Find and set up the Back button
        Button backButton = optionsMenu.Q<Button>("back-btn");
        if (backButton != null) backButton.clicked += HideOptionsMenu;
    }

    private void HideOptionsMenu()
    {
        Debug.Log("MainMenu: Closing Options Menu!");

        // Remove options menu if it exists
        if (optionsMenu != null)
        {
            root.Remove(optionsMenu);
            optionsMenu = null;
        }

        // Show main menu
        mainMenu.style.display = DisplayStyle.Flex;
    }

    private void QuitGame()
    {
        Debug.Log("MainMenu: Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
