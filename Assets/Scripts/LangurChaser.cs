using System.Collections.Generic;
using UnityEngine;

public class LangurChaser : MonoBehaviour
{
    public static LangurChaser Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform _visualRoot;
    [SerializeField] private PlayerController _player;
    [SerializeField] private LangurProceduralAnimator _animator;

    [Header("Chase Distance Settings")]
    [Tooltip("Distance behind player when close and visible on screen (meters).")]
    [SerializeField] private float _closeDistance = 1.9f;
    [Tooltip("Distance behind player when fallen back out of view (meters).")]
    [SerializeField] private float _fallbackDistance = 8.5f;
    [Tooltip("Duration in seconds to fall back out of view at start of run.")]
    [SerializeField] private float _startFallbackDuration = 4.0f;
    [Tooltip("Recovery duration in seconds without stumbling before falling back.")]
    [SerializeField] private float _recoveryDuration = 8.0f;

    [Header("Speeds")]
    [SerializeField] private float _catchUpSpeed = 12.0f;
    [SerializeField] private float _retreatSpeed = 2.4f;

    // Path recording for exact delayed replay
    private struct PathPoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Distance;
    }

    private readonly List<PathPoint> _pathHistory = new List<PathPoint>(512);

    // State tracking
    private float _currentDistanceBehind = 1.9f;
    private float _targetDistanceBehind = 8.5f;
    private bool _isClose = false;
    private float _closeTimer = 0f;
    private bool _isStarted = false;
    private float _startFallbackTimer = 0f;
    private bool _isCatching = false;
    private float _chatterTimer = 0f;
    private float _playerGroundY = 0f;

    public bool IsClose => _isClose;
    public bool IsCatching => _isCatching;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_visualRoot == null)
        {
            _visualRoot = transform.Find("Visual");
        }

        if (_animator == null && _visualRoot != null)
        {
            _animator = _visualRoot.GetComponent<LangurProceduralAnimator>();
        }
        if (_animator == null)
        {
            _animator = GetComponentInChildren<LangurProceduralAnimator>();
        }

        _currentDistanceBehind = _closeDistance;
        _targetDistanceBehind = _fallbackDistance;
        _chatterTimer = Random.Range(1.2f, 2.2f);
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
        }

        if (_player != null)
        {
            _playerGroundY = _player.transform.position.y;
            InitializePathHistory();
        }
    }

    private void InitializePathHistory()
    {
        _pathHistory.Clear();

        Vector3 startPos = _player.transform.position;
        startPos.y = _playerGroundY;
        Quaternion startRot = _player.transform.rotation;
        Vector3 forward = startRot * Vector3.forward;

        // Pre-fill history backwards along starting track line from -35m to 0m
        for (float d = -35f; d <= 0f; d += 0.5f)
        {
            _pathHistory.Add(new PathPoint
            {
                Position = startPos + forward * d,
                Rotation = startRot,
                Distance = d
            });
        }

        // Snap langur to starting position
        SamplePathAtDistance(-_closeDistance, out Vector3 p, out Quaternion r);
        transform.position = p;
        transform.rotation = r;
    }

    private void Update()
    {
        if (_player == null) return;

        RecordPlayerPath();
        UpdateChaseState();
        UpdateLangurPosition();
        UpdateAudioAndChatter();
    }

    private void RecordPlayerPath()
    {
        Vector3 currentPos = _player.transform.position;
        currentPos.y = _playerGroundY; // Lock to ground so jumps don't float the langur
        Quaternion currentRot = _player.transform.rotation;
        float currentDist = _player.DistanceTraveled;

        if (_pathHistory.Count == 0 || currentDist > _pathHistory[_pathHistory.Count - 1].Distance + 0.04f)
        {
            _pathHistory.Add(new PathPoint
            {
                Position = currentPos,
                Rotation = currentRot,
                Distance = currentDist
            });

            // Prune points further behind than 50 meters
            float pruneThreshold = currentDist - 50f;
            while (_pathHistory.Count > 60 && _pathHistory[0].Distance < pruneThreshold)
            {
                _pathHistory.RemoveAt(0);
            }
        }
    }

    private void UpdateChaseState()
    {
        bool gameIsRunning = GameManager.Instance != null && GameManager.Instance.IsPlaying;

        if (_isCatching)
        {
            // Pouncing onto player: rapidly close to 0.35m
            _currentDistanceBehind = Mathf.MoveTowards(_currentDistanceBehind, 0.35f, 15f * Time.deltaTime);
            return;
        }

        if (!_isStarted)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsStarted)
            {
                // Run just began! Burst out right behind Tenzin
                _isStarted = true;
                _startFallbackTimer = 0f;
                _currentDistanceBehind = _closeDistance;
            }
            else
            {
                // Before start: crouch right behind Tenzin
                _currentDistanceBehind = _closeDistance;
                return;
            }
        }

        if (!gameIsRunning) return;

        // While meditation flight is active, keep the chaser far behind out of view
        if (MeditationFlight.Instance != null && MeditationFlight.Instance.IsFlying)
        {
            _isClose = false;
            _targetDistanceBehind = _fallbackDistance + 6.0f;
            _currentDistanceBehind = Mathf.MoveTowards(_currentDistanceBehind, _targetDistanceBehind, _catchUpSpeed * Time.deltaTime);
            return;
        }

        // 1. Initial 4-second burst sequence
        if (_startFallbackTimer < _startFallbackDuration)
        {
            _startFallbackTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_startFallbackTimer / _startFallbackDuration);
            // Smoothly fall back out of view over 4 seconds
            float smooth = t * t * (3f - 2f * t);
            _currentDistanceBehind = Mathf.Lerp(_closeDistance, _fallbackDistance, smooth);

            if (_startFallbackTimer >= _startFallbackDuration)
            {
                _isClose = false;
                _targetDistanceBehind = _fallbackDistance;
            }
            return;
        }

        // 2. Stumble-driven close state
        if (_isClose)
        {
            _closeTimer -= Time.deltaTime;

            // Surge to close distance
            _currentDistanceBehind = Mathf.MoveTowards(_currentDistanceBehind, _closeDistance, _catchUpSpeed * Time.deltaTime);

            if (_closeTimer <= 0f)
            {
                // Survived 8 seconds without stumbling: fall back out of view!
                _isClose = false;
                _targetDistanceBehind = _fallbackDistance;
            }
        }
        else
        {
            // Gradually retreat to fallback distance out of view
            _currentDistanceBehind = Mathf.MoveTowards(_currentDistanceBehind, _fallbackDistance, _retreatSpeed * Time.deltaTime);
        }
    }

    private void UpdateLangurPosition()
    {
        float targetDistanceAlongPath = _player.DistanceTraveled - _currentDistanceBehind;

        if (SamplePathAtDistance(targetDistanceAlongPath, out Vector3 sampledPos, out Quaternion sampledRot))
        {
            transform.position = sampledPos;
            transform.rotation = sampledRot;
        }

        if (_animator != null)
        {
            float currentSpeed = GameManager.Instance != null ? GameManager.Instance.CurrentSpeed : 12f;
            _animator.SetSpeed(currentSpeed);
            _animator.IsGalloping = !_isCatching;
        }
    }

    private bool SamplePathAtDistance(float targetDist, out Vector3 position, out Quaternion rotation)
    {
        if (_pathHistory.Count == 0)
        {
            position = transform.position;
            rotation = transform.rotation;
            return false;
        }

        if (targetDist <= _pathHistory[0].Distance)
        {
            position = _pathHistory[0].Position;
            rotation = _pathHistory[0].Rotation;
            return true;
        }

        int lastIdx = _pathHistory.Count - 1;
        if (targetDist >= _pathHistory[lastIdx].Distance)
        {
            position = _pathHistory[lastIdx].Position;
            rotation = _pathHistory[lastIdx].Rotation;
            return true;
        }

        // Search backwards from newest points since target is near the end
        for (int i = lastIdx; i > 0; i--)
        {
            if (_pathHistory[i - 1].Distance <= targetDist && _pathHistory[i].Distance >= targetDist)
            {
                PathPoint pA = _pathHistory[i - 1];
                PathPoint pB = _pathHistory[i];
                float range = pB.Distance - pA.Distance;
                float t = range > 0.0001f ? (targetDist - pA.Distance) / range : 0f;

                position = Vector3.Lerp(pA.Position, pB.Position, t);
                rotation = Quaternion.Slerp(pA.Rotation, pB.Rotation, t);
                return true;
            }
        }

        position = _pathHistory[0].Position;
        rotation = _pathHistory[0].Rotation;
        return true;
    }

    private void UpdateAudioAndChatter()
    {
        if (!_isClose || _isCatching) return;
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        _chatterTimer -= Time.deltaTime;
        if (_chatterTimer <= 0f)
        {
            _chatterTimer = Random.Range(1.8f, 3.2f);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLangurChatter();
            }
        }
    }

    /// <summary>
    /// Drops the langur back out of view during meditation flight.
    /// </summary>
    public void DropBackOutOfView(float extraDistance = 6.0f)
    {
        _isClose = false;
        _isCatching = false;
        _closeTimer = 0f;
        _startFallbackTimer = _startFallbackDuration;
        _targetDistanceBehind = _fallbackDistance + extraDistance;
        _currentDistanceBehind = Mathf.Max(_currentDistanceBehind, _targetDistanceBehind);
    }

    /// <summary>
    /// Called when Tenzin stumbles on an obstacle.
    /// </summary>
    public void OnPlayerStumbled()
    {
        if (_isCatching) return;

        // Skip initial start fallback and trigger close state
        _startFallbackTimer = _startFallbackDuration;
        _isClose = true;
        _closeTimer = _recoveryDuration;
        _targetDistanceBehind = _closeDistance;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLangurChatter();
        }
    }

    /// <summary>
    /// Called when Tenzin stumbles again while the langur is close.
    /// </summary>
    public void TriggerCatch()
    {
        _isCatching = true;
        _isClose = true;

        if (_animator != null)
        {
            _animator.IsLeaping = true;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLangurScreech();
        }
    }
}
