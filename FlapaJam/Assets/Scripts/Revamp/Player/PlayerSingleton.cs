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
