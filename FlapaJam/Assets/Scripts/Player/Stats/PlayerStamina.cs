namespace Player.Stats
{
    using System;
    using UnityEngine;

    public class PlayerStamina : MonoBehaviour
    {
        public float maxStamina = 100f;
        public float staminaRegenRate = 5f;
        public float staminaDrainRate = 30f; // Stamina drains at this rate per second
        public float currentStamina { get; private set; }

        private float staminaRegenDelay = 5f;
        private float extendedRegenDelay = 10f;
        private float lastUsedTime;
        private bool staminaEmpty;
        private bool isUsingStamina;
        private bool isRegenerating;

        private float _lastLoggedStamina;

        private void Start()
        {
            currentStamina = maxStamina;
            staminaEmpty = false;
            isRegenerating = false;
        }

        private void Update()
        {
            if (isUsingStamina)
            {
                DrainStamina();
            }
            else if (!isRegenerating && Time.time - lastUsedTime >= GetRegenDelay())
            {
                isRegenerating = true;
            }

            if (isRegenerating)
            {
                Regenerate();
            }

            if (Mathf.Abs(currentStamina - _lastLoggedStamina) > 0.1f)
            {
                Debug.Log("Current Stamina: " + currentStamina);
                _lastLoggedStamina = currentStamina;
            }
        }

        public void StartUsingStamina()
        {
            if (!staminaEmpty && !isRegenerating)
            {
                isUsingStamina = true;
            }
        }

        public void StopUsingStamina()
        {
            isUsingStamina = false;
            lastUsedTime = Time.time;
        }

        private void DrainStamina()
        {
            if (staminaEmpty)
                return;

            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (currentStamina <= 0)
            {
                staminaEmpty = true;
                isUsingStamina = false;
                lastUsedTime = Time.time;
            }
        }

        private void Regenerate()
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            }

            if (currentStamina >= maxStamina)
            {
                staminaEmpty = false;
                isRegenerating = false;
            }
        }

        private float GetRegenDelay()
        {
            return staminaEmpty ? extendedRegenDelay : staminaRegenDelay;
        }

        public bool CanSprint()
        {
            return !staminaEmpty && !isRegenerating;
        }
    }

}