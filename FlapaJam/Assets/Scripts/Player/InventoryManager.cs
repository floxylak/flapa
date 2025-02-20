using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Player
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxInventorySize = 3;
        [SerializeField] private Transform handPos;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector3 handOffset = new Vector3(0.5f, -0.5f, 1f);

        private List<GameObject> inventory = new List<GameObject>();
        private int currentIndex = 0;
        private InputManager _inputManager;

        private void Awake()
        {
            _inputManager = GetComponent<InputManager>();
            if (_inputManager == null)
            {
                Debug.LogWarning("InputManager not found on player.", this);
            }

            if (handPos == null)
            {
                Debug.LogWarning("HandPos Transform not assigned in Inspector.", this);
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    Debug.LogWarning("No player camera assigned and no main camera found.", this);
                }
            }
        }

        private void Update()
        {
            if (_inputManager == null) return;
            
            if (_inputManager._inventory.Cycle.triggered)
            {
                SwitchSlot();
            }

            if (_inputManager._inventory.Drop.triggered)
            {
                DropItem();
            }
        }

        private void LateUpdate()
        {
            if (playerCamera != null && handPos != null)
            {
                handPos.position = playerCamera.transform.position + 
                                 playerCamera.transform.TransformDirection(handOffset);
                handPos.rotation = Quaternion.LookRotation(playerCamera.transform.forward);
            }
        }

        public GameObject GetHeldItem()
        {
            return (currentIndex >= 0 && currentIndex < inventory.Count) ? inventory[currentIndex] : null;
        }

        public bool AddItem(GameObject item)
        {
            if (item == null)
            {
                Debug.LogWarning("Attempted to add null item to inventory.", this);
                return false;
            }

            if (inventory.Count >= maxInventorySize)
            {
                Debug.Log("Inventory full!", this);
                return false;
            }

            inventory.Add(item);
            item.SetActive(false);
            item.transform.SetParent(handPos);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            if (inventory.Count - 1 == currentIndex)
            {
                EquipItem(currentIndex);
            }

            Debug.Log($"Added item: {item.name} to slot {inventory.Count - 1}. Current slot: {currentIndex}");
            return true;
        }

        public void SwitchSlot()
        {
            if (currentIndex < inventory.Count && inventory[currentIndex] != null)
            {
                inventory[currentIndex].SetActive(false);
            }

            currentIndex = (currentIndex + 1) % maxInventorySize;

            if (currentIndex < inventory.Count && inventory[currentIndex] != null)
            {
                EquipItem(currentIndex);
            }

            Debug.Log($"Switched to slot {currentIndex}" + 
                     (currentIndex < inventory.Count && inventory[currentIndex] != null 
                         ? $": {inventory[currentIndex].name}" 
                         : ": Empty"));
        }

        private void EquipItem(int index)
        {
            if (index < 0 || index >= maxInventorySize)
            {
                Debug.LogWarning($"Invalid inventory slot: {index}", this);
                return;
            }

            if (index >= inventory.Count || inventory[index] == null)
            {
                return;
            }

            GameObject item = inventory[index];
            item.SetActive(true);
            item.transform.SetParent(handPos);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
        }

        private void DropItem()
        {
            if (currentIndex >= inventory.Count || inventory[currentIndex] == null)
            {
                Debug.Log($"Slot {currentIndex} is empty, nothing to drop.", this);
                return;
            }

            GameObject item = inventory[currentIndex];
            inventory.RemoveAt(currentIndex);

            item.transform.SetParent(null);
            item.SetActive(true);

            Vector3 dropPosition = transform.position + transform.forward * 1.5f;
            if (Physics.Raycast(dropPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                item.transform.position = hit.point + Vector3.up * 0.1f;
            }
            else
            {
                item.transform.position = dropPosition + Vector3.down * 2f;
            }

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            Debug.Log($"Dropped item: {item.name} from slot {currentIndex}. Remaining items: {inventory.Count}");

            if (inventory.Count > 0 && currentIndex >= inventory.Count)
            {
                currentIndex = inventory.Count - 1;
                EquipItem(currentIndex);
            }
        }

        public int GetInventoryCount() => inventory.Count;
        public bool IsInventoryFull() => inventory.Count >= maxInventorySize;
        public int GetCurrentSlot() => currentIndex;

        public void ClearInventory()
        {
            foreach (GameObject item in inventory)
            {
                if (item != null) Destroy(item);
            }
            inventory.Clear();
            currentIndex = 0;
        }

        private void OnValidate()
        {
            if (maxInventorySize < 1)
            {
                maxInventorySize = 1;
                Debug.LogWarning("maxInventorySize cannot be less than 1", this);
            }
        }
    }
}