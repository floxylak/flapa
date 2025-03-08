using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    
    public float lightFlickerRange = .1f;
    private float _minIntensity;
    private float _maxIntensity;
    public float flickerSpeed = 5f;      // Speed of flickering

    private Light _lightSource;
    private float _targetIntensity;

    private void Start()
    {
        _minIntensity = _lightSource.intensity - lightFlickerRange;
        _maxIntensity = _lightSource.intensity + lightFlickerRange;

        _lightSource = GetComponent<Light>();
        _targetIntensity = Random.Range(_minIntensity, _maxIntensity);
        StartCoroutine(FlickerLight());
    }

    private IEnumerator FlickerLight()
    {
        while (true)
        {
            var currentIntensity = _lightSource.intensity;
            var newIntensity = Mathf.Lerp(currentIntensity, _targetIntensity, flickerSpeed * Time.deltaTime);
            _lightSource.intensity = newIntensity;

            if (Mathf.Abs(currentIntensity - _targetIntensity) <= 0.05f)
            {
                _targetIntensity = Random.Range(_minIntensity, _maxIntensity);
            }

            yield return null;
        }
    }
}