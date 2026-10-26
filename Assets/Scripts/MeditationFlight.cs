using System.Collections.Generic;
using UnityEngine;

public class MeditationFlight : MonoBehaviour
{
    public static MeditationFlight Instance { get; private set; }

    [Header("Earning & Thresholds")]
    [Tooltip("Lamps required for the very first meditation flight.")]
    [SerializeField] private int _initialThreshold = 200;
    [Tooltip("Initial lamp gap between flights.")]
    [SerializeField] private int _initialGap = 200;
    [Tooltip("Amount the gap increases with each flight (e.g. 200 -> 300 -> 400 ...).")]
    [SerializeField] private int _gapIncrement = 100;

    [Header("Flight Dynamics")]
    [Tooltip("Total duration of meditation flight in seconds.")]
    [SerializeField] private float _flightDuration = 6.0f;
    [Tooltip("Target flying height in meters above the path.")]
    [SerializeField] private float _flightHeight = 3.0f;
    [Tooltip("Duration in seconds to smoothly rise up to flight height.")]
    [SerializeField] private float _takeoffDuration = 0.5f;
    [Tooltip("Duration in seconds over which Tenzin floats back down to ground.")]
    [SerializeField] private float _landingDuration = 0.7f;
    [Tooltip("Run speed multiplier while flying (e.g. 1.6x).")]
    [SerializeField] private float _speedMultiplier = 1.6f;
    [Tooltip("Invincibility duration in seconds after landing.")]
    [SerializeField] private float _postLandingInvincibleDuration = 1.0f;

    [Header("Butter Lamp Spawning Along Path")]
    [Tooltip("Prefab for butter lamp / coin. Falls back to finding Coin.prefab if unassigned.")]
    [SerializeField] private GameObject _lampPrefab;
    [Tooltip("Distance in meters between spawned flight butter lamps.")]
    [SerializeField] private float _lampSpacing = 3.2f;
    [Tooltip("Distance ahead of player to maintain spawned lamps.")]
    [SerializeField] private float _lampSpawnAheadDistance = 22f;
    [Tooltip("Vertical offset adjustment for flight lamps relative to flight height.")]
    [SerializeField] private float _lampCollectionYOffset = 0.0f;

    [Header("Visual Effects - Aura & Trail")]
    [Tooltip("Color of the golden aura light and glow disc.")]
    [SerializeField] private Color _auraGlowColor = new Color(1.0f, 0.86f, 0.38f, 1.0f);
    [Tooltip("Maximum intensity of the aura point light.")]
    [SerializeField] private float _auraLightIntensity = 2.4f;
    [Tooltip("Range of the aura point light in meters.")]
    [SerializeField] private float _auraLightRange = 5.5f;
    [Tooltip("Particle rate per second for the trailing golden particles.")]
    [SerializeField] private float _particleRate = 38f;
    [Tooltip("Lifetime of trailing particles in seconds.")]
    [SerializeField] private float _particleLifetime = 0.55f;
    [Tooltip("Initial size of trailing particles.")]
    [SerializeField] private float _particleStartSize = 0.11f;

    [Header("Procedural Audio")]
    [Tooltip("Duration of the takeoff singing-bowl tone in seconds.")]
    [SerializeField] private float _singingBowlDuration = 3.6f;
    [Tooltip("Fundamental frequency of the deep singing-bowl tone (Hz).")]
    [SerializeField] private float _singingBowlFundamentalFreq = 220f;
    [Tooltip("Volume of the takeoff singing bowl.")]
    [SerializeField] private float _singingBowlVolume = 0.90f;
    [Tooltip("Fundamental frequency of the soft flight hum (Hz).")]
    [SerializeField] private float _humFundamentalFreq = 110f;
    [Tooltip("Volume of the looping flight hum.")]
    [SerializeField] private float _humVolume = 0.42f;

    // State tracking
    private int _lampsThisRun = 0;
    private int _currentGap = 200;
    private int _nextThreshold = 200;
    private int _lastFlightThreshold = 0;

    private bool _isFlying = false;
    private bool _isInvincible = false;
    private float _flightTimer = 0f;
    private float _currentHeightOffset = 0f;
    private bool _hasCrossfadedToRun = false;
    private float _postLandingTimer = 0f;

    // Flight lamp tracking
    private float _nextLampDistance = 0f;
    private readonly List<GameObject> _spawnedFlightLamps = new List<GameObject>(32);

    // Audio components & clips
    private AudioSource _takeoffAudioSource;
    private AudioSource _humAudioSource;
    private AudioClip _singingBowlClip;
    private AudioClip _humClip;

    // Visual effect components
    private Light _auraLight;
    private ParticleSystem _trailParticleSystem;
    private Transform _auraDisc;      // no longer created (was a flat yellow quad)
    private Material _auraDiscMat;

    [Header("Visual Effects - Enlightenment Glow & Rays")]
    [Tooltip("How strongly Tenzin's body glows gold while flying (emission strength).")]
    [SerializeField] private float _bodyGlowStrength = 1.6f;
    [Tooltip("How many light streaks burst out of Tenzin per second.")]
    [SerializeField] private float _rayRate = 160f;
    [Tooltip("Speed range of the streaks (m/s).")]
    [SerializeField] private Vector2 _raySpeed = new Vector2(3.5f, 8f);
    [Tooltip("How long each streak lives (seconds).")]
    [SerializeField] private Vector2 _rayLifetime = new Vector2(0.25f, 0.55f);
    [Tooltip("Thickness range of each streak (m).")]
    [SerializeField] private Vector2 _rayWidth = new Vector2(0.025f, 0.06f);
    [Tooltip("How long the streaks look: higher = longer rays.")]
    [SerializeField] private float _rayStretch = 0.11f;
    [Tooltip("Size of the soft halo behind Tenzin in meters.")]
    [SerializeField] private float _haloSize = 2.0f;
    [Tooltip("Height of the glow centre above Tenzin's feet while meditating.")]
    [SerializeField] private float _glowCenterHeight = 0.75f;

    private Transform _raysRoot;
    private Transform _raysSpin;
    private ParticleSystem _rayBurst;
    private Material _rayMat;
    private Material _haloMat;
    private float _visualIntensity;
    private readonly System.Collections.Generic.List<Material> _bodyMaterials = new System.Collections.Generic.List<Material>();
    private readonly System.Collections.Generic.List<Color> _bodyOriginalEmission = new System.Collections.Generic.List<Color>();
    private readonly System.Collections.Generic.List<bool> _bodyHadEmission = new System.Collections.Generic.List<bool>();

    // Player reference
    private PlayerController _player;

    public bool IsFlying => _isFlying;
    public bool IsInvincible => _isInvincible;
    public float SpeedMultiplier => _speedMultiplier;
    public float CurrentHeightOffset => _currentHeightOffset;
    public int LampsThisRun => _lampsThisRun;
    public int CurrentInCycle => Mathf.Max(0, _lampsThisRun - _lastFlightThreshold);
    public int CycleTarget => _currentGap;
    public float FlightProgress => _flightDuration > 0f ? Mathf.Clamp01(_flightTimer / _flightDuration) : 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        ResetRun();
        SetupAudioSources();
        SetupVisualEffects();

#if UNITY_EDITOR
        if (_lampPrefab == null)
        {
            _lampPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Coin.prefab");
        }
#endif
    }

    private void Start()
    {
        if (_player == null)
        {
            _player = GetComponent<PlayerController>();
            if (_player == null)
            {
                _player = FindAnyObjectByType<PlayerController>();
            }
        }

        if (_lampPrefab == null)
        {
            var coin = FindAnyObjectByType<Coin>(FindObjectsInactive.Include);
            if (coin != null)
            {
                _lampPrefab = coin.gameObject;
            }
        }

        UpdateHUD();
    }

    /// <summary>
    /// Resets lamp counter and threshold gaps at the start of a run.
    /// </summary>
    public void ResetRun()
    {
        _lampsThisRun = 0;
        _currentGap = _initialGap;
        _nextThreshold = _initialThreshold;
        _lastFlightThreshold = 0;

        _isFlying = false;
        _isInvincible = false;
        _flightTimer = 0f;
        _currentHeightOffset = 0f;
        _hasCrossfadedToRun = false;
        _postLandingTimer = 0f;

        ClearFlightLamps();

        if (_humAudioSource != null && _humAudioSource.isPlaying)
        {
            _humAudioSource.Stop();
        }

        SetVisualsActive(false);
        UpdateHUD();
    }

    /// <summary>
    /// Called when butter lamps are collected.
    /// </summary>
    public void OnLampCollected(int amount = 1)
    {
        if (amount <= 0) return;

        _lampsThisRun += amount;

        // Check if next threshold is reached
        if (_lampsThisRun >= _nextThreshold)
        {
            _lastFlightThreshold = _nextThreshold;
            _currentGap += _gapIncrement;
            _nextThreshold = _lastFlightThreshold + _currentGap;

            TriggerFlight();
        }

        UpdateHUD();
    }

    public void TriggerFlight()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>();
            if (_player == null) return;
        }

        if (_player.IsDead) return;

        _isFlying = true;
        _isInvincible = true;
        _flightTimer = 0f;
        _currentHeightOffset = 0f;
        _hasCrossfadedToRun = false;
        _postLandingTimer = 0f;

        // Reset next lamp spawn forward anchor
        _nextLampDistance = _player.DistanceTraveled + 6f;

        // Crossfade to Meditate state
        _player.PlayMeditateAnimation();

        // Monkey chaser drops back out of view
        if (LangurChaser.Instance != null)
        {
            LangurChaser.Instance.DropBackOutOfView();
        }

        // Play procedural audio
        PlayTakeoffAudio();
        StartFlightHumAudio();

        // Turn on golden visuals
        SetVisualsActive(true);

        UpdateHUD();
    }

    private void Update()
    {
        if (_player == null) return;

        // 1. Post-landing invincibility tick
        if (!_isFlying && _isInvincible)
        {
            _postLandingTimer -= Time.deltaTime;
            if (_postLandingTimer <= 0f)
            {
                _isInvincible = false;
            }
        }

        // 2. Flight active update
        if (_isFlying)
        {
            UpdateFlight();
        }

        // 3. Clean up passed or collected flight lamps
        UpdateFlightLamps();
    }

    private void UpdateFlight()
    {
        _flightTimer += Time.deltaTime;

        // Height interpolation
        float cruisingStart = _takeoffDuration;
        float landingStart = Mathf.Max(cruisingStart, _flightDuration - _landingDuration);

        if (_flightTimer < cruisingStart)
        {
            // Phase 1: Take-off rise (smoothstep 0 -> _flightHeight)
            float t = Mathf.Clamp01(_flightTimer / _takeoffDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            _currentHeightOffset = smooth * _flightHeight;

            // Fade in aura light and hum
            UpdateVisualIntensity(smooth);
            UpdateHumVolume(smooth);
        }
        else if (_flightTimer < landingStart)
        {
            // Phase 2: Cruising at flight height
            _currentHeightOffset = _flightHeight;
            UpdateVisualIntensity(1f);
            UpdateHumVolume(1f);

            // Continuously spawn line of lamps ahead along the path
            SpawnLampsAhead();
        }
        else if (_flightTimer < _flightDuration)
        {
            // Phase 3: Landing float down (smoothstep _flightHeight -> 0)
            if (!_hasCrossfadedToRun)
            {
                _hasCrossfadedToRun = true;
                _player.PlayRunAnimation(0.25f);
            }

            float landingElapsed = _flightTimer - landingStart;
            float t = Mathf.Clamp01(landingElapsed / _landingDuration);
            float smooth = Mathf.SmoothStep(1f, 0f, t);
            _currentHeightOffset = smooth * _flightHeight;

            // Fade out aura light and hum
            UpdateVisualIntensity(smooth);
            UpdateHumVolume(smooth);
        }
        else
        {
            // Flight completed!
            CompleteFlight();
        }

    }

    private void CompleteFlight()
    {
        _isFlying = false;
        _currentHeightOffset = 0f;

        // Ensure grounded and running
        _player.OnMeditationFlightLanded();

        // 1 second post-landing invulnerability
        _isInvincible = true;
        _postLandingTimer = _postLandingInvincibleDuration;

        // Stop hum audio & particle trail
        if (_humAudioSource != null && _humAudioSource.isPlaying)
        {
            _humAudioSource.Stop();
        }

        SetVisualsActive(false);
        UpdateHUD();
    }

    private void SpawnLampsAhead()
    {
        if (_lampPrefab == null || _player == null) return;

        float maxSpawnDistance = _player.DistanceTraveled + _lampSpawnAheadDistance;

        while (_nextLampDistance < maxSpawnDistance)
        {
            Vector3 spawnPos = CalculateFlightPathPosition(_nextLampDistance, _player.CurrentLane);
            GameObject lamp = Instantiate(_lampPrefab, spawnPos, Quaternion.identity);

            // Ensure active and tracking
            lamp.SetActive(true);
            _spawnedFlightLamps.Add(lamp);

            _nextLampDistance += _lampSpacing;
        }
    }

    private Vector3 CalculateFlightPathPosition(float distanceTraveledTarget, int laneIndex)
    {
        float lateralOffset = laneIndex * 2.0f;
        Vector3 playerPos = _player.transform.position;
        Vector3 forward = _player.ForwardDirection;
        Vector3 right = _player.RightDirection;

        float deltaDist = distanceTraveledTarget - _player.DistanceTraveled;

        // Check if there is an upcoming corner between player and spawn point
        CornerSegment corner = FindCornerAhead(playerPos, forward, deltaDist);
        if (corner != null && !corner.HasTurned)
        {
            Vector3 pivot = corner.PivotPosition;
            float distToPivot = Vector3.Dot(pivot - playerPos, forward);
            if (distToPivot > 0f && distToPivot < deltaDist)
            {
                float distPastPivot = deltaDist - distToPivot;
                Vector3 newFwd = corner.NewForwardDirection;
                Vector3 newRight = corner.NewRightDirection;

                Vector3 cornerPos = pivot + newFwd * distPastPivot + newRight * lateralOffset;
                cornerPos.y = _player.GroundY + _flightHeight + _lampCollectionYOffset;
                return cornerPos;
            }
        }

        Vector3 straightPos = playerPos + forward * deltaDist;
        straightPos.y = _player.GroundY + _flightHeight + _lampCollectionYOffset;
        return straightPos;
    }

    private CornerSegment FindCornerAhead(Vector3 fromPos, Vector3 forward, float maxDist)
    {
        var corners = FindObjectsByType<CornerSegment>(FindObjectsInactive.Exclude);
        CornerSegment bestCorner = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < corners.Length; i++)
        {
            var c = corners[i];
            if (c == null || c.HasTurned) continue;

            float dot = Vector3.Dot(c.PivotPosition - fromPos, forward);
            if (dot > 0f && dot < maxDist && dot < bestDist)
            {
                bestDist = dot;
                bestCorner = c;
            }
        }

        return bestCorner;
    }

    private void UpdateFlightLamps()
    {
        if (_player == null) return;

        Vector3 playerPos = _player.transform.position;
        Vector3 forward = _player.ForwardDirection;

        for (int i = _spawnedFlightLamps.Count - 1; i >= 0; i--)
        {
            GameObject lamp = _spawnedFlightLamps[i];
            if (lamp == null)
            {
                _spawnedFlightLamps.RemoveAt(i);
                continue;
            }

            // If collected, coin disables itself
            if (!lamp.activeSelf)
            {
                Destroy(lamp);
                _spawnedFlightLamps.RemoveAt(i);
                continue;
            }

            // If passed behind player by more than 6m
            float distAlongFwd = Vector3.Dot(lamp.transform.position - playerPos, forward);
            if (distAlongFwd < -6.0f)
            {
                Destroy(lamp);
                _spawnedFlightLamps.RemoveAt(i);
            }
        }
    }

    private void ClearFlightLamps()
    {
        for (int i = 0; i < _spawnedFlightLamps.Count; i++)
        {
            if (_spawnedFlightLamps[i] != null)
            {
                Destroy(_spawnedFlightLamps[i]);
            }
        }
        _spawnedFlightLamps.Clear();
    }

    private void UpdateHUD()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMeditationBar(CurrentInCycle, CycleTarget, _isFlying);
        }
    }

    #region Procedural Audio Setup & Playback

    private void SetupAudioSources()
    {
        _takeoffAudioSource = gameObject.AddComponent<AudioSource>();
        _takeoffAudioSource.playOnAwake = false;
        _takeoffAudioSource.spatialBlend = 0f; // 2D clean stereo

        _humAudioSource = gameObject.AddComponent<AudioSource>();
        _humAudioSource.playOnAwake = false;
        _humAudioSource.loop = true;
        _humAudioSource.spatialBlend = 0f;

        const int sampleRate = 44100;
        _singingBowlClip = GenerateDeepSingingBowlClip(sampleRate);
        _humClip = GenerateFlightHumClip(sampleRate);
    }

    private void PlayTakeoffAudio()
    {
        if (_takeoffAudioSource == null || _singingBowlClip == null) return;
        if (AudioManager.Instance != null && AudioManager.Instance.IsMuted) return;

        _takeoffAudioSource.volume = _singingBowlVolume;
        _takeoffAudioSource.PlayOneShot(_singingBowlClip, _singingBowlVolume);
    }

    private void StartFlightHumAudio()
    {
        if (_humAudioSource == null || _humClip == null) return;

        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        _humAudioSource.mute = muted;
        _humAudioSource.clip = _humClip;
        _humAudioSource.volume = 0f;
        _humAudioSource.Play();
    }

    private void UpdateHumVolume(float normalizedVolume)
    {
        if (_humAudioSource == null) return;
        bool muted = AudioManager.Instance != null && AudioManager.Instance.IsMuted;
        _humAudioSource.mute = muted;
        _humAudioSource.volume = muted ? 0f : (_humVolume * normalizedVolume);
    }

    private AudioClip GenerateDeepSingingBowlClip(int sampleRate)
    {
        int sampleCount = Mathf.FloorToInt(sampleRate * _singingBowlDuration);
        float[] samples = new float[sampleCount];

        // Tibetan singing bowl acoustic modes (deep resonant meditation bowl):
        // Fundamental: 220Hz (A3)
        // Overtone mode with acoustic beating: 587.3Hz & 589.5Hz (2.2Hz calming shimmer)
        // Upper harmonic chime: 1175Hz
        float f0 = _singingBowlFundamentalFreq;
        float f1 = f0 * 2.67f;
        float f2 = f1 + 2.2f; // Acoustic beating
        float f3 = f0 * 5.34f;

        float phase0 = 0f;
        float phase1 = 0f;
        float phase2 = 0f;
        float phase3 = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            phase0 += 2f * Mathf.PI * f0 / sampleRate;
            phase1 += 2f * Mathf.PI * f1 / sampleRate;
            phase2 += 2f * Mathf.PI * f2 / sampleRate;
            phase3 += 2f * Mathf.PI * f3 / sampleRate;

            // Soft padded mallet strike attack over 28ms
            float attack = Mathf.Clamp01(t / 0.028f);

            // Resonant long modes with gentle decay
            float env0 = Mathf.Exp(-t * 0.85f);
            float envBeating = Mathf.Exp(-t * 1.15f);
            float envHigh = Mathf.Exp(-t * 3.5f);

            float s0 = Mathf.Sin(phase0) * env0 * 0.60f;
            float s1 = Mathf.Sin(phase1) * envBeating * 0.25f;
            float s2 = Mathf.Sin(phase2) * envBeating * 0.25f;
            float s3 = Mathf.Sin(phase3) * envHigh * 0.12f;

            float sample = (s0 + s1 + s2 + s3) * attack;
            samples[i] = Mathf.Clamp(sample * 0.90f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_DeepSingingBowl", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateFlightHumClip(int sampleRate)
    {
        // 2.0-second seamless looping buffer
        float duration = 2.0f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Fundamental drone: 110Hz (A2).
        // Exactly 220 integer cycles in 2 seconds -> zero phase discontinuity across the loop seam.
        float f0 = _humFundamentalFreq;
        float f1 = f0 * 2.0f;
        float f2 = f0 * 3.0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float phase0 = 2f * Mathf.PI * f0 * t;
            float phase1 = 2f * Mathf.PI * f1 * t;
            float phase2 = 2f * Mathf.PI * f2 * t;

            // Gentle 1.0Hz tremolo (exactly 2 full cycles in 2.0s -> seamless)
            float tremolo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 1.0f * t);

            // Warm resonant sine fundamental + subtle harmonics
            float s0 = Mathf.Sin(phase0) * 0.68f;
            float s1 = Mathf.Sin(phase1) * 0.22f;
            float s2 = Mathf.Sin(phase2) * 0.10f;

            float sample = (s0 + s1 + s2) * tremolo;
            samples[i] = Mathf.Clamp(sample * 0.85f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_FlightHum", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    #endregion

    #region Visual Effects Setup

    private void SetupVisualEffects()
    {
        // 1. Point Light for soft golden aura
        GameObject lightObj = new GameObject("MeditationAuraLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0f, 0.9f, 0f);

        _auraLight = lightObj.AddComponent<Light>();
        _auraLight.type = LightType.Point;
        _auraLight.color = _auraGlowColor;
        _auraLight.range = _auraLightRange;
        _auraLight.intensity = 0f;
        _auraLight.enabled = false;

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            _auraLight.cullingMask &= ~(1 << playerLayer);
        }

        // 2. Soft golden particle trail
        GameObject trailObj = new GameObject("MeditationTrailParticles");
        trailObj.transform.SetParent(transform, false);
        trailObj.transform.localPosition = new Vector3(0f, 0.7f, -0.2f);

        _trailParticleSystem = trailObj.AddComponent<ParticleSystem>();
        var main = _trailParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = true;
        main.startLifetime = _particleLifetime;
        main.startSpeed = 1.6f;
        main.startSize = _particleStartSize;
        main.startColor = _auraGlowColor;
        main.gravityModifier = -0.1f; // Gentle golden float

        var emission = _trailParticleSystem.emission;
        emission.rateOverTime = _particleRate;
        emission.enabled = false;

        var shape = _trailParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        var col = _trailParticleSystem.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.92f, 0.45f), 0.0f), new GradientColorKey(new Color(1f, 0.70f, 0.15f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.8f, 0.4f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        col.color = grad;

        var sizeOverLife = _trailParticleSystem.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 1.0f);
        sizeCurve.AddKey(0.4f, 1.2f);
        sizeCurve.AddKey(1.0f, 0.0f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        var renderer = trailObj.GetComponent<ParticleSystemRenderer>();
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
        if (particleShader != null)
        {
            Material trailMat = new Material(particleShader);
            trailMat.name = "FX_MeditationTrailMat";
            trailMat.SetFloat("_Surface", 1f); // Transparent
            trailMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            trailMat.SetFloat("_Blend", 2f);   // Additive
            trailMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            trailMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            trailMat.SetInt("_ZWrite", 0);
            trailMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            renderer.material = trailMat;
        }

        // Give the trail particles a soft round texture so they aren't drawn as squares.
        Texture2D softCircle = CreateSoftCircleTexture(64);
        if (renderer.sharedMaterial != null)
        {
            renderer.sharedMaterial.SetTexture("_BaseMap", softCircle);
            renderer.sharedMaterial.SetTexture("_MainTex", softCircle);
        }

        // 3. Enlightenment rays and halo behind Tenzin (replaces the old flat disc).
        if (particleShader != null)
        {
            _raysRoot = new GameObject("MeditationEnlightenmentRays").transform;
            _raysRoot.SetParent(transform, false);
            _raysRoot.localPosition = new Vector3(0f, _glowCenterHeight, 0f);

            _raysSpin = new GameObject("Spin").transform;
            _raysSpin.SetParent(_raysRoot, false);

            // Soft halo
            _haloMat = CreateAdditiveMaterial(particleShader, softCircle, "FX_MeditationHalo");
            GameObject halo = CreateFxQuad("Halo", _raysRoot, _haloMat);
            halo.transform.localScale = new Vector3(_haloSize, _haloSize, 1f);

            // Many small light streaks bursting out of his body in random directions.
            GameObject burstObj = new GameObject("EnlightenmentStreaks");
            burstObj.transform.SetParent(transform, false);
            burstObj.transform.localPosition = new Vector3(0f, _glowCenterHeight, 0f);
            _rayBurst = burstObj.AddComponent<ParticleSystem>();
            _rayBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var bm = _rayBurst.main;
            bm.playOnAwake = false;
            bm.loop = true;
            bm.simulationSpace = ParticleSystemSimulationSpace.Local; // streaks travel with him
            bm.startLifetime = new ParticleSystem.MinMaxCurve(_rayLifetime.x, _rayLifetime.y);
            bm.startSpeed = new ParticleSystem.MinMaxCurve(_raySpeed.x, _raySpeed.y);
            bm.startSize = new ParticleSystem.MinMaxCurve(_rayWidth.x, _rayWidth.y);
            bm.startColor = new ParticleSystem.MinMaxGradient(_auraGlowColor, new Color(1f, 0.98f, 0.85f, 1f));
            bm.maxParticles = 500;

            var be = _rayBurst.emission;
            be.rateOverTime = 0f;

            var bs = _rayBurst.shape;
            bs.shapeType = ParticleSystemShapeType.Sphere;
            bs.radius = 0.2f;
            bs.randomDirectionAmount = 1f;

            var bc = _rayBurst.colorOverLifetime;
            bc.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0.7f, 0.6f), new GradientAlphaKey(0f, 1f) });
            bc.color = g;

            var br = burstObj.GetComponent<ParticleSystemRenderer>();
            br.renderMode = ParticleSystemRenderMode.Stretch;
            br.velocityScale = _rayStretch;
            br.lengthScale = 2f;
            br.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            br.receiveShadows = false;
            _rayMat = CreateAdditiveMaterial(particleShader, CreateStreakTexture(16, 64), "FX_MeditationStreaks");
            _rayMat.SetColor("_BaseColor", Color.white);
            br.sharedMaterial = _rayMat;

            _raysRoot.gameObject.SetActive(false);
        }

        CacheBodyMaterials();
    }

    private GameObject CreateFxQuad(string name, Transform parent, Material mat)
    {
        GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = name;
        var c = q.GetComponent<Collider>();
        if (c != null) Destroy(c);
        q.transform.SetParent(parent, false);
        var r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return q;
    }

    private Material CreateAdditiveMaterial(Shader shader, Texture2D tex, string name)
    {
        Material m = new Material(shader);
        m.name = name;
        m.SetTexture("_BaseMap", tex);
        m.SetTexture("_MainTex", tex);
        m.SetColor("_BaseColor", new Color(_auraGlowColor.r, _auraGlowColor.g, _auraGlowColor.b, 0f));
        m.SetFloat("_Surface", 1f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetFloat("_Blend", 2f); // Additive
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        m.SetInt("_ZWrite", 0);
        m.SetFloat("_Cull", 0f); // visible from both sides
        m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return m;
    }

    private static Texture2D CreateSoftCircleTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float c = (size - 1) * 0.5f;
        var px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - d);
                px[y * size + x] = new Color(1f, 1f, 1f, a * a);
            }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // Bright at the bottom (centre of the burst), fading to nothing at the tip and the sides.
    private static Texture2D CreateRayTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;
                float across = 1f - Mathf.Abs(u * 2f - 1f);
                float along = Mathf.Pow(1f - v, 1.6f);
                px[y * width + x] = new Color(1f, 1f, 1f, across * across * along);
            }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // A thin streak, brightest in the middle, fading at both ends and the sides.
    private static Texture2D CreateStreakTexture(int width, int height)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;
                float across = 1f - Mathf.Abs(u * 2f - 1f);
                float along = Mathf.Sin(v * Mathf.PI);
                px[y * width + x] = new Color(1f, 1f, 1f, across * across * along);
            }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    // Makes per-instance copies of Tenzin's materials so the gold glow only affects him.
    private void CacheBodyMaterials()
    {
        _bodyMaterials.Clear();
        _bodyOriginalEmission.Clear();
        _bodyHadEmission.Clear();
        foreach (var r in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            foreach (var m in r.materials)
            {
                if (m == null || !m.HasProperty("_EmissionColor")) continue;
                _bodyMaterials.Add(m);
                _bodyOriginalEmission.Add(m.GetColor("_EmissionColor"));
                _bodyHadEmission.Add(m.IsKeywordEnabled("_EMISSION"));
            }
        }
    }

    private void SetBodyGlow(float intensity)
    {
        for (int i = 0; i < _bodyMaterials.Count; i++)
        {
            Material m = _bodyMaterials[i];
            if (m == null) continue;
            if (intensity > 0.001f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", _auraGlowColor * (_bodyGlowStrength * intensity));
            }
            else
            {
                m.SetColor("_EmissionColor", _bodyOriginalEmission[i]);
                if (!_bodyHadEmission[i]) m.DisableKeyword("_EMISSION");
            }
        }
    }

    private void LateUpdate()
    {
        if (_raysRoot == null || !_raysRoot.gameObject.activeSelf) return;
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Face the camera, and sit just behind Tenzin so the rays shine out from behind him.
            Vector3 center = transform.position + Vector3.up * _glowCenterHeight;
            _raysRoot.position = center + cam.transform.forward * 0.45f;
            _raysRoot.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
        }
        // Gentle pulse on the halo
        float pulse = 1f + 0.06f * Mathf.Sin(Time.time * 3f);
        _raysRoot.localScale = new Vector3(pulse, pulse, 1f);
    }

    private void SetVisualsActive(bool active)
    {
        if (_auraLight != null)
        {
            _auraLight.enabled = active;
            _auraLight.intensity = active ? _auraLightIntensity : 0f;
        }

        if (_trailParticleSystem != null)
        {
            var emission = _trailParticleSystem.emission;
            emission.enabled = active;
            if (active)
            {
                _trailParticleSystem.Play();
            }
            else
            {
                _trailParticleSystem.Stop();
            }
        }

        if (_raysRoot != null)
        {
            _raysRoot.gameObject.SetActive(active);
        }

        if (_rayBurst != null)
        {
            if (active) _rayBurst.Play();
            else _rayBurst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (!active)
        {
            UpdateVisualIntensity(0f);
        }
    }

    private void UpdateVisualIntensity(float normalizedIntensity)
    {
        if (_auraLight != null && _auraLight.enabled)
        {
            _auraLight.intensity = _auraLightIntensity * normalizedIntensity;
        }

        _visualIntensity = normalizedIntensity;
        Color c = _auraGlowColor;
        if (_rayBurst != null)
        {
            var em = _rayBurst.emission;
            em.rateOverTime = _rayRate * normalizedIntensity;
        }
        if (_haloMat != null) _haloMat.SetColor("_BaseColor", new Color(c.r, c.g, c.b, 0.5f * normalizedIntensity));
        SetBodyGlow(normalizedIntensity);
    }

    #endregion
}
