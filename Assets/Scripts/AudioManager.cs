using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const string PREF_MUTED = "TenzinRun_AudioMuted";

    private AudioClip _jumpClip;
    private AudioClip _slideClip;
    private AudioClip _lampChimeClip;
    private AudioClip _turnWhooshClip;
    private AudioClip _crashThudClip;
    private AudioClip _singingBowlClip;
    private AudioClip _langurChatterClip;
    private AudioClip _langurScreechClip;

    private AudioSource _sfxSource;
    private AudioSource _bellSource;

    private bool _isMuted = false;
    public bool IsMuted => _isMuted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        _bellSource = gameObject.AddComponent<AudioSource>();
        _bellSource.playOnAwake = false;

        _isMuted = PlayerPrefs.GetInt(PREF_MUTED, 0) == 1;
        ApplyMuteState();

        GenerateProceduralClips();
    }

    public void SetMuted(bool muted)
    {
        _isMuted = muted;
        PlayerPrefs.SetInt(PREF_MUTED, _isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMuteState();
    }

    public bool ToggleMute()
    {
        SetMuted(!_isMuted);
        return _isMuted;
    }

    private void ApplyMuteState()
    {
        AudioListener.volume = _isMuted ? 0f : 1f;
    }

    public void PlayJump()
    {
        if (_isMuted || _jumpClip == null) return;
        _sfxSource.PlayOneShot(_jumpClip, 0.75f);
    }

    public void PlaySlide()
    {
        if (_isMuted || _slideClip == null) return;
        _sfxSource.PlayOneShot(_slideClip, 0.7f);
    }

    public void PlayLampPickup()
    {
        if (_isMuted || _lampChimeClip == null) return;
        _sfxSource.PlayOneShot(_lampChimeClip, 0.85f);
    }

    public void PlayTurnWhoosh()
    {
        if (_isMuted || _turnWhooshClip == null) return;
        _sfxSource.PlayOneShot(_turnWhooshClip, 0.8f);
    }

    public void PlayCrashThud()
    {
        if (_isMuted || _crashThudClip == null) return;
        _sfxSource.PlayOneShot(_crashThudClip, 1.0f);
    }

    public void PlaySingingBowl()
    {
        if (_isMuted || _singingBowlClip == null) return;
        _bellSource.PlayOneShot(_singingBowlClip, 0.85f);
    }

    public void PlayLangurChatter()
    {
        if (_isMuted || _langurChatterClip == null) return;
        _sfxSource.PlayOneShot(_langurChatterClip, 0.8f);
    }

    public void PlayLangurScreech()
    {
        if (_isMuted || _langurScreechClip == null) return;
        _sfxSource.PlayOneShot(_langurScreechClip, 1.0f);
    }

    private void GenerateProceduralClips()
    {
        const int sampleRate = 44100;

        _jumpClip = GenerateJumpClip(sampleRate);
        _slideClip = GenerateSlideClip(sampleRate);
        _lampChimeClip = GenerateLampChimeClip(sampleRate);
        _turnWhooshClip = GenerateTurnWhooshClip(sampleRate);
        _crashThudClip = GenerateCrashThudClip(sampleRate);
        _singingBowlClip = GenerateSingingBowlClip(sampleRate);
        _langurChatterClip = GenerateLangurChatterClip(sampleRate);
        _langurScreechClip = GenerateLangurScreechClip(sampleRate);
    }

    private AudioClip GenerateJumpClip(int sampleRate)
    {
        float duration = 0.22f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Upward frequency sweep from 260Hz to 620Hz
            float freq = Mathf.Lerp(260f, 620f, Mathf.Sqrt(t));
            phase += 2f * Mathf.PI * freq / sampleRate;

            // Amplitude envelope: quick attack, smooth exponential decay
            float env = Mathf.Sin(t * Mathf.PI);
            float sample = Mathf.Sin(phase) * env;
            samples[i] = Mathf.Clamp(sample * 0.8f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_Jump", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateSlideClip(int sampleRate)
    {
        float duration = 0.35f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        float lastNoise = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Downward resonant frequency sweep 170Hz to 85Hz
            float freq = Mathf.Lerp(170f, 85f, t);
            phase += 2f * Mathf.PI * freq / sampleRate;

            // Low-pass filtered noise
            float white = Random.Range(-1f, 1f);
            lastNoise = Mathf.Lerp(lastNoise, white, 0.25f);

            // Friction envelope
            float env = Mathf.Sin(Mathf.Pow(t, 0.6f) * Mathf.PI);
            float sample = (Mathf.Sin(phase) * 0.45f + lastNoise * 0.55f) * env;
            samples[i] = Mathf.Clamp(sample * 0.75f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_Slide", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateLampChimeClip(int sampleRate)
    {
        float duration = 0.55f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Three sparkling bell harmonics (C6, E6, G6) with staggered onset
        float[] freqs = new float[] { 1046.5f, 1318.5f, 1567.9f, 2637.0f };
        float[] delays = new float[] { 0.0f, 0.025f, 0.05f, 0.015f };
        float[] weights = new float[] { 0.45f, 0.35f, 0.30f, 0.15f };

        for (int h = 0; h < freqs.Length; h++)
        {
            float freq = freqs[h];
            float delay = delays[h];
            int delaySamples = Mathf.FloorToInt(delay * sampleRate);
            float weight = weights[h];

            float phase = 0f;
            for (int i = delaySamples; i < sampleCount; i++)
            {
                float timeFromStart = (float)(i - delaySamples) / sampleRate;
                phase += 2f * Mathf.PI * freq / sampleRate;

                // Bell strike decay envelope
                float env = Mathf.Exp(-timeFromStart * 7.5f) * Mathf.Min(1f, timeFromStart * 120f);
                samples[i] += Mathf.Sin(phase) * env * weight;
            }
        }

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Clamp(samples[i] * 0.9f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_LampChime", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateTurnWhooshClip(int sampleRate)
    {
        float duration = 0.25f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        float filteredNoise = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Frequency modulation sweep: drops then rises
            float freq = 280f + 180f * Mathf.Sin(t * Mathf.PI);
            phase += 2f * Mathf.PI * freq / sampleRate;

            float white = Random.Range(-1f, 1f);
            filteredNoise = Mathf.Lerp(filteredNoise, white, 0.35f);

            // Fast bell-shaped whoosh envelope
            float env = Mathf.Pow(Mathf.Sin(t * Mathf.PI), 1.8f);
            float sample = (Mathf.Sin(phase) * 0.3f + filteredNoise * 0.7f) * env;
            samples[i] = Mathf.Clamp(sample * 0.85f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_TurnWhoosh", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateCrashThudClip(int sampleRate)
    {
        float duration = 0.45f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / duration;
            // Low pitch drop 155Hz -> 40Hz
            float freq = Mathf.Lerp(155f, 40f, Mathf.Pow(t, 0.4f));
            phase += 2f * Mathf.PI * freq / sampleRate;

            // Initial noise crunch burst for first 60ms
            float noise = (t < 0.12f) ? Random.Range(-0.6f, 0.6f) * (1f - t / 0.12f) : 0f;

            // Rapid exponential decay
            float env = Mathf.Exp(-t * 8.5f);
            float sample = (Mathf.Sin(phase) + noise) * env;

            // Soft saturation punch
            sample = Mathf.Clamp(sample * 1.25f, -1f, 1f);
            samples[i] = sample;
        }

        AudioClip clip = AudioClip.Create("Procedural_CrashThud", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateSingingBowlClip(int sampleRate)
    {
        float duration = 2.8f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Tibetan singing bowl acoustic modes
        // Fundamental: 392Hz (G4)
        // Overtone mode with acoustic beating: 1081.9Hz & 1084.2Hz (produces 2.3Hz soothing shimmer)
        // High strike chime mode: 2155Hz
        float f0 = 392.0f;
        float f1 = 1081.9f;
        float f2 = 1084.2f;
        float f3 = 2155.0f;

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

            // Gentle strike attack (soft mallet) over first 18ms
            float attack = Mathf.Clamp01(t / 0.018f);

            // Mode decays
            float env0 = Mathf.Exp(-t * 1.1f);
            float envBeating = Mathf.Exp(-t * 1.4f);
            float envHigh = Mathf.Exp(-t * 4.8f);

            float s0 = Mathf.Sin(phase0) * env0 * 0.55f;
            float s1 = Mathf.Sin(phase1) * envBeating * 0.28f;
            float s2 = Mathf.Sin(phase2) * envBeating * 0.28f;
            float s3 = Mathf.Sin(phase3) * envHigh * 0.18f;

            float sample = (s0 + s1 + s2 + s3) * attack;
            samples[i] = Mathf.Clamp(sample * 0.85f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_SingingBowl", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateLangurChatterClip(int sampleRate)
    {
        float duration = 0.22f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase1 = 0f;
        float phase2 = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;

            // Chirp 1 (0.01s - 0.09s)
            if (t >= 0.01f && t < 0.09f)
            {
                float u = (t - 0.01f) / 0.08f;
                float freq = 1800f + 800f * Mathf.Sin(u * Mathf.PI);
                phase1 += 2f * Mathf.PI * freq / sampleRate;

                float fm = Mathf.Sin(2f * Mathf.PI * 35f * t) * 0.2f;
                float env = Mathf.Sin(u * Mathf.PI);
                sample += Mathf.Sin(phase1 + fm) * env * 0.7f;
            }

            // Chirp 2 (0.12s - 0.21s) - slightly higher pitch
            if (t >= 0.12f && t < 0.21f)
            {
                float u = (t - 0.12f) / 0.09f;
                float freq = 2100f + 950f * Mathf.Sin(u * Mathf.PI);
                phase2 += 2f * Mathf.PI * freq / sampleRate;

                float fm = Mathf.Sin(2f * Mathf.PI * 40f * t) * 0.25f;
                float env = Mathf.Sin(u * Mathf.PI);
                sample += Mathf.Sin(phase2 + fm) * env * 0.75f;
            }

            samples[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_LangurChatter", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip GenerateLangurScreechClip(int sampleRate)
    {
        float duration = 0.7f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float phase = 0f;
        float noiseFilter = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float normT = t / duration;

            float freq = Mathf.Lerp(1200f, 3100f, Mathf.Pow(Mathf.Clamp01(normT * 2.5f), 0.5f));
            if (normT > 0.4f)
            {
                freq -= (normT - 0.4f) * 1100f;
            }

            float fm1 = Mathf.Sin(2f * Mathf.PI * 22f * t) * 350f;
            float fm2 = Mathf.Sin(2f * Mathf.PI * 180f * t) * 180f;
            phase += 2f * Mathf.PI * (freq + fm1 + fm2) / sampleRate;

            float white = Random.Range(-1f, 1f);
            noiseFilter = Mathf.Lerp(noiseFilter, white, 0.45f);

            float attack = Mathf.Clamp01(t / 0.04f);
            float decay = Mathf.Exp(-Mathf.Max(0f, normT - 0.25f) * 4.5f);
            float env = attack * decay;

            float sample = (Mathf.Sin(phase) * 0.65f + noiseFilter * 0.35f) * env;
            samples[i] = Mathf.Clamp(sample * 1.15f, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create("Procedural_LangurScreech", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
