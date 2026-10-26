using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class ButterLamp : MonoBehaviour
{
    private static readonly List<ButterLamp> s_activeLamps = new List<ButterLamp>(64);
    private static readonly ButterLamp[] s_nearest4 = new ButterLamp[4];
    private static readonly float[] s_nearestDistSq = new float[4];
    private static Transform s_cachedPlayer;
    private static int s_lastUpdateFrame = -1;

    [Header("Components")]
    [SerializeField] private Light _pointLight;
    [SerializeField] private Transform _flameTransform;

    [Header("Settings")]
    [SerializeField] private float _baseLightIntensity = 1.2f;
    [SerializeField] private float _flickerSpeed = 9f;
    [SerializeField] private float _flickerAmount = 0.25f;

    private float _flickerOffset;
    private Vector3 _originalFlameScale = Vector3.one;

    private void Awake()
    {
        _flickerOffset = Random.Range(0f, 100f);

        if (_flameTransform != null)
        {
            _originalFlameScale = _flameTransform.localScale;
        }

        if (_pointLight != null)
        {
            _pointLight.enabled = false; // Only nearest 4 to Tenzin enabled
            _pointLight.range = 3.0f;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                _pointLight.cullingMask &= ~(1 << playerLayer);
            }
        }
    }

    private void OnEnable()
    {
        if (!s_activeLamps.Contains(this))
        {
            s_activeLamps.Add(this);
        }
    }

    private void OnDisable()
    {
        s_activeLamps.Remove(this);
        if (_pointLight != null)
        {
            _pointLight.enabled = false;
        }
    }

    private void Update()
    {
        // 1. Centralized update of nearest 4 point lights once per frame
        if (Time.frameCount != s_lastUpdateFrame)
        {
            s_lastUpdateFrame = Time.frameCount;
            UpdateActivePointLights();
        }

        // 2. Flame flicker scale for emissive glow
        float noise = Mathf.Sin((Time.time + _flickerOffset) * _flickerSpeed);

        if (_pointLight != null && _pointLight.enabled)
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

    private static void UpdateActivePointLights()
    {
        if (s_cachedPlayer == null)
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) s_cachedPlayer = pc.transform;
            if (s_cachedPlayer == null) return;
        }

        Vector3 playerPos = s_cachedPlayer.position;

        for (int i = 0; i < 4; i++)
        {
            s_nearest4[i] = null;
            s_nearestDistSq[i] = float.MaxValue;
        }

        int count = s_activeLamps.Count;
        for (int i = 0; i < count; i++)
        {
            ButterLamp lamp = s_activeLamps[i];
            if (lamp == null || lamp._pointLight == null) continue;

            float distSq = (lamp.transform.position - playerPos).sqrMagnitude;

            // Maintain sorted top 4 nearest
            for (int k = 0; k < 4; k++)
            {
                if (distSq < s_nearestDistSq[k])
                {
                    for (int m = 3; m > k; m--)
                    {
                        s_nearest4[m] = s_nearest4[m - 1];
                        s_nearestDistSq[m] = s_nearestDistSq[m - 1];
                    }
                    s_nearest4[k] = lamp;
                    s_nearestDistSq[k] = distSq;
                    break;
                }
            }
        }

        // Enable point lights for at most 4 nearest lamps, disable for all others
        for (int i = 0; i < count; i++)
        {
            ButterLamp lamp = s_activeLamps[i];
            if (lamp == null || lamp._pointLight == null) continue;

            bool isNearest = (lamp == s_nearest4[0] || lamp == s_nearest4[1] || lamp == s_nearest4[2] || lamp == s_nearest4[3]);
            lamp._pointLight.enabled = isNearest;
        }
    }
}
