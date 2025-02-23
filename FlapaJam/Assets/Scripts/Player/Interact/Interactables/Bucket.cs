using UnityEngine;

namespace Player.Interact
{
    public class Bucket : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

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
                    Debug.LogError("Bucket: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }
            
            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Bucket: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("Bucket: InputManager not found on player.", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;
            inventory.AddItem(gameObject); // returns true;
        }
        

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("Bucket: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("Bucket: Cannot function without an InputManager.", this);
                return false;
            }
            return true;
        }
    }
}