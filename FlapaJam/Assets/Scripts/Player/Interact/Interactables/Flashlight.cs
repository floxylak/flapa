using UnityEngine;

namespace Player.Interact
{
    public class Flashlight : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Light flashlight; 

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
                    Debug.LogError("Flashlight: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }
            
            if (flashlight == null)
            {
                flashlight = GetComponent<Light>();
                if (flashlight == null)
                {
                    Debug.LogWarning("Flashlight: No Light component found. " +
                                   "Attach a Light component to this GameObject.", this);
                    return;
                }
            }
            flashlight.enabled = false;
            
            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Flashlight: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("Flashlight: InputManager not found on player.", this);
            }
        }

        private void Update()
        {
            if (!IsValidSetup()) return;
            
            if (inventory.GetHeldItem() == gameObject && 
                inputManager._inventory.Interact2.triggered)
            {
                ToggleFlashlight();
            }
            else if (inventory.GetHeldItem() != gameObject && flashlight.enabled)
            {
                flashlight.enabled = false;
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"Flashlight: Added {gameObject.name} to inventory.", this);
            }
        }

        private void ToggleFlashlight()
        {
            flashlight.enabled = !flashlight.enabled;
            Debug.Log($"Flashlight: Turned {(flashlight.enabled ? "on" : "off")}", this);
        }

        private bool IsValidSetup()
        {
            if (flashlight == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without a Light component.", this);
                return false;
            }
            if (inventory == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without an InputManager.", this);
                return false;
            }
            return true;
        }
    }
}