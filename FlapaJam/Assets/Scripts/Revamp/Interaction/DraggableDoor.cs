using UnityEngine;
using UnityEngine.Events;
using Player;
using System.Collections;

public class DraggableDoor : MonoBehaviour
{
    public enum DoorState
    {
        Open,
        Closed,
        Moving
    }

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

    [Header("Debug")]
    [SerializeField] private DoorState state = DoorState.Closed;
    [SerializeField] private float currentAngle;
    [SerializeField] private bool drawGizmos = false;

    private Camera cam;
    private bool isDragging = false;
    private bool isAutoClosing = false; // Flag 
    private Quaternion initialRotation;
    private Vector3 lastHitDirection; 
    private float accumulatedAngle;   
    private Coroutine autoCloseCoroutine; 
    private Vector3 initialHitPoint;
    
    void Start()
    {
        cam = PlayerSingleton.instance.cam.cam;

        if (pivotPoint == null)
        {
            pivotPoint = transform;
            Debug.LogWarning("PivotPoint not assigned, using transform instead");
        }

        initialRotation = pivotPoint.rotation;
        accumulatedAngle = 0f;
        UpdateObjectState();
    }

    void Update()
    {
        if (isDragging)
        {
            DragObject();
        }
        else if (isAutoClosing || (state != DoorState.Moving && currentAngle <= snapAngleThreshold && currentAngle > closeAngleThreshold))
        {
            SmoothClose();
        }
        UpdateObjectState();
    }

    public void StartDragging()
    {
        isDragging = true;
        state = DoorState.Moving;
        isAutoClosing = false;
        
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Check if we hit the door
        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
        {
            initialHitPoint = hit.point; // Store the initial hit point
            Plane pivotPlane = new Plane(Vector3.up, initialHitPoint); // Use hit point instead of pivotPoint.position
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

    public void DragObject()
    {
        if (!isDragging) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane pivotPlane = new Plane(Vector3.up, initialHitPoint); // Use initial hit point instead of pivotPoint.position
        float distance;

        if (pivotPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 currentDirection = (hitPoint - pivotPoint.position).normalized;
            currentDirection.y = 0f;
            
            float angleDiff = Vector3.SignedAngle(lastHitDirection, currentDirection, Vector3.up);

            // Accumulate the angle and clamp it
            accumulatedAngle += angleDiff;
            accumulatedAngle = Mathf.Clamp(accumulatedAngle, -maxAngle, maxAngle);
            
            Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, accumulatedAngle, 0f);

            // Smoothly rotate 
            pivotPoint.rotation = Quaternion.RotateTowards(
                pivotPoint.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Update current angle and last direction
            currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);
            lastHitDirection = currentDirection;

            if (drawGizmos)
            {
                Debug.DrawRay(initialHitPoint, lastHitDirection * 2f, Color.yellow);
                Debug.DrawRay(initialHitPoint, currentDirection * 2f, Color.green);
            }
            Debug.Log($"Angle current direc: {currentDirection}, Accumulated Angle: {accumulatedAngle}, Total Angle: {currentAngle}");
        }
    }

    public void ReleaseObject()
    {
        isDragging = false;
        UpdateObjectState();
        
        if (state == DoorState.Open)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }

    private void UpdateObjectState()
    {
        currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);

        if (!isDragging)
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
        if (isDragging || state == DoorState.Closed) return;
        
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

        // Update current angle
        currentAngle = Quaternion.Angle(initialRotation, pivotPoint.rotation);
        
        if (currentAngle <= closeAngleThreshold)
        {
            state = DoorState.Closed;
            OnClose.Invoke();
            isAutoClosing = false; 
        }
    }
    
    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (!isDragging && state == DoorState.Open)
        {
            isAutoClosing = true; // Start smooth closing animation
        }
        autoCloseCoroutine = null;
    }

    void OnDrawGizmos()
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