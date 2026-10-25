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

    [Header("Slide & Collider Sizing")]
    [Tooltip("Duration of slide in seconds.")]
    [SerializeField] private float _slideDuration = 0.75f;
    [Tooltip("Capsule collider height when standing (Tenzin is 1.48m tall).")]
    [SerializeField] private float _standingColliderHeight = 1.48f;
    [Tooltip("Capsule collider center Y when standing.")]
    [SerializeField] private float _standingColliderCenterY = 0.74f;
    [Tooltip("Capsule collider radius.")]
    [SerializeField] private float _colliderRadius = 0.30f;
    [Tooltip("Capsule collider height when sliding (low enough to slide under high beams).")]
    [SerializeField] private float _slidingColliderHeight = 0.65f;
    [Tooltip("Capsule collider center Y when sliding.")]
    [SerializeField] private float _slidingColliderCenterY = 0.325f;

    [Header("Visual Model & Animation")]
    [Tooltip("Transform of the child visual model (capsule or character mesh).")]
    [SerializeField] private Transform _visualModel;
    [Tooltip("Animator driving Tenzin's character animations.")]
    [SerializeField] private Animator _animator;
    [Tooltip("Base running speed at which the run animation plays at 1.0x speed.")]
    [SerializeField] private float _baseRunAnimSpeed = 12.0f;
    [Tooltip("Crossfade duration between animation states in seconds.")]
    [SerializeField] private float _animCrossFadeDuration = 0.1f;

    [Header("Turn & Lean Settings")]
    [Tooltip("Speed in degrees/sec at which the character rotates toward the new direction.")]
    [SerializeField] private float _turnSmoothSpeed = 360.0f;
    [Tooltip("Maximum roll angle (degrees) to bank/lean into lane changes.")]
    [SerializeField] private float _maxLaneLeanRoll = 12.0f;
    [Tooltip("Maximum yaw angle (degrees) to turn slightly into lane changes.")]
    [SerializeField] private float _maxLaneLeanYaw = 10.0f;
    [Tooltip("Speed of lane lean interpolation.")]
    [SerializeField] private float _laneLeanSpeed = 12.0f;

    [Header("Touch / Swipe Settings")]
    [Tooltip("Minimum drag distance in pixels to register a swipe.")]
    [SerializeField] private float _swipeThreshold = 40.0f;

    // Animation state hashes
    private static readonly int AnimStateIdle = Animator.StringToHash("Idle");
    private static readonly int AnimStateRun = Animator.StringToHash("Run");
    private static readonly int AnimStateJump = Animator.StringToHash("Jump");
    private static readonly int AnimStateSlide = Animator.StringToHash("Slide");
    private static readonly int AnimStateFall = Animator.StringToHash("Fall");
    private static readonly int AnimStateStumble = Animator.StringToHash("Stumble");

    // Movement state
    private int _currentLane = 0; // -1: Left, 0: Mid, 1: Right
    private float _targetLateralOffset = 0f;
    private float _verticalVelocity = 0f;
    private bool _isGrounded = true;
    private float _groundY = 0f;

    // Direction & Track line state
    private Quaternion _targetHeading = Quaternion.identity;
    private Vector3 _trackLineAnchor = Vector3.zero;
    private float _distanceTraveled = 0f;

    // Visual turn & lean interpolation
    private float _visualTurnOffsetAngle = 0f;
    private float _targetLeanRoll = 0f;
    private float _targetLeanYaw = 0f;
    private float _currentLeanRoll = 0f;
    private float _currentLeanYaw = 0f;

    // Corner / Turn Zone state
    private CornerSegment _currentCorner;

    // Slide state
    private bool _isSliding = false;
    private float _slideTimer = 0f;

    // Animation state tracking
    private int _currentAnimState = 0;

    // Game state
    private bool _isDead = false;
    private float _runDustTimer = 0f;

    // Swipe input tracking
    private Vector2 _touchStartPos;
    private bool _isSwiping = false;

    // Components
    private CapsuleCollider _capsuleCollider;

    public bool IsDead => _isDead;
    public bool IsSliding => _isSliding;
    public bool IsGrounded => _isGrounded;
    public int CurrentLane => _currentLane;
    public float DistanceTraveled => _distanceTraveled;
    public float TargetYaw => _targetHeading.eulerAngles.y;
    public Vector3 ForwardDirection => _targetHeading * Vector3.forward;
    public Vector3 RightDirection => _targetHeading * Vector3.right;
    public bool IsInTurnZone => _currentCorner != null && !_currentCorner.HasTurned && _currentCorner.IsPlayerInTurnZone(transform.position);

    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = _standingColliderHeight;
            _capsuleCollider.center = new Vector3(0f, _standingColliderCenterY, 0f);
            _capsuleCollider.radius = _colliderRadius;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (_visualModel == null)
        {
            var tenzin = transform.Find("Tenzin");
            if (tenzin != null) _visualModel = tenzin;
            else
            {
                var vis = transform.Find("Visual");
                if (vis != null) _visualModel = vis;
            }
        }

        if (_animator == null && _visualModel != null)
        {
            _animator = _visualModel.GetComponentInChildren<Animator>();
        }
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        if (_animator != null)
        {
            _animator.applyRootMotion = false;
        }

        var skirt = GetComponentInChildren<TenzinSkirtFollow>(true);
        if (skirt != null)
        {
            skirt.enabled = true;
        }

        _groundY = transform.position.y;
        _targetHeading = transform.rotation;
        _trackLineAnchor = transform.position;
        _targetLateralOffset = _currentLane * _laneDistance;
    }

    private void Start()
    {
        PlayAnimation(AnimStateIdle, 0f);
    }

    private void Update()
    {
        if (_isDead)
        {
            // Continue falling physics if dead from falling off edge
            UpdateVerticalMovement();
            UpdateVisualOrientation();
            return;
        }

        // 1. Wait for player input on Start Screen before running
        if (GameManager.Instance != null && !GameManager.Instance.IsStarted)
        {
            if (_currentAnimState != AnimStateIdle)
            {
                PlayAnimation(AnimStateIdle, _animCrossFadeDuration);
            }
            CheckStartInput();
            UpdateVisualOrientation();
            return;
        }

        // 2. Pause check
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        // If game has started and we are still in Idle, switch to Run
        if (_currentAnimState == AnimStateIdle && _isGrounded)
        {
            PlayAnimation(AnimStateRun);
        }

        CheckCornerEdge();
        HandleInput();
        UpdateSlide();
        UpdateVerticalMovement();
        UpdateHorizontalMovement();
        UpdateForwardMovement();
        UpdateVisualOrientation();
        UpdateAnimationSpeed();
        UpdateRunDust();
    }

    private void CheckStartInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                GameManager.Instance.StartGame();
                PlayAnimation(AnimStateRun);
                return;
            }
        }

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            GameManager.Instance.StartGame();
            PlayAnimation(AnimStateRun);
            return;
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            GameManager.Instance.StartGame();
            PlayAnimation(AnimStateRun);
            return;
        }
    }

    private void PlayAnimation(int stateHash, float crossFadeTime = -1f)
    {
        if (_animator == null) return;
        if (_currentAnimState == stateHash) return;

        _currentAnimState = stateHash;
        float duration = crossFadeTime >= 0f ? crossFadeTime : _animCrossFadeDuration;
        _animator.CrossFade(stateHash, duration);
    }

    private void UpdateAnimationSpeed()
    {
        if (_animator == null) return;

        if (_currentAnimState == AnimStateRun)
        {
            float speed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 12.0f;
            _animator.speed = Mathf.Max(0.1f, speed / _baseRunAnimSpeed);
        }
        else
        {
            _animator.speed = 1.0f;
        }
    }

    private void UpdateVisualOrientation()
    {
        if (_visualModel == null) return;

        // 1. Smooth turn toward new track direction
        _visualTurnOffsetAngle = Mathf.MoveTowards(_visualTurnOffsetAngle, 0f, _turnSmoothSpeed * Time.deltaTime);
        Quaternion turnRot = Quaternion.Euler(0f, _visualTurnOffsetAngle, 0f);

        // 2. Lean into lane changes
        _currentLeanRoll = Mathf.Lerp(_currentLeanRoll, _targetLeanRoll, _laneLeanSpeed * Time.deltaTime);
        _currentLeanYaw = Mathf.Lerp(_currentLeanYaw, _targetLeanYaw, _laneLeanSpeed * Time.deltaTime);
        Quaternion leanRot = Quaternion.Euler(0f, _currentLeanYaw, _currentLeanRoll);

        _visualModel.localRotation = turnRot * leanRot;
    }

    private void UpdateRunDust()
    {
        if (!_isGrounded || _isSliding || _isDead) return;

        _runDustTimer -= Time.deltaTime;
        if (_runDustTimer <= 0f)
        {
            _runDustTimer = 0.12f;
            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.EmitRunDust(transform.position, ForwardDirection);
            }
        }
    }

    private void CheckCornerEdge()
    {
        if (_currentCorner != null && !_currentCorner.HasTurned)
        {
            if (_currentCorner.HasPlayerRunOffEdge(transform.position))
            {
                FallOffEdge();
            }
        }
    }

    private void HandleInput()
    {
        // 1. Keyboard Input (New Input System)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                OnTurnOrLaneInput(TurnDirection.Left);
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                OnTurnOrLaneInput(TurnDirection.Right);
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
            if (delta.x > 0f) OnTurnOrLaneInput(TurnDirection.Right);
            else OnTurnOrLaneInput(TurnDirection.Left);
        }
        else
        {
            if (delta.y > 0f) Jump();
            else Slide();
        }
    }

    private void OnTurnOrLaneInput(TurnDirection dir)
    {
        if (IsInTurnZone)
        {
            AttemptTurn(dir);
        }
        else
        {
            ChangeLane((int)dir);
        }
    }

    public void AttemptTurn(TurnDirection dir)
    {
        if (_currentCorner == null || _isDead) return;

        if (dir == _currentCorner.Direction)
        {
            ExecuteTurn(_currentCorner);
        }
        else
        {
            // Turned wrong direction in corner -> run off edge / Game Over
            FallOffEdge();
        }
    }

    private void ExecuteTurn(CornerSegment corner)
    {
        corner.OnTurnSuccess();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTurnWhoosh();
        }

        Vector3 oldForward = ForwardDirection;
        float turnAngle = corner.Direction == TurnDirection.Right ? 90f : -90f;
        _targetHeading = _targetHeading * Quaternion.Euler(0f, turnAngle, 0f);
        transform.rotation = _targetHeading;

        // Smoothly rotate character body toward new heading
        _visualTurnOffsetAngle -= turnAngle;

        Vector3 pivot = corner.PivotPosition;
        _trackLineAnchor = pivot;

        // Keep the exact same lane index in the new direction
        _targetLateralOffset = _currentLane * _laneDistance;

        // Position player on the new track line, preserving forward distance past pivot
        Vector3 fromPivot = transform.position - pivot;
        float forwardProgress = Vector3.Dot(fromPivot, oldForward);
        float newForwardProgress = Mathf.Max(0f, forwardProgress);

        Vector3 newPos = pivot + RightDirection * _targetLateralOffset + ForwardDirection * newForwardProgress;
        newPos.y = transform.position.y;
        transform.position = newPos;
    }

    public void EnterCorner(CornerSegment corner)
    {
        _currentCorner = corner;
    }

    public void ExitCorner(CornerSegment corner)
    {
        if (_currentCorner == corner)
        {
            _currentCorner = null;
        }
    }

    public void FallOffEdge()
    {
        if (_isDead) return;

        _isDead = true;
        _isGrounded = false;
        _verticalVelocity = -12f;

        PlayAnimation(AnimStateFall);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrashThud();
        }

        if (FollowCamera.Instance != null)
        {
            FollowCamera.Instance.TriggerShake(0.4f, 0.5f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void ChangeLane(int direction)
    {
        int nextLane = Mathf.Clamp(_currentLane + direction, -1, 1);
        if (nextLane != _currentLane)
        {
            _currentLane = nextLane;
            _targetLateralOffset = _currentLane * _laneDistance;
        }
    }

    public void Jump()
    {
        if (_isGrounded && !_isSliding)
        {
            _verticalVelocity = _jumpForce;
            _isGrounded = false;

            PlayAnimation(AnimStateJump);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJump();
            }
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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlide();
            }
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
            _capsuleCollider.radius = _colliderRadius;
        }

        PlayAnimation(AnimStateSlide);
    }

    private void StopSlide()
    {
        _isSliding = false;

        if (_capsuleCollider != null)
        {
            _capsuleCollider.height = _standingColliderHeight;
            _capsuleCollider.center = new Vector3(0f, _standingColliderCenterY, 0f);
            _capsuleCollider.radius = _colliderRadius;
        }

        if (_isGrounded && !_isDead)
        {
            PlayAnimation(AnimStateRun);
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

            if (!_isDead && pos.y <= _groundY)
            {
                pos.y = _groundY;
                _verticalVelocity = 0f;
                _isGrounded = true;

                if (!_isSliding)
                {
                    PlayAnimation(AnimStateRun);
                }

                if (EffectsManager.Instance != null)
                {
                    EffectsManager.Instance.EmitLandingDust(transform.position);
                }
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
        Vector3 toPlayer = transform.position - _trackLineAnchor;
        float currentForwardDist = Vector3.Dot(toPlayer, ForwardDirection);
        float currentLat = Vector3.Dot(toPlayer, RightDirection);

        float newLat = Mathf.MoveTowards(currentLat, _targetLateralOffset, _laneSwitchSpeed * Time.deltaTime);

        // Calculate lateral velocity to drive lane change lean
        float lateralDelta = newLat - currentLat;
        float lateralSpeed = Time.deltaTime > 0f ? (lateralDelta / Time.deltaTime) : 0f;
        float normalizedLateral = Mathf.Clamp(lateralSpeed / _laneSwitchSpeed, -1f, 1f);

        _targetLeanRoll = -normalizedLateral * _maxLaneLeanRoll;
        _targetLeanYaw = normalizedLateral * _maxLaneLeanYaw;

        Vector3 pos = _trackLineAnchor + ForwardDirection * currentForwardDist + RightDirection * newLat;
        pos.y = transform.position.y;
        transform.position = pos;
    }

    private void UpdateForwardMovement()
    {
        float speed = 12.0f;
        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.CurrentSpeed;
        }

        float deltaDist = speed * Time.deltaTime;
        _distanceTraveled += deltaDist;
        transform.position += ForwardDirection * deltaDist;
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

        PlayAnimation(AnimStateFall);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrashThud();
        }

        if (FollowCamera.Instance != null)
        {
            FollowCamera.Instance.TriggerShake(0.35f, 0.45f);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
