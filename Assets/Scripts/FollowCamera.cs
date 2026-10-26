using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public static FollowCamera Instance { get; private set; }

    [Header("Target & Offset")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2.6f, -3.8f);

    [Header("Smoothing")]
    [Tooltip("Smoothing factor for vertical (Y) following.")]
    [SerializeField] private float _ySmoothSpeed = 8f;
    [Tooltip("Time in seconds to smoothly rotate to new direction (about 0.3s).")]
    [SerializeField] private float _rotationSmoothTime = 0.3f;

    [Header("Rotation")]
    [SerializeField] private float _pitchAngle = 14f;

    [Header("Dynamic FOV")]
    [SerializeField] private float _baseFOV = 60f;
    [SerializeField] private float _maxFOV = 69f;
    [SerializeField] private float _fovLerpSpeed = 3f;
    [Tooltip("Target base FOV in portrait mode (aspect < 1.0) so all 3 lanes stay visible.")]
    [SerializeField] private float _portraitBaseFOV = 82f;
    [Tooltip("Target max FOV in portrait mode at top running speed.")]
    [SerializeField] private float _portraitMaxFOV = 92f;

    private float _currentYaw;
    private float _yawVelocity;
    private float _currentY;
    private PlayerController _playerController;
    private Camera _cameraComponent;

    // Shake state
    private float _shakeTimer = 0f;
    private float _shakeDuration = 0f;
    private float _shakeMagnitude = 0f;

    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            if (_target != null)
            {
                _playerController = _target.GetComponent<PlayerController>();
            }
        }
    }

    private void Awake()
    {
        Instance = this;
        _cameraComponent = GetComponent<Camera>();
        if (_cameraComponent != null)
        {
            float aspect = _cameraComponent.aspect;
            float portraitFactor = aspect < 1.0f ? Mathf.Clamp01((1.0f - aspect) / 0.55f) : 0f;
            _cameraComponent.fieldOfView = Mathf.Lerp(_baseFOV, _portraitBaseFOV, portraitFactor);
        }
    }

    private void Start()
    {
        if (_target == null)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                _target = pc.transform;
            }
        }

        if (_target != null)
        {
            _playerController = _target.GetComponent<PlayerController>();
            _currentYaw = _playerController != null ? _playerController.TargetYaw : _target.eulerAngles.y;
            _currentY = _target.position.y;

            Quaternion yawOnly = Quaternion.Euler(0f, _currentYaw, 0f);
            Vector3 rotatedOffset = yawOnly * new Vector3(0f, 0f, _offset.z);
            transform.position = new Vector3(_target.position.x + rotatedOffset.x, _currentY + _offset.y, _target.position.z + rotatedOffset.z);
            transform.rotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);
        }
    }

    public void TriggerShake(float duration = 0.35f, float magnitude = 0.35f)
    {
        _shakeDuration = duration;
        _shakeTimer = duration;
        _shakeMagnitude = magnitude;
    }

    private void LateUpdate()
    {
        // 1. Dynamic FOV with speed and screen aspect ratio (widened in portrait for lane visibility)
        if (_cameraComponent != null)
        {
            float aspect = _cameraComponent.aspect;
            float portraitFactor = aspect < 1.0f ? Mathf.Clamp01((1.0f - aspect) / 0.55f) : 0f;

            float currentBaseFOV = Mathf.Lerp(_baseFOV, _portraitBaseFOV, portraitFactor);
            float currentMaxFOV = Mathf.Lerp(_maxFOV, _portraitMaxFOV, portraitFactor);

            float speedRatio = 0f;
            if (GameManager.Instance != null)
            {
                speedRatio = Mathf.InverseLerp(GameManager.Instance.StartSpeed, GameManager.Instance.MaxSpeed, GameManager.Instance.CurrentSpeed);
            }

            float targetFOV = Mathf.Lerp(currentBaseFOV, currentMaxFOV, speedRatio);
            _cameraComponent.fieldOfView = Mathf.Lerp(_cameraComponent.fieldOfView, targetFOV, _fovLerpSpeed * Time.deltaTime);
        }

        if (_target == null) return;

        float targetYaw = _playerController != null ? _playerController.TargetYaw : _target.eulerAngles.y;
        _currentYaw = Mathf.SmoothDampAngle(_currentYaw, targetYaw, ref _yawVelocity, _rotationSmoothTime);

        // Update rotation
        transform.rotation = Quaternion.Euler(_pitchAngle, _currentYaw, 0f);

        // Smoothly interpolate Y for gentle reaction to jumps
        _currentY = Mathf.Lerp(_currentY, _target.position.y, _ySmoothSpeed * Time.deltaTime);

        // Rotate horizontal offset to stay behind the player relative to current yaw
        Quaternion yawOnly = Quaternion.Euler(0f, _currentYaw, 0f);
        Vector3 rotatedOffset = yawOnly * new Vector3(_offset.x, 0f, _offset.z);

        Vector3 camPos = _target.position + rotatedOffset;
        camPos.y = _currentY + _offset.y;

        // Camera Shake offset
        if (_shakeTimer > 0f)
        {
            _shakeTimer -= Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(_shakeTimer / Mathf.Max(0.001f, _shakeDuration));
            float currentMag = _shakeMagnitude * progress;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * currentMag,
                Random.Range(-0.8f, 0.8f) * currentMag,
                Random.Range(-0.5f, 0.5f) * currentMag
            );
            camPos += shakeOffset;
        }

        transform.position = camPos;
    }
}

