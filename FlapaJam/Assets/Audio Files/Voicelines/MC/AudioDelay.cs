using UnityEngine;

public class PlayAudioWithDelay : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public float delay = 4f; // Delay in seconds

    void Start()
    {
        Invoke("PlayDelayedAudio", delay); // Call the method after a delay
    }

    void PlayDelayedAudio()
    {
        if (!audioSource.isPlaying) // Check if audio is not already playing
        {
            audioSource.Play(); // Play the audio
        }
    }
}