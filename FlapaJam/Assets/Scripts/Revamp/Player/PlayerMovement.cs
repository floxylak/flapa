using UnityEngine;
using System.Collections;

namespace Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputCont))]
    public class PlayerMovement : MonoBehaviour
    {
        private CharacterController _characterController;
        private PlayerInputCont _inputController;
        private Transform _model;

        private Vector3 _playerVelocity;
        private Vector3 _moveDirection;

        private bool _isStumbling;
        private bool _isGrounded;
        private bool _lerpCrouch;
        private bool _isSprinting;
        private bool _freezeMovement;

        private float _fearFactor = 1f;

        // Stamina
        [Header("Stamina")]
        public float maxStamina = 100f;
        public float currentStamina;
        [SerializeField] private float staminaRechargeCooldown = 2f;
        [SerializeField] private float staminaRegenSpeed = 10f;
        [SerializeField] private float staminaDegenSpeed = 20f;
        private float _staminaCooldownTimer;

        // Movement speeds
        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float crouchSpeedFactor = 0.5f;
        [SerializeField] private float movementInertia = 3f;
        [SerializeField] private float gravity = -29.43f;

        private float _currentSpeed;
        private Coroutine _stumbleCoroutine;

        // Stumble
        [Header("Stumble")]
        [SerializeField] private float stumbleDuration = 0.5f;
        [SerializeField] private float stumbleIntensity = 0.3f;

        // Fear System
        [Header("Fear System")]
        [SerializeField] private float fearRecoverySpeed = 2f;

        // Audio
        [Header("Audio")]
        [SerializeField] private AudioSource outOfBreathSound;
        [SerializeField] private float crouchingVolume = 0.1f;
        [SerializeField] private float normalVolume = 1f;
        private float _currentFootstepVolume;

        // Crouch
        private float _crouchTimer;
        [SerializeField] private Transform headCheck;
        [SerializeField] private float headCheckRadius = 0.4f;
        [SerializeField] private LayerMask headMask;

        public bool moving => _characterController.velocity.magnitude > 0.1f;
        public bool sprinting => _isSprinting;
        public bool crouching => _inputController.IsCrouching;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputController = GetComponent<PlayerInputCont>();
            _model = transform.Find("Model") ?? throw new MissingReferenceException("Child 'Model' not found.");

            _currentSpeed = baseSpeed;
            _currentFootstepVolume = normalVolume;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            if (_freezeMovement) return;

            _isGrounded = _characterController.isGrounded;

            HandleCrouchLerp();
            UpdateFearFactor();
            HandleStamina();
            ApplyGravity();

            // Check if sprinting should stop due to stamina
            if (_isSprinting && !HasStamina())
            {
                StopSprinting();
            }

            float effectiveSpeed = _currentSpeed * _fearFactor * (_isStumbling ? stumbleIntensity : 1f);
            Vector3 movement = (_moveDirection * effectiveSpeed + _playerVelocity) * Time.deltaTime;

            _characterController.Move(movement);
            UpdateFootsteps();
        }

        public void Move(Vector2 input)
        {
            if (_freezeMovement || input.sqrMagnitude < 0.01f || _isStumbling)
            {
                _moveDirection = Vector3.Slerp(_moveDirection, Vector3.zero, movementInertia * Time.deltaTime);
                return;
            }

            Vector3 targetDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y).normalized);
            _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, movementInertia * Time.deltaTime);
        }

        public void Crouch(bool isCrouching)
        {
            if (isCrouching || !Physics.CheckSphere(headCheck.position, headCheckRadius, headMask))
            {
                _lerpCrouch = true;
                _crouchTimer = 0f;
                _currentSpeed = isCrouching ? baseSpeed * crouchSpeedFactor : baseSpeed;
                SetFootstepVolume(isCrouching ? crouchingVolume : normalVolume);

                // Ensure sprinting stops when crouching
                if (isCrouching && _isSprinting)
                {
                    StopSprinting();
                }
            }
        }

        public void Sprint(bool isSprinting)
        {
            if (isSprinting && HasStamina() && !_inputController.IsCrouching)
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
            if (_isSprinting || _isStumbling) return;
            _isSprinting = true;
            _currentSpeed = baseSpeed * sprintMultiplier;
        }

        private void StopSprinting()
        {
            if (!_isSprinting) return;
            _isSprinting = false;
            _currentSpeed = baseSpeed;
        }

        public void Stumble()
        {
            if (_isStumbling) return;
            if (_stumbleCoroutine != null) StopCoroutine(_stumbleCoroutine);
            _stumbleCoroutine = StartCoroutine(StumbleRoutine());
        }

        public void ApplyFear(float intensity, float duration)
        {
            _fearFactor = Mathf.Min(_fearFactor, 1f - Mathf.Clamp01(intensity));
            StartCoroutine(FearRoutine(duration));
        }

        public void FreezeMovement(float duration)
        {
            if (_stumbleCoroutine != null) StopCoroutine(_stumbleCoroutine);
            _stumbleCoroutine = StartCoroutine(FreezeRoutine(duration));
        }

        private bool HasStamina() => currentStamina > 0;

        private void HandleStamina()
        {
            _staminaCooldownTimer += Time.deltaTime;

            if (_isSprinting)
            {
                currentStamina -= staminaDegenSpeed * Time.deltaTime;
                _staminaCooldownTimer = 0;
            }
            else if (_staminaCooldownTimer > staminaRechargeCooldown)
            {
                currentStamina += staminaRegenSpeed * Time.deltaTime;
            }

            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (currentStamina <= 0 && outOfBreathSound && !_isSprinting && !outOfBreathSound.isPlaying)
            {
                outOfBreathSound.Play();
            }
        }

        private void ApplyGravity()
        {
            _playerVelocity.y = _isGrounded ? -2f : _playerVelocity.y + gravity * Time.deltaTime;
        }

        private void UpdateFootsteps()
        {
            // Hook in your footstep system here
        }

        private void SetFootstepVolume(float volume)
        {
            _currentFootstepVolume = volume;
            // Hook your footstep system volume update here
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            _freezeMovement = true;
            Vector3 savedDirection = _moveDirection;
            _moveDirection = Vector3.zero;

            yield return new WaitForSeconds(duration);

            _freezeMovement = false;
            _moveDirection = savedDirection;
        }

        private IEnumerator StumbleRoutine()
        {
            _isStumbling = true;
            StopSprinting();
            _currentSpeed = baseSpeed;

            float timer = 0f;
            while (timer < stumbleDuration)
            {
                timer += Time.deltaTime;
                _moveDirection += new Vector3(
                    Random.Range(-stumbleIntensity, stumbleIntensity),
                    0f,
                    Random.Range(-stumbleIntensity, stumbleIntensity)
                );
                yield return null;
            }

            _isStumbling = false;
            _moveDirection = Vector3.zero;
        }

        private IEnumerator FearRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private void UpdateFearFactor()
        {
            _fearFactor = Mathf.MoveTowards(_fearFactor, 1f, fearRecoverySpeed * Time.deltaTime);
        }

        private void HandleCrouchLerp()
        {
            if (!_lerpCrouch) return;

            _crouchTimer += Time.deltaTime * 0.5f;
            float t = Mathf.SmoothStep(0f, 1f, _crouchTimer / 1.5f);

            _characterController.height = Mathf.Lerp(_characterController.height, _inputController.IsCrouching ? 1f : 2f, t);
            _model.localScale = Vector3.Lerp(_model.localScale, _inputController.IsCrouching ? new Vector3(1f, 0.5f, 1f) : Vector3.one, t);

            if (t >= 1f) _lerpCrouch = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }
    }
}