using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // Assign this in the Inspector
    [SerializeField] private Collider triggerBox;     // Assign your trigger box collider here
    private bool hasPlayed = false;

    void Start()
    {
        // Check if the audio source is assigned
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned in the Inspector!");
            return;
        }

        // Check if the trigger box is assigned and set as a trigger
        if (triggerBox == null)
        {
            Debug.LogError("Trigger Box Collider is not assigned in the Inspector!");
        }
        else if (!triggerBox.isTrigger)
        {
            Debug.LogWarning("Assigned Trigger Box is not set as a trigger! Enabling it now.");
            triggerBox.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasPlayed && audioSource != null)
        {
            audioSource.Play();
            hasPlayed = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
        }
    }
}