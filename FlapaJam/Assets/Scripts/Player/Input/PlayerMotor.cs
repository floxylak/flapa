using UnityEngine;
using UnityEngine.Serialization;

namespace  Player
{
    public class PlayerMotor : MonoBehaviour
    {
        private CharacterController _controller;
        private InputManager _inputManager;
        private Vector3 _playerVelocity;
        private Vector3 _moveDirection;
        private bool _isGrounded;
        private float _originalSpeed;

        public float speed = 5f;
        public float gravity = -9.81f * 3f;
        public float movementInertia = 3f;

        private float _sprintSpeed;

        private bool _lerpCrouch;
        private float _crouchTimer;

        public bool toggleSprint; // If true, sprint is toggled else it's hold
        public bool toggleCrouch; // If true, crouch is toggled else it's hold

        public Transform playerModel;

        private void Awake()
        {
            _inputManager = GetComponent<InputManager>();
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _originalSpeed = speed;
        }

        private void Update()
        {
            _sprintSpeed = speed * 1.5f;

            _isGrounded = _controller.isGrounded;
            HandleCrouchLerp();

            if (_isGrounded && _playerVelocity.y < 0)
            {
                _playerVelocity.y = -2f;
            }
            else if (!_isGrounded)
            {
                _playerVelocity.y += gravity * Time.deltaTime;
            }

            _controller.Move((_moveDirection * speed + _playerVelocity) * Time.deltaTime);
        }


        public void ProcessMove(Vector2 input)
        {
            if (input.magnitude == 0)
            {
                _moveDirection = Vector3.Lerp(_moveDirection, Vector3.zero, movementInertia * Time.deltaTime);
                return;
            }

            var targetDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y));
            _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, movementInertia * Time.deltaTime);
        }

        public void Crouch(bool keyDown)
        {
            _lerpCrouch = true;
            _crouchTimer = 0f;
            speed = keyDown ? _originalSpeed / 2 : _originalSpeed;
        }

        public void Sprint(bool keyDown)
        {
            speed = keyDown ? _sprintSpeed : _originalSpeed;
        }

        private void HandleCrouchLerp()
        {
            if (!_lerpCrouch)
                return;

            _crouchTimer += Time.deltaTime;
            float p = _crouchTimer / 1;
            p *= p;

            _controller.height = Mathf.Lerp(_controller.height, _inputManager.IsCrouching ? 1 : 2, p);

            if (playerModel is not null)
            {
                var targetScale = _inputManager.IsCrouching ? new Vector3(1, 0.5f, 1) : Vector3.one;
                playerModel.localScale = Vector3.Lerp(playerModel.localScale, targetScale, p);
            }

            if (p > 1)
            {
                _lerpCrouch = false;
                _crouchTimer = 0f;
            }
        }
    }
}