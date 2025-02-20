using UnityEngine;


namespace Player.Input
{
    public class PlayerLook : MonoBehaviour
    {
        public Camera cam;
        private float _xRotation;

        public float xSensitivity = 0.2f;
        public float ySensitivity = 0.2f;

        private void Awake()
        {
            if (cam == null)
            {
                cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogError("PlayerLook: No camera assigned and no Main Camera found in the scene!");
                }
            }
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void ProcessLook(Vector2 input)
        {
            if (cam is null)
                return;
    
            var mouseX = input.x * xSensitivity;
            var mouseY = input.y * ySensitivity;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
    
            cam.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

}