using Core;
using UnityEngine;

namespace Player.Interact
{
    public class PlayerInteract : MonoBehaviour
    {
        [SerializeField] private float _distance = 3f;
        [SerializeField] private LayerMask _mask;

        private Camera _camera;
        private PlayerUI _playerUI;
        private InputManager _inputManager;

        private void Start()
        {
            _camera = Camera.main ?? throw new MissingReferenceException($"{nameof(PlayerLook)} or its camera not found.");
            _playerUI = GetComponent<PlayerUI>() ?? throw new MissingReferenceException($"{nameof(PlayerUI)} not found.");
            _inputManager = GetComponent<InputManager>() ?? throw new MissingComponentException($"{nameof(InputManager)} not found.");
        }

        private void Update()
        {
            _playerUI.UpdatePromptText(string.Empty);
            Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, _distance, _mask))
            {
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
                if (interactable != null)
                {
                    string prompt = string.IsNullOrEmpty(interactable.promptAction) ? "Press 'E' to interact" : interactable.promptAction;

                    if (interactable is Radio radio)
                    {
                        if (radio.IsBroken())
                        {
                            prompt = radio.IsRepairing()
                                ? $"Hold 'E' to repair ({radio.GetRepairProgress():F1}/{radio.GetRepairDuration():F1})"
                                : "Hold 'E' to repair";
                        }
                        else if (radio.IsPlaced())
                        {
                            prompt = $"Right Click to {(radio.IsEnabled() ? "disable" : "enable")} radio";
                        }
                    }
                    else if (interactable is Stove stove)
                    {
                        prompt = $"Press 'E' to {(stove.IsOn() ? "turn off" : "turn on")} stove";
                        if (stove.IsOn() && stove.HasRatSnap())
                        {
                            prompt += $" (Cooking: {stove.GetCookProgress():F1}/{stove.GetCookDuration():F1})";
                        }
                    }
                    else if (interactable is Desk desk)
                    {
                        prompt = desk.IsCrafting()
                            ? $"Hold 'E' to craft ({desk.GetCraftProgress():F1}/{desk.GetCraftDuration():F1})"
                            : "Hold 'E' to craft";
                    }

                    _playerUI.UpdatePromptText(prompt);

                    if (_inputManager.OnFoot.Interact.triggered)
                    {
                        interactable.BaseInteract();
                    }

                    if (interactable is Radio radioInteract && 
                        !radioInteract.IsBroken() && 
                        radioInteract.IsPlaced() && 
                        _inputManager.OnFoot.Interact.triggered)
                    {
                        radioInteract.ToggleRadio();
                    }
                }
            }
        }

        public bool IsInteractHeld()
        {
            return _inputManager.OnFoot.Interact.IsPressed();
        }
    }
}