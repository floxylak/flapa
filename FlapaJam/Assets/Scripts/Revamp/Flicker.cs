using UnityEngine;

public class Flicker : MonoBehaviour
{
    private Light lightSource;
    public float minIntensity = 0f;
    public float maxIntensity = 1f;
    public float flickerSpeed = 5f;

    void Start()
    {
        lightSource = GetComponent<Light>();
    }

    void Update()
    {
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise(Time.time * flickerSpeed, 0f));
    }
}