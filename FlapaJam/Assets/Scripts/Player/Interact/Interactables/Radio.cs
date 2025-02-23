using UnityEngine;
using System.Collections;

namespace Player.Interact
{
    public class Radio : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Radio Settings")]
        [SerializeField] private bool isBroken = true;
        [SerializeField] private float repairHoldDuration = 3f;
        [SerializeField] private string batteriesTag = "battery";
        [SerializeField] private string wiresTag = "wire";

        private InventoryManager inventory;
        private PlayerInteract playerInteract;
        private TaskManager taskManager;
        private bool isRepairing = false;
        private float repairTimer = 0f;
        private bool isPlaced = false;
        private bool isEnabled = false;

        private void Awake()
        {
            InitializeReferences();
        }

        private void Update()
        {
            if (isRepairing && playerInteract != null && playerInteract.IsInteractHeld())
            {
                repairTimer += Time.deltaTime;
                Debug.Log($"Repair progress: {repairTimer}/{repairHoldDuration}", this);
                
                if (repairTimer >= repairHoldDuration)
                {
                    CompleteRepair();
                }
            }
            else if (isRepairing && (playerInteract == null || !playerInteract.IsInteractHeld()))
            {
                ResetRepair();
            }
        }

        private void InitializeReferences()
        {
            if (player == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.transform.parent != null && 
                    mainCamera.transform.parent.parent != null)
                {
                    player = mainCamera.transform.parent.parent;
                    Debug.Log("Radio: Player assigned from Camera.main.transform.parent.parent", this);
                }
                else
                {
                    Debug.LogError("Radio: Failed to assign player from Camera.main.transform.parent.parent.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Radio: InventoryManager not found on player.", this);
            }

            playerInteract = player.GetComponent<PlayerInteract>();
            if (playerInteract == null)
            {
                Debug.LogWarning("Radio: PlayerInteract not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("Radio: TaskManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (isBroken)
            {
                if (!isRepairing)
                {
                    AttemptRepair();
                }
            }
            else if (!isPlaced)
            {
                // Allow picking up unless it's in RadioSnap with tag "Radio"
                if (transform.parent == null || 
                    !(transform.parent.CompareTag("Radio") && transform.parent.name == "RadioSnap"))
                {
                    if (inventory.AddItem(gameObject))
                    {
                        Debug.Log($"Radio: Added {gameObject.name} to inventory.", this);
                    }
                }
                else
                {
                    Debug.Log("Radio: Cannot pick up - placed in RadioSnap.", this);
                }
            }
        }

        private void AttemptRepair()
        {
            if (HasRequiredItems())
            {
                if (!isRepairing)
                {
                    isRepairing = true;
                    repairTimer = 0f;
                    Debug.Log("Hold E to repair the radio...", this);
                }
            }
            else
            {
                Debug.Log("Radio: Requires Battery and Wire to repair!", this);
            }
        }

        private bool HasRequiredItems()
        {
            return inventory.HasItem(batteriesTag) && inventory.HasItem(wiresTag);
        }

        private void CompleteRepair()
        {
            isBroken = false;
            isRepairing = false;
            inventory.RemoveItem(batteriesTag);
            inventory.RemoveItem(wiresTag);
            Debug.Log("Radio: Successfully repaired!", this);

            // Complete the "Locate and Repair Radio" task
            if (taskManager != null)
            {
                gameObject.tag = "RadioBroken"; // Temporarily set tag to match task
                taskManager.CheckTaskProgress(gameObject);
                gameObject.tag = "Radio"; 
            }
        }

        private void ResetRepair()
        {
            isRepairing = false;
            repairTimer = 0f;
            Debug.Log("Radio: Repair cancelled", this);
        }

        public void ToggleRadio()
        {
            if (!isBroken && isPlaced)
            {
                // Only allow toggling if placed in RadioSnap with tag "Radio"
                if (transform.parent != null && 
                    transform.parent.CompareTag("Radio") && 
                    transform.parent.name == "RadioSnap")
                {
                    isEnabled = !isEnabled;
                    Debug.Log($"Radio: {(isEnabled ? "Enabled" : "Disabled")}", this);
                    if (isEnabled)
                    {
                        Debug.Log("Radio: Playing radio sounds...", this);
                        // Add code to start audio or enable radio behavior
                    }
                    else
                    {
                        Debug.Log("Radio: Stopped radio sounds...", this);
                        // Add code to stop audio or disable radio behavior
                    }
                }
                else
                {
                    Debug.Log("Radio: Cannot toggle - must be placed in RadioSnap.", this);
                }
            }
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("Radio: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (playerInteract == null)
            {
                Debug.LogWarning("Radio: Cannot function without a PlayerInteract.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("Radio: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }

        public void PlaceRadio()
        {
            isPlaced = true;
            isEnabled = false; // Radio starts disabled when placed
            Debug.Log("Radio: Placed in the world.", this);

            // Check if placed in a RadioSnap with tag "Radio" and complete "Extract Radio" task
            if (transform.parent != null && 
                transform.parent.CompareTag("Radio") && 
                transform.parent.name == "RadioSnap")
            {
                if (taskManager != null)
                {
                    gameObject.tag = "Radio"; // Ensure tag matches task
                    taskManager.CheckTaskProgress(gameObject);
                    Debug.Log("Radio: 'Extract Radio' task completed!", this);
                }
            }
        }

        // Public methods for PlayerInteract to access radio state
        public bool IsRepairing() => isRepairing;
        public float GetRepairProgress() => repairTimer;
        public float GetRepairDuration() => repairHoldDuration;
        public bool IsBroken() => isBroken;
        public bool IsPlaced() => isPlaced;
        public bool IsEnabled() => isEnabled;
    }
}