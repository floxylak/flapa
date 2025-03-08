using UnityEngine;


namespace Player
{
    public class PlayerSingleton : MonoBehaviour
    {
        public static PlayerSingleton instance;

        public PlayerDetectability detectability;
        public PlayerPauseManager pausing;
        public PlayerHealth health;
        public PlayerMovement movement;
        public PlayerCamera cam;
        public PlayerInteraction1 interact;
        public PlayerInputCont input;
        
        // public LighterController flashlight;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
    }

}