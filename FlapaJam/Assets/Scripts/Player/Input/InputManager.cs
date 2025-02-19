using UnityEngine;

namespace Player
{
    public class InputManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private PlayerInput.OnFootActions _onFoot;

        private PlayerMotor _motor;
        private PlayerLook _look;

        private bool _isSprinting;
        private bool _isCrouching;
        public bool IsCrouching => _isCrouching;


        private void Awake()
        {
            _playerInput = new PlayerInput();
            _onFoot = _playerInput.OnFoot;

            _motor = GetComponent<PlayerMotor>();
            _look = GetComponent<PlayerLook>();

            _onFoot.Crouch.started += ctx => HandleCrouch(true);
            _onFoot.Crouch.canceled += ctx => HandleCrouch(false);

            _onFoot.Sprint.started += ctx => HandleSprint(true);
            _onFoot.Sprint.canceled += ctx => HandleSprint(false);
        }

        private void FixedUpdate()
        {
            _motor.ProcessMove(_onFoot.Movement.ReadValue<Vector2>());
        }

        private void LateUpdate()
        {
            _look.ProcessLook(_onFoot.Look.ReadValue<Vector2>());
        }

        private void OnEnable()
        {
            _onFoot.Enable();
        }

        private void OnDisable()
        {
            _onFoot.Disable();
        }

        private void HandleCrouch(bool keyDown)
        {
            if (_motor.toggleCrouch)
            {
                if (keyDown)
                {
                    _isCrouching = !_isCrouching;
                }
            }
            else
            {
                _isCrouching = keyDown;
            }

            _motor.Crouch(_isCrouching);
        }

        private void HandleSprint(bool keyDown)
        {

            if (_motor.toggleSprint)
            {
                if (keyDown)
                {
                    _isSprinting = !_isSprinting;
                }
            }
            else
            {
                _isSprinting = keyDown;
            }

            _motor.Sprint(_isSprinting);
        }
    }
}
