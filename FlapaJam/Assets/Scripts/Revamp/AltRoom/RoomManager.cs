using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private List<GameObject> stagePrefabs;
    [SerializeField] private GameObject hallwayPrefab;
    private Room currentRoom;
    private Room previousRoom;
    private Door previousSpawningDoor;
    private HashSet<Vector3> occupiedPositions;
    private HashSet<Door> doorsThatSpawned;
    private int currentStage = 0;
    private bool usedShardInCurrentRoom = false;
    private bool isNextRoomHallway = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        occupiedPositions = new HashSet<Vector3>();
        doorsThatSpawned = new HashSet<Door>();
    }

    private void Start()
    {
        if (stagePrefabs == null || stagePrefabs.Count < 5)
        {
            Debug.LogError("Stage prefabs list must contain at least 5 stages!");
            return;
        }
        SpawnRoom(null, new Vector3(20f, 0.003f, -32f), Quaternion.identity);
    }

    public void SpawnRoom(Door sourceDoor, Vector3 position, Quaternion rotation)
    {
        Vector3 roundedPosition = RoundPosition(position);
        if (occupiedPositions.Contains(roundedPosition) || IsRoomAtPosition(roundedPosition))
            return;

        GameObject newRoomObj;
        if (isNextRoomHallway)
        {
            newRoomObj = Instantiate(hallwayPrefab, position, rotation);
            isNextRoomHallway = false;
            currentStage = 0;
        }
        else
        {
            newRoomObj = Instantiate(stagePrefabs[currentStage], position, rotation);
        }

        Room newRoom = newRoomObj.GetComponent<Room>();
        if (newRoom == null)
        {
            Destroy(newRoomObj);
            return;
        }

        if (sourceDoor != null)
        {
            Transform pivot = sourceDoor.transform.Find("Pivot");
            if (pivot == null)
            {
                Destroy(newRoomObj);
                return;
            }
            newRoomObj.transform.position = pivot.position;
            float newYRotation = (pivot.rotation.eulerAngles.y + 180f) % 360f;
            newRoomObj.transform.rotation = Quaternion.Euler(0f, newYRotation, 0f);
            newRoom.SetSpawningDoor(sourceDoor);
        }

        occupiedPositions.Add(roundedPosition);
        previousRoom = currentRoom;
        currentRoom = newRoom;
        previousSpawningDoor = sourceDoor;

        currentRoom.Initialize(currentStage + 1);
        Debug.Log($"Spawned room: Stage {currentStage + 1}, Has Anomaly: {currentRoom.HasAnomaly()}");

        if (previousRoom != null)
            LockAllDoors(currentRoom);
    }

    public void OnShardInteracted(Vector3 playerPosition)
    {
        // Find the room the player is in
        Room playerRoom = GetRoomAtPosition(playerPosition);
        if (playerRoom == null)
        {
            Debug.Log("Shard used outside of any room - no effect.");
            return;
        }

        if (playerRoom != currentRoom)
        {
            Debug.Log("Shard used in a non-current room - no effect.");
            return;
        }

        usedShardInCurrentRoom = true;
        bool hasAnomaly = currentRoom.HasAnomaly();
        Debug.Log($"Shard used in Stage {currentStage + 1}. Has Anomaly: {hasAnomaly}");

        if (hasAnomaly)
            currentStage = Mathf.Min(currentStage + 1, stagePrefabs.Count - 1); // Next stage
        else
            currentStage = 0; // Reset to Stage1
    }

    public void OnDoorInteracted(Door interactedDoor)
    {
        if (interactedDoor == null || doorsThatSpawned.Contains(interactedDoor))
            return;

        if (previousRoom != null && previousRoom.GetDoors().Contains(interactedDoor))
        {
            if (interactedDoor.isLocked)
                return;

            if (currentRoom != null)
            {
                Vector3 currentPosition = RoundPosition(currentRoom.transform.position);
                occupiedPositions.Remove(currentPosition);
                foreach (Door door in currentRoom.GetDoors())
                    doorsThatSpawned.Remove(door);
                doorsThatSpawned.Remove(previousSpawningDoor);
                Destroy(currentRoom.gameObject);
            }

            currentRoom = previousRoom;
            previousRoom = null;
            previousSpawningDoor = null;
            UnlockAllDoors(currentRoom);

            Transform pivot = interactedDoor.transform.Find("Pivot");
            if (pivot != null)
            {
                SpawnRoom(interactedDoor, pivot.position, Quaternion.identity);
                doorsThatSpawned.Add(interactedDoor);
            }
            return;
        }

        if (interactedDoor.isLocked || previousRoom != null)
            return;

        if (currentRoom != null && !usedShardInCurrentRoom && currentRoom.HasAnomaly())
        {
            isNextRoomHallway = true;
        }
        else if (!usedShardInCurrentRoom && !currentRoom.HasAnomaly())
        {
            currentStage = Mathf.Min(currentStage + 1, stagePrefabs.Count - 1);
        }

        usedShardInCurrentRoom = false;
        Transform newPivot = interactedDoor.transform.Find("Pivot");
        if (newPivot != null)
        {
            SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
            doorsThatSpawned.Add(interactedDoor);
        }
    }

    private Room GetRoomAtPosition(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 0.1f);
        foreach (var collider in hitColliders)
        {
            Room room = collider.GetComponent<Room>();
            if (room != null && room.playerInside)
                return room;
        }
        return null;
    }

    private Vector3 RoundPosition(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x * 1000f) / 1000f,
            Mathf.Round(position.y * 1000f) / 1000f,
            Mathf.Round(position.z * 1000f) / 1000f
        );
    }

    private bool IsRoomAtPosition(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, 2.0f);
        foreach (var collider in hitColliders)
        {
            if (collider.GetComponent<Room>() != null && collider.GetComponent<Room>() != currentRoom)
                return true;
        }
        return false;
    }

    private void LockAllDoors(Room room)
    {
        Transform doorsContainer = room.transform.Find("Doors");
        if (doorsContainer != null)
        {
            Door[] doors = doorsContainer.GetComponentsInChildren<Door>();
            foreach (Door door in doors)
                door.isLocked = true;
        }
    }

    private void UnlockAllDoors(Room room)
    {
        Transform doorsContainer = room.transform.Find("Doors");
        if (doorsContainer != null)
        {
            Door[] doors = doorsContainer.GetComponentsInChildren<Door>();
            foreach (Door door in doors)
                door.isLocked = false;
        }
    }

    private void ReparentWallToNewRoom(Door door, Room newRoom)
    {
        if (door == null || newRoom == null) return;
        Transform wall = door.transform.parent;
        if (wall != null)
        {
            wall.SetParent(newRoom.transform, true);
            Transform oldPivot = door.transform.Find("Pivot");
            Transform oldArchPivot = door.transform.Find("ArchPivot");
            if (oldPivot != null && oldArchPivot != null)
            {
                oldPivot.name = "ArchPivot";
                oldArchPivot.name = "Pivot";
            }
        }
    }

    private bool IsDoorClosed(Door door)
    {
        return door != null && door.state == Door.DoorState.Closed;
    }
}