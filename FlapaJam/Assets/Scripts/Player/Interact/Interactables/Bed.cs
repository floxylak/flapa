using UnityEngine;

namespace Player.Interact
{
    public class Bed : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        private InputManager inputManager;
        private TaskManager taskManager;
        private GameManager gameManager;
        private bool inUse = false; // Tracks if the bed has been used

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (player == null)
            {
                if (Camera.main != null && Camera.main.transform.parent != null && 
                    Camera.main.transform.parent.parent != null)
                {
                    player = Camera.main.transform.parent.parent;
                }
                else
                {
                    Debug.LogError("Bed: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("Bed: InputManager not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("Bed: TaskManager not found in scene!", this);
            }
            
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Bed: GameManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup() || inUse) return;
            UseBed();
        }

        private void UseBed()
        {
            if (inUse) return;
            
            gameManager.Sleep();

            if (gameManager.CanSleep())
            {
                Debug.Log($"Bed: Player used {gameObject.name} to rest.", this);
                inUse = true; // Mark as used (can be reset if needed)
            
            
                // Add any resting effects here (e.g., restore health, advance time)
            }
        }

        private bool IsValidSetup()
        {
            if (inputManager == null)
            {
                Debug.LogWarning("Bed: Cannot function without an InputManager.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("Bed: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }
    }
}