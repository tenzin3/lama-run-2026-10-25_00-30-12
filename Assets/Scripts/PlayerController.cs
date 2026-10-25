using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Lane Movement")]
    [Tooltip("Distance between lanes in world units.")]
    [SerializeField] private float _laneDistance = 2.0f;
    [Tooltip("Speed of switching between lanes.")]
    [SerializeField] private float _laneSwitchSpeed = 16.0f;

    [Header("Jump & Physics")]
    [Tooltip("Upward impulse velocity for jumping.")]
    [SerializeField] private float _jumpForce = 8.5f;
    [Tooltip("Gravity acceleration applied when airborne.")]
    [SerializeField] private float _gravity = 26.0f;
    [Tooltip("Fast fall acceleration multiplier when swiping down mid-air.")]
    [SerializeField] private float _fastFallMultiplier = 1.6f;

    [Header("Slide")]
    [Tooltip("Duration of slide in seconds.")]
    [SerializeField] private float _slideDuration = 0.75f;
    [Tooltip("Capsule collider height when sliding.")]
    [SerializeField] private float _slidingColliderHeight = 0.8f;
    [Tooltip("Capsule collider center Y when sliding.")]
    [SerializeField] private float _slidingColliderCenterY = 0.4f;

    [Header("Visual Model Reference")]
    [Tooltip("Transform of the child visual model (capsule or character mesh).")]
    [SerializeField] private Transform _visualModel;

    [Header("Touch / Swipe Settings")]
    [Tooltip("Minimum drag distance in pixels to register a swipe.")]
    [SerializeField] private float _swipeThreshold = 40.0f;

    // Movement state
    private int _currentLane = 0; // -1: Left, 0: Mid, 1: Right
    private float _targetX = 0f;
    private float _verticalVelocity = 0f;
    private bool _isGrounded = true;
    private float _groundY = 0f;

    // Slide state
    private bool _isSliding = false;
    private float _slideTimer = 0f;
    private float _standingColliderHeight = 2.0f;
    private float _standingColliderCenterY = 1.0f;
    private Vector3 _standingVisualScale = Vector3.one;
    private Vector3 _standingVisualPosition = new Vector3(0f, 1.0f, 0f);

    // Game state
    private bool _isDead = false;

    // Swipe input tracking
    private Vector2 _touchStartPos;
    private bool _isSwiping = false;

    // Components
    private CapsuleCollider _capsuleCollider;

    public bool IsDead => _isDead;
    public bool IsSliding => _isSliding;
    public bool IsGrounded => _isGrounded;
    public int CurrentLane => _currentLane;

    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider != null)
        {
            _standingColliderHeight = _capsuleCollider.height;
            _standingColliderCenterY = _capsuleCollider.center.y;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (_visualModel != null)
        {
            _standingVisualScale = _visualModel.localScale;
            _standingVisualPosition = _visualModel.localPosition;
        }

        _groundY = transform.position.y;
    }

    private void Update()
    {
        if (_isDead) return;

        HandleInput();
        UpdateSlide();
        UpdateVerticalMovement();
        UpdateHorizontalMovement();
        UpdateForwardMovement();
    }

    private void HandleInput()
    {
        // 1. Keyboard Input (New Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                ChangeLane(-1);
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                ChangeLane(1);
            }

            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                Jump();
            }

            if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                Slide();
            }
        }

        // 2. Touch / Mouse Swipe Input
        HandleSwipeInput();
    }

    private void HandleSwipeInput()
    {
        Vector2 pointerPos = Vector2.zero;
        bool pointerDown = false;
        bool pointerActive = false;
        bool pointerUp = false;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            pointerPos = touch.primaryTouch.position.ReadValue();
            pointerDown = touch.primaryTouch.press.wasPressedThisFrame;
            pointerActive = true;
            pointerUp = touch.primaryTouch.press.wasReleasedThisFrame;
        }
        else
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                pointerPos = mouse.position.ReadValue();
                pointerDown = mouse.leftButton.wasPressedThisFrame;
                pointerActive = mouse.leftButton.isPressed;
                pointerUp = mouse.leftButton.wasReleasedThisFrame;
            }
        }

        if (pointerDown)
        {
            _touchStartPos = pointerPos;
            _isSwiping = true;
        }
        else if (_isSwiping && pointerActive)
        {
            Vector2 delta = pointerPos - _touchStartPos;
            if (delta.magnitude >= _swipeThreshold)
            {
                ExecuteSwipe(delta);
                // Reset start pos so continued dragging can register further swipes
                _touchStartPos = pointerPos;
            }
        }
        else if (pointerUp)
        {
            _isSwiping = false;
        }
    }

    private void ExecuteSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0f) ChangeLane(1);
            else ChangeLane(-1);
        }
        else
        {
            if (delta.y > 0f) Jump();
            else Slide();
        }
    }

    public void ChangeLane(int direction)
    {
        int nextLane = Mathf.Clamp(_currentLane + direction, -1, 1);
        if (nextLane != _currentLane)
        {
            _currentLane = nextLane;
            _targetX = _currentLane * _laneDistance;
        }
    }

    public void Jump()
    {
        if (_isGrounded && !_isSliding)
        {
            _verticalVelocity = _jumpForce;
            _isGrounded = false;
        }
    }

    public void Slide()
    {
        if (!_isGrounded)
        {
            // Fast fall mid-air
            _verticalVelocity = -_jumpForce * _fastFallMultiplier;
        }

        if (!_isSliding)
        {
            StartSlide();
        }
    }

    private void StartSlide()
    {
        _isSliding = true;
        _slideTimer = _slideDuration;

        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = _slidingColliderHeight;
            _capsuleCollider.center = new Vector3(0f, _slidingColliderCenterY, 0f);
        }

        if (_visualModel != null)
        {
            _visualModel.localScale = new Vector3(_standingVisualScale.x, _standingVisualScale.y * 0.4f, _standingVisualScale.z);
            _visualModel.localPosition = new Vector3(0f, 0.4f, 0f);
        }
    }

    private void StopSlide()
    {
        _isSliding = false;

        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = _standingColliderHeight;
            _capsuleCollider.center = new Vector3(0f, _standingColliderCenterY, 0f);
        }

        if (_visualModel != null)
        {
            _visualModel.localScale = _standingVisualScale;
            _visualModel.localPosition = _standingVisualPosition;
        }
    }

    private void UpdateSlide()
    {
        if (_isSliding)
        {
            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0f)
            {
                StopSlide();
            }
        }
    }

    private void UpdateVerticalMovement()
    {
        Vector3 pos = transform.position;

        if (!_isGrounded)
        {
            _verticalVelocity -= _gravity * Time.deltaTime;
            pos.y += _verticalVelocity * Time.deltaTime;

            if (pos.y <= _groundY)
            {
                pos.y = _groundY;
                _verticalVelocity = 0f;
                _isGrounded = true;
            }
        }
        else
        {
            pos.y = _groundY;
        }

        transform.position = pos;
    }

    private void UpdateHorizontalMovement()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, _targetX, _laneSwitchSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void UpdateForwardMovement()
    {
        float speed = 12.0f;
        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.CurrentSpeed;
        }

        Vector3 pos = transform.position;
        pos.z += speed * Time.deltaTime;
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Obstacle>(out var obstacle))
        {
            HitObstacle(obstacle);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Obstacle>(out var obstacle))
        {
            HitObstacle(obstacle);
        }
    }

    public void HitObstacle(Obstacle obstacle)
    {
        if (_isDead) return;

        _isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
