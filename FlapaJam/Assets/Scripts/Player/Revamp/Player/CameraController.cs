using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Added to check scene name

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [SerializeField] private Transform cameraPos;
        [SerializeField] private Transform player;

        private static Camera _mainCamera;

        private float _shakeTimer;
        private bool _isShaking;
        private Vector3 _originalCameraPosition;
        private Quaternion _forcedRotation;
        private bool _isForcedLooking;
        private Coroutine _shakeCoroutine;
        private float _xRotation;
        private float _sensitivityFactor = 1f;

        private static float _shakeIntensity = 0.1f;
        private static float _shakeFrequency = 10f;
        private const float SensitivityRecoverySpeed = 2f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (!player) player = transform;
            if (!cameraPos && Main) cameraPos = Main.transform.parent;
        }

        [SerializeField] private float xSens = 0.2f;
        [SerializeField] private float ySens = 0.2f;

        public static Transform Player => Instance?.player;
        public static Camera Main
        {
            get
            {
                if (_mainCamera is null)
                {
                    _mainCamera = Camera.main;
                    if (_mainCamera is null)
                    {
                        Debug.LogError("CameraController: No MainCamera found in the scene!");
                    }
                }
                return _mainCamera;
            }
        }

        public static float ShakeIntensity => _shakeIntensity;
        public static float ShakeFrequency => _shakeFrequency;

        public static void SetShakeIntensity(float value) => _shakeIntensity = value;
        public static void SetShakeFrequency(float value) => _shakeFrequency = value;

        private void Start()
        {
            if (Main)
            {
                _originalCameraPosition = Main.transform.localPosition;
            }

            // Ensure cursor updates correctly on scene start
            UpdateCursorState();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateCursorState();
        }

        private void UpdateCursorState()
        {
            if (SceneManager.GetActiveScene().name == "pridebunk")
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void Update()
        {
            if (!Main) return;

            UpdateSensitivityFactor();
            if (_isShaking) HandleCameraShake();
            if (_isForcedLooking)
            {
                Main.transform.rotation = Quaternion.Slerp(
                    Main.transform.rotation,
                    _forcedRotation,
                    Time.deltaTime * 5f
                );
            }
        }

        public void Look(Vector2 input)
        {
            if (!Main || _isForcedLooking) return;

            var effectiveXSensitivity = xSens * _sensitivityFactor;
            var effectiveYSensitivity = ySens * _sensitivityFactor;

            var mouseX = input.x * effectiveXSensitivity;
            var mouseY = input.y * effectiveYSensitivity;

            _xRotation = Mathf.Clamp(_xRotation - mouseY, -80f, 80f);

            Main.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            player.Rotate(Vector3.up * mouseX);
        }

        public void StartShake(float duration)
        {
            if (!Main || _isShaking) return;

            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _originalCameraPosition = Main.transform.localPosition;
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration));
        }

        public void ApplyDisorientation(float intensity, float duration)
        {
            if (!Main || intensity < 0f || intensity > 1f) return;

            _sensitivityFactor = Mathf.Min(_sensitivityFactor, 1f - intensity);
            StartCoroutine(DisorientationRoutine(duration));
        }

        public void ForceLookAt(Vector3 target, float duration)
        {
            if (!Main) return;

            _isForcedLooking = true;
            var direction = (target - Main.transform.position).normalized;
            _forcedRotation = Quaternion.LookRotation(direction);
            StartCoroutine(ForceLookRoutine(duration));
        }

        private void HandleCameraShake()
        {
            var xShake = (Mathf.PerlinNoise(Time.time * _shakeFrequency, 0f) - 0.5f) * 2f;
            var yShake = (Mathf.PerlinNoise(0f, Time.time * _shakeFrequency) - 0.5f) * 2f;
            var shakeOffset = new Vector3(xShake, yShake, 0f) * _shakeIntensity;
            Main.transform.localPosition = _originalCameraPosition + shakeOffset;
        }

        private void UpdateSensitivityFactor()
        {
            if (_sensitivityFactor < 1f)
            {
                _sensitivityFactor = Mathf.MoveTowards(
                    _sensitivityFactor,
                    1f,
                    SensitivityRecoverySpeed * Time.deltaTime
                );
            }
        }

        private IEnumerator ShakeRoutine(float duration)
        {
            _isShaking = true;
            _shakeTimer = duration;

            while (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;
                yield return null;
            }

            _isShaking = false;
            if (Main) Main.transform.localPosition = _originalCameraPosition;
        }

        private static IEnumerator DisorientationRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator ForceLookRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isForcedLooking = false;
        }
    }
}
