using System;
using Player.Input;
using UnityEngine;

namespace Player.Interact
{
    public class PlayerInteract : MonoBehaviour
    {
        private Camera cam;
        [SerializeField]
        private float distance = 3f;

        [SerializeField]
        private LayerMask mask;

        private PlayerUI _playerUI;
        private InputManager _inputManager;
        
        private void Start()
        {
            cam = GetComponent<PlayerLook>().cam;
            _playerUI = GetComponent<PlayerUI>();
            _inputManager = GetComponent<InputManager>();
        }

        private void Update()
        {
            _playerUI.UpdateText(string.Empty);
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, distance, mask))
            {
                if (hitInfo.collider.GetComponent<Interactable>() is not null)
                {
                    var interactable = hitInfo.collider.GetComponent<Interactable>();
                    _playerUI.UpdateText(interactable.promptMessage);
                    
                    if (_inputManager._onFoot.Interact.triggered)
                    {
                        interactable.BaseInteract();
                    }
                }
            }
        }
    }
}