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

    [Header("Special Segment Prefabs")]
    [Tooltip("Wooden plank rope bridge over open air with cloud sea below.")]
    [SerializeField] private TrackSegment _ropeBridgePrefab;
    [Tooltip("Void gap in path across all lanes that must be jumped (only after 200m).")]
    [SerializeField] private TrackSegment _gapPrefab;
    [Tooltip("Golden temple rooftop with upturned eaves and butter lamps.")]
    [SerializeField] private TrackSegment _goldenRooftopPrefab;
    [Tooltip("Whitewashed stone arch with maroon band and low lintel requiring slide.")]
    [SerializeField] private TrackSegment _gateArchPrefab;
    [Tooltip("Chance to spawn a special segment when eligible.")]
    [Range(0f, 1f)]
    [SerializeField] private float _specialSegmentChance = 0.35f;
    [Tooltip("Minimum distance traveled before gaps can start spawning.")]
    [SerializeField] private float _gapMinDistance = 200f;

    [Header("Difficulty & Dynamic Obstacles")]
    [SerializeField] private Obstacle _lowBarrierPrefab;
    [SerializeField] private Obstacle _highBeamPrefab;
    [SerializeField] private Obstacle _laneBlockPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player to keep populated with track segments.")]
    [SerializeField] private float _viewDistanceAhead = 150f;
    [Tooltip("Distance behind player before segments are recycled/destroyed.")]
    [SerializeField] private float _destroyDistanceBehind = 20f;
    [Tooltip("Number of safe segments spawned at the very beginning.")]
    [SerializeField] private int _initialSafeSegments = 2;

    private PlayerController _playerController;
    private Vector3 _nextSpawnPosition = Vector3.zero;
    private Quaternion _nextSpawnRotation = Quaternion.identity;
    private int _straightSinceCorner = 0;
    private float _totalPathSpawned = 0f;
    private bool _lastSegmentWasSpecial = false;
    private bool _lastSegmentWasCorner = false;
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

    public TrackSegment RopeBridgePrefab
    {
        get => _ropeBridgePrefab;
        set => _ropeBridgePrefab = value;
    }

    public TrackSegment GapPrefab
    {
        get => _gapPrefab;
        set => _gapPrefab = value;
    }

    public TrackSegment GoldenRooftopPrefab
    {
        get => _goldenRooftopPrefab;
        set => _goldenRooftopPrefab = value;
    }

    public TrackSegment GateArchPrefab
    {
        get => _gateArchPrefab;
        set => _gateArchPrefab = value;
    }

    public float SpecialSegmentChance
    {
        get => _specialSegmentChance;
        set => _specialSegmentChance = value;
    }

    public float GapMinDistance
    {
        get => _gapMinDistance;
        set => _gapMinDistance = value;
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

        _lastSegmentWasCorner = false;
        _lastSegmentWasSpecial = false;

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
        // Rule: Never a special segment directly before a corner (if last was special, corner cannot spawn now)
        bool canSpawnCorner = _straightSinceCorner >= _minStraightBetweenCorners
            && !_lastSegmentWasSpecial
            && (_leftCornerPrefab != null || _rightCornerPrefab != null);

        if (canSpawnCorner && Random.value < _cornerChance)
        {
            _straightSinceCorner = 0;
            _lastSegmentWasCorner = true;
            _lastSegmentWasSpecial = false;

            if (_leftCornerPrefab != null && _rightCornerPrefab != null)
            {
                return Random.value < 0.5f ? _leftCornerPrefab : _rightCornerPrefab;
            }
            return _leftCornerPrefab != null ? _leftCornerPrefab : _rightCornerPrefab;
        }

        _straightSinceCorner++;

        // Rule: Never two special segments in a row, never directly after a corner,
        // and if a special segment spawns, the next segment cannot be a corner (enforced by canSpawnCorner checking !_lastSegmentWasSpecial).
        float currentDist = _playerController != null ? _playerController.DistanceTraveled : _totalPathSpawned;
        List<TrackSegment> specialCandidates = GetAvailableSpecialPrefabs(currentDist);

        bool canSpawnSpecial = !_lastSegmentWasSpecial
            && !_lastSegmentWasCorner
            && specialCandidates.Count > 0;

        if (canSpawnSpecial && Random.value < _specialSegmentChance)
        {
            _lastSegmentWasSpecial = true;
            _lastSegmentWasCorner = false;
            return specialCandidates[Random.Range(0, specialCandidates.Count)];
        }

        _lastSegmentWasSpecial = false;
        _lastSegmentWasCorner = false;
        return GetDifficultyWeightedStraightSegmentPrefab();
    }

    private List<TrackSegment> GetAvailableSpecialPrefabs(float currentDist)
    {
        List<TrackSegment> list = new List<TrackSegment>();
        if (_ropeBridgePrefab != null) list.Add(_ropeBridgePrefab);
        if (_goldenRooftopPrefab != null) list.Add(_goldenRooftopPrefab);
        if (_gateArchPrefab != null) list.Add(_gateArchPrefab);

        // Gap segment only spawns after 200m
        if (currentDist >= _gapMinDistance && _gapPrefab != null)
        {
            list.Add(_gapPrefab);
        }

        return list;
    }

    private bool IsSpecialSegment(TrackSegment segment, TrackSegment prefab)
    {
        if (segment != null && segment.IsSpecial) return true;
        if (prefab != null && (prefab == _ropeBridgePrefab || prefab == _gapPrefab || prefab == _goldenRooftopPrefab || prefab == _gateArchPrefab || prefab.IsSpecial)) return true;
        return false;
    }

    private TrackSegment GetDifficultyWeightedStraightSegmentPrefab()
    {
        if (_obstacleSegmentPrefabs == null || _obstacleSegmentPrefabs.Length == 0)
        {
            return _startSegmentPrefab;
        }

        float playerDist = _playerController != null ? _playerController.DistanceTraveled : 0f;

        // Weights: [0] Empty, [1] LowBarrier, [2] HighBeam, [3] LaneBlocks, [4] MixedHazard
        float wEmpty = Mathf.Lerp(45f, 2f, Mathf.Clamp01(playerDist / 600f));
        float wSingle = Mathf.Lerp(45f, 48f, Mathf.Clamp01(playerDist / 600f));
        float wMixed = Mathf.Lerp(10f, 50f, Mathf.Clamp01(playerDist / 600f));

        float totalWeight = wEmpty + wSingle + wMixed;
        float roll = Random.Range(0f, totalWeight);

        if (roll < wEmpty)
        {
            return _obstacleSegmentPrefabs[0]; // Empty
        }
        else if (roll < wEmpty + wSingle)
        {
            // Pick randomly between LowBarrier, HighBeam, and LaneBlocks
            int singleIdx = Random.Range(1, Mathf.Min(4, _obstacleSegmentPrefabs.Length));
            return _obstacleSegmentPrefabs[singleIdx];
        }
        else
        {
            // Pick denser mixed hazard
            int mixedIdx = Mathf.Min(4, _obstacleSegmentPrefabs.Length - 1);
            return _obstacleSegmentPrefabs[mixedIdx];
        }
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

        // Apply dynamic obstacle scaling for standard straight segments as distance increases
        if (!segment.IsCorner && !IsSpecialSegment(segment, prefab))
        {
            ApplyDynamicObstacles(segment);
        }
    }

    private void ApplyDynamicObstacles(TrackSegment segment)
    {
        float playerDist = _playerController != null ? _playerController.DistanceTraveled : 0f;
        if (playerDist < 160f) return;

        // Gather existing obstacles in this segment
        var existingObstacles = segment.GetComponentsInChildren<Obstacle>();
        List<Vector2> occupiedPositions = new List<Vector2>(); // (laneX, localZ)
        foreach (var obs in existingObstacles)
        {
            occupiedPositions.Add(new Vector2(obs.transform.localPosition.x, obs.transform.localPosition.z));
        }

        // Target obstacle count scales with distance
        int targetObstacles = 1;
        if (playerDist > 550f) targetObstacles = 3;
        else if (playerDist > 300f) targetObstacles = 2;

        if (existingObstacles.Length >= targetObstacles) return;

        // Available candidate lanes: -2 (Left), 0 (Mid), +2 (Right)
        float[] lanes = new float[] { -2f, 0f, 2f };
        float[] candidateZs = new float[] { 8f, 16f, 24f };

        int needed = targetObstacles - existingObstacles.Length;
        for (int i = 0; i < candidateZs.Length && needed > 0; i++)
        {
            float z = candidateZs[i];

            // Find which lanes already have obstacles near this Z (within 5.5 meters)
            HashSet<int> blockedLanes = new HashSet<int>();
            for (int p = 0; p < occupiedPositions.Count; p++)
            {
                if (Mathf.Abs(occupiedPositions[p].y - z) < 5.5f)
                {
                    int laneIdx = Mathf.RoundToInt(occupiedPositions[p].x / 2f) + 1; // 0, 1, or 2
                    blockedLanes.Add(laneIdx);
                }
            }

            // Guarantee: At least ONE lane MUST remain completely free at this Z cross section!
            // If already 2 lanes blocked, cannot add any more here.
            if (blockedLanes.Count >= 2) continue;

            // Pick a free lane
            List<int> openLanes = new List<int>();
            for (int l = 0; l < 3; l++)
            {
                if (!blockedLanes.Contains(l)) openLanes.Add(l);
            }

            // Only add if we can leave at least 1 lane open after adding
            if (openLanes.Count <= 1) continue;

            int chosenLaneIdx = openLanes[Random.Range(0, openLanes.Count)];
            float chosenLaneX = lanes[chosenLaneIdx];

            // Spawn dynamic obstacle
            Obstacle chosenPrefab = GetRandomObstaclePrefab();
            if (chosenPrefab != null)
            {
                Obstacle newObs = Instantiate(chosenPrefab, segment.transform);
                newObs.transform.localPosition = new Vector3(chosenLaneX, 0f, z);
                newObs.transform.localRotation = Quaternion.identity;

                occupiedPositions.Add(new Vector2(chosenLaneX, z));
                needed--;
            }
        }
    }

    private Obstacle GetRandomObstaclePrefab()
    {
        List<Obstacle> pool = new List<Obstacle>();
        if (_lowBarrierPrefab != null) pool.Add(_lowBarrierPrefab);
        if (_highBeamPrefab != null) pool.Add(_highBeamPrefab);
        if (_laneBlockPrefab != null) pool.Add(_laneBlockPrefab);

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private void RecyclePassedSegments()
    {
        if (_playerController == null && _player != null)
        {
            _playerController = _player.GetComponent<PlayerController>();
        }

        float playerDist = _playerController != null ? _playerController.DistanceTraveled : (_player != null ? _player.position.z : 0f);

        while (_activeSegments.Count > 0)
        {
            TrackSegment oldest = _activeSegments.Peek();
            if (oldest == null)
            {
                _activeSegments.Dequeue();
                continue;
            }

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

