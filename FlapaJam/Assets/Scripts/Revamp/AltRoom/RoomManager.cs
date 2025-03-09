using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    
    [SerializeField] private GameObject roomPrefab;
    private Room currentRoom;
    private Room previousRoom;
    private Door previousSpawningDoor;
    private HashSet<Vector3> occupiedPositions;
    private HashSet<Door> doorsThatSpawned;

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
        SpawnRoom(null, new Vector3(20f, 0.003f, -32f), Quaternion.identity);
    }

    public void SpawnRoom(Door sourceDoor, Vector3 position, Quaternion rotation)
    {
        Vector3 roundedPosition = new Vector3(
            Mathf.Round(position.x * 1000f) / 1000f,
            Mathf.Round(position.y * 1000f) / 1000f,
            Mathf.Round(position.z * 1000f) / 1000f
        );

        if (occupiedPositions.Contains(roundedPosition) || IsRoomAtPosition(roundedPosition))
            return;

        GameObject newRoomObj = Instantiate(roomPrefab, position, rotation);
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
        previousRoom = currentRoom; // Move current to previous
        currentRoom = newRoom;      // Set new room as current
        previousSpawningDoor = sourceDoor;

        if (previousRoom != null)
        {
            LockAllDoors(currentRoom); // Lock doors in the new current room
        }
    }

    public void CheckDestroyPreviousRoom(Room newRoom)
    {
        if (previousRoom != null && previousRoom != newRoom && IsDoorClosed(previousSpawningDoor))
        {
            Vector3 previousPosition = new Vector3(
                Mathf.Round(previousRoom.transform.position.x * 1000f) / 1000f,
                Mathf.Round(previousRoom.transform.position.y * 1000f) / 1000f,
                Mathf.Round(previousRoom.transform.position.z * 1000f) / 1000f
            );

            if (previousSpawningDoor != null)
                ReparentWallToNewRoom(previousSpawningDoor, newRoom);

            occupiedPositions.Remove(previousPosition);
            foreach (Door door in previousRoom.GetDoors())
                doorsThatSpawned.Remove(door);
            //doorsThatSpawned.Remove(previousRoom.mainDoor);
            doorsThatSpawned.Remove(previousSpawningDoor);
            
            Destroy(previousRoom.gameObject);
            previousRoom = null;
            previousSpawningDoor = null;

            if (currentRoom != null)
            {
                UnlockAllDoors(currentRoom);
            }
        }
    }

    private void LockAllDoors(Room room)
    {
        Transform doorsContainer = room.transform.Find("Doors");
        if (doorsContainer != null)
        {
            Door[] doors = doorsContainer.GetComponentsInChildren<Door>();
            foreach (Door door in doors)
            {
                door.isLocked = true;
            }
        }
    }

    private void UnlockAllDoors(Room room)
    {
        Transform doorsContainer = room.transform.Find("Doors");
        if (doorsContainer != null)
        {
            Door[] doors = doorsContainer.GetComponentsInChildren<Door>();
            foreach (Door door in doors)
            {
                door.isLocked = false;
            }
        }
    }

    private void ReparentWallToNewRoom(Door door, Room newRoom)
    {
        if (door == null || newRoom == null)
            return;

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
        if (door == null)
            return false;

        return door.state == Door.DoorState.Closed;
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

    public void OnDoorInteracted(Door interactedDoor)
    {
        if (interactedDoor == null || doorsThatSpawned.Contains(interactedDoor))
            return;

        // Check if the interacted door belongs to the previous room
        if (previousRoom != null && previousRoom.GetDoors().Contains(interactedDoor))
        {
            if (interactedDoor.isLocked)
                return;

            // Destroy the current room
            if (currentRoom != null)
            {
                Vector3 currentPosition = new Vector3(
                    Mathf.Round(currentRoom.transform.position.x * 1000f) / 1000f,
                    Mathf.Round(currentRoom.transform.position.y * 1000f) / 1000f,
                    Mathf.Round(currentRoom.transform.position.z * 1000f) / 1000f
                );
                occupiedPositions.Remove(currentPosition);
                foreach (Door door in currentRoom.GetDoors())
                    doorsThatSpawned.Remove(door);
                Destroy(currentRoom.gameObject);
                
                doorsThatSpawned.Remove(previousSpawningDoor);
            }

            // Reset state: previousRoom becomes currentRoom
            currentRoom = previousRoom;
            previousRoom = null; // Clear previousRoom since it’s now current
            previousSpawningDoor = null; // Reset the spawning door
            UnlockAllDoors(currentRoom); // Unlock doors in the new current room (old previousRoom)

            // Spawn a new room at the interacted door
            Transform pivot = interactedDoor.transform.Find("Pivot");
            if (pivot != null)
            {
                SpawnRoom(interactedDoor, pivot.position, Quaternion.identity);
                doorsThatSpawned.Add(interactedDoor);
            }

            return; // Exit after handling previous room door interaction
        }

        // Original behavior for spawning from current room
        if (interactedDoor.isLocked || previousRoom != null)
            return;

        Transform newPivot = interactedDoor.transform.Find("Pivot");
        if (newPivot != null)
        {
            SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
            doorsThatSpawned.Add(interactedDoor);
        }
    }
}