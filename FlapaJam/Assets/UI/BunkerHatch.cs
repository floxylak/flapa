using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // For TextMeshPro

namespace Player.Interact
{
    public class BunkerHatch : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject interactiveCanvas; // The Canvas for the UI
        [SerializeField] private TMP_Text promptText; // TextMeshPro text for the prompt

        [Header("Interaction Settings")]
        [SerializeField] private string targetSceneName = "MAP BUNKER 2"; // Scene to load (update this to match your scene name)
        [SerializeField] private float interactionDistance = 2f; // How close the player needs to be
        [SerializeField] private KeyCode interactKey = KeyCode.E; // Key to interact

        private InventoryManager inventory;
        private InputManager inputManager;
        private bool isPlayerInRange = false;

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
                    Debug.Log("BunkerHatch: Player automatically assigned via camera hierarchy.");
                }
                else
                {
                    Debug.LogError("BunkerHatch: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually in the Inspector.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("BunkerHatch: InventoryManager not found on player. Interaction may fail.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("BunkerHatch: InputManager not found on player. Interaction may fail.", this);
            }

            if (interactiveCanvas == null)
            {
                Debug.LogWarning("BunkerHatch: Interactive Canvas is not assigned in the Inspector!", this);
            }

            if (promptText == null)
            {
                Debug.LogWarning("BunkerHatch: Prompt Text is not assigned in the Inspector!", this);
            }

            if (interactiveCanvas != null)
            {
                interactiveCanvas.SetActive(false); // Start with UI hidden
            }
        }

        private void Update()
        {
            if (!IsValidSetup()) return;

            // Check if player is in range
            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= interactionDistance;

            // Show/hide and update UI with detailed logging
            if (interactiveCanvas != null)
            {
                interactiveCanvas.SetActive(isPlayerInRange);
                Debug.Log($"BunkerHatch: Canvas active: {isPlayerInRange}, Distance: {distance}");

                if (isPlayerInRange && promptText != null)
                {
                    promptText.text = $"Press {interactKey} to enter bunker";
                    Debug.Log("BunkerHatch: Updated prompt text to 'Press E to enter bunker'");
                }
                else if (isPlayerInRange && promptText == null)
                {
                    Debug.LogWarning("BunkerHatch: Prompt Text is null, UI won't display text!", this);
                }
            }
            else
            {
                Debug.LogWarning("BunkerHatch: Interactive Canvas is null, UI won't display!", this);
            }

            // Handle interaction if not held (simplified since bunker hatch isn’t held in inventory)
            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                Interact();
                Debug.Log("BunkerHatch: Attempting to enter bunker...");
            }
        }

        private void Interact()
        {
            if (!IsValidSetup()) return;

            SceneManager.LoadScene(targetSceneName);
            Debug.Log($"BunkerHatch: Loading scene: {targetSceneName}", this);
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