/*using Core;
using UnityEngine;
using Player; // For TaskManager

namespace Player.Interact
{
    public class Rat : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        private InventoryController inventory;
        private InputController inputManager;
        private TaskManager taskManager;
        private bool taskContributed = false; // Flag to track if this rat has contributed to the task

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            // Get player from Camera.main.transform.parent.parent if not assigned
            if (player == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.transform.parent != null && 
                    mainCamera.transform.parent.parent != null)
                {
                    player = mainCamera.transform.parent.parent;
                    Debug.Log("Rat: Player assigned from Camera.main.transform.parent.parent", this);
                }
                else
                {
                    Debug.LogError("Rat: Failed to assign player from Camera.main.transform.parent.parent. " +
                                  "Ensure the camera is nested correctly or assign player manually in Inspector.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryController>();
            if (inventory == null)
            {
                Debug.LogWarning("Rat: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputController>();
            if (inputManager == null)
            {
                Debug.LogWarning("Rat: InputManager not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("Rat: TaskManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"Rat: Added {gameObject.name} to inventory.", this);
                
                if (taskManager != null && !taskContributed)
                {
                    gameObject.tag = "Rat"; 
                    taskManager.CheckTaskProgress(gameObject);
                    taskContributed = true;
                    Debug.Log("Rat: Contributed to 'Scavenge for food' task.", this);
                }
                else if (taskManager == null)
                {
                    Debug.LogWarning("Rat: TaskManager not found, cannot update 'Scavenge for food' task!", this);
                }
                else if (taskContributed)
                {
                    Debug.Log("Rat: Task already contributed by this rat.", this);
                }
            }
            else
            {
                Debug.Log("Rat: Failed to add to inventory.", this);
            }
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("Rat: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("Rat: Cannot function without an InputManager.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("Rat: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }
    }
}*/