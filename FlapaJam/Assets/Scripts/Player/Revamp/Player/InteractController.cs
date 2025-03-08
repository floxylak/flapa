/*using Core;
using UnityEngine;

namespace Player
{
    public class InteractController : MonoBehaviour
    {
        private Camera _camera;
        private UIController _uiController;
        private InputController _inputController;
        
        public const string DefaultLayer = "Default";
        public const string InteractableLayer = "Interactable";
        
        [SerializeField] private float distance = 3f;
        
        private void Start()
        {
            _camera = CameraController.Main ?? throw new MissingReferenceException($"{nameof(CameraController)} or its camera not found.");
            _uiController = GetComponent<UIController>() ?? throw new MissingReferenceException($"{nameof(UIController)} not found.");
            _inputController = GetComponent<InputController>() ?? throw new MissingComponentException($"{nameof(InputController)} not found.");
        }

        private void Update()
        {
            _uiController.UpdatePromptText(string.Empty);
            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            
            if (Physics.Raycast(ray, out var hitInfo, distance, LayerMask.NameToLayer(InteractableLayer)))
            {
                var interactable = hitInfo.collider.GetComponent<Interactable>();
                if (interactable == null) return;

                var prompt = (interactable.isHold ? "Hold" : "Press");
                prompt += " E to ";
                prompt += string.IsNullOrEmpty(interactable.promptAction) ? "interact" : interactable.promptAction;
                
                
                _uiController.UpdatePromptText(prompt);

                if (_inputController.InteractHeld)
                {
                    interactable.BaseInteract();
                }
            }
        }
    }
}*/