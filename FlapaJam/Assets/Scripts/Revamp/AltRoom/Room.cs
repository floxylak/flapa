using UnityEngine;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    public Door[] doors;
    
    private int stage;
    private bool hasAnomaly;
    
    public Anomaly[] anomalies;
    public Anomaly activeAnomaly;
    
    public Task[] tasks;
    public Task activeTask;
    
    private RoomManager roomManager;
    
    public bool isAnomaly => activeAnomaly != null;
    public bool isTask => activeTask != null;
    

    private bool hasPlayerEntered;
    public bool playerInside;
    public Door spawningDoor;
    
    public void Initialize(int currentStage)
    {
        stage = currentStage;
        float anomalyChance = 0.5f; // 50% chance, adjust as needed
        hasAnomaly = Random.value < anomalyChance;
        if (hasAnomaly)
        {
            anomaly = gameObject.AddComponent<Anomaly>();
            anomaly.Initialize(); // Define anomaly type in Anomaly.cs
        }
    }
    
    public void SetSpawningDoor(Door door)
    {
        spawningDoor = door;
    }

    private void Start()
    {
        roomManager = RoomManager.Instance;
        InitializeRoom();
        hasPlayerEntered = false;
        playerInside = false;

        Transform colliderTransform = transform;
        if (colliderTransform != null)
        {
            Collider collider = colliderTransform.GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }
    }

    private void InitializeRoom()
    {
        if (doors.Length == 0)
        {
            Debug.LogError("Room is missing additional doors!");
        }
    }

    private void Update()
    {
        if (playerInside && roomManager != null)
        {
            roomManager.CheckDestroyPreviousRoom(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayerEntered)
        {
            hasPlayerEntered = true;
        }
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform colliderTransform = transform;
        if (colliderTransform != null)
        {
            Collider collider = colliderTransform.GetComponent<Collider>();
            if (collider is BoxCollider boxCollider)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = colliderTransform.localToWorldMatrix;
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
        }
    }

    public Door[] GetDoors()
    {
        return doors;
    }
}