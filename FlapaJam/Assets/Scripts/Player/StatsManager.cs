using UnityEngine;
using System;
using System.Collections;

namespace Player
{
    public class StatsManager : MonoBehaviour
    {
        [Header("Hunger Settings")]
        [SerializeField] private float _maxHunger = 100f;
        [SerializeField] private float _currentHunger = 100f;
        [SerializeField] private float _hungerDecreaseRate = 1f;
        [SerializeField] private float _lowHungerThreshold = 25f;
        [SerializeField] private float _criticalHungerThreshold = 10f;

        [Header("Thirst Settings")]
        [SerializeField] private float _maxThirst = 100f;
        [SerializeField] private float _currentThirst = 100f;
        [SerializeField] private float _thirstDecreaseRate = 1.5f;
        [SerializeField] private float _lowThirstThreshold = 25f;
        [SerializeField] private float _criticalThirstThreshold = 10f;

        [Header("Dynamic Effects")]
        [SerializeField] private float _fearDepletionMultiplier = 1.5f;
        [SerializeField] private float _minDecreaseFactor = 0.5f;
        [SerializeField] private float _recoverySpeed = 2f;
        [SerializeField] private float _boostMultiplier = 0.5f;

        public event Action<float> OnHungerChanged;
        public event Action OnHungerLow;
        public event Action OnHungerCritical;
        public event Action OnHungerDepleted;

        public event Action<float> OnThirstChanged;
        public event Action OnThirstLow;
        public event Action OnThirstCritical;
        public event Action OnThirstDepleted;

        private bool _isLowHungerNotified;
        private bool _isCriticalHungerNotified;
        private bool _isLowThirstNotified;
        private bool _isCriticalThirstNotified;
        private float _hungerFearFactor = 1f;
        private float _thirstFearFactor = 1f;
        private float _hungerDecreaseFactor = 1f;
        private float _thirstDecreaseFactor = 1f;
        private bool _isBoosted;
        private Coroutine _boostCoroutine;

        private void Start()
        {
            _currentHunger = Mathf.Clamp(_currentHunger, 0f, _maxHunger);
            OnHungerChanged?.Invoke(_currentHunger);

            _currentThirst = Mathf.Clamp(_currentThirst, 0f, _maxThirst);
            OnThirstChanged?.Invoke(_currentThirst);
        }

        private void Update()
        {
            UpdateFactors();

            float effectiveHungerRate = _hungerDecreaseRate * _hungerFearFactor * _hungerDecreaseFactor;
            float effectiveThirstRate = _thirstDecreaseRate * _thirstFearFactor * _thirstDecreaseFactor;

            DecreaseHunger(effectiveHungerRate * Time.deltaTime);
            DecreaseThirst(effectiveThirstRate * Time.deltaTime);

            CheckHungerStates();
            CheckThirstStates();
        }

        public void ApplyFearEffect(float intensity, float duration, bool affectHunger = true, bool affectThirst = true)
        {
            if (intensity < 0f || intensity > 1f) return;
            if (affectHunger) _hungerFearFactor = Mathf.Max(_hungerFearFactor, 1f + (intensity * (_fearDepletionMultiplier - 1f)));
            if (affectThirst) _thirstFearFactor = Mathf.Max(_thirstFearFactor, 1f + (intensity * (_fearDepletionMultiplier - 1f)));
            StartCoroutine(FearEffectRoutine(duration));
        }

        public void BoostStats(float duration, float multiplier = 0.5f)
        {
            if (_isBoosted) return;
            if (_boostCoroutine != null) StopCoroutine(_boostCoroutine);
            _boostCoroutine = StartCoroutine(BoostRoutine(duration, multiplier));
        }

        public void ApplyStrain(float intensity, float duration, bool affectHunger = true, bool affectThirst = true)
        {
            if (intensity < 0f || intensity > 1f) return;
            if (affectHunger)
            {
                _hungerDecreaseFactor = Mathf.Min(_hungerDecreaseFactor, 1f - intensity);
                _currentHunger = Mathf.Max(_currentHunger - (_maxHunger * intensity * 0.5f), 0f);
            }
            if (affectThirst)
            {
                _thirstDecreaseFactor = Mathf.Min(_thirstDecreaseFactor, 1f - intensity);
                _currentThirst = Mathf.Max(_currentThirst - (_maxThirst * intensity * 0.5f), 0f);
            }
            StartCoroutine(StrainRoutine(duration));
        }

        private void DecreaseHunger(float amount)
        {
            _currentHunger = Mathf.Clamp(_currentHunger - amount, 0f, _maxHunger);
            OnHungerChanged?.Invoke(_currentHunger);
        }

        private void CheckHungerStates()
        {
            if (_currentHunger <= 0f)
            {
                OnHungerDepleted?.Invoke();
            }
            else if (_currentHunger <= _criticalHungerThreshold && !_isCriticalHungerNotified)
            {
                OnHungerCritical?.Invoke();
                _isCriticalHungerNotified = true;
                _isLowHungerNotified = true;
            }
            else if (_currentHunger <= _lowHungerThreshold && !_isLowHungerNotified)
            {
                OnHungerLow?.Invoke();
                _isLowHungerNotified = true;
            }
            else if (_currentHunger > _lowHungerThreshold)
            {
                _isLowHungerNotified = false;
                _isCriticalHungerNotified = false;
            }
        }

        public void ReduceHunger(float amount) => DecreaseHunger(amount);

        public void AddHunger(float amount)
        {
            _currentHunger = Mathf.Clamp(_currentHunger + amount, 0f, _maxHunger);
            OnHungerChanged?.Invoke(_currentHunger);
        }

        public void ResetHunger()
        {
            _currentHunger = _maxHunger;
            _isLowHungerNotified = false;
            _isCriticalHungerNotified = false;
            OnHungerChanged?.Invoke(_currentHunger);
        }

        public void SetHunger(float value)
        {
            _currentHunger = Mathf.Clamp(value, 0f, _maxHunger);
            OnHungerChanged?.Invoke(_currentHunger);
        }

        public float GetCurrentHunger() => _currentHunger;
        public float GetMaxHunger() => _maxHunger;
        public float GetHungerPercentage() => _currentHunger / _maxHunger;
        public bool IsHungerLow() => _currentHunger <= _lowHungerThreshold;
        public bool IsHungerCritical() => _currentHunger <= _criticalHungerThreshold;
        public bool IsHungerDepleted() => _currentHunger <= 0f;

        private void DecreaseThirst(float amount)
        {
            _currentThirst = Mathf.Clamp(_currentThirst - amount, 0f, _maxThirst);
            OnThirstChanged?.Invoke(_currentThirst);
        }

        private void CheckThirstStates()
        {
            if (_currentThirst <= 0f)
            {
                OnThirstDepleted?.Invoke();
            }
            else if (_currentThirst <= _criticalThirstThreshold && !_isCriticalThirstNotified)
            {
                OnThirstCritical?.Invoke();
                _isCriticalThirstNotified = true;
                _isLowThirstNotified = true;
            }
            else if (_currentThirst <= _lowThirstThreshold && !_isLowThirstNotified)
            {
                OnThirstLow?.Invoke();
                _isLowThirstNotified = true;
            }
            else if (_currentThirst > _lowThirstThreshold)
            {
                _isLowThirstNotified = false;
                _isCriticalThirstNotified = false;
            }
        }

        public void ReduceThirst(float amount) => DecreaseThirst(amount);

        public void AddThirst(float amount)
        {
            _currentThirst = Mathf.Clamp(_currentThirst + amount, 0f, _maxThirst);
            OnThirstChanged?.Invoke(_currentThirst);
        }

        public void ResetThirst()
        {
            _currentThirst = _maxThirst;
            _isLowThirstNotified = false;
            _isCriticalThirstNotified = false;
            OnThirstChanged?.Invoke(_currentThirst);
        }

        public void SetThirst(float value)
        {
            _currentThirst = Mathf.Clamp(value, 0f, _maxThirst);
            OnThirstChanged?.Invoke(_currentThirst);
        }

        public float GetCurrentThirst() => _currentThirst;
        public float GetMaxThirst() => _maxThirst;
        public float GetThirstPercentage() => _currentThirst / _maxThirst;
        public bool IsThirstLow() => _currentThirst <= _lowThirstThreshold;
        public bool IsThirstCritical() => _currentThirst <= _criticalThirstThreshold;
        public bool IsThirstDepleted() => _currentThirst <= 0f;

        private void UpdateFactors()
        {
            if (_hungerFearFactor > 1f) _hungerFearFactor = Mathf.MoveTowards(_hungerFearFactor, 1f, _recoverySpeed * Time.deltaTime);
            if (_thirstFearFactor > 1f) _thirstFearFactor = Mathf.MoveTowards(_thirstFearFactor, 1f, _recoverySpeed * Time.deltaTime);
            if (_hungerDecreaseFactor < 1f) _hungerDecreaseFactor = Mathf.MoveTowards(_hungerDecreaseFactor, 1f, _recoverySpeed * Time.deltaTime);
            if (_thirstDecreaseFactor < 1f) _thirstDecreaseFactor = Mathf.MoveTowards(_thirstDecreaseFactor, 1f, _recoverySpeed * Time.deltaTime);
        }

        private IEnumerator FearEffectRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator BoostRoutine(float duration, float multiplier)
        {
            _isBoosted = true;
            float originalHungerRate = _hungerDecreaseRate;
            float originalThirstRate = _thirstDecreaseRate;
            _hungerDecreaseRate *= multiplier;
            _thirstDecreaseRate *= multiplier;

            yield return new WaitForSeconds(duration);

            _isBoosted = false;
            _hungerDecreaseRate = originalHungerRate;
            _thirstDecreaseRate = originalThirstRate;
        }

        private IEnumerator StrainRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }
    }
}