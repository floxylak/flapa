namespace Player.Stats
{
    using UnityEngine;
    using System.Collections;

    public class PlayerStamina : MonoBehaviour
    {
        [Header("Base Stamina Settings")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaRegenRate = 5f;
        [SerializeField] private float _staminaDrainRate = 30f;
        public float currentStamina { get; private set; }

        [Header("Regen Settings")]
        [SerializeField] private float _staminaRegenDelay = 5f;
        [SerializeField] private float _extendedRegenDelay = 10f;

        [Header("Dynamic Effects")]
        [SerializeField] private float _fearDrainMultiplier = 1.5f;
        [SerializeField] private float _minRegenFactor = 0.5f;
        [SerializeField] private float _boostDrainMultiplier = 0.8f;
        [SerializeField] private float _regenRecoverySpeed = 2f;

        private float _lastUsedTime;
        private bool _staminaEmpty;
        private bool _isUsingStamina;
        private bool _isRegenerating;
        private float _fearFactor = 1f;
        private float _regenFactor = 1f;
        private bool _isBoosted;
        private Coroutine _boostCoroutine;

        private void Start()
        {
            currentStamina = _maxStamina;
        }

        private void Update()
        {
            if (_isUsingStamina)
            {
                DrainStamina();
            }
            else if (!_isRegenerating && Time.time - _lastUsedTime >= GetRegenDelay())
            {
                _isRegenerating = true;
            }

            if (_isRegenerating)
            {
                Regenerate();
            }

            UpdateRegenFactor();
        }

        public void StartUsingStamina()
        {
            if (!_staminaEmpty && !_isRegenerating)
            {
                _isUsingStamina = true;
            }
        }

        public void StopUsingStamina()
        {
            _isUsingStamina = false;
            _lastUsedTime = Time.time;
        }

        public void ApplyFearPenalty(float intensity, float duration)
        {
            if (intensity < 0f || intensity > 1f) return;
            _fearFactor = Mathf.Max(_fearFactor, 1f + (intensity * (_fearDrainMultiplier - 1f)));
            StartCoroutine(FearPenaltyRoutine(duration));
        }

        public void BoostStamina(float duration, float regenMultiplier = 2f)
        {
            if (_isBoosted) return;
            if (_boostCoroutine != null) StopCoroutine(_boostCoroutine);
            _boostCoroutine = StartCoroutine(BoostRoutine(duration, regenMultiplier));
        }

        public void ApplyExhaustion(float intensity, float duration)
        {
            if (intensity < 0f || intensity > 1f) return;
            _regenFactor = Mathf.Min(_regenFactor, 1f - intensity);
            currentStamina = Mathf.Max(currentStamina - (_maxStamina * intensity * 0.5f), 0f);
            StartCoroutine(ExhaustionRoutine(duration));
        }

        private void DrainStamina()
        {
            if (_staminaEmpty) return;

            float effectiveDrainRate = _staminaDrainRate * _fearFactor * (_isBoosted ? _boostDrainMultiplier : 1f);
            currentStamina = Mathf.Clamp(currentStamina - effectiveDrainRate * Time.deltaTime, 0f, _maxStamina);

            if (currentStamina <= 0f)
            {
                _staminaEmpty = true;
                _isUsingStamina = false;
                _lastUsedTime = Time.time;
            }
        }

        private void Regenerate()
        {
            if (currentStamina < _maxStamina)
            {
                float effectiveRegenRate = _staminaRegenRate * _regenFactor * (_isBoosted ? _boostDrainMultiplier : 1f);
                currentStamina = Mathf.Clamp(currentStamina + effectiveRegenRate * Time.deltaTime, 0f, _maxStamina);
            }

            if (currentStamina >= _maxStamina)
            {
                _staminaEmpty = false;
                _isRegenerating = false;
            }
        }

        private float GetRegenDelay()
        {
            return _staminaEmpty ? _extendedRegenDelay : _staminaRegenDelay;
        }

        private void UpdateRegenFactor()
        {
            if (_regenFactor < 1f)
            {
                _regenFactor = Mathf.MoveTowards(_regenFactor, 1f, _regenRecoverySpeed * Time.deltaTime);
            }
            if (_fearFactor > 1f)
            {
                _fearFactor = Mathf.MoveTowards(_fearFactor, 1f, _regenRecoverySpeed * Time.deltaTime);
            }
        }

        private IEnumerator FearPenaltyRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator BoostRoutine(float duration, float regenMultiplier)
        {
            _isBoosted = true;
            float originalRegenRate = _staminaRegenRate;
            _staminaRegenRate *= regenMultiplier;

            yield return new WaitForSeconds(duration);

            _isBoosted = false;
            _staminaRegenRate = originalRegenRate;
        }

        private IEnumerator ExhaustionRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        public bool CanSprint()
        {
            return !_staminaEmpty && !_isRegenerating;
        }
    }
}