using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private PlayerInput.OnFootActions _onFoot;
        private PlayerInput.InventoryActions _inventory;

        private PlayerMotor _playerMotor;
        private PlayerLook _playerLook;

        private bool _isSprinting;
        private bool _isCrouching;

        // Public getters for input actions
        public PlayerInput.OnFootActions OnFoot => _onFoot;
        public PlayerInput.InventoryActions Inventory => _inventory;

        public bool IsCrouching => _isCrouching;

        private void Awake()
        {
            // Get components and enforce their presence at startup
            _playerMotor = GetComponent<PlayerMotor>() ?? throw new MissingComponentException($"{nameof(PlayerMotor)} is required on {gameObject.name}");
            _playerLook = GetComponent<PlayerLook>() ?? throw new MissingComponentException($"{nameof(PlayerLook)} is required on {gameObject.name}");
        }

        private void OnEnable()
        {
            _playerInput = new PlayerInput();
            _onFoot = _playerInput.OnFoot;
            _inventory = _playerInput.Inventory;

            _onFoot.Enable();
            _inventory.Enable();

            // Subscribe to input events
            _onFoot.Crouch.started += HandleCrouchStart;
            _onFoot.Crouch.canceled += HandleCrouchCancel;
            _onFoot.Sprint.started += HandleSprintStart;
            _onFoot.Sprint.canceled += HandleSprintCancel;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            _onFoot.Crouch.started -= HandleCrouchStart;
            _onFoot.Crouch.canceled -= HandleCrouchCancel;
            _onFoot.Sprint.started -= HandleSprintStart;
            _onFoot.Sprint.canceled -= HandleSprintCancel;

            _onFoot.Disable();
            _inventory.Disable();
        }

        private void FixedUpdate()
        {
            _playerMotor.ProcessMove(_onFoot.Movement.ReadValue<Vector2>());
        }

        private void LateUpdate()
        {
            // No null checks
            _playerLook.ProcessLook(_onFoot.Look.ReadValue<Vector2>());
        }

        private void HandleCrouchStart(InputAction.CallbackContext ctx)
        {
            HandleCrouch(true);
        }

        private void HandleCrouchCancel(InputAction.CallbackContext ctx)
        {
            HandleCrouch(false);
        }

        private void HandleSprintStart(InputAction.CallbackContext ctx)
        {
            HandleSprint(true);
        }

        private void HandleSprintCancel(InputAction.CallbackContext ctx)
        {
            HandleSprint(false);
        }

        private void HandleCrouch(bool isKeyDown)
        {
            _isCrouching = _playerMotor.toggleCrouch ? (isKeyDown && !_isCrouching) : isKeyDown;
            _playerMotor.Crouch(_isCrouching);
        }

        private void HandleSprint(bool isKeyDown)
        {
            _isSprinting = _playerMotor.toggleSprint ? (isKeyDown && !_isSprinting) : isKeyDown;
            _playerMotor.Sprint(_isSprinting);
        }
    }
}