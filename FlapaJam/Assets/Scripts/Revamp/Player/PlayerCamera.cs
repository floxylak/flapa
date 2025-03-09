using UnityEngine;
using System.Collections;
using Player;

public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float sensitivityFactor = 1f;
    private const float SensitivityRecoverySpeed = 2f;

    public float GetEffectiveSensitivity => sensitivity * sensitivityFactor;
    
    [Header("Zoom")]
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private float zoomAmount = 20f;
    [SerializeField] private float zoomSpeed = 5f;

    [Header("Lean")]
    [SerializeField] private LayerMask leanCheckMask;
    [SerializeField] private float leanLength = 0.6f;
    [SerializeField] private float leanSpeed = 6f;
    [SerializeField] private float maxLeanAngle = 30f;
    [SerializeField] private float leanSmoothTime = 0.1f;
    [SerializeField] private float leanCheckRadius = 0.1f;
    [SerializeField] private float maxVerticalAngleForLean = 30f; // New: Threshold for disabling lean

    [Header("Rotation Smoothing")]
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("References")]
    [SerializeField] public Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private Transform leftCheck;
    [SerializeField] private Transform rightCheck;

    private float targetVerticalRotation;
    private float currentVerticalRotation;
    private float verticalRotationVelocity;
    private float targetHorizontalRotation;
    private float currentHorizontalRotation;
    private float horizontalRotationVelocity;
    private float leanInput;

    private Vector3 leanPositionTarget;
    private Vector3 currentLeanPosition;
    private Vector3 currentLeanVelocity;
    private float leanAngleTarget;
    private float currentLeanAngle;
    private float leanAngleVelocity;

    private float initialZoom;
    private float zoomTarget;
    private bool isFrozen;

    [Header("Shake")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeFrequency = 10f;
    private float shakeTimer;
    private bool isShaking;
    private Vector3 originalCameraPosition;

    private Quaternion forcedRotation;
    private bool isForcedLooking;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        initialZoom = cam.fieldOfView;
        originalCameraPosition = cam.transform.localPosition;
        leanPositionTarget = originalCameraPosition;
        currentLeanPosition = originalCameraPosition;
        targetHorizontalRotation = player.eulerAngles.y;
        currentHorizontalRotation = player.eulerAngles.y;
        isFrozen = false;
    }

    private void Start()
    {
        PlayerSingleton.instance.pausing.Pause += FreezeCameraMovement;
        PlayerSingleton.instance.pausing.Unpause += UnfreezeCameraMovement;
    }

    private void OnDisable()
    {
        PlayerSingleton.instance.pausing.Pause -= FreezeCameraMovement;
        PlayerSingleton.instance.pausing.Unpause -= UnfreezeCameraMovement;
    }

    void Update()
    {
        if (!isFrozen)
        {
            UpdateSensitivityFactor();
            if (isShaking) HandleCameraShake();
            if (isForcedLooking) HandleForcedLook();
            else
            {
                HandleLean();
                HandleCameraRotation();
            }
            HandleZoom();
        }
    }

    void LateUpdate()
    {
        if (!isFrozen && !isShaking && !isForcedLooking)
        {
            // Apply position and rotation in LateUpdate for synchronization
            cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, currentLeanPosition, Time.deltaTime * leanSpeed);
            Quaternion verticalRot = Quaternion.Euler(currentVerticalRotation, 0f, 0f);
            Quaternion leanRot = Quaternion.Euler(0f, 0f, -currentLeanAngle);
            cam.transform.localRotation = verticalRot * leanRot;
        }
    }

    public void Look(Vector2 input)
    {
        if (isFrozen || isForcedLooking) return;

        float effectiveSensitivity = sensitivity * sensitivityFactor;
        targetHorizontalRotation += input.x * effectiveSensitivity;
        targetVerticalRotation -= input.y * effectiveSensitivity;
        targetVerticalRotation = Mathf.Clamp(targetVerticalRotation, -90f, 90f);
    }

    public void SetLeaning(float leanValue)
    {
        // Disable leaning if looking up or down too much
        if (Mathf.Abs(currentVerticalRotation) > maxVerticalAngleForLean)
        {
            leanInput = 0f;
        }
        else
        {
            leanInput = Mathf.Clamp(leanValue, -1f, 1f); // Ensure leanInput is within valid range
        }
    }

    private void HandleZoom()
    {
        zoomTarget = Input.GetKey(zoomKey) ? initialZoom - zoomAmount : initialZoom;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, zoomTarget, zoomSpeed * Time.deltaTime);
    }

    private void HandleLean()
    {
        // Check for collisions with a small tolerance to avoid abrupt cancellation
        if (leanCheckColliding())
        {
            leanInput = Mathf.Lerp(leanInput, 0f, Time.deltaTime * leanSpeed); // Smoothly reduce lean
        }

        leanAngleTarget = leanInput * maxLeanAngle;
        Vector3 leanOffset = Vector3.right * leanInput * leanLength;

        leanPositionTarget = originalCameraPosition + leanOffset;
        currentLeanPosition = Vector3.SmoothDamp(
            currentLeanPosition,
            leanPositionTarget,
            ref currentLeanVelocity,
            leanSmoothTime,
            leanSpeed
        );

        currentLeanAngle = Mathf.SmoothDamp(
            currentLeanAngle,
            leanAngleTarget,
            ref leanAngleVelocity,
            leanSmoothTime
        );
    }

    private bool leanCheckColliding()
    {
        // Debug collision checks
        bool leftColliding = Physics.CheckSphere(leftCheck.position, leanCheckRadius, leanCheckMask, QueryTriggerInteraction.Ignore);
        bool rightColliding = Physics.CheckSphere(rightCheck.position, leanCheckRadius, leanCheckMask, QueryTriggerInteraction.Ignore);
        if (leftColliding || rightColliding)
        {
            // Debug.Log($"Collision detected - Left: {leftColliding}, Right: {rightColliding}, leanInput: {leanInput}");
        }
        return leftColliding || rightColliding;
    }

    private void HandleCameraRotation()
    {
        // Smooth both horizontal and vertical rotations
        currentHorizontalRotation = Mathf.SmoothDampAngle(
            currentHorizontalRotation,
            targetHorizontalRotation,
            ref horizontalRotationVelocity,
            rotationSmoothTime
        );

        currentVerticalRotation = Mathf.SmoothDampAngle(
            currentVerticalRotation,
            targetVerticalRotation,
            ref verticalRotationVelocity,
            rotationSmoothTime
        );

        // Apply smoothed horizontal rotation to player
        player.rotation = Quaternion.Euler(0f, currentHorizontalRotation, 0f);
    }

    private void UpdateSensitivityFactor()
    {
        if (sensitivityFactor < 1f)
        {
            sensitivityFactor = Mathf.MoveTowards(sensitivityFactor, 1f, SensitivityRecoverySpeed * Time.deltaTime);
        }
    }

    #region Existing Features
    public void StartShake(float duration)
    {
        if (isShaking) return;
        originalCameraPosition = cam.transform.localPosition;
        isShaking = true;
        shakeTimer = duration;
    }

    private void HandleCameraShake()
    {
        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0)
        {
            isShaking = false;
            cam.transform.localPosition = currentLeanPosition;
            return;
        }
        float xShake = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f;
        float yShake = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f;
        cam.transform.localPosition = currentLeanPosition + new Vector3(xShake, yShake, 0) * shakeIntensity;
    }

    public void ApplyDisorientation(float intensity, float duration)
    {
        if (intensity < 0f || intensity > 1f) return;
        sensitivityFactor = Mathf.Min(sensitivityFactor, 1f - intensity);
        StartCoroutine(DisorientationRoutine(duration));
    }

    public void ForceLookAt(Vector3 target, float duration)
    {
        isForcedLooking = true;
        Vector3 direction = (target - cam.transform.position).normalized;
        forcedRotation = Quaternion.LookRotation(direction);
        StartCoroutine(ForceLookRoutine(duration));
    }

    private void HandleForcedLook()
    {
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, forcedRotation, Time.deltaTime * 5f);
    }

    private IEnumerator DisorientationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator ForceLookRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isForcedLooking = false;
    }
    #endregion

    private void FreezeCameraMovement() => isFrozen = true;
    private void UnfreezeCameraMovement() => isFrozen = false;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(leftCheck.position, leanCheckRadius);
        Gizmos.DrawWireSphere(rightCheck.position, leanCheckRadius);
    }
}