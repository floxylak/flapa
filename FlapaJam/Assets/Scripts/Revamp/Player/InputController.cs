using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class InputController : MonoBehaviour
    {
        private PlayerController _playerController;
        private CameraController _cameraController;
        private InventoryController _inventoryController;
        
        private PlayerInput _playerInput;
        private PlayerInput.OnFootActions _onFoot;
        private PlayerInput.InventoryActions _inventory;

        private bool _isSprinting;
        private bool _isCrouching;
        
        private bool _interactHeld;
        
        // Getters
        public PlayerInput.OnFootActions OnFoot => _onFoot;
        public PlayerInput.InventoryActions Inventory => _inventory;
        
        public bool IsCrouching => _isCrouching;
        public bool IsSprinting => _isSprinting;
        
        public bool InteractHeld => _onFoot.Interact.IsPressed();
        
        private void Awake()
        {
            _playerController = GetComponent<PlayerController>() ?? throw new MissingComponentException($"{nameof(PlayerController)} is required on {gameObject.name}");
            _cameraController = GetComponent<CameraController>() ?? throw new MissingComponentException($"{nameof(CameraController)} is required on {gameObject.name}");
            _inventoryController = GetComponent<InventoryController>() ?? throw new MissingComponentException($"{nameof(InventoryController)} is required on {gameObject.name}");
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
            
            _inventoryController.SwitchSlot(scrollUp);
        }
        
        private void HandleSprintStart(InputAction.CallbackContext ctx) => HandleSprint(true);
        private void HandleSprintCancel(InputAction.CallbackContext ctx) => HandleSprint(false);

        private void HandleCrouch(bool isKeyDown)
        {
            _isCrouching = _playerController.toggleCrouch ? isKeyDown && !_isCrouching : isKeyDown;
            _playerController.Crouch(_isCrouching);
        }

        private void HandleSprint(bool isKeyDown)
        {
            _isSprinting = _playerController.toggleSprint ? isKeyDown && !_isSprinting : isKeyDown;
            _playerController.Sprint(_isSprinting);
        }
    }
}
