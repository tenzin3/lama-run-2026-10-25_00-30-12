using UnityEngine;

public class LangurProceduralAnimator : MonoBehaviour
{
    [Header("Root & Spine")]
    [SerializeField] private Transform _torso;
    [SerializeField] private Transform _chest;
    [SerializeField] private Transform _head;

    [Header("Forelimbs (Arms)")]
    [SerializeField] private Transform _leftUpperArm;
    [SerializeField] private Transform _leftForearm;
    [SerializeField] private Transform _rightUpperArm;
    [SerializeField] private Transform _rightForearm;

    [Header("Hindlimbs (Legs)")]
    [SerializeField] private Transform _leftThigh;
    [SerializeField] private Transform _leftShin;
    [SerializeField] private Transform _rightThigh;
    [SerializeField] private Transform _rightShin;

    [Header("Tail Segments")]
    [SerializeField] private Transform[] _tailSegments;

    [Header("Kinematic Tuning")]
    [SerializeField] private float _baseTorsoHeight = 0.44f;
    [SerializeField] private float _bobAmplitude = 0.075f;
    [SerializeField] private float _pitchAmplitude = 12.0f;
    [SerializeField] private float _armSwingAngle = 36.0f;
    [SerializeField] private float _legSwingAngle = 40.0f;

    private float _gallopPhase = 0f;
    private bool _isLeaping = false;
    private float _leapProgress = 0f;
    private float _currentSpeed = 12f;
    private bool _isGalloping = true;

    // Base pitch profile for langur tail loop (arches up and curves forward over back)
    private static readonly float[] TailBasePitch = new float[] { 62f, -20f, -24f, -20f, -15f };

    public bool IsLeaping
    {
        get => _isLeaping;
        set => _isLeaping = value;
    }

    public bool IsGalloping
    {
        get => _isGalloping;
        set => _isGalloping = value;
    }

    public void SetSpeed(float speed)
    {
        _currentSpeed = Mathf.Max(2f, speed);
    }

    private void Awake()
    {
        AutoFindTransforms();
    }

    public void AutoFindTransforms()
    {
        if (_torso == null) _torso = FindDeepChild(transform, "Torso");
        if (_chest == null) _chest = FindDeepChild(transform, "Chest");
        if (_head == null) _head = FindDeepChild(transform, "Head");

        if (_leftUpperArm == null) _leftUpperArm = FindDeepChild(transform, "UpperArm_L");
        if (_leftForearm == null) _leftForearm = FindDeepChild(transform, "Forearm_L");
        if (_rightUpperArm == null) _rightUpperArm = FindDeepChild(transform, "UpperArm_R");
        if (_rightForearm == null) _rightForearm = FindDeepChild(transform, "Forearm_R");

        if (_leftThigh == null) _leftThigh = FindDeepChild(transform, "Thigh_L");
        if (_leftShin == null) _leftShin = FindDeepChild(transform, "Shin_L");
        if (_rightThigh == null) _rightThigh = FindDeepChild(transform, "Thigh_R");
        if (_rightShin == null) _rightShin = FindDeepChild(transform, "Shin_R");

        if (_tailSegments == null || _tailSegments.Length == 0)
        {
            var tails = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < 5; i++)
            {
                var t = FindDeepChild(transform, "Tail_" + i);
                if (t != null) tails.Add(t);
            }
            if (tails.Count > 0) _tailSegments = tails.ToArray();
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent == null) return null;
        if (parent.name == name) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            var result = FindDeepChild(parent.GetChild(i), name);
            if (result != null) return result;
        }
        return null;
    }

    private void Update()
    {
        if (_torso == null)
        {
            AutoFindTransforms();
            if (_torso == null) return;
        }

        if (_isLeaping)
        {
            _leapProgress = Mathf.MoveTowards(_leapProgress, 1f, Time.deltaTime * 3.5f);
            AnimateLeap();
        }
        else
        {
            _leapProgress = Mathf.MoveTowards(_leapProgress, 0f, Time.deltaTime * 4f);
            if (_isGalloping)
            {
                float strideFreq = Mathf.Clamp(1.8f + _currentSpeed * 0.16f, 2.2f, 5.5f);
                _gallopPhase += Time.deltaTime * strideFreq * Mathf.PI * 2f;
                AnimateGallop();
            }
        }
    }

    private void AnimateGallop()
    {
        // 1. Torso Bob, Pitch & Squash-Stretch
        float bob = Mathf.Sin(_gallopPhase) * _bobAmplitude;
        float pitch = Mathf.Sin(_gallopPhase - 0.45f) * _pitchAmplitude;
        float roll = Mathf.Sin(_gallopPhase * 0.5f) * 3f;
        float stretch = Mathf.Sin(_gallopPhase) * 0.14f;

        _torso.localPosition = new Vector3(0f, _baseTorsoHeight + bob, 0f);
        _torso.localRotation = Quaternion.Euler(pitch, 0f, roll);
        _torso.localScale = new Vector3(1f - stretch * 0.35f, 1f - stretch * 0.35f, 1f + stretch);

        // 2. Head Gaze Stabilization
        if (_head != null)
        {
            _head.localRotation = Quaternion.Euler(-pitch * 0.75f + 4f, 0f, 0f);
        }

        // 3. Forelimbs (Long Arms swinging in bounding gallop)
        float armPhaseL = _gallopPhase + 0.25f;
        float armPhaseR = _gallopPhase + 0.55f;

        float armPitchL = Mathf.Sin(armPhaseL) * _armSwingAngle + 12f;
        float armPitchR = Mathf.Sin(armPhaseR) * _armSwingAngle + 12f;

        if (_leftUpperArm != null)
        {
            _leftUpperArm.localRotation = Quaternion.Euler(armPitchL, 0f, 5f);
        }
        if (_leftForearm != null)
        {
            float flexL = Mathf.Clamp(-Mathf.Sin(armPhaseL) * 30f + 25f, 8f, 75f);
            _leftForearm.localRotation = Quaternion.Euler(flexL, 0f, 0f);
        }

        if (_rightUpperArm != null)
        {
            _rightUpperArm.localRotation = Quaternion.Euler(armPitchR, 0f, -5f);
        }
        if (_rightForearm != null)
        {
            float flexR = Mathf.Clamp(-Mathf.Sin(armPhaseR) * 30f + 25f, 8f, 75f);
            _rightForearm.localRotation = Quaternion.Euler(flexR, 0f, 0f);
        }

        // 4. Hindlimbs (Legs pushing back and cycling forward)
        float legPhaseL = _gallopPhase + Mathf.PI;
        float legPhaseR = _gallopPhase + Mathf.PI + 0.3f;

        float legPitchL = Mathf.Sin(legPhaseL) * _legSwingAngle - 10f;
        float legPitchR = Mathf.Sin(legPhaseR) * _legSwingAngle - 10f;

        if (_leftThigh != null)
        {
            _leftThigh.localRotation = Quaternion.Euler(legPitchL, 0f, -5f);
        }
        if (_leftShin != null)
        {
            float flexL = Mathf.Clamp(-Mathf.Sin(legPhaseL) * 35f + 30f, 8f, 85f);
            _leftShin.localRotation = Quaternion.Euler(flexL, 0f, 0f);
        }

        if (_rightThigh != null)
        {
            _rightThigh.localRotation = Quaternion.Euler(legPitchR, 0f, 5f);
        }
        if (_rightShin != null)
        {
            float flexR = Mathf.Clamp(-Mathf.Sin(legPhaseR) * 35f + 30f, 8f, 85f);
            _rightShin.localRotation = Quaternion.Euler(flexR, 0f, 0f);
        }

        // 5. Long Curling Tail (High arch over back with dynamic horizontal and vertical swaying)
        if (_tailSegments != null)
        {
            float tailSway = Mathf.Sin(_gallopPhase * 0.5f) * 14f;

            for (int i = 0; i < _tailSegments.Length; i++)
            {
                if (_tailSegments[i] == null) continue;

                float basePitch = (i < TailBasePitch.Length) ? TailBasePitch[i] : -15f;
                float wave = Mathf.Sin(_gallopPhase - i * 0.55f) * 8f;
                float segSway = tailSway * (0.6f + i * 0.25f);

                _tailSegments[i].localRotation = Quaternion.Euler(basePitch + wave, segSway, 0f);
            }
        }
    }

    private void AnimateLeap()
    {
        float p = _leapProgress;

        // Torso extends forward and pitches upward in predatory pounce
        _torso.localPosition = Vector3.Lerp(_torso.localPosition, new Vector3(0f, 0.65f, 0.1f), p * Time.deltaTime * 8f);
        _torso.localRotation = Quaternion.Slerp(_torso.localRotation, Quaternion.Euler(-18f, 0f, 0f), p * Time.deltaTime * 8f);
        _torso.localScale = Vector3.Lerp(_torso.localScale, new Vector3(0.92f, 0.92f, 1.2f), p * Time.deltaTime * 8f);

        // Head lunges forward aggressively
        if (_head != null)
        {
            _head.localRotation = Quaternion.Slerp(_head.localRotation, Quaternion.Euler(-12f, 0f, 0f), p * Time.deltaTime * 8f);
        }

        // Arms reach straight out forward to grab Tenzin
        if (_leftUpperArm != null)
        {
            _leftUpperArm.localRotation = Quaternion.Slerp(_leftUpperArm.localRotation, Quaternion.Euler(-62f, 0f, 14f), p * Time.deltaTime * 10f);
        }
        if (_leftForearm != null)
        {
            _leftForearm.localRotation = Quaternion.Slerp(_leftForearm.localRotation, Quaternion.Euler(8f, 0f, 0f), p * Time.deltaTime * 10f);
        }

        if (_rightUpperArm != null)
        {
            _rightUpperArm.localRotation = Quaternion.Slerp(_rightUpperArm.localRotation, Quaternion.Euler(-62f, 0f, -14f), p * Time.deltaTime * 10f);
        }
        if (_rightForearm != null)
        {
            _rightForearm.localRotation = Quaternion.Slerp(_rightForearm.localRotation, Quaternion.Euler(8f, 0f, 0f), p * Time.deltaTime * 10f);
        }

        // Legs trail behind
        if (_leftThigh != null)
        {
            _leftThigh.localRotation = Quaternion.Slerp(_leftThigh.localRotation, Quaternion.Euler(52f, 0f, -8f), p * Time.deltaTime * 8f);
        }
        if (_leftShin != null)
        {
            _leftShin.localRotation = Quaternion.Slerp(_leftShin.localRotation, Quaternion.Euler(20f, 0f, 0f), p * Time.deltaTime * 8f);
        }

        if (_rightThigh != null)
        {
            _rightThigh.localRotation = Quaternion.Slerp(_rightThigh.localRotation, Quaternion.Euler(52f, 0f, 8f), p * Time.deltaTime * 8f);
        }
        if (_rightShin != null)
        {
            _rightShin.localRotation = Quaternion.Slerp(_rightShin.localRotation, Quaternion.Euler(20f, 0f, 0f), p * Time.deltaTime * 8f);
        }

        // Tail whips tensely upward
        if (_tailSegments != null)
        {
            for (int i = 0; i < _tailSegments.Length; i++)
            {
                if (_tailSegments[i] == null) continue;
                float basePitch = (i < TailBasePitch.Length) ? TailBasePitch[i] : -15f;
                _tailSegments[i].localRotation = Quaternion.Slerp(_tailSegments[i].localRotation, Quaternion.Euler(basePitch * 1.2f, 0f, 0f), p * Time.deltaTime * 8f);
            }
        }
    }
}
