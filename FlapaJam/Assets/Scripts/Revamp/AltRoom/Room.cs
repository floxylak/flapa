using UnityEngine;

public class Room : MonoBehaviour
{
    public Door[] doors;
    public bool playerInside;
    private Door mainDoor;
    private Anomaly activeAnomaly;
    private int stage;
    private bool hasAnomaly;

    public bool HasAnomaly() => hasAnomaly;

    public void Initialize(int currentStage)
    {
        stage = currentStage;
        hasAnomaly = Random.value < 0.5f; // 50% chance of anomaly
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

    private void Start()
    {
        if (doors.Length == 0)
            Debug.LogError("Room is missing additional doors!");

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