using UnityEngine;

public class ButterLamp : MonoBehaviour
{
    private static int s_lampSpawnCount = 0;

    [Header("Components")]
    [SerializeField] private Light _pointLight;
    [SerializeField] private Transform _flameTransform;

    [Header("Settings")]
    [Tooltip("Only every Nth butter lamp enables its point light for performance.")]
    [SerializeField] private int _lightFrequency = 4;
    [SerializeField] private float _baseLightIntensity = 1.2f;
    [SerializeField] private float _flickerSpeed = 9f;
    [SerializeField] private float _flickerAmount = 0.25f;

    private bool _hasLight;
    private float _flickerOffset;
    private Vector3 _originalFlameScale = Vector3.one;

    private void Awake()
    {
        _flickerOffset = Random.Range(0f, 100f);

        if (_flameTransform != null)
        {
            _originalFlameScale = _flameTransform.localScale;
        }

        // Enable point light only on every Nth lamp
        _hasLight = (s_lampSpawnCount++ % _lightFrequency == 0);

        if (_pointLight != null)
        {
            _pointLight.enabled = _hasLight;
        }
    }

    private void Update()
    {
        float noise = Mathf.Sin((Time.time + _flickerOffset) * _flickerSpeed);

        if (_hasLight && _pointLight != null && _pointLight.enabled)
        {
            _pointLight.intensity = _baseLightIntensity + noise * _flickerAmount;
        }

        if (_flameTransform != null)
        {
            float scaleMod = 1f + noise * 0.12f;
            _flameTransform.localScale = new Vector3(
                _originalFlameScale.x * (1f - noise * 0.05f),
                _originalFlameScale.y * scaleMod,
                _originalFlameScale.z * (1f - noise * 0.05f)
            );
        }
    }
}
