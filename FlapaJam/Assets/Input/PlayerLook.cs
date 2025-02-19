using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float _xRotation;
    
    public float xSensitivity = 10f;
    public float ySensitivity = 10f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ProcessLook(Vector2 input)
    {
        if (cam is null)
            return;
        
        var mouseX = input.x * xSensitivity * Time.deltaTime;
        var mouseY = input.y * ySensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        
        cam.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}