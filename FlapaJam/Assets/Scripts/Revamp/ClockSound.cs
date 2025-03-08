using UnityEngine;

public class ClockSound : MonoBehaviour
{
    public float interval = 1;
    public AudioSource tickSound;

    private float _timer = 0f;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > interval)
        {
            tickSound.Play();
            _timer = 0;
        }
    }
}