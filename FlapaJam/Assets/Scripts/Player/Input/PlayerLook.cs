using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] public Camera _camera;
        private float _xRotation;

        [Header("Look Settings")]
        [SerializeField] private float _xSensitivity = 0.2f;
        [SerializeField] private float _ySensitivity = 0.2f;

        [Header("Dynamic Effects")]
        [SerializeField] private float _shakeIntensity = 0.1f;
        [SerializeField] private float _shakeFrequency = 10f;
        [SerializeField] private float _minSensitivityFactor = 0.3f;
        [SerializeField] private float _sensitivityRecoverySpeed = 2f;

        private float _shakeTimer;
        private bool _isShaking;
        private float _sensitivityFactor = 1f;
        private Vector3 _originalCameraPosition;
        private Quaternion _forcedRotation;
        private bool _isForcedLooking;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _camera = GetComponentInChildren<Camera>() ?? Camera.main ?? throw new MissingReferenceException("No camera assigned and no main camera found.");
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _originalCameraPosition = _camera.transform.localPosition;
        }

        private void Update()
        {
            UpdateSensitivityFactor();
            if (_isShaking) HandleCameraShake();
            if (_isForcedLooking) _camera.transform.rotation = Quaternion.Slerp(_camera.transform.rotation, _forcedRotation, Time.deltaTime * 5f);
        }

        public void ProcessLook(Vector2 input)
        {
            if (_isForcedLooking) return;

            float effectiveXSensitivity = _xSensitivity * _sensitivityFactor;
            float effectiveYSensitivity = _ySensitivity * _sensitivityFactor;

            float mouseX = input.x * effectiveXSensitivity;
            float mouseY = input.y * effectiveYSensitivity;

            _xRotation = Mathf.Clamp(_xRotation - mouseY, -80f, 80f);

            _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        public void StartShake(float duration)
        {
            if (_isShaking) return;
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration));
        }

        public void ApplyDisorientation(float intensity, float duration)
        {
            if (intensity < 0f || intensity > 1f) return;
            _sensitivityFactor = Mathf.Min(_sensitivityFactor, 1f - intensity);
            StartCoroutine(DisorientationRoutine(duration));
        }

        public void ForceLookAt(Vector3 target, float duration)
        {
            _isForcedLooking = true;
            Vector3 direction = (target - _camera.transform.position).normalized;
            _forcedRotation = Quaternion.LookRotation(direction);
            StartCoroutine(ForceLookRoutine(duration));
        }

        private void HandleCameraShake()
        {
            float xShake = Mathf.PerlinNoise(Time.time * _shakeFrequency, 0f) - 0.5f;
            float yShake = Mathf.PerlinNoise(0f, Time.time * _shakeFrequency) - 0.5f;
            Vector3 shakeOffset = new Vector3(xShake, yShake, 0f) * _shakeIntensity;
            _camera.transform.localPosition = _originalCameraPosition + shakeOffset;
        }

        private void UpdateSensitivityFactor()
        {
            if (_sensitivityFactor < 1f)
            {
                _sensitivityFactor = Mathf.MoveTowards(_sensitivityFactor, 1f, _sensitivityRecoverySpeed * Time.deltaTime);
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
            _camera.transform.localPosition = _originalCameraPosition;
        }

        private IEnumerator DisorientationRoutine(float duration)
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