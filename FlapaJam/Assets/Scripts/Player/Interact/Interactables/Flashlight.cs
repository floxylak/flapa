using UnityEngine;
using System.Collections;

namespace Player.Interact
{
    public class Flashlight : Interactable
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Light flashlight;

        [Header("Flicker Settings")]
        [SerializeField] private float flickerMinInterval = 0.1f;
        [SerializeField] private float flickerMaxInterval = 0.5f;
        [SerializeField] private float flickerDuration = 2f;

        [Header("Dim Settings")]
        [SerializeField] private float minIntensity = 0.2f;
        [SerializeField] private float maxIntensity = 1.5f;
        [SerializeField] private float dimSpeed = 1f;

        private InventoryManager inventory;
        private InputManager inputManager;
        private bool isFlickering = false;
        private float originalIntensity;
        private Coroutine flickerCoroutine;

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
                }
                else
                {
                    Debug.LogError("Flashlight: Player reference could not be assigned. " +
                                 "Ensure the camera has enough parent levels or assign manually.", this);
                    return;
                }
            }
            
            if (flashlight == null)
            {
                flashlight = GetComponent<Light>();
                if (flashlight == null)
                {
                    Debug.LogWarning("Flashlight: No Light component found. " +
                                   "Attach a Light component to this GameObject.", this);
                    return;
                }
            }
            flashlight.enabled = false;
            originalIntensity = flashlight.intensity;
            
            inventory = player.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning("Flashlight: InventoryManager not found on player.", this);
            }

            inputManager = player.GetComponent<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("Flashlight: InputManager not found on player.", this);
            }
        }

        private void Update()
        {
            if (!IsValidSetup()) return;
            
            if (inventory.GetHeldItem() == gameObject)
            {
                if (inputManager.Inventory.Interact2.triggered)
                {
                    ToggleFlashlight();
                }
            }
            else if (flashlight.enabled)
            {
                flashlight.enabled = false;
                StopFlicker();
            }
        }

        protected override void Interact()
        {
            if (!IsValidSetup()) return;

            if (inventory.AddItem(gameObject))
            {
                Debug.Log($"Flashlight: Added {gameObject.name} to inventory.", this);
            }
        }

        private void ToggleFlashlight()
        {
            flashlight.enabled = !flashlight.enabled;
            if (!flashlight.enabled)
            {
                StopFlicker();
            }
            Debug.Log($"Flashlight: Turned {(flashlight.enabled ? "on" : "off")}", this);
        }

        // New method to start flickering effect
        public void StartFlicker()
        {
            if (!flashlight.enabled || isFlickering) return;
            if (flickerCoroutine != null) StopCoroutine(flickerCoroutine);
            flickerCoroutine = StartCoroutine(FlickerRoutine());
        }

        // New method to stop flickering
        public void StopFlicker()
        {
            if (flickerCoroutine != null)
            {
                StopCoroutine(flickerCoroutine);
                isFlickering = false;
                flashlight.enabled = true;
                flashlight.intensity = originalIntensity;
            }
        }

        // New method to temporarily disable flashlight
        public void DisableForDuration(float duration)
        {
            if (!IsValidSetup() || !flashlight.enabled) return;
            StartCoroutine(DisableRoutine(duration));
        }

        // New method to dim/flare the light
        public void DimLight(bool shouldDim, float duration = 1f)
        {
            if (!IsValidSetup() || !flashlight.enabled || isFlickering) return;
            StopFlicker();
            StartCoroutine(DimRoutine(shouldDim, duration));
        }

        private IEnumerator FlickerRoutine()
        {
            isFlickering = true;
            float timer = 0f;

            while (timer < flickerDuration && flashlight.enabled)
            {
                flashlight.enabled = !flashlight.enabled;
                float randomInterval = Random.Range(flickerMinInterval, flickerMaxInterval);
                yield return new WaitForSeconds(randomInterval);
                timer += randomInterval;
            }

            flashlight.enabled = true;
            flashlight.intensity = originalIntensity;
            isFlickering = false;
        }

        private IEnumerator DisableRoutine(float duration)
        {
            bool wasEnabled = flashlight.enabled;
            StopFlicker();
            flashlight.enabled = false;
            yield return new WaitForSeconds(duration);
            flashlight.enabled = wasEnabled;
            flashlight.intensity = originalIntensity;
        }

        private IEnumerator DimRoutine(bool shouldDim, float duration)
        {
            float targetIntensity = shouldDim ? minIntensity : maxIntensity;
            float startIntensity = flashlight.intensity;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                flashlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t * dimSpeed);
                yield return null;
            }

            flashlight.intensity = targetIntensity;
        }

        private bool IsValidSetup()
        {
            if (flashlight == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without a Light component.", this);
                return false;
            }
            if (inventory == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without an InventoryManager.", this);
                return false;
            }
            if (inputManager == null)
            {
                Debug.LogWarning("Flashlight: Cannot function without an InputManager.", this);
                return false;
            }
            return true;
        }
    }
}