using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Player;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField] private List<GameObject> stageRooms;
    [SerializeField] private GameObject hallwayPrefab;
    [SerializeField] private GameObject endgamePrefab;
    private Room currentRoom;
    private Room previousRoom;
    private Door previousSpawningDoor;
    private HashSet<Vector3> occupiedPositions;
    private HashSet<Door> doorsThatSpawned;
    private int currentStage = 0;
    private bool stage1Completed = false;
    private bool testInputUsedInCurrentRoom = false;
    private bool shardEverPickedUp = false;

    public bool IsStage1Completed => stage1Completed;
    public bool HasShardBeenPickedUp => shardEverPickedUp;

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
        if (stageRooms.Count > 0 && hallwayPrefab != null && endgamePrefab != null)
        {
            GameObject spawnDoorObj = GameObject.Find("SpawnDoor");
            if (spawnDoorObj != null)
            {
                Transform pivot = spawnDoorObj.transform.Find("Pivot");
                if (pivot != null)
                {
                    Door spawnDoor = spawnDoorObj.GetComponent<Door>();
                    SpawnRoom(spawnDoor, pivot.position, Quaternion.identity, true);
                }
                else
                {
                    SpawnRoom(null, new Vector3(20f, 0.003f, -32f), Quaternion.identity, true);
                }
            }
            else
            {
                SpawnRoom(null, new Vector3(20f, 0.003f, -32f), Quaternion.identity, true);
            }
        }
    }

    private void Update()
    {
        CheckDestroyPreviousRoom();
        if (shardEverPickedUp && Input.GetKeyDown(KeyCode.K) && currentRoom != null && !currentRoom.IsDisabled)
        {
            OnTestInput(PlayerPosition());
            testInputUsedInCurrentRoom = true;
        }
    }

    public void CheckDestroyPreviousRoom()
    {
        if (previousRoom != null && currentRoom != null && currentRoom.playerInside && IsDoorClosed(previousSpawningDoor))
        {
            Vector3 previousPosition = new Vector3(
                Mathf.Round(previousRoom.transform.position.x * 1000f) / 1000f,
                Mathf.Round(previousRoom.transform.position.y * 1000f) / 1000f,
                Mathf.Round(previousRoom.transform.position.z * 1000f) / 1000f
            );

            if (previousSpawningDoor != null)
            {
                ReparentWallToNewRoom(previousSpawningDoor, currentRoom);
                if (currentRoom == endgamePrefab.GetComponent<Room>())
                {
                    previousSpawningDoor.isLocked = true;
                }
            }

            occupiedPositions.Remove(previousPosition);
            foreach (Door door in previousRoom.GetDoors())
                doorsThatSpawned.Remove(door);
            doorsThatSpawned.Remove(previousSpawningDoor);

            Destroy(previousRoom.gameObject);
            previousRoom = null;
            previousSpawningDoor = null;

            UnlockAllDoors(currentRoom);
        }
    }

    public void OnTestInput(Vector3 playerPosition)
    {
        Room[] allRooms = FindObjectsOfType<Room>();
        Room playerRoom = null;
        foreach (Room room in allRooms)
        {
            if (room.playerInside)
            {
                playerRoom = room;
                break;
            }
        }

        if (playerRoom == null || playerRoom != currentRoom)
            return;

        if (playerRoom.Completed || playerRoom.IsHallway || (playerRoom.Stage == 0 && stage1Completed))
            return;

        bool hasAnomaly = playerRoom.HasAnomaly();
        playerRoom.SetSolved(hasAnomaly);
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

        Transform newPivot = interactedDoor.transform.Find("Pivot");
        if (newPivot == null)
            return;

        if (currentRoom != null)
        {
            bool hasAnomaly = currentRoom.HasAnomaly();

            if (currentRoom.IsHallway)
            {
                currentStage = 0;
                SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
            }
            else if (hasAnomaly && testInputUsedInCurrentRoom)
            {
                if (currentStage + 1 >= stageRooms.Count)
                {
                    SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity, false, true);
                }
                else
                {
                    currentStage++;
                    SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
                }
            }
            else if (hasAnomaly && !testInputUsedInCurrentRoom)
            {
                SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity, true);
                currentStage = 0;
            }
            else if (!hasAnomaly && testInputUsedInCurrentRoom)
            {
                currentStage = 0;
                SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
            }
            else
            {
                if (currentStage + 1 >= stageRooms.Count)
                {
                    SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity, false, true);
                }
                else
                {
                    currentStage++;
                    SpawnRoom(interactedDoor, newPivot.position, Quaternion.identity);
                }
            }

            if (currentStage == 0)
                stage1Completed = true;

            doorsThatSpawned.Add(interactedDoor);
        }
    }

    public void OnShardInteracted(Vector3 playerPosition)
    {
        shardEverPickedUp = true;
    }

    public void SpawnRoom(Door sourceDoor, Vector3 position, Quaternion rotation, bool isHallway = false, bool isEndgame = false)
    {
        Vector3 roundedPosition = new Vector3(
            Mathf.Round(position.x * 1000f) / 1000f,
            Mathf.Round(position.y * 1000f) / 1000f,
            Mathf.Round(position.z * 1000f) / 1000f
        );

        if (occupiedPositions.Contains(roundedPosition) || IsRoomAtPosition(roundedPosition))
            return;

        GameObject prefab = isHallway ? hallwayPrefab : (isEndgame ? endgamePrefab : 
            (currentStage < stageRooms.Count ? stageRooms[currentStage] : stageRooms[stageRooms.Count - 1]));
        
        GameObject newRoomObj = Instantiate(prefab, position, rotation);
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

        newRoom.Initialize(currentStage, isHallway);
        occupiedPositions.Add(roundedPosition);
        previousRoom = currentRoom;
        currentRoom = newRoom;
        previousSpawningDoor = sourceDoor;

        if (previousRoom != null)
        {
            LockAllDoors(currentRoom);
            previousRoom.Deactivate();
        }

        testInputUsedInCurrentRoom = false;
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
        return door != null && door.state == Door.DoorState.Closed;
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

    private Vector3 PlayerPosition()
    {
        GameObject player = PlayerSingleton.instance.gameObject;
        return player != null ? player.transform.position : Vector3.zero;
    }
}