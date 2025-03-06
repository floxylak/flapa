using UnityEngine;
using System.Collections.Generic;
using Core;
using Player; // For TaskManager

namespace Player.Interact
{
    public class RatTrap : Interactable
    {
        [Header("References")]
        [SerializeField] private GameObject ratPrefab;

        [Header("Trap Settings")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float spawnCheckInterval = 2f;
        [SerializeField] private float spawnChance = 0.2f;
        [SerializeField] private float ratCollisionRadius = 0.1f;

        private Transform player;
        private InventoryController inventory;
        private InputController inputManager;
        private TaskManager taskManager;
        private bool isPlaced = false;
        private bool isPermanentlyPlaced = false;
        private float spawnTimer;
        private List<GameObject> spawnedRats = new List<GameObject>();
        private const int MAX_RATS = 3;
        private readonly Vector3 RAT_SIZE = new Vector3(0.0027f, 0.0032f, 0.0027f);

        private readonly Vector3 corner1 = new Vector3(0.1f, -0.005f, -0.04f);
        private readonly Vector3 corner2 = new Vector3(-0.07f, -0.005f, 0.05f);

        private void Awake()
        {
            InitializeReferences();
            Debug.Log($"RatTrap: Initial state - isPlaced: {isPlaced}, isPermanentlyPlaced: {isPermanentlyPlaced}", this);
        }

        private void Update()
        {
            if (isPlaced && isPermanentlyPlaced && ratPrefab != null && spawnedRats.Count < MAX_RATS)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    Debug.Log("RatTrap: Attempting to spawn rat...", this);
                    TrySpawnRat();
                    spawnTimer = spawnCheckInterval;
                }
            }
            else
            {
                Debug.Log($"RatTrap: Update check failed - isPlaced: {isPlaced}, " +
                         $"isPermanentlyPlaced: {isPermanentlyPlaced}, " +
                         $"ratPrefab: {(ratPrefab != null)}, " +
                         $"ratCount: {spawnedRats.Count}/{MAX_RATS}", this);
            }
        }

        protected override void Interact()
        {
            Debug.Log($"RatTrap: Interact called - isPlaced: {isPlaced}, isPermanentlyPlaced: {isPermanentlyPlaced}", this);

            if (!IsValidSetup())
            {
                Debug.Log("RatTrap: Invalid setup preventing interaction", this);
                return;
            }

            if (isPermanentlyPlaced)
            {
                Debug.Log("RatTrap: Cannot pick up - trap is permanently placed", this);
                return;
            }

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"RatTrap: Added {gameObject.name} to inventory", this);
                isPlaced = false;
                isPermanentlyPlaced = false; // Ensure this stays false when picked up
                foreach (GameObject rat in spawnedRats)
                {
                    if (rat != null) Destroy(rat);
                }
                spawnedRats.Clear();
            }
            else
            {
                Debug.Log("RatTrap: Failed to add to inventory", this);
            }
        }

        public void PlaceTrap()
        {
            Debug.Log("RatTrap: PlaceTrap called", this);
            isPlaced = true;
            isPermanentlyPlaced = true;
            spawnTimer = spawnCheckInterval;
            // Change layer from "Interactable" to "Default"
            gameObject.layer = LayerMask.NameToLayer("Default");
            Debug.Log($"RatTrap: Trap permanently placed at {transform.position}, Layer changed to Default", this);

            // Complete the "Locate a Mouse Trap" task
            if (taskManager != null)
            {
                gameObject.tag = "MouseTrap"; // Ensure tag matches task
                taskManager.CheckTaskProgress(gameObject);
                Debug.Log("RatTrap: 'Locate a Mouse Trap' task completed!", this);
            }
            else
            {
                Debug.LogWarning("RatTrap: TaskManager not found, cannot complete 'Locate a Mouse Trap' task!", this);
            }
        }

        private void TrySpawnRat()
        {
            float distance = Vector3.Distance(transform.position, player.position);
            Debug.Log($"RatTrap: Player distance: {distance}, Min distance: {minDistance}", this);

            if (distance < minDistance)
            {
                Debug.Log("RatTrap: Player too close", this);
                return;
            }

            float chanceRoll = Random.value;
            Debug.Log($"RatTrap: Chance roll: {chanceRoll}, Required: <= {spawnChance}", this);
            if (chanceRoll > spawnChance)
            {
                Debug.Log("RatTrap: Chance failed", this);
                return;
            }

            Vector3 spawnPosition = GetRandomSpawnPosition();
            Debug.Log($"RatTrap: Attempting spawn at world position: {spawnPosition}", this);

            if (CanSpawnAtPosition(spawnPosition))
            {
                SpawnRat(spawnPosition);
            }
            else
            {
                Debug.Log("RatTrap: Spawn position blocked by existing rat", this);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            float x = Random.Range(Mathf.Min(corner1.x, corner2.x), Mathf.Max(corner1.x, corner2.x));
            float y = corner1.y;
            float z = Random.Range(Mathf.Min(corner1.z, corner2.z), Mathf.Max(corner1.z, corner2.z));
            Vector3 localPos = new Vector3(x, y, z);
            Vector3 worldPos = transform.TransformPoint(localPos);
            Debug.Log($"RatTrap: Local spawn pos: {localPos}, World pos: {worldPos}", this);
            return worldPos;
        }

        private bool CanSpawnAtPosition(Vector3 position)
        {
            foreach (GameObject rat in spawnedRats)
            {
                if (rat != null)
                {
                    float distToRat = Vector3.Distance(rat.transform.position, position);
                    Debug.Log($"RatTrap: Distance to existing rat: {distToRat}, Min: {ratCollisionRadius}", this);
                    if (distToRat < ratCollisionRadius)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void SpawnRat(Vector3 position)
        {
            GameObject rat = Instantiate(ratPrefab, position, Quaternion.identity, transform);
            rat.transform.localScale = RAT_SIZE;
            rat.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            spawnedRats.Add(rat);
            Debug.Log($"RatTrap: Rat spawned at {position}! Total rats: {spawnedRats.Count}", rat);
        }

        private void InitializeReferences()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != null && 
                mainCamera.transform.parent.parent != null)
            {
                player = mainCamera.transform.parent.parent;
                Debug.Log("RatTrap: Player assigned from Camera.main.transform.parent.parent", this);
            }
            else
            {
                Debug.LogError("RatTrap: Failed to assign player from Camera.main.transform.parent.parent. " +
                              "Ensure the camera is nested correctly or assign player manually in Inspector.", this);
                return;
            }
            
            
            inventory = player.GetComponent<InventoryController>();
            if (inventory == null)
            {
                Debug.LogWarning("RatTrap: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputController>();
            if (inputManager == null)
            {
                Debug.LogWarning("RatTrap: InputManager not found on player.", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("RatTrap: TaskManager not found in scene!", this);
            }
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("RatTrap: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("RatTrap: Cannot function without an InputManager.", this);
                return false;
            }
            return true;
        }
    }
}