using System;
using Player.Stats;
using UnityEngine;

namespace Player
{
public class PlayerMotor : MonoBehaviour
{
    private CharacterController _controller;
    private InputManager _inputManager;
    private PlayerStamina _stamina;
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
    private bool _isSprinting;
    
    public bool toggleSprint; // If true, sprint is toggled else it's hold
    public bool toggleCrouch; // If true, crouch is toggled else it's hold

    public Transform playerModel;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _inputManager = GetComponent<InputManager>();
        _stamina = GetComponent<PlayerStamina>();
    }

    private void Start()
    {
        _originalSpeed = speed;
        _sprintSpeed = speed * 1.5f;
    }

    private void Update()
    {
        _isGrounded = _controller.isGrounded;
        HandleCrouchLerp();

        if (_isGrounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f))
            {
                _playerVelocity.y = -2f;
            }
        }
        else
        {
            _playerVelocity.y += gravity * Time.deltaTime;
        }

        if (_isSprinting && _stamina.currentStamina <= 0)
        {
            StopSprinting(); 
        }

        _controller.Move((_moveDirection * speed + _playerVelocity) * Time.deltaTime);
    }

    public void ProcessMove(Vector2 input)
    {
        if (input.magnitude == 0)
        {
            _moveDirection = Vector3.Slerp(_moveDirection, Vector3.zero, movementInertia * Time.deltaTime);
            return;
        }

        var targetDirection = transform.TransformDirection(new Vector3(input.x, 0, input.y));
        _moveDirection = Vector3.Lerp(_moveDirection, targetDirection, movementInertia * Time.deltaTime);
    }

    public void Crouch(bool keyDown)
    {
        _lerpCrouch = true;
        _crouchTimer = 0f;
        if (keyDown && _inputManager.IsCrouching)
        {
            speed = _originalSpeed / 2;
        }
        else if (!keyDown && !_inputManager.IsCrouching)
        {
            speed = _originalSpeed;
        }
    }

    public void Sprint(bool keyDown)
    {
        if (keyDown)
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
        if (_stamina.CanSprint() && !_isSprinting)
        {
            _isSprinting = true;
            speed = _sprintSpeed;
            _stamina.StartUsingStamina();
        }
    }


    private void StopSprinting()
    {
        if (_isSprinting)
        {
            _isSprinting = false;
            speed = _originalSpeed;
            _stamina.StopUsingStamina();
        }
    }

    private void HandleCrouchLerp()
    {
        if (!_lerpCrouch)
            return;

        _crouchTimer += Time.deltaTime * 0.5f; 
        float p = _crouchTimer / 1.5f; 
        p = Mathf.SmoothStep(0, 1, p);

        _controller.height = Mathf.Lerp(_controller.height, _inputManager.IsCrouching ? 1 : 2, p);

        if (playerModel is not null)
        {
            var targetScale = _inputManager.IsCrouching ? new Vector3(1, 0.5f, 1) : Vector3.one;
            playerModel.localScale = Vector3.Lerp(playerModel.localScale, targetScale, p);
        }

        if (p >= 1)
        {
            _lerpCrouch = false;
            _crouchTimer = 0f;
        }
    }

}

}