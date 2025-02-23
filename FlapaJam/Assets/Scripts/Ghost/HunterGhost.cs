using UnityEngine;
using UnityEngine.AI;

public class SimpleHunterGhost : MonoBehaviour
{
    public float spawnRange = 10f;  // Spawn within this range of the player
    public float chaseSpeed = 5f;   // Speed when chasing
    private Transform player;
    private NavMeshAgent navAgent;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        navAgent = GetComponent<NavMeshAgent>();

        if (!player || !navAgent)
        {
            Debug.LogError("SimpleHunterGhost: Missing Player or NavMeshAgent!", this);
            return;
        }

        SpawnNearPlayer();
    }

    private void Update()
    {
        if (player)
        {
            navAgent.SetDestination(player.position);
        }
    }

    private void SpawnNearPlayer()
    {
        if (!player) return;

        Vector3 spawnPosition;
        if (FindSpawnPosition(out spawnPosition))
        {
            transform.position = spawnPosition;
            navAgent.Warp(spawnPosition);
            Debug.Log($"SimpleHunterGhost Spawned at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning("SimpleHunterGhost: Failed to find a valid spawn position!");
        }
    }

    private bool FindSpawnPosition(out Vector3 position)
    {
        int attempts = 0;
        while (attempts < 10)
        {
            Vector3 randomPoint = player.position + Random.insideUnitSphere * spawnRange;
            randomPoint.y = player.position.y; // Keep it at ground level

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, spawnRange, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
            attempts++;
        }

        position = player.position; // Fallback if no valid position found
        return false;
    }
}