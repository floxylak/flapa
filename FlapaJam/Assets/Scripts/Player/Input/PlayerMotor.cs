using UnityEngine;
using System.Collections;

namespace Player
{
    public class PlayerMotor : MonoBehaviour
    {
        private CharacterController _characterController;
        private InputManager _inputManager;
        private Stats.PlayerStamina _stamina;
        private Transform _playerModel;

        private Vector3 _playerVelocity;
        private Vector3 _moveDirection;
        private float _originalSpeed;
        private float _sprintSpeed;
        private float _fearFactor = 1f;
        private bool _isGrounded;
        private bool _isSprinting;
        private bool _isStumbling;
        private bool _lerpCrouch;
        private float _crouchTimer;
        private Coroutine _stumbleCoroutine;

        [Header("Movement Settings")]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _gravity = -29.43f;
        [SerializeField] private float _movementInertia = 3f;

        [Header("Dynamic Effects")]
        [SerializeField] private float _stumbleDuration = 0.5f;
        [SerializeField] private float _stumbleIntensity = 0.3f;
        [SerializeField] private float _minSpeedFactor = 0.5f;
        [SerializeField] private float _fearRecoverySpeed = 2f;

        public bool toggleSprint;
        public bool toggleCrouch;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>() ?? throw new MissingComponentException($"{nameof(CharacterController)} is required on {gameObject.name}");
            _inputManager = GetComponent<InputManager>() ?? throw new MissingComponentException($"{nameof(InputManager)} is required on {gameObject.name}");
            _stamina = GetComponent<Player.Stats.PlayerStamina>() ?? throw new MissingComponentException($"{nameof(Player.Stats.PlayerStamina)} is required on {gameObject.name}");
            _playerModel = transform.Find("Model") ?? throw new MissingReferenceException("Child object 'Model' not found on this GameObject.");
        }

        private void Start()
        {
            _originalSpeed = _speed;
            _sprintSpeed = _speed * 1.5f;
        }

        private void Update()
        {
            _isGrounded = _characterController.isGrounded;

            HandleCrouchLerp();
            UpdateFearFactor();

            if (_isGrounded)
            {
                _playerVelocity.y = -2f;
            }
            else
            {
                _playerVelocity.y += _gravity * Time.deltaTime;
            }

            if (_isSprinting && _stamina.currentStamina <= 0)
            {
                StopSprinting();
            }

            float effectiveSpeed = _speed * _fearFactor * (_isStumbling ? _stumbleIntensity : 1f);
            _characterController.Move((_moveDirection * effectiveSpeed + _playerVelocity) * Time.deltaTime);
        }

        public void ProcessMove(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f || _isStumbling)
            {
                _moveDirection = Vector3.Slerp(_moveDirection, Vector3.zero, _movementInertia * Time.deltaTime);
                return;
            }

            Vector3 targetDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y).normalized);
            _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, _movementInertia * Time.deltaTime);
        }

        public void Crouch(bool isCrouching)
        {
            _lerpCrouch = true;
            _crouchTimer = 0f;
            _speed = isCrouching ? _originalSpeed * 0.5f : _originalSpeed;
        }

        public void Sprint(bool isSprinting)
        {
            if (isSprinting)
            {
                StartSprinting();
            }
            else
            {
                StopSprinting();
            }
        }

        private void StartSprinting()
        {
            if (_stamina.CanSprint() && !_isSprinting && !_isStumbling)
            {
                _isSprinting = true;
                _speed = _sprintSpeed;
                _stamina.StartUsingStamina();
            }
        }

        private void StopSprinting()
        {
            if (_isSprinting)
            {
                _isSprinting = false;
                _speed = _originalSpeed;
                _stamina.StopUsingStamina();
            }
        }

        public void Stumble()
        {
            if (_isStumbling) return;
            if (_stumbleCoroutine != null) StopCoroutine(_stumbleCoroutine);
            _stumbleCoroutine = StartCoroutine(StumbleRoutine());
        }

        public void ApplyFear(float intensity, float duration)
        {
            if (intensity < 0f || intensity > 1f) return;
            _fearFactor = Mathf.Min(_fearFactor, 1f - intensity);
            StartCoroutine(FearRoutine(duration));
        }

        public void FreezeMovement(float duration)
        {
            if (_stumbleCoroutine != null) StopCoroutine(_stumbleCoroutine);
            _stumbleCoroutine = StartCoroutine(FreezeRoutine(duration));
        }

        private IEnumerator StumbleRoutine()
        {
            _isStumbling = true;
            _isSprinting = false;
            _speed = _originalSpeed;
            float elapsed = 0f;

            while (elapsed < _stumbleDuration)
            {
                elapsed += Time.deltaTime;
                Vector3 randomOffset = new Vector3(
                    Random.Range(-_stumbleIntensity, _stumbleIntensity),
                    0f,
                    Random.Range(-_stumbleIntensity, _stumbleIntensity)
                );
                _moveDirection += randomOffset;
                yield return null;
            }

            _isStumbling = false;
            _moveDirection = Vector3.zero;
        }

        private IEnumerator FearRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            _isStumbling = true;
            Vector3 savedDirection = _moveDirection;
            _moveDirection = Vector3.zero;
            yield return new WaitForSeconds(duration);
            _isStumbling = false;
            _moveDirection = savedDirection;
        }

        private void UpdateFearFactor()
        {
            if (_fearFactor < 1f)
            {
                _fearFactor = Mathf.MoveTowards(_fearFactor, 1f, _fearRecoverySpeed * Time.deltaTime);
            }
        }

        private void HandleCrouchLerp()
        {
            if (!_lerpCrouch) return;

            _crouchTimer += Time.deltaTime * 0.5f;
            float t = Mathf.SmoothStep(0f, 1f, _crouchTimer / 1.5f);

            _characterController.height = Mathf.Lerp(_characterController.height, _inputManager.IsCrouching ? 1f : 2f, t);

            Vector3 targetScale = _inputManager.IsCrouching ? new Vector3(1f, 0.5f, 1f) : Vector3.one;
            _playerModel.localScale = Vector3.Lerp(_playerModel.localScale, targetScale, t);

            if (t >= 1f)
            {
                _lerpCrouch = false;
            }
        }
    }
}