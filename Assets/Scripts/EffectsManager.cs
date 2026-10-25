using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    private ParticleSystem _dustParticleSystem;
    private ParticleSystem _sparkleParticleSystem;

    private Material _particleMat;

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

        SetupParticleSystems();
    }

    private void SetupParticleSystems()
    {
        // Try finding URP ParticlesUnlit material or shader
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Standard Unlit");
        }

        if (particleShader != null)
        {
            _particleMat = new Material(particleShader);
        }

        // 1. Dust Particle System (World Space)
        GameObject dustObj = new GameObject("FX_DustParticles");
        dustObj.transform.SetParent(transform);
        _dustParticleSystem = dustObj.AddComponent<ParticleSystem>();

        var dustMain = _dustParticleSystem.main;
        dustMain.simulationSpace = ParticleSystemSimulationSpace.World;
        dustMain.playOnAwake = false;
        dustMain.loop = false;
        dustMain.startLifetime = 0.38f;
        dustMain.startSpeed = 1.2f;
        dustMain.startSize = 0.22f;
        dustMain.startColor = new Color(0.85f, 0.80f, 0.72f, 0.55f);
        dustMain.gravityModifier = -0.15f; // Slight float up

        var dustEmission = _dustParticleSystem.emission;
        dustEmission.rateOverTime = 0;

        var dustShape = _dustParticleSystem.shape;
        dustShape.shapeType = ParticleSystemShapeType.Sphere;
        dustShape.radius = 0.15f;

        var dustColor = _dustParticleSystem.colorOverLifetime;
        dustColor.enabled = true;
        Gradient dustGrad = new Gradient();
        dustGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.88f, 0.82f, 0.74f), 0.0f), new GradientColorKey(new Color(0.80f, 0.76f, 0.70f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.6f, 0.0f), new GradientAlphaKey(0.4f, 0.3f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        dustColor.color = dustGrad;

        var dustSize = _dustParticleSystem.sizeOverLifetime;
        dustSize.enabled = true;
        AnimationCurve dustSizeCurve = new AnimationCurve();
        dustSizeCurve.AddKey(0.0f, 0.6f);
        dustSizeCurve.AddKey(1.0f, 1.5f);
        dustSize.size = new ParticleSystem.MinMaxCurve(1.0f, dustSizeCurve);

        var dustRenderer = dustObj.GetComponent<ParticleSystemRenderer>();
        if (_particleMat != null)
        {
            dustRenderer.material = _particleMat;
        }

        // 2. Lamp Golden Sparkle Particle System (World Space)
        GameObject sparkleObj = new GameObject("FX_LampSparkles");
        sparkleObj.transform.SetParent(transform);
        _sparkleParticleSystem = sparkleObj.AddComponent<ParticleSystem>();

        var spMain = _sparkleParticleSystem.main;
        spMain.simulationSpace = ParticleSystemSimulationSpace.World;
        spMain.playOnAwake = false;
        spMain.loop = false;
        spMain.startLifetime = 0.55f;
        spMain.startSpeed = 2.5f;
        spMain.startSize = 0.28f;
        spMain.startColor = new Color(1.0f, 0.88f, 0.25f, 1.0f);
        spMain.gravityModifier = 0.3f;

        var spEmission = _sparkleParticleSystem.emission;
        spEmission.rateOverTime = 0;

        var spShape = _sparkleParticleSystem.shape;
        spShape.shapeType = ParticleSystemShapeType.Sphere;
        spShape.radius = 0.2f;

        var spColor = _sparkleParticleSystem.colorOverLifetime;
        spColor.enabled = true;
        Gradient spGrad = new Gradient();
        spGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.95f, 0.45f), 0.0f), new GradientColorKey(new Color(1f, 0.65f, 0.1f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.9f, 0.4f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        spColor.color = spGrad;

        var spSize = _sparkleParticleSystem.sizeOverLifetime;
        spSize.enabled = true;
        AnimationCurve spSizeCurve = new AnimationCurve();
        spSizeCurve.AddKey(0.0f, 1.2f);
        spSizeCurve.AddKey(0.3f, 1.4f);
        spSizeCurve.AddKey(1.0f, 0.1f);
        spSize.size = new ParticleSystem.MinMaxCurve(1.0f, spSizeCurve);

        var spRenderer = sparkleObj.GetComponent<ParticleSystemRenderer>();
        if (_particleMat != null)
        {
            spRenderer.material = _particleMat;
        }
    }

    public void EmitRunDust(Vector3 footPosition, Vector3 moveDirection)
    {
        if (_dustParticleSystem == null) return;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = footPosition + Vector3.up * 0.05f;
        // Drift backward and slightly upward
        Vector3 backDrift = -moveDirection.normalized * Random.Range(0.4f, 0.9f) + Vector3.up * Random.Range(0.2f, 0.6f);
        backDrift += Vector3.Cross(moveDirection, Vector3.up) * Random.Range(-0.3f, 0.3f);
        emitParams.velocity = backDrift;
        emitParams.startSize = Random.Range(0.14f, 0.22f);
        emitParams.startLifetime = Random.Range(0.3f, 0.45f);

        _dustParticleSystem.Emit(emitParams, 1);
    }

    public void EmitLandingDust(Vector3 centerPosition)
    {
        if (_dustParticleSystem == null) return;

        int count = 9;
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0.2f, Mathf.Sin(angle));

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = centerPosition + new Vector3(Mathf.Cos(angle) * 0.15f, 0.05f, Mathf.Sin(angle) * 0.15f);
            emitParams.velocity = dir * Random.Range(1.2f, 2.0f);
            emitParams.startSize = Random.Range(0.18f, 0.28f);
            emitParams.startLifetime = Random.Range(0.35f, 0.5f);

            _dustParticleSystem.Emit(emitParams, 1);
        }
    }

    public void EmitLampSparkle(Vector3 lampPosition)
    {
        if (_sparkleParticleSystem == null) return;

        int count = 14;
        for (int i = 0; i < count; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y) + 0.35f; // Bias upward

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = lampPosition + Random.insideUnitSphere * 0.2f;
            emitParams.velocity = randomDir.normalized * Random.Range(1.8f, 3.2f);
            emitParams.startSize = Random.Range(0.18f, 0.32f);
            emitParams.startLifetime = Random.Range(0.4f, 0.65f);
            emitParams.startColor = Color.Lerp(new Color(1f, 0.92f, 0.4f, 1f), new Color(1f, 0.72f, 0.15f, 1f), Random.value);

            _sparkleParticleSystem.Emit(emitParams, 1);
        }
    }
}
