using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 3.8f, -5.5f);

    [Header("Smoothing")]
    [Tooltip("Smoothing factor for horizontal (X) following.")]
    [SerializeField] private float _xSmoothSpeed = 10f;
    [Tooltip("Smoothing factor for vertical (Y) following.")]
    [SerializeField] private float _ySmoothSpeed = 8f;

    [Header("Rotation")]
    [SerializeField] private float _pitchAngle = 18f;

    private float _currentX;
    private float _currentY;

    public Transform Target
    {
        get => _target;
        set => _target = value;
    }

    private void Start()
    {
        if (_target != null)
        {
            _currentX = _target.position.x;
            _currentY = _target.position.y;
            transform.position = new Vector3(_currentX + _offset.x, _currentY + _offset.y, _target.position.z + _offset.z);
            transform.rotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPos = _target.position;

        // Smoothly interpolate X to avoid jarring lane switch snaps
        _currentX = Mathf.Lerp(_currentX, targetPos.x, _xSmoothSpeed * Time.deltaTime);

        // Smoothly interpolate Y for gentle reaction to jumps
        _currentY = Mathf.Lerp(_currentY, targetPos.y, _ySmoothSpeed * Time.deltaTime);

        // Follow Z directly for tight forward response
        float targetZ = targetPos.z + _offset.z;

        transform.position = new Vector3(_currentX + _offset.x, _currentY + _offset.y, targetZ);
        transform.rotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
    }
}
