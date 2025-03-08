using UnityEngine;

public class JitteryMovementDetector : MonoBehaviour
{
    public float movementThreshold = 0.1f; // Adjust this value to control sensitivity of jitter detection

    private Vector3 _originalPosition;
    private Vector3 _previousPosition;
    private CharacterController _characterController;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _originalPosition = transform.position;
        _previousPosition = _originalPosition;
    }

    private void Update()
    {
        if (IsJitteryMovement())
        {
            TeleportToOriginalPosition();
        }

        // Update the previous position at the end of each frame
        _previousPosition = transform.position;
    }
    
    private bool IsJitteryMovement()
    {
        // Calculate the distance between the previous position and the current position
        float distanceToPreviousPosition = Vector3.Distance(_previousPosition, transform.position);

        // Calculate the threshold distance for the current fixed frame based on movementThreshold and fixedDeltaTime
        float frameThreshold = movementThreshold * Time.fixedDeltaTime;

        // If the distance is greater than the frame threshold, consider it jittery movement
        // Update the original position to the current position
        _originalPosition = transform.position;
        return distanceToPreviousPosition > frameThreshold;
    }

    private void TeleportToOriginalPosition()
    {
        _characterController.enabled = false;
        //transform.position = originalPosition;
        transform.position = new Vector3(_originalPosition.x, 1, _originalPosition.z);
        _characterController.enabled = true;
    }
}