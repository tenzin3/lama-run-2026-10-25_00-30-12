using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Player Target")]
    [SerializeField] private Transform _player;

    [Header("Straight Segment Prefabs")]
    [Tooltip("Starting safe straight segment without obstacles.")]
    [SerializeField] private TrackSegment _startSegmentPrefab;
    [Tooltip("Pool of straight segments containing hazards, obstacles, and coin patterns.")]
    [SerializeField] private TrackSegment[] _obstacleSegmentPrefabs;

    [Header("Corner Segment Prefabs")]
    [Tooltip("Corner segment turning 90 degrees Left.")]
    [SerializeField] private CornerSegment _leftCornerPrefab;
    [Tooltip("Corner segment turning 90 degrees Right.")]
    [SerializeField] private CornerSegment _rightCornerPrefab;
    [Tooltip("Chance to spawn a corner when eligible.")]
    [Range(0f, 1f)]
    [SerializeField] private float _cornerChance = 0.35f;
    [Tooltip("Minimum straight segments between corners.")]
    [SerializeField] private int _minStraightBetweenCorners = 3;

    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player to keep populated with track segments.")]
    [SerializeField] private float _viewDistanceAhead = 150f;
    [Tooltip("Distance behind player before segments are recycled/destroyed.")]
    [SerializeField] private float _destroyDistanceBehind = 35f;
    [Tooltip("Number of safe segments spawned at the very beginning.")]
    [SerializeField] private int _initialSafeSegments = 2;

    private PlayerController _playerController;
    private Vector3 _nextSpawnPosition = Vector3.zero;
    private Quaternion _nextSpawnRotation = Quaternion.identity;
    private int _straightSinceCorner = 0;
    private float _totalPathSpawned = 0f;
    private readonly Queue<TrackSegment> _activeSegments = new Queue<TrackSegment>();

    public CornerSegment LeftCornerPrefab
    {
        get => _leftCornerPrefab;
        set => _leftCornerPrefab = value;
    }

    public CornerSegment RightCornerPrefab
    {
        get => _rightCornerPrefab;
        set => _rightCornerPrefab = value;
    }

    private void Start()
    {
        if (_player == null)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) _player = pc.transform;
        }

        if (_player != null)
        {
            _playerController = _player.GetComponent<PlayerController>();
        }

        // Spawn initial safe straight segments
        for (int i = 0; i < _initialSafeSegments; i++)
        {
            SpawnSegment(_startSegmentPrefab);
            _straightSinceCorner++;
        }

        // Populate initial distance ahead
        EnsureSufficientTrack();
    }

    private void Update()
    {
        if (_player == null) return;

        EnsureSufficientTrack();
        RecyclePassedSegments();
    }

    private void EnsureSufficientTrack()
    {
        if (_player == null) return;

        float playerDist = _playerController != null ? _playerController.DistanceTraveled : _player.position.z;

        while (_totalPathSpawned - playerDist < _viewDistanceAhead)
        {
            TrackSegment prefabToSpawn = ChooseNextSegmentPrefab();
            if (prefabToSpawn != null)
            {
                SpawnSegment(prefabToSpawn);
            }
            else
            {
                break;
            }
        }
    }

    private TrackSegment ChooseNextSegmentPrefab()
    {
        bool canSpawnCorner = _straightSinceCorner >= _minStraightBetweenCorners
            && (_leftCornerPrefab != null || _rightCornerPrefab != null);

        if (canSpawnCorner && Random.value < _cornerChance)
        {
            _straightSinceCorner = 0;

            if (_leftCornerPrefab != null && _rightCornerPrefab != null)
            {
                return Random.value < 0.5f ? _leftCornerPrefab : _rightCornerPrefab;
            }
            return _leftCornerPrefab != null ? _leftCornerPrefab : _rightCornerPrefab;
        }

        _straightSinceCorner++;
        return GetRandomStraightSegmentPrefab();
    }

    private TrackSegment GetRandomStraightSegmentPrefab()
    {
        if (_obstacleSegmentPrefabs == null || _obstacleSegmentPrefabs.Length == 0)
        {
            return _startSegmentPrefab;
        }

        int index = Random.Range(0, _obstacleSegmentPrefabs.Length);
        return _obstacleSegmentPrefabs[index];
    }

    private void SpawnSegment(TrackSegment prefab)
    {
        if (prefab == null) return;

        TrackSegment segment = Instantiate(prefab, _nextSpawnPosition, _nextSpawnRotation, transform);
        _activeSegments.Enqueue(segment);

        _totalPathSpawned += segment.Length;
        segment.PathEndDistance = _totalPathSpawned;

        _nextSpawnPosition = segment.EndPosition;
        _nextSpawnRotation = segment.EndRotation;
    }

    private void RecyclePassedSegments()
    {
        float playerDist = _playerController != null ? _playerController.DistanceTraveled : _player.position.z;

        while (_activeSegments.Count > 0)
        {
            TrackSegment oldest = _activeSegments.Peek();
            if (playerDist - oldest.PathEndDistance > _destroyDistanceBehind)
            {
                _activeSegments.Dequeue();
                Destroy(oldest.gameObject);
            }
            else
            {
                break;
            }
        }
    }
}

