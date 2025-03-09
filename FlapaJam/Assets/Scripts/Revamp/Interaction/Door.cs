using Player;
using UnityEngine;
using UnityEngine.Events;

public class Door : Interactable
{
    private bool _draggingDoor;
    private bool _isActuallyDragging;
    
    public AudioSource source;
    public AudioClip openSound;
    public AudioClip closedSound;
    public bool isLocked = false;

    [Header("Events")]
    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    [Header("Settings")]
    public float maxAngle = 90f;
    public float rotationSpeed = 500f;
    public float closeAngleThreshold = 2f;
    public float snapAngleThreshold = 20f;
    public float closeAnimationSpeed = 500f;

    [Header("Components")]
    public Transform pivotPoint;
    
    public enum DoorState { Open, Closed, Moving }
    public DoorState state = DoorState.Closed;
    [SerializeField] private float currentAngle;
    [SerializeField] private bool drawGizmos = false;

    private Camera cam;
    private bool isAutoClosing = false;
    private Quaternion initialRotation;
    private Vector3 lastHitDirection;
    private float accumulatedAngle;
    private Coroutine autoCloseCoroutine;
    private Vector3 initialHitPoint;

    private void Awake()
    {
        interactType = InteractType.Persistent;
        isActive = false;
        bailOutRange = 5f;

        if (pivotPoint == null)
        {
            pivotPoint = transform;
            Debug.LogWarning("PivotPoint not assigned, using transform instead");
        }
    }

    private void Start()
    {
        cam = PlayerSingleton.instance.cam.cam;
        initialRotation = pivotPoint.rotation;
        accumulatedAngle = 0f;
        UpdateObjectState();
        
        // Initial lock state for specialItem doors
        if (transform.CompareTag("specialItem"))
        {
            isLocked = !RoomManager.Instance.HasShardBeenPickedUp;
        }
    }

    private void Update()
    {
        if (_isActuallyDragging)
        {
            if (isLocked)
            {
                ReleaseObject();
                _draggingDoor = false;
                _isActuallyDragging = false;
            }
            else
            {
                DragObject();
            }
        }
        else if (isAutoClosing || (state != DoorState.Moving && currentAngle <= snapAngleThreshold && currentAngle > closeAngleThreshold))
        {
            SmoothClose();
        }
        UpdateObjectState();
    }

    public override void Interact()
    {
        // Handle different door types
        if (transform.CompareTag("specialItem"))
        {
            isLocked = !RoomManager.Instance.HasShardBeenPickedUp;
            if (isLocked)
            {
                Debug.Log("Door is locked. Requires shard to open.");
                return;
            }
        }
        else if (transform.CompareTag("SafeDoor"))
        {
            if (!_draggingDoor)
            {
                RoomManager.Instance.OnDoorInteracted(this);
            }
        }
        // Regular "Door" tag will proceed normally without additional checks

        base.Interact();
        _draggingDoor = !_draggingDoor;

        if (_draggingDoor)
        {
            if (!isLocked)
            {
                StartDragging();
                _isActuallyDragging = true;
                if (source && openSound) source.PlayOneShot(openSound);
            }
        }
        else
        {
            if (_isActuallyDragging)
            {
                ReleaseObject();
                _isActuallyDragging = false;
                if (source && closedSound) source.PlayOneShot(closedSound);
            }
        }
    }

    private void StartDragging()
    {
        if (isLocked) return;

        state = DoorState.Moving;
        isAutoClosing = false;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
        {
            initialHitPoint = hit.point;
            Plane pivotPlane = new Plane(Vector3.up, initialHitPoint);
            float distance;

            if (pivotPlane.Raycast(ray, out distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 currentDirection = (hitPoint - initialHitPoint).normalized;
                currentDirection.y = 0f;

                lastHitDirection = currentDirection;

                Vector3 currentForward = pivotPoint.forward;
                currentForward.y = 0f;
                Vector3 initialForward = initialRotation * Vector3.forward;
                initialForward.y = 0f;
                accumulatedAngle = Vector3.SignedAngle(initialForward, currentForward, Vector3.up);
                accumulatedAngle = Mathf.Clamp(accumulatedAngle, -maxAngle, maxAngle);
            }
        }
    }

    private void DragObject()
    {
        if (isLocked) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane pivotPlane = new Plane(Vector3.up, initialHitPoint);
        float distance;

        if (pivotPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 currentDirection = (hitPoint - pivotPoint.position).normalized;
            currentDirection.y = 0f;

            float angleDiff = Vector3.SignedAngle(lastHitDirection, currentDirection, Vector3.up);
            accumulatedAngle += angleDiff;
            accumulatedAngle = Mathf.Clamp(accumulatedAngle, -maxAngle, maxAngle);

            Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, accumulatedAngle, 0f);
            pivotPoint.rotation = Quaternion.RotateTowards(
                pivotPoint.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);
            lastHitDirection = currentDirection;

            if (drawGizmos)
            {
                Debug.DrawRay(initialHitPoint, lastHitDirection * 2f, Color.yellow);
                Debug.DrawRay(initialHitPoint, currentDirection * 2f, Color.green);
            }
        }
    }

    private void ReleaseObject()
    {
        _isActuallyDragging = false;
        UpdateObjectState();

        if (state == DoorState.Open)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    private void UpdateObjectState()
    {
        currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);

        if (!_isActuallyDragging)
        {
            if (currentAngle <= closeAngleThreshold && state != DoorState.Closed)
            {
                state = DoorState.Closed;
                OnClose.Invoke();
                isAutoClosing = false;
            }
            else if (currentAngle > closeAngleThreshold && state != DoorState.Open)
            {
                state = DoorState.Open;
                OnOpen.Invoke();
            }
        }
        else
        {
            state = DoorState.Moving;
        }
    }

    private void SmoothClose()
    {
        if (_isActuallyDragging || state == DoorState.Closed || isLocked) return;

        Quaternion targetRotation = initialRotation;
        pivotPoint.rotation = Quaternion.RotateTowards(
            pivotPoint.rotation,
            targetRotation,
            closeAnimationSpeed * Time.deltaTime
        );

        Vector3 currentForward = pivotPoint.forward;
        currentForward.y = 0f;
        Vector3 initialForward = initialRotation * Vector3.forward;
        initialForward.y = 0f;
        accumulatedAngle = Vector3.SignedAngle(initialForward, currentForward, Vector3.up);
        accumulatedAngle = Mathf.Clamp(accumulatedAngle, -maxAngle, maxAngle);

        currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);

        if (currentAngle <= closeAngleThreshold)
        {
            state = DoorState.Closed;
            OnClose.Invoke();
            isAutoClosing = false;
        }
    }

    private System.Collections.IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (!_isActuallyDragging && state == DoorState.Open && !isLocked)
        {
            isAutoClosing = true;
        }
        autoCloseCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (pivotPoint != null && drawGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(initialHitPoint, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(initialHitPoint, pivotPoint.forward * 2f);
        }
    }
}