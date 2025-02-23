using UnityEngine;

public class RandomAudioTrigger : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip[] audioClips;  // Array of audio clips to choose from
    public float minDelay = 5f;     // Minimum delay between audio plays
    public float maxDelay = 10f;    // Maximum delay between audio plays

    private float nextPlayTime;     // Time when the next audio will play

    void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
            return;
        }

        if (audioClips.Length == 0)
        {
            Debug.LogError("No audio clips assigned!");
            return;
        }

        // Set the initial time for the first audio play
        nextPlayTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    void Update()
    {
        // Check if it's time to play the next audio clip
        if (Time.time >= nextPlayTime)
        {
            PlayRandomClip();
            // Set the next play time
            nextPlayTime = Time.time + Random.Range(minDelay, maxDelay);
        }
    }

    void PlayRandomClip()
    {
        if (audioClips.Length > 0)
        {
            // Choose a random clip from the array
            int randomIndex = Random.Range(0, audioClips.Length);
            audioSource.clip = audioClips[randomIndex];

            // Play the audio clip
            audioSource.Play();
        }
    }
}