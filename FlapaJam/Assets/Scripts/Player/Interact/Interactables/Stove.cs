/*using Core;
using UnityEngine;
using Player; // Add this to access TaskManager

namespace Player.Interact
{
    public class Stove : Interactable
    {
        [Header("Stove Settings")]
        [SerializeField] private GameObject _cookedRatPrefab;
        [SerializeField] private float _cookTime = 5f;

        private bool _isOn;
        private float _cookTimer;
        private GameObject _currentRatSnap;
        private Vector3 _cookPosition;
        private Quaternion _cookRotation;
        private bool _loggedNoRatSnap;
        private Light _stoveLight;
        private TaskManager _taskManager; // New reference to TaskManager

        private void Awake()
        {
            if (_cookedRatPrefab == null) throw new MissingReferenceException("CookedRat prefab not assigned!");
            _stoveLight = GetComponent<Light>();
            if (_stoveLight != null) _stoveLight.enabled = _isOn;

            _taskManager = FindObjectOfType<TaskManager>();
            if (_taskManager == null)
            {
                Debug.LogWarning("Stove: TaskManager not found in scene!", this);
            }
        }

        private void Update()
        {
            if (_isOn && _currentRatSnap != null && HasRatChild())
            {
                _cookTimer += Time.deltaTime;
                if (_cookTimer >= _cookTime) CookRat();
            }
            else if (!_isOn)
            {
                _loggedNoRatSnap = false;
            }
        }

        protected override void Interact()
        {
            ToggleStove();
        }

        private void ToggleStove()
        {
            _isOn = !_isOn;
            if (_stoveLight != null) _stoveLight.enabled = _isOn;

            if (!_isOn)
            {
                ResetCooking();
            }
            else
            {
                foreach (Transform child in transform)
                {
                    if (child.name.StartsWith("RatSnap"))
                    {
                        _currentRatSnap = child.gameObject;
                        _cookPosition = _currentRatSnap.transform.position;
                        _cookRotation = _currentRatSnap.transform.rotation;
                        _cookTimer = 0f;
                        return;
                    }
                }
            }
        }

        private void CookRat()
        {
            if (_currentRatSnap != null && HasRatChild())
            {
                Destroy(_currentRatSnap.transform.GetChild(0).gameObject);
                GameObject cookedRat = Instantiate(_cookedRatPrefab, _currentRatSnap.transform);
                cookedRat.transform.localPosition = Vector3.zero;
                cookedRat.transform.localRotation = Quaternion.identity;
                _cookTimer = 0f;

                // Complete the "Prepare the Rat" task
                if (_taskManager != null)
                {
                    cookedRat.tag = "RatCooked"; // Ensure the cooked rat has the correct tag
                    _taskManager.CheckTaskProgress(cookedRat);
                    Debug.Log("Stove: 'Prepare the Rat' task completed!", this);
                }
                else
                {
                    Debug.LogWarning("Stove: TaskManager not found, cannot complete 'Prepare the Rat' task!", this);
                }
            }
        }

        private void ResetCooking()
        {
            _cookTimer = 0f;
        }

        private void OnTransformChildrenChanged()
        {
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("RatSnap"))
                {
                    _currentRatSnap = child.gameObject;
                    _cookPosition = _currentRatSnap.transform.position;
                    _cookRotation = _currentRatSnap.transform.rotation;
                    _cookTimer = 0f;
                    return;
                }
            }
            if (_currentRatSnap != null && !_currentRatSnap.transform.IsChildOf(transform))
            {
                _currentRatSnap = null;
                _cookTimer = 0f;
                _loggedNoRatSnap = false;
            }
        }

        private bool HasRatChild()
        {
            return _currentRatSnap != null && _currentRatSnap.transform.childCount > 0 && _currentRatSnap.transform.GetChild(0).CompareTag("Rat");
        }

        public bool IsOn() => _isOn;
        public bool HasRatSnap() => _currentRatSnap != null && HasRatChild();
        public float GetCookProgress() => _cookTimer;
        public float GetCookDuration() => _cookTime;
    }
}*/