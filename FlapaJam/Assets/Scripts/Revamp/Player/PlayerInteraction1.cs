using Unity.Mathematics;
using UnityEngine;

namespace Player
{
    public class PlayerInteraction1 : MonoBehaviour
    {
        public KeyCode drop = KeyCode.G;
        public KeyCode interact = KeyCode.F;
        public KeyCode use = KeyCode.Mouse0;
        
        public float interactDistance = 5f;
        public LayerMask interactMask;
        
        public Transform holdPoint;
        public Transform dropPoint;

        private Camera _camera;
        private PickupSO _pickupInHand = null;
        private Interactable _currentInteractable;
        private bool _persistentInteracting = false;
        private Interactable _persistentInteractable = null;
        private bool _holdInteracting = false;

        // Getters
        public PickupSO PickupInHand => _pickupInHand;
        private bool BusyHand => PickupInHand != null;
        private bool IsClick() => _currentInteractable?.interactType == Interactable.InteractType.Click;
        private bool IsHold() => _currentInteractable?.interactType == Interactable.InteractType.Hold;
        private bool IsPersistent() => _currentInteractable?.interactType == Interactable.InteractType.Persistent;
        private bool InteractableInPersistentRange() => 
            _persistentInteractable != null && 
            (_persistentInteractable.transform.position - transform.position).magnitude < _persistentInteractable.bailOutRange;

        // Setters
        private void SetPickUpInHand(PickupSO pSo) => _pickupInHand = pSo;

        private void Start()
        {
            _camera = PlayerSingleton.instance.cam.cam;
        }

        private void Update()
        {
            // Handle persistent interaction
            if (_persistentInteracting && _persistentInteractable != null)
            {
                if (Input.GetKeyUp(use) || !InteractableInPersistentRange())
                {
                    _persistentInteractable.Interact();
                    _persistentInteracting = false;
                    if (_currentInteractable == _persistentInteractable)
                        _currentInteractable.NotLookingAt();
                    _persistentInteractable = null;
                }
                return; // Persistent has highest priority
            }

            // Handle hold interaction
            if (_holdInteracting && _currentInteractable != null && IsHold())
            {
                if (Input.GetKey(use))
                {
                    _currentInteractable.Interact();
                }
                if (Input.GetKeyUp(use))
                {
                    _holdInteracting = false;
                }
                return; // Hold has priority over click
            }

            Interaction();

            if (Input.GetKeyDown(drop) && BusyHand)
                Drop();
        }

        private void OnDrawGizmos()
        {
            if (_camera == null) return;
            Gizmos.DrawLine(_camera.transform.position, _camera.transform.forward * interactDistance);
        }

        public void Drop()
        {
            if (!BusyHand) return;

            var equipModel = holdPoint.GetChild(0);
            Destroy(equipModel.gameObject);

            var pickUp = Instantiate(PickupInHand.PickupObject, dropPoint.position, 
                Quaternion.Euler(0, _camera.transform.rotation.y, 0));
    
            var rb = pickUp.AddComponent<Rigidbody>();
            rb.mass = 60f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.AddForce(_camera.transform.forward * 150, ForceMode.Impulse);
            Destroy(rb, 3f);
    
            SetPickUpInHand(null);
        }

        public void RemoveCurrentInteractable()
        {
            if (_currentInteractable == null) return;
            
            _currentInteractable.NotLookingAt();
            if (_currentInteractable != _persistentInteractable)
                _currentInteractable = null;
        }

        private void Interaction()
        {
            RaycastHit hit;
            if (Physics.Raycast(
                    _camera.transform.position, _camera.transform.forward,
                    out hit, interactDistance,
                    interactMask, QueryTriggerInteraction.Collide))
            {
                var interactable = hit.transform.GetComponent<Interactable>() ??
                                 hit.transform.GetComponentInParent<Interactable>();
                
                if (interactable != null)
                {
                    if (_currentInteractable != interactable)
                    {
                        RemoveCurrentInteractable();
                        _currentInteractable = interactable;
                        _currentInteractable.LookingAt();
                    }

                    // Persistent interaction (highest priority)
                    if (IsPersistent() && !_persistentInteracting && !_holdInteracting)
                    {
                        if (Input.GetKeyDown(use))
                        {
                            _currentInteractable.Interact();
                            _persistentInteracting = true;
                            _persistentInteractable = _currentInteractable;
                        }
                    }
                    // Hold interaction (medium priority)
                    else if (IsHold() && !_persistentInteracting && !_holdInteracting)
                    {
                        if (Input.GetKeyDown(use))
                        {
                            _currentInteractable.Interact();
                            _holdInteracting = true;
                        }
                    }
                    // Click interaction (lowest priority)
                    else if (IsClick() && !_persistentInteracting && !_holdInteracting)
                    {
                        if (interactable is Pickup pickup && Input.GetKeyDown(interact) && !BusyHand)
                        {
                            SetPickUpInHand(pickup.SO);
                            Instantiate(pickup.SO.PickupObject, holdPoint);
                            Destroy(hit.transform.gameObject);
                            return;
                        }
                        else if (Input.GetKeyDown(use))
                        {
                            if (BusyHand && holdPoint.childCount > 0 && holdPoint.GetChild(0).GetComponent<Useable>() != null)
                            {
                                holdPoint.GetChild(0).GetComponent<Useable>().Interact();
                            }
                            else if (!BusyHand)
                            {
                                _currentInteractable.Interact();
                            }
                        }
                    }
                }
                else if (_currentInteractable != null)
                {
                    HadInteractableNowDont();
                }
            }
            else if (_currentInteractable != null)
            {
                HadInteractableNowDont();
            }
        }
        
        private void HadInteractableNowDont()
        {
            if (_currentInteractable == null) return;
            RemoveCurrentInteractable();
        }
    }
}