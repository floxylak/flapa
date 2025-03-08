/*
using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _characterController;
        private InputController _inputController;
        private Transform _model;
        
        private Vector3 _playerVelocity;
        private Vector3 _moveDirection;
        
        private bool _isStumbling;
        private bool _isGrounded;
        private bool _lerpCrouch;
        private bool _isSprinting;
        
        private float _fearFactor = 1f;
        
        private Coroutine _stumbleCoroutine;
        
        private float _sprintSpeed;
        private float _crouchTimer;
        private float _originalSpeed;
        
        public bool toggleSprint;
        public bool toggleCrouch;
        
        private void Awake()
        {
            _characterController = GetComponent<CharacterController>() ?? throw new MissingComponentException($"{nameof(CharacterController)} is required on {gameObject.name}");
            _inputController = GetComponent<InputController>() ?? throw new MissingComponentException($"{nameof(InputController)} is required on {gameObject.name}");
            _model = transform.Find("Model") ?? throw new MissingReferenceException("Child object 'Model' not found on this GameObject.");
        }
        
        [SerializeField] private float speed = 5f;
        [SerializeField] private float gravity = -29.43f;
        [SerializeField] private float movementInertia = 3f;
        
        [SerializeField] private float stumbleDuration = 0.5f;
        [SerializeField] private float stumbleIntensity = 0.3f;
        [SerializeField] private float minSpeedFactor = 0.5f;
        [SerializeField] private float fearRecoverySpeed = 2f;

        private void Start()
        {
            _originalSpeed = speed;
            _sprintSpeed = speed * 1.5f;
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
                _playerVelocity.y += gravity * Time.deltaTime;
            }

            if (_isSprinting)
            {
                StopSprinting();
            }

            var effectiveSpeed = speed * _fearFactor * (_isStumbling ? stumbleIntensity : 1f);
            _characterController.Move((_moveDirection * effectiveSpeed + _playerVelocity) * Time.deltaTime);
        }

        public void Move(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f || _isStumbling)
            {
                _moveDirection = Vector3.Slerp(_moveDirection, Vector3.zero, movementInertia * Time.deltaTime);
                return;
            }

            var targetDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y).normalized);
            _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, movementInertia * Time.deltaTime);
        }

        public void Crouch(bool isCrouching)
        {
            _lerpCrouch = true;
            _crouchTimer = 0f;
            speed = isCrouching ? _originalSpeed * 0.5f : _originalSpeed;
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
            if (_isSprinting || _isStumbling) return;
            
            _isSprinting = true;
            speed = _sprintSpeed;
            
        }

        private void StopSprinting()
        {
            if (!_isSprinting) return;
            
            _isSprinting = false;
            speed = _originalSpeed;
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
            speed = _originalSpeed;
            var elapsed = 0f;

            while (elapsed < stumbleDuration)
            {
                elapsed += Time.deltaTime;
                var randomOffset = new Vector3(
                    Random.Range(-stumbleIntensity, stumbleIntensity),
                    0f,
                    Random.Range(-stumbleIntensity, stumbleIntensity)
                );
                _moveDirection += randomOffset;
                yield return null;
            }

            _isStumbling = false;
            _moveDirection = Vector3.zero;
        }

        private static IEnumerator FearRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            _isStumbling = true;
            var savedDirection = _moveDirection;
            _moveDirection = Vector3.zero;
            yield return new WaitForSeconds(duration);
            _isStumbling = false;
            _moveDirection = savedDirection;
        }

        private void UpdateFearFactor()
        {
            if (_fearFactor < 1f)
            {
                _fearFactor = Mathf.MoveTowards(_fearFactor, 1f, fearRecoverySpeed * Time.deltaTime);
            }
        }

        private void HandleCrouchLerp()
        {
            if (!_lerpCrouch) return;

            _crouchTimer += Time.deltaTime * 0.5f;
            var t = Mathf.SmoothStep(0f, 1f, _crouchTimer / 1.5f);

            _characterController.height = Mathf.Lerp(_characterController.height, _inputController.IsCrouching ? 1f : 2f, t);

            var targetScale = _inputController.IsCrouching ? new Vector3(1f, 0.5f, 1f) : Vector3.one;
            _model.localScale = Vector3.Lerp(_model.localScale, targetScale, t);

            if (t >= 1f)
            {
                _lerpCrouch = false;
            }
        }
    }
}
*/
