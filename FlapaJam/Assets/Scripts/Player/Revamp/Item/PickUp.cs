/*using Core;
using Player;
using UnityEngine;

namespace Item
{
    public class PickUp : Interactable
    {
        private Transform _player;
        private InventoryController _inventory;

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            _player = CameraController.Player;
            if (!_player)
            {
                Debug.LogError($"{nameof(PickUp)}: Player Transform not found.", this);
                return;
            }
            

            _inventory = _player.GetComponent<InventoryController>();
            if (!_inventory)
            {
                Debug.LogError($"{nameof(PickUp)}: InventoryController not found on player.", this);
            }
        }

        protected override void Interact()
        {
            if (!_inventory)
            {
                Debug.LogWarning($"{nameof(PickUp)}: Cannot interact - InventoryController missing.", this);
                return;
            }

            if (_inventory.AddItem(gameObject))
            {
                Debug.Log($"Added {gameObject.name} to inventory.", this);
            }
            else
            {
                Debug.Log($"Failed to add {gameObject.name} to inventory - possibly full or invalid item.", this);
            }
        }
    }
}*/