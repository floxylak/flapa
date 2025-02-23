using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Player
{
    public class RandomRoom : MonoBehaviour
    {
        public bool ghostEnabled = true;  // Toggle ghost activity on/off
        public bool debugLogsEnabled = true; // Toggle logs on/off for performance

        [Header("Ghost Settings")]
        [SerializeField] private float disappearDistance = 5f;
        [SerializeField] private float minNoiseInterval = 5f;
        [SerializeField] private float maxNoiseInterval = 15f;
        [SerializeField] private AudioClip[] noiseClips;
        [SerializeField] private float flickerLightChance = 0.3f;

        [Header("Spawn Settings")]
        [SerializeField] private Transform roomSpawnPointsParent;
        [SerializeField] private float minSpawnDistance = 5f;
        [SerializeField] private float maxSpawnDistance = 15f;
        [SerializeField] private float baseSpawnChance = 0.2f;
        [SerializeField] private float spawnCheckInterval = 30f;
        [SerializeField] private float respawnDelay = 10f;

        [Header("Ghost Model")]
        [SerializeField] private Transform ghostModel;

        private AudioSource audioSource;
        private Transform player;
        private GameManager gameManager;
        private Renderer ghostRenderer;
        private float noiseTimer;
        private float spawnTimer;
        private float respawnDelayTimer;
        private List<Transform> roomSpawnPoints = new List<Transform>();
        private bool isActive = false;
        private Light[] nearbyLights;

        public delegate void GhostEventHandler(RandomRoom ghost);
        public event GhostEventHandler OnSpawned;
        public event GhostEventHandler OnDespawned;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            gameManager = FindObjectOfType<GameManager>();
            InitializeSpawnPoints();

            if (ghostModel == null)
            {
                ghostModel = transform.Find("RoomGhostModel");
                if (ghostModel == null) Debug.LogError("GhostModel child not found in RandomRoom!", this);
            }

            ghostRenderer = ghostModel?.GetComponentInChildren<Renderer>();
            if (!audioSource) Debug.LogError("AudioSource missing on RandomRoom!", this);
            if (!player) Debug.LogError("Player not found for RandomRoom!", this);
            if (!gameManager) Debug.LogError("GameManager not found for RandomRoom!", this);
            if (roomSpawnPoints.Count == 0) Debug.LogError("No RoomSpawnPoints found under RoomSpawnPointsParent!", this);
            if (!ghostRenderer) Debug.LogError("Ghost Renderer missing on ghost model!", this);

            if (debugLogsEnabled) Debug.Log("RandomRoom Awake: Initialized components.", this);
        }

        private void Start()
        {
            spawnTimer = 0f; // Immediate spawn for testing
            respawnDelayTimer = 0f;
            if (ghostEnabled)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom Start: Attempting immediate spawn.", this);
                Spawn();
            }
        }

        private void Update()
        {
            if (!ghostEnabled) return;

            if (!isActive)
            {
                if (respawnDelayTimer > 0f)
                {
                    respawnDelayTimer -= Time.deltaTime;
                    return;
                }

                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0f)
                {
                    TryRespawn();
                    spawnTimer = spawnCheckInterval;
                }
                return;
            }

            float distanceToPlayer = Vector3.Distance(ghostModel.position, player.position);
            if (distanceToPlayer < disappearDistance)
            {
                Despawn();
                return;
            }

            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0f)
            {
                PlayRandomNoise();
                ScheduleNextNoise();
                TryFlickerLights();
            }
        }

        private void InitializeSpawnPoints()
        {
            if (roomSpawnPointsParent == null)
            {
                roomSpawnPointsParent = transform.Find("RoomSpawnPoints");
                if (roomSpawnPointsParent == null)
                {
                    Debug.LogError("RoomSpawnPoints parent not found as child of RandomRoom!", this);
                    return;
                }
            }

            foreach (Transform child in roomSpawnPointsParent)
            {
                roomSpawnPoints.Add(child);
                if (debugLogsEnabled) Debug.Log($"RandomRoom InitializeSpawnPoints: Added spawn point: {child.name}", this);
            }
            if (debugLogsEnabled) Debug.Log($"RandomRoom InitializeSpawnPoints: Total spawn points: {roomSpawnPoints.Count}", this);
        }

        public void Spawn()
        {
            if (!ghostEnabled || isActive || roomSpawnPoints.Count == 0)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom Spawn: Cannot spawn - disabled, active, or no spawn points.", this);
                return;
            }

            List<Transform> validSpawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in roomSpawnPoints)
            {
                float distanceToPlayer = Vector3.Distance(spawnPoint.position, player.position);
                if (distanceToPlayer >= minSpawnDistance && distanceToPlayer <= maxSpawnDistance)
                {
                    validSpawnPoints.Add(spawnPoint);
                }
            }

            if (validSpawnPoints.Count == 0)
            {
                if (debugLogsEnabled) Debug.LogWarning("RandomRoom Spawn: No valid spawn points found!", this);
                return;
            }

            int spawnIndex = Random.Range(0, validSpawnPoints.Count);
            ghostModel.position = validSpawnPoints[spawnIndex].position;
            float distanceCheck = Vector3.Distance(ghostModel.position, player.position);
            if (debugLogsEnabled) Debug.Log($"RandomRoom Spawn: Moving ghost model to {validSpawnPoints[spawnIndex].name}, distance to player: {distanceCheck:F2}m", this);

            if (distanceCheck < minSpawnDistance)
            {
                if (debugLogsEnabled) Debug.LogWarning("RandomRoom Spawn: Spawn point too close, repositioning...", this);
                Spawn();
                return;
            }

            gameObject.SetActive(true);
            isActive = true;
            ScheduleNextNoise();
            OnSpawned?.Invoke(this);
            if (debugLogsEnabled) Debug.Log($"RandomRoom Spawn: Ghost spawned at {validSpawnPoints[spawnIndex].name} on Day {gameManager?.CurrentDay}", this);
        }

        public void Despawn()
        {
            if (!isActive)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom Despawn: Cannot despawn - not active.", this);
                return;
            }

            isActive = false;
            gameObject.SetActive(false);
            respawnDelayTimer = respawnDelay;
            OnDespawned?.Invoke(this);
            if (debugLogsEnabled) Debug.Log($"RandomRoom Despawn: Ghost immediately despawned, respawn delay set to {respawnDelay}s", this);
        }

        private void TryRespawn()
        {
            if (!ghostEnabled || gameManager == null)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom TryRespawn: Cannot respawn - disabled or no GameManager.", this);
                return;
            }

            float dayProgress = (float)gameManager.CurrentDay / gameManager.MaxDays;
            float spawnChance = Mathf.Lerp(baseSpawnChance, 1f, dayProgress);
            if (debugLogsEnabled) Debug.Log($"RandomRoom TryRespawn: Day progress: {dayProgress:F2}, spawn chance: {spawnChance:F2}", this);

            if (Random.value < spawnChance)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom TryRespawn: Spawn chance succeeded, spawning ghost.", this);
                Spawn();
            }
            else
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom TryRespawn: Spawn chance failed, no spawn this time.", this);
            }
        }

        private static float globalNoiseCooldown = 0f;

        private void PlayRandomNoise()
        {
            if (!ghostEnabled || audioSource == null || noiseClips.Length == 0 || audioSource.isPlaying)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom PlayRandomNoise: Cannot play noise - disabled, no source, no clips, or already playing.", this);
                return;
            }

            if (globalNoiseCooldown > 0f)
            {
                if (debugLogsEnabled) Debug.Log($"RandomRoom PlayRandomNoise: Global noise cooldown active, time left: {globalNoiseCooldown:F2}s", this);
                return;
            }

            audioSource.clip = noiseClips[Random.Range(0, noiseClips.Length)];
            audioSource.Play();
            globalNoiseCooldown = 5f;
            if (debugLogsEnabled) Debug.Log("RandomRoom PlayRandomNoise: Playing random noise, cooldown set to 5s.", this);

            StartCoroutine(ResetNoiseCooldown());
        }

        private IEnumerator ResetNoiseCooldown()
        {
            yield return new WaitForSeconds(globalNoiseCooldown);
            globalNoiseCooldown = 0f;
            if (debugLogsEnabled) Debug.Log("RandomRoom ResetNoiseCooldown: Noise cooldown reset.", this);
        }

        private void ScheduleNextNoise()
        {
            noiseTimer = Random.Range(minNoiseInterval, maxNoiseInterval);
            if (debugLogsEnabled) Debug.Log($"RandomRoom ScheduleNextNoise: Next noise scheduled in {noiseTimer:F2}s", this);
        }

        private void TryFlickerLights()
        {
            if (!ghostEnabled || Random.value >= flickerLightChance || nearbyLights.Length == 0)
            {
                if (debugLogsEnabled) Debug.Log("RandomRoom TryFlickerLights: No flicker - disabled, chance failed, or no lights.", this);
                return;
            }

            if (debugLogsEnabled) Debug.Log("RandomRoom TryFlickerLights: Would flicker lights if implemented!", this);
        }

        private void OnValidate()
        {
            if (noiseClips == null || noiseClips.Length == 0)
                Debug.LogWarning("No noise clips assigned to RandomRoom ghost!", this);
            if (disappearDistance < 0f) disappearDistance = 5f;
            if (minNoiseInterval < 0f) minNoiseInterval = 0f;
            if (maxNoiseInterval < minNoiseInterval) maxNoiseInterval = minNoiseInterval;
            if (minSpawnDistance < disappearDistance) minSpawnDistance = disappearDistance;
            if (maxSpawnDistance < minSpawnDistance) maxSpawnDistance = minSpawnDistance;
            if (baseSpawnChance < 0f || baseSpawnChance > 1f) baseSpawnChance = 0.2f;
            if (spawnCheckInterval < 1f) spawnCheckInterval = 30f;
            if (respawnDelay < 0f) respawnDelay = 10f;
        }
    }
}