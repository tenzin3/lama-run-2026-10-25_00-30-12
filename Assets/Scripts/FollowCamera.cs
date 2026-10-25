using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 3.8f, -5.5f);

    [Header("Smoothing")]
    [Tooltip("Smoothing factor for vertical (Y) following.")]
    [SerializeField] private float _ySmoothSpeed = 8f;
    [Tooltip("Time in seconds to smoothly rotate to new direction (about 0.3s).")]
    [SerializeField] private float _rotationSmoothTime = 0.3f;

    [Header("Rotation")]
    [SerializeField] private float _pitchAngle = 18f;

    private float _currentYaw;
    private float _yawVelocity;
    private float _currentY;
    private PlayerController _playerController;

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

    private void LateUpdate()
    {
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
        transform.position = camPos;
    }
}

