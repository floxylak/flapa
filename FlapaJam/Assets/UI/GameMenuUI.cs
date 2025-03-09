using UnityEngine;
using UnityEngine.UIElements;

public class GameMenuUI : MonoBehaviour
{
    [Header("Player References")]
    public PlayerPauseManager pauseManager; // Reference to PlayerPauseManager
    public UIDocument uiDocument;          // The game menu UI
    public VisualTreeAsset optionsMenuUXML; // Options Menu UXML

    private VisualElement root;
    private VisualElement mainMenu;
    private VisualElement optionsMenu;
    private Button newGameBtn;
    private Button optionsBtn;

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            Debug.LogError("GameMenuUI: UIDocument is not assigned!");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("GameMenuUI: Root VisualElement is null!");
            return;
        }

        // Get the main menu container
        mainMenu = root.Q<VisualElement>("main-menu");
        if (mainMenu == null)
        {
            Debug.LogError("GameMenuUI: 'main-menu' element not found!");
            return;
        }

        // Get buttons from the UI
        newGameBtn = mainMenu.Q<Button>("new-game-btn");
        optionsBtn = mainMenu.Q<Button>("options-btn");

        // Register button click events
        if (newGameBtn != null) newGameBtn.clicked += StartGame;
        if (optionsBtn != null) optionsBtn.clicked += OpenOptionsMenu;

        // Ensure the Options Menu is hidden initially
        HideOptionsMenu();

        // Pause the game when the menu is active
        if (pauseManager != null)
        {
            pauseManager.Freeze();
        }
    }

    private void StartGame()
    {
        Debug.Log("GameMenuUI: Starting Game!");

        // Unfreeze the player
        if (pauseManager != null)
        {
            pauseManager.Unfreeze();
        }

        // Hide the menu UI
        root.style.display = DisplayStyle.None;
    }

    private void OpenOptionsMenu()
    {
        Debug.Log("GameMenuUI: Opening Options Menu!");

        if (optionsMenuUXML == null)
        {
            Debug.LogError("GameMenuUI: OptionsMenu UXML is not assigned!");
            return;
        }

        // Hide main menu
        mainMenu.style.display = DisplayStyle.None;

        // Create and add the Options Menu
        optionsMenu = optionsMenuUXML.Instantiate();
        root.Add(optionsMenu);

        // Find and set up the Back button
        Button backButton = optionsMenu.Q<Button>("back-btn");
        if (backButton != null) backButton.clicked += HideOptionsMenu;
    }

    private void HideOptionsMenu()
    {
        Debug.Log("GameMenuUI: Closing Options Menu!");

        // Remove options menu if it exists
        if (optionsMenu != null)
        {
            root.Remove(optionsMenu);
            optionsMenu = null;
        }

        // Show main menu again
        mainMenu.style.display = DisplayStyle.Flex;
    }
}
