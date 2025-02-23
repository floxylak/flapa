using UnityEngine;

namespace Player.Interact
{
    public class StairwayLoop : MonoBehaviour
    {
        public GameObject stairwayPrefab;
        public Transform player;
        public float triggerRadius = 5f;
        public Transform triggerPos;

        private Vector3 spawnOffset = new Vector3(4.86f, 7.05f, 4.86f);
        private Vector3 currentPosition;
        private GameObject lastTrigger;
        private GameObject lastDoor;
        private int stairwayCounter = 0;
        private int triggerCount = 0;

        private GameManager gameManager;
        private TaskManager taskManager;

        private void Awake()
        {
            Debug.Log("StairwayLoop: Awake called on " + gameObject.name);

            gameManager = FindObjectOfType<GameManager>();
            taskManager = FindObjectOfType<TaskManager>();

            if (gameManager == null) Debug.LogError("StairwayLoop: GameManager not found in scene!");
            if (taskManager == null) Debug.LogError("StairwayLoop: TaskManager not found in scene!");
        }

        private void Start()
        {
            if (stairwayPrefab == null || player == null || triggerPos == null) return;

            DisableInitialObjects();
            currentPosition = transform.position;
            SpawnStairway();
        }

        void DisableInitialObjects()
        {
            GameObject initialStairway = GameObject.Find("Stairway");
            if (initialStairway != null)
            {
                initialStairway.SetActive(false);
                Debug.Log($"StairwayLoop: Disabled initial stairway at {initialStairway.transform.position}");
            }
        }

        void SpawnStairway()
        {
            if (lastTrigger != null)
            {
                SphereCollider oldTriggerCollider = lastTrigger.GetComponent<SphereCollider>();
                if (oldTriggerCollider != null) oldTriggerCollider.enabled = false;
            }

            if (lastDoor != null)
            {
                Destroy(lastDoor);
            }

            GameObject newStairway = Instantiate(stairwayPrefab, currentPosition, Quaternion.identity);
            newStairway.transform.SetParent(transform);
            newStairway.SetActive(true);
            stairwayCounter++;

            lastDoor = newStairway.transform.Find("Door")?.gameObject;
            if (lastDoor == null) Debug.LogError("StairwayLoop: No 'Door' found in the stairway prefab!");

            OnStairwaySpawned(newStairway, stairwayCounter);

            Vector3 triggerWorldPos = newStairway.transform.TransformPoint(triggerPos.localPosition);
            GameObject triggerObj = new GameObject("StairwayTrigger");
            triggerObj.transform.position = triggerWorldPos;
            triggerObj.transform.SetParent(newStairway.transform);

            SphereCollider triggerCollider = triggerObj.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;

            StairwayTrigger triggerScript = triggerObj.AddComponent<StairwayTrigger>();
            triggerScript.onPlayerEnter = OnPlayerEnterTrigger;

            currentPosition += spawnOffset;
            lastTrigger = triggerObj;

            if (stairwayCounter >= 5)
            {
                CompleteExitTaskAndResetDay();
            }
        }

        private void OnStairwaySpawned(GameObject newStairway, int stairwayCount)
        {
            Debug.Log($"StairwayLoop: New stairway spawned (Count: {stairwayCount}) at {newStairway.transform.position}");
        }

        void OnPlayerEnterTrigger()
        {
            triggerCount++;
            int stairwaysToSpawn = 2 + (triggerCount - 1);
            Debug.Log($"StairwayLoop: Spawning {stairwaysToSpawn} stairways for trigger {triggerCount}");

            for (int i = 0; i < stairwaysToSpawn && stairwayCounter < 5; i++)
            {
                SpawnStairway();
            }
        }

        void CompleteExitTaskAndResetDay()
        {
            if (taskManager != null)
            {
                gameObject.tag = "Exit";
                taskManager.CheckTaskProgress(gameObject);
                Debug.Log("StairwayLoop: 'Exit' task completed after 5 stairways spawned.");
            }
            
            LockExitDoor();
            
            if (gameManager != null)
            {
                gameManager.Sleep(); // Trigger sleep
                ResetStairwayLoop(); 
            }
        }

        void LockExitDoor()
        {
            GameObject exitDoorObj = GameObject.FindGameObjectWithTag("ExitDoor");
            if (exitDoorObj != null)
            {
                Door exitDoor = exitDoorObj.GetComponent<Door>();
                if (exitDoor != null)
                {
                    exitDoor.LockDoor();
                    Debug.Log("StairwayLoop: ExitDoor locked after 5 stairways spawned.");
                }
                else
                {
                    Debug.LogWarning("StairwayLoop: No Door component found on ExitDoor object!");
                }
            }
            else
            {
                Debug.LogWarning("StairwayLoop: No GameObject with tag 'ExitDoor' found in the scene!");
            }
        }

        void ResetStairwayLoop()
        {
            // Destroy all spawned stairways
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("StairwayLoop: All stairways destroyed.");
        }

        private void OnDrawGizmos()
        {
            if (lastTrigger != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(lastTrigger.transform.position, triggerRadius);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, triggerRadius);
            }
        }

        public int GetStairwayCount() => stairwayCounter;
        public int GetTriggerCount() => triggerCount;
    }
}