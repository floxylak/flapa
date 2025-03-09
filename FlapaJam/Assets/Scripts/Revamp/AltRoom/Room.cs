using UnityEngine;
using Random = UnityEngine.Random;

public class Room : MonoBehaviour
{
    public Door[] doors;
    public bool playerInside;
    private Door mainDoor;
    
    public bool isAnomaly => activeAnomaly != null;
    
    private Anomaly activeAnomaly;
    private int stage;
    private bool hasAnomaly;
    private bool hasPlayerEntered;
    
    private RoomManager roomManager;   
    
    public void Initialize(int currentStage)
    {
        stage = currentStage;
        hasAnomaly = Random.value < 0.5f;

        if (!hasAnomaly) return;
        
        activeAnomaly = gameObject.AddComponent<Anomaly>();
        activeAnomaly.Initialize();
    }
    
    public bool HasAnomaly()
    {
        return hasAnomaly;
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
    
    public void SetSpawningDoor(Door door)
    {
        mainDoor = door;
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