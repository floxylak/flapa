using UnityEngine;

namespace Item
{
    public class RoomController : MonoBehaviour
    {
        [SerializeField] private GameObject floor;
        [SerializeField] private Transform player;
        
        public float roomSize = 10f;
        public float spawnDistance = 5f; 

        private Vector2Int _currentGridPos; 
        private GameObject[,] _floorGrid; // 2D array to store spawned floors
        private int _gridSize = 100; // Max grid size
        private Vector3 _lastPlayerPos; // Track last position to calculate movement
    }
}