/*using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Player
{
    public class GameManager : MonoBehaviour
    {
        [Header("Day Settings")]
        [SerializeField] private int maxDays = 5;
        [SerializeField] private float dayLength = 60f * 10f;

        [Header("Pipe Leak Settings")]
        [SerializeField] private GameObject pipeLeakPrefab;

        [Header("Safe Room Settings")]
        [SerializeField] private GameObject safeRoomTrigger; // Reference to the trigger object

        private Transform safeRoomSpawn;
        private Transform player;
        private TaskManager taskManager;
        private GameObject bomb;
        public GameObject Bomb => bomb;
        
        private List<GameObject> specialItems = new List<GameObject>();
        private List<GameObject> taskItems = new List<GameObject>();
        private List<Transform> itemSpawnPoints = new List<Transform>();
        private List<GameObject> collectedItems = new List<GameObject>();
        private List<Transform> waterSpawnPoints = new List<Transform>();
        private List<GameObject> activePipeLeaks = new List<GameObject>();
        private List<GameObject> spawnedSpecialItems = new List<GameObject>();
        
        private int currentDay = 0;
        private bool safeRoomLocked = false;
        private bool gameEnded = false;
        private bool canCraftBomb = false;
        private bool isPlayerInSafeRoom = false;

        private InputController inputManager;
        
        public int CurrentDay => currentDay;
        public List<GameObject> TaskItems => taskItems;
        public bool CanCraftBomb => canCraftBomb;
        public int MaxDays => maxDays;

        private void Awake()
        {
            player = GameObject.Find("Player")?.transform;
            inputManager = player?.GetComponent<InputController>();
            taskManager = GameObject.Find("TaskManager")?.GetComponent<TaskManager>();
            
            if (inputManager == null) Debug.LogWarning("InputManager not found on player!", this);
            if (taskManager == null) Debug.LogWarning("TaskManager not assigned!", this);
        }

        private void Start()
        {
            InitializeSafeRoom();
            InitializeTaskItems();
            InitializeSpecialItems();
            InitializeSpawnPoints();
            InitializeWaterSpawnPoints(); 
            
            currentDay = 0;
            gameEnded = false;
            canCraftBomb = false;
            StartNewDay();
        }

        private void InitializeSafeRoom()
        {
            safeRoomSpawn = transform.Find("Bed/BedSpawnPoint");
            if (safeRoomSpawn == null) Debug.LogWarning("SafeRoomSpawn not found!", this);

            if (safeRoomTrigger == null)
            {
                safeRoomTrigger = transform.Find("SafeRoomTrigger")?.gameObject;
                if (safeRoomTrigger == null) Debug.LogWarning("SafeRoomTrigger not assigned or found!", this);
                else if (!safeRoomTrigger.GetComponent<Collider>()) Debug.LogWarning("SafeRoomTrigger has no Collider!", this);
                else if (!safeRoomTrigger.GetComponent<Collider>().isTrigger) Debug.LogWarning("SafeRoomTrigger Collider is not set to IsTrigger!", this);
            }
        }

        private void InitializeTaskItems()
        {
            Transform taskItemsParent = transform.Find("TaskItems");
            if (taskItemsParent == null)
            {
                Debug.LogError("TaskItems GameObject not found as child of GameManager!", this);
                return;
            }

            foreach (Transform child in taskItemsParent)
            {
                taskItems.Add(child.gameObject);
                Debug.Log($"Added task item: {child.gameObject.name} with tag: {child.gameObject.tag}", this);
            }
        }

        private void InitializeSpecialItems()
        {
            Transform specialItemsParent = transform.Find("SpecialItems");
            if (specialItemsParent == null)
            {
                Debug.LogWarning("SpecialItems not found!", this);
                return;
            }

            foreach (Transform child in specialItemsParent)
            {
                specialItems.Add(child.gameObject);
            }
        }

        private void InitializeSpawnPoints()
        {
            Transform itemSpawnPointsParent = transform.Find("ItemSpawnPoints");
            if (itemSpawnPointsParent == null)
            {
                Debug.LogWarning("ItemSpawnPoints not found!", this);
                return;
            }

            foreach (Transform child in itemSpawnPointsParent)
            {
                itemSpawnPoints.Add(child);
                Debug.Log($"Added spawn point: {child.name}", this);
            }
        }

        private void InitializeWaterSpawnPoints()
        {
            Transform waterSpawnPointsParent = transform.Find("WaterSpawnPoints");
            if (waterSpawnPointsParent == null)
            {
                Debug.LogWarning("WaterSpawnPoints not found!", this);
                return;
            }

            foreach (Transform child in waterSpawnPointsParent)
            {
                waterSpawnPoints.Add(child);
                Debug.Log($"Added water spawn point: {child.name}", this);
            }
        }

        private void StartNewDay()
        {
            currentDay++;
            Debug.Log($"Starting Day {currentDay}", this);

            safeRoomLocked = false;
            ResetPlayerPosition();
            SpawnSpecialItemsForDay();
            AssignAndSpawnTasks();
            SpawnNewPipeLeak();
            UpdateSafeRoomState();
            isPlayerInSafeRoom = true; // Player starts in safe room
        }

        private void SpawnSpecialItemsForDay()
        {
            int spawnCount;
            if (currentDay == maxDays) // Day 5: Spawn all unspawned special items
            {
                spawnCount = specialItems.Count - spawnedSpecialItems.Count;
                Debug.Log($"Day {currentDay}: Spawning all remaining {spawnCount} special items.");
            }
            else // Days 1-4: Randomly spawn 0, 1, or 2
            {
                spawnCount = Random.Range(0, 3); // 0, 1, or 2
                Debug.Log($"Day {currentDay}: Randomly spawning {spawnCount} special items.");
            }

            SpawnSpecialItems(spawnCount);
        }

        private void SpawnNewPipeLeak()
        {
            if (pipeLeakPrefab == null)
            {
                Debug.LogError("PipeLeak prefab not assigned in GameManager!", this);
                return;
            }

            if (waterSpawnPoints.Count == 0)
            {
                Debug.LogError("No water spawn points available for pipe leaks!", this);
                return;
            }

            List<Transform> availableWaterSpawnPoints = waterSpawnPoints.FindAll(spawn => spawn.childCount == 0);
            if (availableWaterSpawnPoints.Count == 0)
            {
                Debug.LogWarning("No available water spawn points for new pipe leak!", this);
                return;
            }
            
            int spawnIndex = Random.Range(0, availableWaterSpawnPoints.Count);
            Transform spawnPoint = availableWaterSpawnPoints[spawnIndex];

            GameObject newPipeLeak = Instantiate(pipeLeakPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
            activePipeLeaks.Add(newPipeLeak);
            Debug.Log($"Spawned new PipeLeak at {spawnPoint.name} on Day {currentDay}. Total active leaks: {activePipeLeaks.Count}", this);
        }

        private void ResetPlayerPosition()
        {
            if (player != null && safeRoomSpawn != null)
            {
                player.position = safeRoomSpawn.position;
                player.rotation = safeRoomSpawn.rotation;
            }
        }

        private void AssignAndSpawnTasks()
        {
            if (taskManager == null)
            {
                Debug.LogError("TaskManager is null, cannot spawn task items!", this);
                return;
            }

            taskManager.AssignNewTasks(currentDay);
            SpawnTaskItems();
        }

        private void SpawnTaskItems()
        {
            if (taskItems.Count == 0 || itemSpawnPoints.Count == 0) return;

            List<Task> currentTasks = taskManager.GetCurrentTasks();
            Debug.Log($"Spawning items for {currentTasks.Count} tasks on Day {currentDay}", this);

            foreach (Task task in currentTasks)
            {
                if (task.GetCount() == 0) continue;
                SpawnTaskItem(task);
            }
        }

        private void SpawnTaskItem(Task task)
        {
            Debug.Log($"Processing task: {task.Name} with target tag: {task.TargetObjectTag}", this);

            GameObject taskItemPrefab = taskItems.Find(item => item.CompareTag(task.TargetObjectTag));
            if (taskItemPrefab == null)
            {
                Debug.LogError($"No task item found with tag '{task.TargetObjectTag}' for task '{task.Name}'!", this);
                return;
            }

            int spawnCount = CalculateSpawnCount(task);
            List<Transform> availableSpawnPoints = itemSpawnPoints.FindAll(spawn => spawn.childCount == 0);
            spawnCount = Mathf.Min(spawnCount, availableSpawnPoints.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[spawnIndex];
                availableSpawnPoints.RemoveAt(spawnIndex);

                GameObject spawnedItem = Instantiate(taskItemPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
                Debug.Log($"Spawned {spawnedItem.name} at {spawnPoint.name} for task '{task.Name}'", this);
            }
        }

        private int CalculateSpawnCount(Task task)
        {
            int spawnCount = task.GetCount();
            return task.GetCount() > 1 ? spawnCount + Random.Range(0, 3) : spawnCount;
        }

        private Transform FindAvailableSpawnPoint()
        {
            if (itemSpawnPoints.Count == 0) return null;

            int spawnIndex = Random.Range(0, itemSpawnPoints.Count);
            int attempts = 0;

            while (itemSpawnPoints[spawnIndex].childCount > 0 && attempts < itemSpawnPoints.Count)
            {
                spawnIndex = Random.Range(0, itemSpawnPoints.Count);
                attempts++;
            }

            return attempts < itemSpawnPoints.Count ? itemSpawnPoints[spawnIndex] : null;
        }

        private void Update()
        {
            if (gameEnded) return;
            if (!safeRoomLocked && !isPlayerInSafeRoom) LockSafeRoom();
            CheckDeskForBombCrafting();
        }

        // New method to be called by SafeRoomTrigger
        public void SetPlayerInSafeRoom(bool inSafeRoom)
        {
            isPlayerInSafeRoom = inSafeRoom;
            Debug.Log($"Player {(inSafeRoom ? "entered" : "exited")} the safe room.", this);
        }

        private bool IsPlayerInSafeRoom()
        {
            return isPlayerInSafeRoom;
        }

        private void SpawnSpecialItems(int count)
        {
            if (specialItems.Count == 0 || itemSpawnPoints.Count == 0) return;

            List<GameObject> itemsToSpawn = new List<GameObject>();

            if (currentDay == maxDays) // Day 5: Spawn all unspawned items
            {
                foreach (GameObject item in specialItems)
                {
                    if (!spawnedSpecialItems.Contains(item) && !collectedItems.Contains(item))
                    {
                        itemsToSpawn.Add(item);
                    }
                }
            }
            else // Days 1-4: Randomly select up to 'count' unique items
            {
                List<GameObject> availableItems = specialItems.FindAll(item => !spawnedSpecialItems.Contains(item) && !collectedItems.Contains(item));
                for (int i = 0; i < count && availableItems.Count > 0; i++)
                {
                    int index = Random.Range(0, availableItems.Count);
                    itemsToSpawn.Add(availableItems[index]);
                    availableItems.RemoveAt(index);
                }
            }

            foreach (GameObject specialItem in itemsToSpawn)
            {
                Transform spawnPoint = FindAvailableSpawnPoint();
                if (spawnPoint == null) continue;

                GameObject spawnedItem = Instantiate(specialItem, spawnPoint.position, Quaternion.identity, spawnPoint);
                spawnedSpecialItems.Add(specialItem); // Track as spawned
                Debug.Log($"Spawned {spawnedItem.name} on Day {currentDay}", this);
            }
        }

        private bool IsItemInWorld(GameObject item)
        {
            return GameObject.FindGameObjectsWithTag(item.tag).Length > 0;
        }

        private void UpdateSafeRoomState()
        {
            float deterioration = (float)currentDay / maxDays;
            Debug.Log($"Safe Room: {(safeRoomLocked ? "Locked" : "Unlocked")}. Deterioration: {deterioration:P0}", this);
        }

        private void LockSafeRoom()
        {
            safeRoomLocked = true;
            Debug.Log("Safe room locked behind you.", this);
        }

        public void UnlockSafeRoom()
        {
            safeRoomLocked = false;
            Debug.Log("All mandatory tasks completed. Safe room unlocked.", this);
        }

        public void CollectItem(GameObject item)
        {
            if (specialItems.Contains(item) && !collectedItems.Contains(item))
            {
                collectedItems.Add(item);
                item.SetActive(false);
                Debug.Log($"Collected {item.name}. Total: {collectedItems.Count}/{specialItems.Count}", this);
            }
            taskManager?.CheckTaskProgress(item);
        }

        public bool CanSleep() => taskManager?.AreMandatoryTasksCompleted() ?? false;

        public void Sleep()
        {
            if (CanSleep())
            {
                Debug.Log("Player sleeps... Day ends.", this);
                Invoke(nameof(StartNewDay), 2f);
            }
            else
            {
                Debug.Log("Player tries to sleep but tasks are incomplete.", this);
            }
        }
        
        public void GameOver()
        {
            gameEnded = true;
            Debug.Log("Game Over - Player caught by HunterGhost!");
            // SceneManager.LoadScene("GameOverScene"); // Uncomment if you have a game over scene
        }

        private void EndSequence()
        {
            Debug.Log("The main character used the bomb... *silence*", this);
        }

        private void CheckDeskForBombCrafting()
        {
            GameObject desk = GameObject.Find("Desk");
            if (desk == null)
            {
                Debug.LogWarning("Desk object not found in scene!", this);
                return;
            }

            Transform[] allSnaps = desk.GetComponentsInChildren<Transform>();
            bool allFilled = true;

            foreach (Transform snap in allSnaps)
            {
                if (snap.name.StartsWith("SpecialItemSnap") && snap.childCount == 0)
                {
                    allFilled = false;
                    break;
                }
            }

            if (allFilled && !canCraftBomb)
            {
                canCraftBomb = true;
                Debug.Log("Bomb is ready to be crafted", this);
                CompleteBombItemTask(); // Complete "Find Bomb Components" task
            }
            else if (!allFilled && canCraftBomb)
            {
                canCraftBomb = false;
                Debug.Log("Bomb crafting no longer available - Desk incomplete", this);
            }
        }

        private void CompleteBombItemTask()
        {
            if (taskManager != null && currentDay == maxDays) // Ensure it's Day 5
            {
                taskManager.CheckTaskProgress(bomb);
                Debug.Log("GameManager: 'Find Bomb Components' task completed as bomb is ready to be crafted.");
            }
        }

        private void OnValidate()
        {
            if (maxDays < 1) maxDays = 1;
            if (dayLength < 1f) dayLength = 180f;
            if (pipeLeakPrefab == null) Debug.LogWarning("PipeLeak prefab not assigned in GameManager!", this);
            if (safeRoomTrigger == null) Debug.LogWarning("SafeRoomTrigger not assigned in GameManager!", this);
        }
    }
}*/