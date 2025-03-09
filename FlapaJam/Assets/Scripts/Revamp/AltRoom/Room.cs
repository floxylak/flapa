using UnityEngine;

public class Room : MonoBehaviour
{
    public Door[] doors;
    public bool playerInside;
    private Door mainDoor;
    private Anomaly activeAnomaly;
    private int stage;
    private bool hasAnomaly;
    private bool solved;          // Tracks if the anomaly was correctly identified
    private bool stageCompleted;  // Prevents re-solving stage 0
    private bool isHallway;       // Identifies hallway rooms
    private bool completed;       // Tracks if the room has been completed (test input used)
    private bool isDisabled;      // Tracks if the room is deactivated

    public bool HasAnomaly() => hasAnomaly;
    public int Stage => stage;
    public bool Solved => solved;
    public bool IsHallway => isHallway;
    public bool Completed => completed;
    public bool IsDisabled => isDisabled;

    public void Initialize(int currentStage, bool isHallwayRoom = false)
    {
        stage = currentStage; // 0-based
        isHallway = isHallwayRoom;
        hasAnomaly = !isHallway && Random.value < 0.5f; // 50% chance of anomaly, no anomalies in hallways
        stageCompleted = (stage == 0 && RoomManager.Instance.IsStage1Completed);
        completed = false;
        isDisabled = false;
        if (hasAnomaly)
        {
            activeAnomaly = gameObject.AddComponent<Anomaly>();
            activeAnomaly.Initialize();
        }
    }

    public void SetSpawningDoor(Door door)
    {
        mainDoor = door;
    }

    public void SetSolved(bool value)
    {
        solved = value;
        completed = true; // Mark room as completed when test input is used
    }

    public void Deactivate()
    {
        if (activeAnomaly != null)
            activeAnomaly.enabled = false;
        isDisabled = true; // Prevent further test input interaction
    }

    private void Start()
    {
        // if (doors.Length == 0)
            // Debug.LogError("Room is missing additional doors!");

        Transform colliderTransform = transform;
        if (colliderTransform != null)
        {
            Collider collider = colliderTransform.GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
                collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    public Door[] GetDoors()
    {
        return doors;
    }
}