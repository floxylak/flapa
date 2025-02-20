using System;
using UnityEngine;

namespace Player.Input
{
    public class InputManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        public PlayerInput.OnFootActions _onFoot;
    
        private PlayerMotor _motor;
        private PlayerLook _look;
    
        private bool _isSprinting;
        private bool _isCrouching;
        public bool IsCrouching => _isCrouching;
        
    
        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _look = GetComponent<PlayerLook>();
        }
    
        private void FixedUpdate()
        {
            if (_motor == null || _playerInput == null) return; // Extra safeguard
            _motor.ProcessMove(_onFoot.Movement.ReadValue<Vector2>());
        }
    
        private void LateUpdate()
        {
            if (_look == null || _playerInput == null) return; // Extra safeguard
            _look.ProcessLook(_onFoot.Look.ReadValue<Vector2>());
        }
    
        private void OnEnable()
        {
            _playerInput = new PlayerInput();
            _onFoot = _playerInput.OnFoot;
            _onFoot.Enable();
            
            _onFoot.Crouch.started += ctx => HandleCrouch(true);
            _onFoot.Crouch.canceled += ctx => HandleCrouch(false);
    
            _onFoot.Sprint.started += ctx => HandleSprint(true);
            _onFoot.Sprint.canceled += ctx => HandleSprint(false);
        }
    
        private void OnDisable()
        {
            _onFoot.Disable();
            
            _onFoot.Crouch.started -= ctx => HandleCrouch(true);
            _onFoot.Crouch.canceled -= ctx => HandleCrouch(false);
    
            _onFoot.Sprint.started -= ctx => HandleSprint(true);
            _onFoot.Sprint.canceled -= ctx => HandleSprint(false);
        }
    
    
        private void HandleCrouch(bool keyDown)
        {
            if (_motor == null) return;
    
            _isCrouching = _motor.toggleCrouch ? keyDown && !_isCrouching : keyDown;
            _motor.Crouch(_isCrouching);
        }
    
        private void HandleSprint(bool keyDown)
        {
            if (_motor == null) return;
    
            _isSprinting = _motor.toggleSprint ? keyDown && !_isSprinting : keyDown;
            _motor.Sprint(_isSprinting);
        }
    
    }

}