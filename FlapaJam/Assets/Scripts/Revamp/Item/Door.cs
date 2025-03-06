using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public float doorPickupRange = 2f;
    public float dragSpeed = 10f;
    public float maxGrabDistance = 3f;
    public float minHeight = 0.1f; // Minimum height to prevent door going underground

    [Header("Camera Reference")]
    public Camera playerCam;

    private Rigidbody heldDoor;
    private bool draggingDoor;
    private float grabDistance;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabDoor();
        }

        if (draggingDoor)
        {
            DragDoor();

            if (Input.GetMouseButtonUp(0))
            {
                ReleaseDoor();
            }
        }
    }

    void TryGrabDoor()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, doorPickupRange) && hit.collider.CompareTag("Door"))
        {
            heldDoor = hit.collider.GetComponent<Rigidbody>();

            if (heldDoor != null)
            {
                draggingDoor = true;
                heldDoor.isKinematic = false;
                heldDoor.useGravity = false;
                grabDistance = Vector3.Distance(playerCam.transform.position, heldDoor.transform.position);
            }
        }
    }

    void DragDoor()
    {
        Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPosition = playerCam.transform.position + ray.direction * grabDistance;
        targetPosition.y = Mathf.Max(targetPosition.y, minHeight); // Clamp height

        Vector3 desiredVelocity = (targetPosition - heldDoor.position) * dragSpeed;
        heldDoor.velocity = Vector3.Lerp(heldDoor.velocity, desiredVelocity, Time.deltaTime * 10f);

        if ((heldDoor.position - playerCam.transform.position).sqrMagnitude > maxGrabDistance * maxGrabDistance)
        {
            ReleaseDoor();
        }
    }

    void ReleaseDoor()
    {
        if (heldDoor != null)
        {
            heldDoor.velocity = Vector3.zero;
            heldDoor.useGravity = true;
            heldDoor.isKinematic = true;
            heldDoor = null;
            draggingDoor = false;
        }
    }
}