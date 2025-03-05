using Core;
using UnityEngine;

namespace Player.Interact
{
    public class Desk : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Desk Settings")]
        [SerializeField] private float craftHoldDuration = 3f; // Time to hold E to craft

        private InventoryManager inventory;
        private PlayerInteract playerInteract;
        private TaskManager taskManager;
        private GameManager gameManager;
        private bool isCrafting = false;
        private float craftTimer = 0f;

        private void Awake()
        {
            InitializeReferences();
        }

        private void Update()
        {
            if (isCrafting && playerInteract != null && playerInteract.IsInteractHeld())
            {
                craftTimer += Time.deltaTime;
                Debug.Log($"Crafting progress: {craftTimer}/{craftHoldDuration}", this);

                if (craftTimer >= craftHoldDuration)
                {
                    CompleteCrafting();
                }
            }
            else if (isCrafting && (playerInteract == null || !playerInteract.IsInteractHeld()))
            {
                ResetCrafting();
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
                    Debug.Log("Desk: Player assigned from Camera.main.transform.parent.parent", this);
                }
                else
                {
                    Debug.LogError("Desk: Failed to assign player from Camera.main.transform.parent.parent.", this);
                    return;
                }
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Desk: InventoryManager not found on player.", this);
            }

            playerInteract = player.GetComponent<PlayerInteract>();
            if (playerInteract == null)
            {
                Debug.LogWarning("Desk: PlayerInteract not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("Desk: TaskManager not found in scene!", this);
            }

            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("Desk: GameManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (gameManager.CanCraftBomb && !isCrafting)
            {
                StartCrafting();
            }
            else if (!gameManager.CanCraftBomb)
            {
                Debug.Log("Desk: Cannot craft bomb - not all components are placed!", this);
            }
        }

        private void StartCrafting()
        {
            isCrafting = true;
            craftTimer = 0f;
            Debug.Log("Hold E to craft the bomb...", this);
        }

        private void CompleteCrafting()
        {
            isCrafting = false;

            // Destroy all SpecialItemSnap children
            Transform[] allSnaps = GetComponentsInChildren<Transform>();
            foreach (Transform snap in allSnaps)
            {
                if (snap.name.StartsWith("SpecialItemSnap"))
                {
                    Destroy(snap.gameObject);
                    Debug.Log($"Desk: Destroyed {snap.name}", this);
                }
            }

            // Spawn the bomb using prefab from GameManager
            if (gameManager != null && gameManager.Bomb != null) // Access bombPrefab via GameManager
            {
                GameObject bomb = Instantiate(gameManager.Bomb, transform.position, Quaternion.identity, transform);
                Debug.Log("Desk: Bomb crafted and spawned!", this);

                // Complete the "Assemble the Device" task
                if (taskManager != null)
                {
                    bomb.tag = "Bomb"; // Ensure the bomb has the correct tag
                    taskManager.CheckTaskProgress(bomb);
                    Debug.Log("Desk: 'Assemble the Device' task completed!", this);
                }
            }
            else
            {
                Debug.LogError("Desk: Bomb prefab not assigned in GameManager!", this);
            }
        }

        private void ResetCrafting()
        {
            isCrafting = false;
            craftTimer = 0f;
            Debug.Log("Desk: Crafting cancelled", this);
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("Desk: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (playerInteract == null)
            {
                Debug.LogWarning("Desk: Cannot function without a PlayerInteract.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("Desk: Cannot function without a TaskManager.", this);
                return false;
            }
            if (gameManager == null)
            {
                Debug.LogWarning("Desk: Cannot function without a GameManager.", this);
                return false;
            }
            return true;
        }

        // Public methods for PlayerInteract to access desk state
        public bool IsCrafting() => isCrafting;
        public float GetCraftProgress() => craftTimer;
        public float GetCraftDuration() => craftHoldDuration;
    }
}