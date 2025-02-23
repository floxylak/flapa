using UnityEngine;
using Player; // Add this to access TaskManager

namespace Player.Interact
{
    public class Door : Interactable
    {
        [SerializeField]
        private GameObject door; // The door object with the Animator

        [Header("Door Settings")]
        [SerializeField] private string accessCodeTag = "AccessCode"; // Tag for the required item

        private Animator doorAnimator;
        private bool doorOpen = false;
        private bool isLocked = false;
        private float shakeTimer = 0f; // For spooky shake effect
        private Vector3 originalPosition;
        private InventoryManager inventory; // To check and remove AccessCode
        private TaskManager taskManager; // To complete the task
        private Transform player; // To get InventoryManager

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (door == null)
            {
                Debug.LogError("Door GameObject not assigned in Inspector!", this);
                door = gameObject;
            }

            doorAnimator = door.GetComponent<Animator>();
            if (doorAnimator == null)
            {
                Debug.LogError("Animator not found on door GameObject!", this);
            }

            originalPosition = door.transform.position;

            // Get player from Camera.main.transform.parent.parent
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.parent != null && 
                mainCamera.transform.parent.parent != null)
            {
                player = mainCamera.transform.parent.parent;
                Debug.Log("Door: Player assigned from Camera.main.transform.parent.parent", this);
            }
            else
            {
                Debug.LogError("Door: Failed to assign player from Camera.main.transform.parent.parent.", this);
                return;
            }

            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Door: InventoryManager not found on player!", this);
            }

            taskManager = FindObjectOfType<TaskManager>();
            if (taskManager == null)
            {
                Debug.LogWarning("Door: TaskManager not found in scene!", this);
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (isLocked)
            {
                Debug.Log($"{gameObject.name} is locked!", this);
                return;
            }

            // Check for AccessCode if this is an ExitDoor
            if (gameObject.CompareTag("ExitDoor"))
            {
                if (inventory.HasItem(accessCodeTag))
                {
                    OpenExitDoor();
                }
                else
                {
                    Debug.Log($"{gameObject.name} requires an AccessCode to open!", this);
                    ShakeDoor(); // Optional feedback
                }
            }
            else
            {
                ToggleDoor(); // Normal door behavior
                Debug.Log($"Interacting with {gameObject.name} - Door {(doorOpen ? "opened" : "closed")}", this);
            }
        }

        private void OpenExitDoor()
        {
            if (doorOpen) return;

            doorOpen = true;
            doorAnimator.SetBool("isOpen", doorOpen);
            Debug.Log($"{gameObject.name} opened with AccessCode.", this);

            // Remove the AccessCode item from inventory
            inventory.RemoveItem(accessCodeTag);
            Debug.Log("Door: AccessCode removed from inventory.", this);

            // Complete the "Unlock the Exit" task
            if (taskManager != null)
            {
                gameObject.tag = "ExitDoor"; // Ensure tag matches task (should already be set)
                taskManager.CheckTaskProgress(gameObject);
                Debug.Log("Door: 'Unlock the Exit' task completed!", this);
            }
            else
            {
                Debug.LogWarning("Door: TaskManager not found, cannot complete 'Unlock the Exit' task!", this);
            }
        }

        public void ToggleDoor()
        {
            if (isLocked) return;

            doorOpen = !doorOpen;
            doorAnimator.SetBool("isOpen", doorOpen);
        }

        public void OpenDoor()
        {
            if (isLocked || doorOpen) return;

            doorOpen = true;
            doorAnimator.SetBool("isOpen", doorOpen);
            Debug.Log($"{gameObject.name} opened.", this);
        }

        public void CloseDoor()
        {
            if (isLocked || !doorOpen) return;

            doorOpen = false;
            doorAnimator.SetBool("isOpen", doorOpen);
            Debug.Log($"{gameObject.name} closed.", this);
        }

        public void LockDoor()
        {
            isLocked = true;
            Debug.Log($"{gameObject.name} is now locked.", this);
        }

        public void UnlockDoor()
        {
            isLocked = false;
            Debug.Log($"{gameObject.name} is now unlocked.", this);
        }

        public bool IsLocked()
        {
            return isLocked;
        }

        public bool IsOpen()
        {
            return doorOpen;
        }

        public void ShakeDoor(float duration = 0.5f, float intensity = 0.1f)
        {
            if (shakeTimer > 0) return;

            shakeTimer = duration;
            StartCoroutine(ShakeEffect(intensity));
            Debug.Log($"{gameObject.name} shakes ominously...", this);
        }

        public void RandomHaunt(float chance = 0.1f)
        {
            if (isLocked || Random.value > chance) return;

            ToggleDoor();
            ShakeDoor(0.3f, 0.05f);
            Debug.Log($"{gameObject.name} moves on its own!", this);
        }

        private System.Collections.IEnumerator ShakeEffect(float intensity)
        {
            while (shakeTimer > 0)
            {
                shakeTimer -= Time.deltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * intensity;
                door.transform.position = originalPosition + shakeOffset;
                yield return null;
            }
            door.transform.position = originalPosition;
        }

        private void Update()
        {
            // if (!isLocked && Random.value < 0.001f) RandomHaunt(0.3f);
        }

        private void OnDisable()
        {
            door.transform.position = originalPosition;
            shakeTimer = 0f;
        }

        private bool IsValidSetup()
        {
            if (inventory == null)
            {
                Debug.LogWarning("Door: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (taskManager == null)
            {
                Debug.LogWarning("Door: Cannot function without a TaskManager.", this);
                return false;
            }
            return true;
        }
    }
}