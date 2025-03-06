using Core;
using UnityEngine;

namespace Player.Interact
{
    public class SpecialItem : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        private InventoryController inventory;
        private InputController inputManager;
        private TaskManager taskManager;

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
                    Debug.LogError("SpecialItem: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryController>();
            if (inventory == null)
            {
                Debug.LogWarning("SpecialItem: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputController>();
            if (inputManager == null)
            {
                Debug.LogWarning("SpecialItem: InputManager not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("SpecialItem: TaskManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"SpecialItem: Added {gameObject.name} to inventory.", this);
                
                if (taskManager != null)
                {
                    taskManager.CheckTaskProgress(gameObject);
                }
            }
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("SpecialItem: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("SpecialItem: Cannot function without an InputManager.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("SpecialItem: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }
    }
}