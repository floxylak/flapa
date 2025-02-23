using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    public class TaskManager : MonoBehaviour
    {
        [Header("Task Settings")]
        [SerializeField] private GameObject notePrefab; // Prefab for task notes
        [SerializeField] private List<Transform> noteSpawnPoints; // Locations where notes spawn
        [SerializeField] private int maxTasksPerDay = 3; // Maximum tasks per day
        
        private Task[,] dailyTasks; // 2D array of tasks [day, task index]
        private int currentDay = -1; // Current day index (0-based)
        private GameManager gameManager;

        private void Awake()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null) Debug.LogWarning("TaskManager: GameManager not found!", this);
            InitializeTasks();
        }
        
        private void InitializeTasks()
        {
            dailyTasks = new Task[5, 3]
            {
                // Day 1
                {
                    new Task("Collect Food", "Search for food.", 3, true, "FoodItem"),
                    new Task("Gather Water", "Locate drinkable water.", 1, true, "WaterBucket"),
                    new Task("Find a flashlight", "Search the area for a flashlight.", 1, false, "Flashlight")
                },
                // Day 2
                {
                    new Task("Locate and Repair Radio", "Find the damaged radio and repair it.", 1, true, "RadioBroken"),
                    new Task("Extract Radio", "Carry the radio to the safe room.", 1, true, "Radio"),
                    null
                },
                // Day 3
                {
                    new Task("Locate and Place a Mouse Trap", "Place a mouse trap.", 1, true, "MouseTrap"),
                    new Task("Scavenge for food", "Search for more food.", 2, true, "Rat"),
                    new Task("Prepare the Rat", "Cook the dead rat.", 0, true, "RatCooked")
                },
                // Day 4
                {
                    new Task("Unlock the Exit", "Enter the access code.", 1, true, "ExitDoor"),
                    new Task("Escape The Bunker", "Use the exit door.", 1, true, "Exit"),
                    null
                },
                // Day 5
                {
                    new Task("Find Bomb Components", "Search for bomb parts.", 1, true, "BombItem"),
                    new Task("Assemble the Device", "Craft the bomb.", 1, true, "Bomb"),
                    new Task("End the Cycle", "Make your final decision.", 1, true, "BombExplode")
                }
            };
        }
        public void AssignNewTasks(int day)
        {
            currentDay = day - 1; // Convert to 0-based index
            if (currentDay >= dailyTasks.GetLength(0))
            {
                Debug.LogWarning($"TaskManager: Day {day} exceeds task array length ({dailyTasks.GetLength(0)} days)!", this);
                return;
            }

            List<Task> currentTasks = GetCurrentTasks();
            for (int i = 0; i < currentTasks.Count && i < noteSpawnPoints.Count; i++)
            {
                if (notePrefab != null && noteSpawnPoints[i] != null)
                {
                    GameObject note = Instantiate(notePrefab, noteSpawnPoints[i].position, Quaternion.identity);
                    // TODO: Add a component to the note to display task info (e.g., TaskNote script)
                    Debug.Log($"Spawned note for task: {currentTasks[i].Name}", note);
                }
                else
                {
                    Debug.LogWarning($"Failed to spawn note for task {currentTasks[i].Name}: notePrefab or spawn point is null!", this);
                }
            }
        }
        
        public bool AreMandatoryTasksCompleted()
        {
            if (!IsValidDay()) return false;

            for (int i = 0; i < dailyTasks.GetLength(1); i++)
            {
                Task task = dailyTasks[currentDay, i];
                if (task != null && task.IsMandatory && !task.IsCompleted)
                {
                    return false;
                }
            }
            return true;
        }
        
        public void CheckTaskProgress(GameObject interactedObject)
        {
            if (!IsValidDay()) return;

            for (int i = 0; i < dailyTasks.GetLength(1); i++)
            {
                Task task = dailyTasks[currentDay, i];
                if (task != null && !task.IsCompleted && task.MatchesTarget(interactedObject))
                {
                    task.UpdateCount();
                    Debug.Log($"Task '{task.Name}' progress updated. Completed: {task.IsCompleted}", this);
                    if (task.IsCompleted && gameManager != null && AreMandatoryTasksCompleted())
                    {
                        gameManager.UnlockSafeRoom();
                    }
                    break; // Exit after finding and updating the matching task
                }
            }
        }
        
        public List<Task> GetCurrentTasks()
        {
            List<Task> tasks = new List<Task>();
            if (IsValidDay())
            {
                for (int i = 0; i < dailyTasks.GetLength(1); i++)
                {
                    if (dailyTasks[currentDay, i] != null)
                    {
                        tasks.Add(dailyTasks[currentDay, i]);
                    }
                }
            }
            return tasks;
        }
        
        private bool IsValidDay()
        {
            if (currentDay < 0 || currentDay >= dailyTasks.GetLength(0))
            {
                Debug.LogWarning($"TaskManager: Invalid day index {currentDay + 1} (valid range: 1-{dailyTasks.GetLength(0)})", this);
                return false;
            }
            return true;
        }

        private void OnValidate()
        {
            if (notePrefab == null) Debug.LogWarning("NotePrefab not assigned in TaskManager!", this);
            if (noteSpawnPoints == null || noteSpawnPoints.Count == 0) Debug.LogWarning("NoteSpawnPoints not assigned or empty in TaskManager!", this);
        }
    }
}