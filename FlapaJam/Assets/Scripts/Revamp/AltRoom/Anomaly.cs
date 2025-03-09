using UnityEngine;

public class Anomaly : MonoBehaviour
{
    public enum AnomalyType
    {
        ObjectDisappear, ObjectMoving, RadioNoise, Static,
        StrangeAudio, FlickeringLights, LargeObject
    }

    private AnomalyType type;

    [SerializeField] private AudioClip[] strangeAudioClips; // Assign random audio clips in Inspector for StrangeAudio
    [SerializeField] private AudioClip radioNoiseClip;      // Assign radio noise clip in Inspector
    [SerializeField] private AudioClip staticClip;          // Assign static noise clip in Inspector
    private AudioSource audioSource;                       // For playing background audio

    public void Initialize()
    {
        // Randomly select an anomaly type
        type = (AnomalyType)Random.Range(0, System.Enum.GetValues(typeof(AnomalyType)).Length);
        Debug.Log($"Anomaly in {gameObject.name}: {type}");

        // Ensure AudioSource is available for audio-based anomalies
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null && (type == AnomalyType.RadioNoise || type == AnomalyType.Static || type == AnomalyType.StrangeAudio))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound for background effects
        }

        switch (type)
        {
            case AnomalyType.ObjectDisappear:
                SetupObjectDisappear();
                break;
            case AnomalyType.ObjectMoving:
                SetupObjectMoving();
                break;
            case AnomalyType.RadioNoise:
                SetupRadioNoise();
                break;
            case AnomalyType.Static:
                SetupStatic();
                break;
            case AnomalyType.StrangeAudio:
                SetupStrangeAudio();
                break;
            case AnomalyType.FlickeringLights:
                SetupFlickeringLights();
                break;
            case AnomalyType.LargeObject:
                // Placeholder for future implementation
                Debug.Log("LargeObject anomaly not yet implemented.");
                break;
        }
    }

    private void SetupObjectDisappear()
    {
        // Find the "Object" empty GameObject in the room (assuming it's a child or in the scene)
        GameObject objectContainer = GameObject.Find("Object");
        if (objectContainer == null)
        {
            Debug.LogError("No 'Object' GameObject found in the scene!");
            return;
        }

        // Get all child objects under "Object"
        Transform[] objects = objectContainer.GetComponentsInChildren<Transform>();
        Transform[] validObjects = System.Array.FindAll(objects, t => t != objectContainer.transform); // Exclude the container itself

        if (validObjects.Length == 0)
        {
            Debug.LogWarning("No objects found under 'Object' to disappear!");
            return;
        }

        // Randomly select and destroy one object
        int randomIndex = Random.Range(0, validObjects.Length);
        Destroy(validObjects[randomIndex].gameObject);
        Debug.Log($"ObjectDisappear: Destroyed {validObjects[randomIndex].name}");
    }

    private void SetupObjectMoving()
    {
        // Find a random object in the scene to apply the Glimmer effect (assuming objects are tagged or in a container)
        GameObject[] potentialObjects = GameObject.FindGameObjectsWithTag("MovableObject"); // Tag your movable objects
        if (potentialObjects.Length == 0)
        {
            Debug.LogError("No objects tagged 'MovableObject' found for ObjectMoving anomaly!");
            return;
        }

        GameObject targetObject = potentialObjects[Random.Range(0, potentialObjects.Length)];
        Glimmer glimmer = targetObject.GetComponent<Glimmer>();
        if (glimmer == null)
        {
            glimmer = targetObject.AddComponent<Glimmer>();
            Debug.Log($"ObjectMoving: Added Glimmer script to {targetObject.name}");
        }
        else
        {
            Debug.Log($"ObjectMoving: {targetObject.name} already has Glimmer script, enabling it.");
            glimmer.enabled = true;
        }
    }

    private void SetupRadioNoise()
    {
        // Find the Radio object and play noise through it
        GameObject radio = GameObject.FindGameObjectWithTag("Radio");
        if (radio == null)
        {
            Debug.LogError("No object tagged 'Radio' found for RadioNoise anomaly!");
            return;
        }

        AudioSource radioSource = radio.GetComponent<AudioSource>();
        if (radioSource == null)
        {
            radioSource = radio.AddComponent<AudioSource>();
        }

        if (radioNoiseClip != null)
        {
            radioSource.clip = radioNoiseClip;
            radioSource.loop = true;
            radioSource.Play();
            Debug.Log("RadioNoise: Playing noise through Radio.");
        }
        else
        {
            Debug.LogError("RadioNoiseClip not assigned in Inspector!");
        }
    }

    private void SetupStatic()
    {
        if (staticClip != null)
        {
            audioSource.clip = staticClip;
            audioSource.loop = true;
            audioSource.Play();
            Debug.Log("Static: Playing static noise in background.");
        }
        else
        {
            Debug.LogError("StaticClip not assigned in Inspector!");
        }
    }

    private void SetupStrangeAudio()
    {
        if (strangeAudioClips == null || strangeAudioClips.Length == 0)
        {
            Debug.LogError("StrangeAudioClips array not assigned or empty in Inspector!");
            return;
        }

        AudioClip randomClip = strangeAudioClips[Random.Range(0, strangeAudioClips.Length)];
        audioSource.clip = randomClip;
        audioSource.loop = false; // Play once, adjust if looping is desired
        audioSource.Play();
        Debug.Log($"StrangeAudio: Playing {randomClip.name} in background.");
    }

    private void SetupFlickeringLights()
    {
        // Find the "Light" empty GameObject with all light sources
        GameObject lightContainer = GameObject.Find("Light");
        if (lightContainer == null)
        {
            Debug.LogError("No 'Light' GameObject found in the scene!");
            return;
        }

        // Get all Light components in children
        Light[] lights = lightContainer.GetComponentsInChildren<Light>();
        if (lights.Length == 0)
        {
            Debug.LogWarning("No Light components found under 'Light' GameObject!");
            return;
        }

        // Add or configure Flicker script for each light
        foreach (Light light in lights)
        {
            Flicker flicker = light.GetComponent<Flicker>();
            if (flicker == null)
            {
                flicker = light.gameObject.AddComponent<Flicker>();
                Debug.Log($"FlickeringLights: Added Flicker script to {light.gameObject.name}");
            }
            else
            {
                flicker.enabled = true;
                Debug.Log($"FlickeringLights: Enabled Flicker script on {light.gameObject.name}");
            }
        }
    }
}