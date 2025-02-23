using UnityEngine;

namespace Player.Interact
{
    public class StairwayTrigger : MonoBehaviour
    {
        public System.Action onPlayerEnter; // Callback when player enters

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"StairwayTrigger at {transform.position}: Collided with {other.gameObject.name}");
            if (other.CompareTag("Player") && onPlayerEnter != null)
            {
                Debug.Log($"StairwayTrigger: Player detected at {transform.position}!");
                onPlayerEnter.Invoke();
            }
            else
            {
                Debug.Log($"StairwayTrigger: Collision with {other.gameObject.name} - not tagged as Player or callback null");
            }
        }
    }
}