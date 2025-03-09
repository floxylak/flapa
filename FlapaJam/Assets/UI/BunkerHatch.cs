using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Player.Interact
{
    public class BunkerHatch : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject interactiveCanvas;
        [SerializeField] private TMP_Text promptText;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private PlayerInputCont inputManager;
        private bool isPlayerInRange = false;

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (player == null)
            {
                if (Camera.main != null && Camera.main.transform.parent != null &&
                    Camera.main.transform.parent.parent != null)
                {
                    player = Camera.main.transform.parent.parent;
                    Debug.Log("BunkerHatch: Player automatically assigned.");
                }
                else
                {
                    Debug.LogError("BunkerHatch: Player reference could not be assigned!", this);
                    return;
                }
            }

            inputManager = player.GetComponent<PlayerInputCont>();
            if (inputManager == null)
            {
                Debug.LogWarning("BunkerHatch: InputManager not found on player!", this);
            }

            if (interactiveCanvas == null)
            {
                Debug.LogWarning("BunkerHatch: Interactive Canvas not assigned!", this);
            }

            if (promptText == null)
            {
                Debug.LogWarning("BunkerHatch: Prompt Text not assigned!", this);
            }

            if (interactiveCanvas != null)
            {
                interactiveCanvas.SetActive(false);
            }
        }

        private void Update()
        {
            if (!IsValidSetup()) return;

            float distance = Vector3.Distance(transform.position, player.position);
            isPlayerInRange = distance <= interactionDistance;

            if (interactiveCanvas != null)
            {
                interactiveCanvas.SetActive(isPlayerInRange);
                if (isPlayerInRange && promptText != null)
                {
                    promptText.text = $"Press {interactKey} to enter bunker";
                }
            }

            if (isPlayerInRange && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }

        private void Interact()
        {
            if (!IsValidSetup()) return;

            SceneManager.LoadScene(0); // Load pridebunk (Scene 0)
            SceneManager.sceneLoaded += OnPrideSceneLoaded;
            Debug.Log("BunkerHatch: Loading Scene 0 (pridebunk)");
        }

        private void OnPrideSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex != 0 || scene.name != "pridebunk")
            {
                Debug.LogError($"BunkerHatch: Expected 'pridebunk' (Scene 0), got '{scene.name}' (Index {scene.buildIndex})!");
                SceneManager.sceneLoaded -= OnPrideSceneLoaded;
                return;
            }

            // Find the player (persisted via DontDestroyOnLoad)
            if (player == null) player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("BunkerHatch: Player not found in pridebunk!");
                return;
            }

            var playerCamera = GameObject.FindWithTag("PlayerCamera");
            if (playerCamera != null)
            {
                player.transform.SetPositionAndRotation(playerCamera.transform.position, playerCamera.transform.rotation);
                Debug.Log("BunkerHatch: Player moved to PlayerCamera position in pridebunk.");
            }
            else
            {
                Debug.LogError("BunkerHatch: 'PlayerCamera' not found in pridebunk!");
            }

            SceneManager.sceneLoaded -= OnPrideSceneLoaded;
        }

        private bool IsValidSetup()
        {
            if (player == null || inputManager == null) return false;
            return true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}