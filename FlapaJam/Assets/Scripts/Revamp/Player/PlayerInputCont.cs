using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputCont : MonoBehaviour
    {
        private PlayerMovement _playerController;
        private PlayerCamera _cameraController;
        // Removed _cameraLeaning reference since CameraLeaning is no longer used
        // private CameraLeaning _cameraLeaning; 
        // private InventoryController _inventoryController;

        private PlayerInput _playerInput;
        private PlayerInput.OnFootActions _onFoot;
        private PlayerInput.InventoryActions _inventory;

        private bool _isSprinting;
        private bool _isCrouching;

        private bool _interactHeld;

        public bool toggleSprint = false;
        public bool toggleCrouch = false;

        public float Leaning { get; private set; }

        // Getters
        public PlayerInput.OnFootActions OnFoot => _onFoot;
        public PlayerInput.InventoryActions Inventory => _inventory;

        public bool IsCrouching => _isCrouching;
        public bool IsSprinting => _isSprinting;

        public bool InteractHeld => _onFoot.Interact.IsPressed();

        private void Awake()
        {
            _playerController = GetComponent<PlayerMovement>() ?? throw new MissingComponentException($"{nameof(PlayerMovement)} is required on {gameObject.name}");
            _cameraController = GetComponent<PlayerCamera>() ?? throw new MissingComponentException($"{nameof(PlayerCamera)} is required on {gameObject.name}");
            // Removed _cameraLeaning = GetComponentInChildren<CameraLeaning>();
            // No need to check for CameraLeaning since it's no longer used
            // _inventoryController = GetComponent<InventoryController>() ?? throw new MissingComponentException($"{nameof(InventoryController)} is required on {gameObject.name}");
        }

        private void OnEnable()
        {
            _playerInput = new PlayerInput();
            _onFoot = _playerInput.OnFoot;
            _inventory = _playerInput.Inventory;

            _onFoot.Enable();
            _inventory.Enable();

            _onFoot.Crouch.started += HandleCrouchStart;
            _onFoot.Crouch.canceled += HandleCrouchCancel;

            _onFoot.Sprint.started += HandleSprintStart;
            _onFoot.Sprint.canceled += HandleSprintCancel;

            _inventory.Cycle.performed += HandleSwitchSlot;
        }

        private void OnDisable()
        {
            _onFoot.Crouch.started -= HandleCrouchStart;
            _onFoot.Crouch.canceled -= HandleCrouchCancel;

            _onFoot.Sprint.started -= HandleSprintStart;
            _onFoot.Sprint.canceled -= HandleSprintCancel;

            _inventory.Cycle.performed -= HandleSwitchSlot;

            _onFoot.Disable();
            _inventory.Disable();
        }

        private void FixedUpdate()
        {
            _playerController.Move(_onFoot.Movement.ReadValue<Vector2>());
            Leaning = _onFoot.Leaning.ReadValue<float>();
            _cameraController.SetLeaning(Leaning); // Only call SetLeaning on PlayerCamera
            // Removed _cameraLeaning.SetLeanInput(Leaning) since CameraLeaning is gone
        }

        private void LateUpdate()
        {
            _cameraController.Look(_onFoot.Look.ReadValue<Vector2>());
        }

        private void HandleCrouchStart(InputAction.CallbackContext ctx) => HandleCrouch(true);
        private void HandleCrouchCancel(InputAction.CallbackContext ctx) => HandleCrouch(false);

        private void HandleSwitchSlot(InputAction.CallbackContext ctx)
        {
            var axisValue = ctx.ReadValue<float>();
            var scrollUp = axisValue > 0f;

            // _inventoryController.SwitchSlot(scrollUp); // Uncomment if InventoryController is re-added
        }

        private void HandleSprintStart(InputAction.CallbackContext ctx) => HandleSprint(true);
        private void HandleSprintCancel(InputAction.CallbackContext ctx) => HandleSprint(false);

        private void HandleCrouch(bool isKeyDown)
        {
            _isCrouching = toggleCrouch ? isKeyDown && !_isCrouching : isKeyDown;
            _playerController.Crouch(_isCrouching);
        }

        private void HandleSprint(bool isKeyDown)
        {
            _isSprinting = toggleSprint ? isKeyDown && !_isSprinting : isKeyDown;
            _playerController.Sprint(_isSprinting);
        }
    }
}