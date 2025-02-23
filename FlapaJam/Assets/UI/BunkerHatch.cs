using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player.Interact
{
    public class BunkerHatch : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Interaction Settings")]
        [SerializeField] private string targetSceneName = "BunkerScene";
        [SerializeField] private float interactionDistance = 2f; // For validation, though PlayerInteract uses its own distance

        public override string promptMessage => "Press 'E' to enter bunker"; // Added for PlayerInteract UI

        private InventoryManager inventory;
        private InputManager inputManager;

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
                    Debug.LogError("BunkerHatch: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("BunkerHatch: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("BunkerHatch: InputManager not found on player.", this);
            }
        }

        private void Update()
        {
            if (!IsValidSetup()) return;

            // Optional: Local distance check for debugging, but PlayerInteract handles interaction
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > interactionDistance)
            {
                return;
            }

            // Removed local input check; PlayerInteract handles this
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            SceneManager.LoadScene(targetSceneName);
            Debug.Log($"BunkerHatch: Loading scene: {targetSceneName}", this);
        }

        public override void BaseInteract()
        {
            Interact(); // Ensure PlayerInteract can trigger this
        }

        private bool IsValidSetup()
        {
            if (player == null)
            {
                Debug.LogWarning("BunkerHatch: Cannot function without a player reference.", this);
                return false;
            }
            if (inventory == null)
            {
                Debug.LogWarning("BunkerHatch: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("BunkerHatch: Cannot function without an InputManager.", this);
                return false;
            }
            return true;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}