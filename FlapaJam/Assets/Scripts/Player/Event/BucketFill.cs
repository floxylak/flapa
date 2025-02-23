using UnityEngine;
using Player; // Add this to access TaskManager

namespace Player.Interact
{
    public class BucketFill : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Transform waterLevel;

        [Header("Bucket Settings")]
        [SerializeField] private float maxCapacity = 10f;
        public float MaxCapacity => maxCapacity;

        [SerializeField] private float initialFill = 5f;
        [SerializeField] private float leakRate = 1f;
        [SerializeField] private float baseFillDuration = 5f;

        private InputManager inputManager;
        private TaskManager taskManager; // New reference to TaskManager
        private float currentFill;
        private float fillStartTime;
        private bool isFilling;
        private bool isUnderPipe;
        private bool hasCompletedTask; // Track if task has been completed to avoid multiple triggers

        public bool IsFull => currentFill >= maxCapacity;
        public float FillPercentage => currentFill / maxCapacity;
        public float CurrentFill => currentFill;

        private void Awake()
        {
            InitializeReferences();
            currentFill = Mathf.Clamp(initialFill, 0f, maxCapacity);
            UpdateWaterLevel();
            hasCompletedTask = false; // Initialize task completion flag
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
                    Debug.Log("BucketFill: Player assigned from Camera.main.transform.parent.parent", this);
                }
                else
                {
                    Debug.LogError("BucketFill: Failed to assign player from Camera.main.transform.parent.parent.", this);
                    return;
                }
            }

            if (player != null)
            {
                inputManager = player.GetComponent<InputManager>();
            }

            taskManager = FindFirstObjectByType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("BucketFill: TaskManager not found in scene!", this);
            }
        }

        private void Update()
        {
            if (isUnderPipe)
            {
                if (!IsFull)
                {
                    float remainingCapacity = maxCapacity - currentFill;
                    float elapsedFillTime = Time.time - fillStartTime;
                    float baseFillRate = maxCapacity / baseFillDuration; // Consistent fill rate

                    // Adjust fill rate based on elapsed time
                    float adjustedFillRate = baseFillRate * Mathf.Lerp(1.2f, 0.8f, FillPercentage);

                    currentFill = Mathf.Min(currentFill + adjustedFillRate * Time.deltaTime, maxCapacity);
                    UpdateWaterLevel();

                    if (IsFull && !hasCompletedTask)
                    {
                        CompleteGatherWaterTask();
                    }
                }
            }
            else if (currentFill > 0f) // Only leak when not under the pipe
            {
                currentFill = Mathf.Max(0f, currentFill - leakRate * Time.deltaTime);
                UpdateWaterLevel();
                if (!IsFull) hasCompletedTask = false; // Reset task completion if bucket empties
            }
        }

        public void StartFilling()
        {
            if (IsFull || isFilling) return;

            isFilling = true;
            fillStartTime = Time.time - (FillPercentage * baseFillDuration); // Adjust start time for partial fills
        }

        public void StopFilling()
        {
            isFilling = false;
        }

        public void LeakWhileHeld(float deltaTime)
        {
            if (!isUnderPipe && currentFill > 0f) // Ensure no leaking under pipe
            {
                currentFill = Mathf.Max(0f, currentFill - leakRate * deltaTime);
                UpdateWaterLevel();
            }
        }

        public void SetUnderPipe(bool underPipe)
        {
            if (isUnderPipe != underPipe) // Prevent unnecessary updates
            {
                isUnderPipe = underPipe;
                if (isUnderPipe)
                {
                    StartFilling();
                }
                else
                {
                    StopFilling();
                }
            }
        }

        private void UpdateWaterLevel()
        {
            if (waterLevel == null) return;

            waterLevel.gameObject.SetActive(currentFill > 0f);
            if (currentFill > 0f)
            {
                float t = FillPercentage;
                float newY = Mathf.Lerp(-0.1926f, 0.0127f, t);
                float newScale = Mathf.Lerp(9f, 12f, t);

                waterLevel.localPosition = new Vector3(0f, newY, 0f);
                waterLevel.localScale = new Vector3(newScale, newScale, newScale);
            }
        }
        
        private void CompleteGatherWaterTask()
        {
            if (taskManager != null)
            {
                taskManager.CheckTaskProgress(gameObject); // Pass this bucket as the interacted object
                hasCompletedTask = true; // Mark task as completed to prevent repeated calls
                Debug.Log("BucketFill: 'Gather Water' task completed!", this);
            }
            else
            {
                Debug.LogWarning("BucketFill: Cannot complete 'Gather Water' task - TaskManager is null!", this);
            }
        }

        private void OnValidate()
        {
            maxCapacity = Mathf.Max(1f, maxCapacity);
            baseFillDuration = Mathf.Max(1f, baseFillDuration);
            leakRate = Mathf.Max(0f, leakRate);
            initialFill = Mathf.Clamp(initialFill, 0f, maxCapacity);
        }
    }
}