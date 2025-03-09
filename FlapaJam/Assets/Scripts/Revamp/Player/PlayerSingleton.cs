using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerSingleton : MonoBehaviour
    {
        public static PlayerSingleton instance;

        public PlayerDetectability detectability;
        public PlayerPauseManager pausing;
        public PlayerHealth health;
        public PlayerMovement movement;
        public PlayerCamera cam;
        public PlayerInteraction1 interact;
        public PlayerInputCont input;
        
        private GameObject currentHeldItem; // Tracks the item being held

        // Method to set the held item (called by PlayerSingleton)
        public void SetHeldItem(GameObject item)
        {
            currentHeldItem = item;
            // Add logic here, e.g., update UI, play pickup animation, etc.
            Debug.Log($"Player is now holding: {item.name}");
        }

        // Method to clear the held item (called by PlayerSingleton)
        public void ClearHeldItem()
        {
            currentHeldItem = null;
            // Add logic here, e.g., reset UI, stop holding animation, etc.
            Debug.Log("Player's hand is now empty.");
        }

        // Optional: Property to check if an item is held
        public bool IsHoldingItem => currentHeldItem != null;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Keep the player across scenes
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            CheckScene();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CheckScene();
        }

        private void CheckScene()
        {
            bool inMenu = SceneManager.GetActiveScene().name == "pridebunk";

            // Disable movement and interaction scripts in the menu
            movement.enabled = !inMenu;
            cam.enabled = !inMenu;
            interact.enabled = !inMenu;
            input.enabled = !inMenu;

            // Unlock cursor in menu, lock it in game
            if (inMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void TeleportToMenu()
        {
            SceneManager.LoadScene("pridebunk");
        }
    }
}
