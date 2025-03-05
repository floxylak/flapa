using UnityEngine;
using System.Collections.Generic;
using Player.Interact; // Add this to access Radio

namespace Player
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int _maxInventorySize = 3;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private Vector3 _handOffset = new Vector3(0.5f, -0.5f, 1f);

        private Transform _handPos;
        private Transform _hand2Pos;
        private List<GameObject> _inventory = new List<GameObject>();
        private int _currentIndex;
        private InputManager _inputManager;

        private const string DEFAULT_LAYER = "Default";
        private const string INTERACTABLE_LAYER = "Interactable";

        private void Awake()
        {
            _inputManager = GetComponent<InputManager>() ?? throw new MissingComponentException($"{nameof(InputManager)} is required on {gameObject.name}");
            _handPos = transform.Find("Hand") ?? throw new MissingReferenceException("Child object 'Hand' not found on this GameObject.");
            _hand2Pos = transform.Find("Hand2") ?? throw new MissingReferenceException("Child object 'Hand2' not found on this GameObject.");
            _playerCamera = Camera.main ?? throw new MissingReferenceException("No main camera found in the scene.");
            _currentIndex = 0;
        }

        private void Update()
        {
            if (_inputManager.Inventory.Cycle.triggered)
            {
                SwitchSlot();
            }

            if (_inputManager.Inventory.Drop.triggered)
            {
                DropItem();
            }
        }

        private void LateUpdate()
        {
            Vector3 cameraPos = _playerCamera.transform.position;
            Vector3 cameraForward = _playerCamera.transform.forward;

            _handPos.position = cameraPos + _playerCamera.transform.TransformDirection(_handOffset);
            _handPos.rotation = Quaternion.LookRotation(cameraForward);

            _hand2Pos.position = cameraPos + _playerCamera.transform.TransformDirection(_handOffset + Vector3.left * 0.5f);
            _hand2Pos.rotation = Quaternion.LookRotation(cameraForward);
        }

        public GameObject GetHeldItem()
        {
            return (_currentIndex >= 0 && _currentIndex < _inventory.Count) ? _inventory[_currentIndex] : null;
        }

        public bool AddItem(GameObject item)
        {
            if (item == null || (!item.CompareTag("FoodItem") && GetRegularItemCount() >= _maxInventorySize))
            {
                return false;
            }

            _inventory.Add(item);
            item.SetActive(false);

            bool isHeavyItem = IsHeavyItem(item);
            Transform targetHand = isHeavyItem ? _hand2Pos : _handPos;
            item.transform.SetParent(targetHand, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            if (_inventory.Count - 1 == _currentIndex)
            {
                EquipItem(_currentIndex);
            }

            return true;
        }

        public bool HasItem(string tag)
        {
            foreach (GameObject item in _inventory)
            {
                if (item != null && item.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(string tag)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (_inventory[i] != null && _inventory[i].CompareTag(tag))
                {
                    GameObject item = _inventory[i];
                    _inventory.RemoveAt(i);
                    item.transform.SetParent(null);
                    item.SetActive(false);

                    if (i <= _currentIndex || _currentIndex >= _inventory.Count)
                    {
                        _currentIndex = Mathf.Max(0, _inventory.Count - 1);
                        if (_inventory.Count > 0) EquipItem(_currentIndex);
                    }
                    return;
                }
            }
        }

        public void SwitchSlot()
        {
            if (_inventory.Count == 0) return;

            GameObject currentItem = GetHeldItem();
            if (currentItem != null)
            {
                if (IsHeavyItem(currentItem))
                {
                    DropItem();
                    return;
                }
                currentItem.SetActive(false);
            }

            _currentIndex = (_currentIndex + 1) % _inventory.Count;
            GameObject newItem = GetHeldItem();
            if (newItem != null) EquipItem(_currentIndex);
        }

        private void EquipItem(int index)
        {
            if (index < 0 || index >= _inventory.Count || _inventory[index] == null) return;

            GameObject item = _inventory[index];
            Transform targetHand = IsHeavyItem(item) ? _hand2Pos : _handPos;

            item.SetActive(true);
            item.transform.SetParent(targetHand, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        public void DropItem()
        {
            // Early exit if inventory is empty or index is invalid
            if (_inventory.Count == 0 || _currentIndex < 0 || _currentIndex >= _inventory.Count || _inventory[_currentIndex] == null)
            {
                Debug.Log($"Slot {_currentIndex} is empty or invalid, nothing to drop. Inventory count: {_inventory.Count}", this);
                _currentIndex = Mathf.Max(0, _inventory.Count - 1); // Reset index safely
                return;
            }

            GameObject item = _inventory[_currentIndex];
            _inventory.RemoveAt(_currentIndex);

            item.transform.SetParent(null);
            item.SetActive(true);

            bool snapped = TrySnapItem(item);
            if (!snapped)
            {
                Vector3 dropPosition = transform.position + transform.forward * 1.5f;
                if (Physics.Raycast(dropPosition, Vector3.down, out RaycastHit hit, 10f, _groundLayer))
                {
                    item.transform.position = hit.point + Vector3.up * 0.1f;
                }
                else
                {
                    item.transform.position = dropPosition + Vector3.down * 2f;
                }
                item.layer = LayerMask.NameToLayer(INTERACTABLE_LAYER);
            }

            item.transform.rotation = Quaternion.Euler(0, 0, 0);

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Check if the dropped item is a RatTrap and call PlaceTrap
            RatTrap ratTrap = item.GetComponent<RatTrap>();
            if (ratTrap != null)
            {
                ratTrap.PlaceTrap();
            }

            // Check if the dropped item is a Radio and call PlaceRadio
            Radio radio = item.GetComponent<Radio>();
            if (radio != null)
            {
                radio.PlaceRadio();
            }

            Debug.Log($"Dropped item: {item.name} from slot {_currentIndex}. " +
                     $"Regular items: {GetRegularItemCount()}/{_maxInventorySize}" +
                     (snapped ? " (Snapped to snap point)" : " (Dropped freely)"), this);

            if (_inventory.Count > 0)
            {
                _currentIndex = Mathf.Min(_currentIndex, _inventory.Count - 1);
                EquipItem(_currentIndex); // Equip the next item
            }
            else
            {
                _currentIndex = 0;
            }
        }

        private bool TrySnapItem(GameObject item)
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 2f);
            string snapPrefix = item.CompareTag("Rat") ? "RatSnap" : "SpecialItemSnap";

            foreach (Collider obj in nearbyObjects)
            {
                if (obj.name.StartsWith(snapPrefix) && 
                    obj.CompareTag(item.tag) && 
                    obj.transform.childCount == 0)
                {
                    item.transform.SetParent(obj.transform, false);
                    item.transform.position = obj.transform.position;
                    item.layer = LayerMask.NameToLayer(DEFAULT_LAYER);
                    return true;
                }
            }
            return false;
        }

        private int GetRegularItemCount()
        {
            int count = 0;
            foreach (GameObject item in _inventory)
            {
                if (item != null && !item.CompareTag("FoodItem")) count++;
            }
            return count;
        }

        private bool IsHeavyItem(GameObject item)
        {
            return item.CompareTag("WaterBucket") || item.CompareTag("Radio") || item.CompareTag("MouseTrap");
        }

        public int GetInventoryCount() => _inventory.Count;
        public bool IsInventoryFull() => GetRegularItemCount() >= _maxInventorySize;
        public int GetCurrentSlot() => _currentIndex;

        public void ClearInventory()
        {
            foreach (GameObject item in _inventory)
            {
                if (item != null) Destroy(item);
            }
            _inventory.Clear();
            _currentIndex = 0;
        }

        private void OnValidate()
        {
            if (_maxInventorySize < 1) _maxInventorySize = 1;
        }
    }
}