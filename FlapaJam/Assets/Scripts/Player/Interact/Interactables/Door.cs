using UnityEngine;

public class DoorRotation : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    
    void Update()
    {
        float deltaX = Input.GetAxis("Mouse X");
        Debug.Log(deltaX);
        
    }
}