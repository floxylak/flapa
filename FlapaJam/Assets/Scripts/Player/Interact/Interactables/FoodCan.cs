using Core;
using UnityEngine;

namespace Player.Interact
{
    public class FoodCan : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Appearance")]
        [SerializeField] private Material[] canMaterials; // Array of 3 materials to randomize between

        private InventoryManager inventory;
        private InputManager inputManager;
        private TaskManager taskManager;
        private bool isUsed = false; // Tracks if the can has been used
        private bool taskContributed = false;

        private void Awake()
        {
            InitializeReferences();
        }

        private void Start()
        {
            // Randomize material when the object is instantiated
            if (canMaterials != null && canMaterials.Length > 0)
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    int randomIndex = Random.Range(0, canMaterials.Length);
                    renderer.material = canMaterials[randomIndex];
                    Debug.Log($"FoodCan: Applied material {canMaterials[randomIndex].name} to {gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("FoodCan: No Renderer found on this object to apply material!", this);
                }
            }
            else
            {
                Debug.LogWarning("FoodCan: No materials assigned to canMaterials array!", this);
            }
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
                    Debug.LogError("FoodCan: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("FoodCan: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("FoodCan: InputManager not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("FoodCan: TaskManager not found in scene!", this);
            }
        }

        private void Update()
        {
            if (!IsValidSetup() || isUsed) return;

            if (inventory.GetHeldItem() == gameObject && 
                inputManager.OnFoot.Interact.triggered)
            {
                UseFoodCan();
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup() || isUsed) return;

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"FoodCan: Added {gameObject.name} to inventory.", this);
                
                if (taskManager != null && !taskContributed)
                {
                    taskManager.CheckTaskProgress(gameObject);
                    taskContributed = true;
                }
            }
        }

        private void UseFoodCan()
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            inventory.DropItem(); 
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            Destroy(gameObject);
            
            Debug.Log($"FoodCan: {gameObject.name} used and disabled.", this);
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("FoodCan: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("FoodCan: Cannot function without an InputManager.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("FoodCan: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }
    }
}