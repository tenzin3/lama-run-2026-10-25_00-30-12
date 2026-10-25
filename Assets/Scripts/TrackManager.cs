using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    [Header("Player Target")]
    [SerializeField] private Transform _player;

    [Header("Segment Prefabs")]
    [Tooltip("Starting safe straight segment without obstacles.")]
    [SerializeField] private TrackSegment _startSegmentPrefab;
    [Tooltip("Pool of segments containing hazards, obstacles, and coin patterns.")]
    [SerializeField] private TrackSegment[] _obstacleSegmentPrefabs;

    [Header("Spawn Settings")]
    [Tooltip("Distance ahead of player to keep populated with track segments.")]
    [SerializeField] private float _viewDistanceAhead = 150f;
    [Tooltip("Distance behind player before segments are recycled/destroyed.")]
    [SerializeField] private float _destroyDistanceBehind = 35f;
    [Tooltip("Number of safe segments spawned at the very beginning.")]
    [SerializeField] private int _initialSafeSegments = 2;

    private float _nextSpawnZ = 0f;
    private readonly Queue<TrackSegment> _activeSegments = new Queue<TrackSegment>();

    private void Start()
    {
        if (_player == null)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) _player = pc.transform;
        }

        // Spawn initial safe segments
        for (int i = 0; i < _initialSafeSegments; i++)
        {
            SpawnSegment(_startSegmentPrefab);
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

        while (_nextSpawnZ - _player.position.z < _viewDistanceAhead)
        {
            TrackSegment prefabToSpawn = GetRandomSegmentPrefab();
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

    private TrackSegment GetRandomSegmentPrefab()
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

        Vector3 spawnPos = new Vector3(0f, 0f, _nextSpawnZ);
        TrackSegment segment = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        _activeSegments.Enqueue(segment);
        _nextSpawnZ += segment.Length;
    }

    private void RecyclePassedSegments()
    {
        while (_activeSegments.Count > 0)
        {
            TrackSegment oldest = _activeSegments.Peek();
            if (_player.position.z - oldest.EndZ > _destroyDistanceBehind)
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
