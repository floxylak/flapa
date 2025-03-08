/*using UnityEngine;

namespace Player
{
    public class SafeRoomTrigger : MonoBehaviour
    {
        private GameManager gameManager;

        private void Awake()
        {
            // Find the GameManager in the scene
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("SafeRoomTrigger: GameManager not found in scene!", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && gameManager != null)
            {
                gameManager.SetPlayerInSafeRoom(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && gameManager != null)
            {
                gameManager.SetPlayerInSafeRoom(false);
            }
        }
    }
}*/