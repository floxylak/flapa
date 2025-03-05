using System.Collections.Generic;
using Player.Interact;
using UnityEngine;

namespace Player
{
    public class InventoryController : MonoBehaviour
    {
        private Camera _camera;
        
        private Transform _handPos;
        private Transform _bothHandPos;
        
        private List<GameObject> _inventory;
        
        private int _currentIndex;

        private void Awake()
        {
            _inventory = new List<GameObject>();
            _camera = CameraController.Main;
            _handPos = _camera.transform.Find("Hand") ?? throw new MissingReferenceException("Child object 'Hand' not found.");
            _bothHandPos = _camera.transform.Find("BothHand") ?? throw new MissingReferenceException("Child object 'BothHand' not found.");
        }
        
        [SerializeField] private int maxSize = 3;
        [SerializeField] private LayerMask groundLayer;

        public int GetInventoryCount() => _inventory.Count;
        public bool IsInventoryFull() => GetRegularItemCount() >= maxSize;
        public int GetCurrentSlot() => _currentIndex;
        public GameObject GetHeldItem() => _currentIndex >= 0 && _currentIndex < _inventory.Count ? _inventory[_currentIndex] : null;
        
        private static bool IsSpecialItem(GameObject item) => item.CompareTag("FoodItem");

        public bool AddItem(GameObject item)
        {
            if (item == null) return false;
            if (!IsSpecialItem(item) && GetRegularItemCount() >= maxSize) return false;
            
            _inventory.Add(item);
            item.SetActive(false);
            
            var targetHand = IsHeavyItem(item) ? _bothHandPos : _handPos;
            item.transform.SetParent(targetHand, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            if (_inventory.Count - 1 == _currentIndex)
                EquipItem(_currentIndex);
            
            return true;
        }

        public bool HasItem(string itemTag)
        {
            foreach (var item in _inventory)
            {
                if (item != null && item.CompareTag(itemTag))
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveItem(string itemTag)
        {
            foreach (var item in _inventory)
            {
                if (item != null && item.CompareTag(itemTag))
                {
                    _inventory.Remove(item);
                    Destroy(item);
                    if (_inventory.IndexOf(item) <= _currentIndex || _currentIndex >= _inventory.Count)
                    {
                        _currentIndex = Mathf.Max(0, _inventory.Count - 1);
                        if (_inventory.Count > 0) EquipItem(_currentIndex);
                    }
                    
                    return;
                }
            }
        }

        public void SwitchSlot(bool scrollUp)
        {
            if (_inventory.Count == 0) return;

            var currentItem = GetHeldItem();
            if (currentItem != null)
            {
                if (IsHeavyItem(currentItem))
                {
                    DropItem();
                    return;
                }
                currentItem.SetActive(false);
            }

            if (scrollUp)
                _currentIndex = (_currentIndex + 1) % _inventory.Count; 
            else
                _currentIndex = (_currentIndex - 1 + _inventory.Count) % _inventory.Count; 

            var newItem = GetHeldItem();
            if (newItem != null) EquipItem(_currentIndex);
        }

        private void EquipItem(int index)
        {
            if (index < 0 || index >= _inventory.Count || _inventory[index] == null) return;

            var item = _inventory[index];
            var targetHand = IsHeavyItem(item) ? _bothHandPos : _handPos;

            item.SetActive(true);
            item.transform.SetParent(targetHand, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        public void DropItem()
        {
            if (_inventory.Count == 0 || _currentIndex < 0 || _currentIndex >= _inventory.Count || _inventory[_currentIndex] == null)
            {
                Debug.Log($"Slot {_currentIndex} is empty or invalid, nothing to drop. Inventory count: {_inventory.Count}", this);
                _currentIndex = Mathf.Max(0, _inventory.Count - 1); // Reset index safely
                return;
            }
            
            var item = _inventory[_currentIndex];
            _inventory.RemoveAt(_currentIndex);

            item.transform.SetParent(null);
            item.SetActive(true);

            var snapped = TrySnapItem(item);
            if (!snapped)
            {
                var bottom = item.transform.Find("Bottom");
                if (bottom == null)
                {
                    Debug.LogWarning($"Item {item.name} has no 'Bottom' child object! Dropping using root transform.", this);
                }
                
                var dropPosition = transform.position + transform.forward * 1.5f;
                var bottomOffset = bottom != null ? item.transform.position - bottom.position : Vector3.zero;
                
                var itemCollider = item.GetComponent<Collider>();
                if (itemCollider == null)
                {
                    Debug.LogWarning($"Item {item.name} has no Collider! Dropping to default position.", this);
                    item.transform.position = dropPosition + Vector3.down * 2f + bottomOffset;
                }
                else
                {
                    item.transform.position = dropPosition; 
                    var finalPosition = SimulateFall(item, itemCollider, dropPosition, bottomOffset);
                    item.transform.position = finalPosition;
                }
                
                item.layer = LayerMask.NameToLayer(InteractController.InteractableLayer);
            }
            
            item.transform.rotation = Quaternion.Euler(0, 0, 0);
            
            var ratTrap = item.GetComponent<RatTrap>();
            if (ratTrap != null) ratTrap.PlaceTrap();
            
            var radio = item.GetComponent<Radio>();
            if (radio != null) radio.PlaceRadio();
            
            Debug.Log($"Dropped item: {item.name} from slot {_currentIndex}. " +
                      $"Regular items: {GetRegularItemCount()}/{maxSize}" +
                      (snapped ? " (Snapped to snap point)" : " (Dropped freely)"), this);
            
            if (_inventory.Count > 0)
            {
                _currentIndex = Mathf.Min(_currentIndex, _inventory.Count - 1);
                EquipItem(_currentIndex);
            }
            else
            {
                _currentIndex = 0;
            }
        }
        
        private Vector3 SimulateFall(GameObject item, Collider itemCollider, Vector3 startPosition, Vector3 bottomOffset)
        {
            const float stepSize = 0.1f; 
            const float  maxDropDistance = 10f; 
            
            var currentPosition = startPosition;
            var  distanceFallen = 0f;
            
            var wasEnabled = itemCollider.enabled;
            itemCollider.enabled = true;

            while (distanceFallen < maxDropDistance)
            {
                var nextPosition = currentPosition + Vector3.down * stepSize;
                distanceFallen += stepSize;
                
                if (Physics.CheckBox(itemCollider.bounds.center, itemCollider.bounds.extents, item.transform.rotation, groundLayer))
                    return currentPosition + bottomOffset + Vector3.up * 0.1f;
                

                currentPosition = nextPosition;
                item.transform.position = currentPosition; 
            }
            
            itemCollider.enabled = wasEnabled; 
            return startPosition + Vector3.down * maxDropDistance + bottomOffset;
        }

        private bool TrySnapItem(GameObject item)
        {
            var nearbyObjects = Physics.OverlapSphere(transform.position, 2f);
            var snapPrefix = item.CompareTag("Rat") ? "RatSnap" : "SpecialItemSnap";

            foreach (var obj in nearbyObjects)
            {
                if (obj.name.StartsWith(snapPrefix) && 
                    obj.CompareTag(item.tag) && 
                    obj.transform.childCount == 0)
                {
                    item.transform.SetParent(obj.transform, false);
                    item.transform.position = obj.transform.position;
                    item.layer = LayerMask.NameToLayer(InteractController.DefaultLayer);
                    return true;
                }
            }
            return false;
        }

        private int GetRegularItemCount()
        {
            var count = 0;
            foreach (var item in _inventory)
            {
                if (item != null && !item.CompareTag("FoodItem")) count++;
            }
            return count;
        }

        private static bool IsHeavyItem(GameObject item)
        {
            return item.CompareTag("WaterBucket") || item.CompareTag("Radio") || item.CompareTag("MouseTrap");
        }



        public void ClearInventory()
        {
            foreach (var item in _inventory)
            {
                if (item != null) Destroy(item);
            }
            _inventory.Clear();
            _currentIndex = 0;
        }

        private void OnValidate()
        {
            if (maxSize < 1) maxSize = 1;
        }
    }
}