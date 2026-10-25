using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    private ParticleSystem _dustParticleSystem;
    private ParticleSystem _sparkleParticleSystem;

    private Material _dustMat;
    private Material _sparkleMat;
    private Texture2D _softCircleTex;

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

    private Texture2D GenerateSoftCircleTexture(int size = 64)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        float radius = center;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float t = Mathf.Clamp01(dist / radius);
                // Soft round texture: white circle fading to transparent
                float alpha = Mathf.SmoothStep(1f, 0f, t);
                alpha = Mathf.Pow(alpha, 1.4f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void SetupParticleSystems()
    {
        _softCircleTex = GenerateSoftCircleTexture(64);

        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Standard Unlit");
        }

        // Dust material: alpha-blended, pale stone colour, soft and semi-transparent
        if (particleShader != null)
        {
            _dustMat = new Material(particleShader);
            _dustMat.name = "FX_DustMat";
            _dustMat.mainTexture = _softCircleTex;
            if (_dustMat.HasProperty("_BaseMap"))
            {
                _dustMat.SetTexture("_BaseMap", _softCircleTex);
            }
            _dustMat.SetColor("_BaseColor", Color.white);
            _dustMat.SetFloat("_Surface", 1f); // Transparent
            _dustMat.SetFloat("_Blend", 0f);   // Alpha blend
            _dustMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _dustMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _dustMat.SetInt("_ZWrite", 0);
            _dustMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _dustMat.SetOverrideTag("RenderType", "Transparent");

            // Lamp sparkle material: additive, gold, soft
            _sparkleMat = new Material(particleShader);
            _sparkleMat.name = "FX_SparkleMat";
            _sparkleMat.mainTexture = _softCircleTex;
            if (_sparkleMat.HasProperty("_BaseMap"))
            {
                _sparkleMat.SetTexture("_BaseMap", _softCircleTex);
            }
            _sparkleMat.SetColor("_BaseColor", Color.white);
            _sparkleMat.SetFloat("_Surface", 1f); // Transparent
            _sparkleMat.SetFloat("_Blend", 2f);   // Additive
            _sparkleMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _sparkleMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _sparkleMat.SetInt("_ZWrite", 0);
            _sparkleMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _sparkleMat.SetOverrideTag("RenderType", "Transparent");
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
        dustMain.startSpeed = 1.0f;
        dustMain.startSize = 0.2f;
        dustMain.startColor = new Color(0.82f, 0.80f, 0.76f, 0.45f); // Pale stone colour, semi-transparent
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
            new GradientColorKey[] { new GradientColorKey(new Color(0.85f, 0.82f, 0.78f), 0.0f), new GradientColorKey(new Color(0.78f, 0.75f, 0.72f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.45f, 0.0f), new GradientAlphaKey(0.3f, 0.4f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        dustColor.color = dustGrad;

        var dustSize = _dustParticleSystem.sizeOverLifetime;
        dustSize.enabled = true;
        AnimationCurve dustSizeCurve = new AnimationCurve();
        dustSizeCurve.AddKey(0.0f, 0.5f);
        dustSizeCurve.AddKey(1.0f, 1.4f);
        dustSize.size = new ParticleSystem.MinMaxCurve(1.0f, dustSizeCurve);

        var dustRenderer = dustObj.GetComponent<ParticleSystemRenderer>();
        if (_dustMat != null)
        {
            dustRenderer.material = _dustMat;
        }

        // 2. Lamp Golden Sparkle Particle System (World Space)
        GameObject sparkleObj = new GameObject("FX_LampSparkles");
        sparkleObj.transform.SetParent(transform);
        _sparkleParticleSystem = sparkleObj.AddComponent<ParticleSystem>();

        var spMain = _sparkleParticleSystem.main;
        spMain.simulationSpace = ParticleSystemSimulationSpace.World;
        spMain.playOnAwake = false;
        spMain.loop = false;
        spMain.startLifetime = 0.4f;
        spMain.startSpeed = 2.0f;
        spMain.startSize = 0.1f;
        spMain.startColor = new Color(1.0f, 0.85f, 0.25f, 1.0f);
        spMain.gravityModifier = 0.25f;

        var spEmission = _sparkleParticleSystem.emission;
        spEmission.rateOverTime = 0;

        var spShape = _sparkleParticleSystem.shape;
        spShape.shapeType = ParticleSystemShapeType.Sphere;
        spShape.radius = 0.15f;

        var spColor = _sparkleParticleSystem.colorOverLifetime;
        spColor.enabled = true;
        Gradient spGrad = new Gradient();
        spGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.92f, 0.4f), 0.0f), new GradientColorKey(new Color(1f, 0.75f, 0.12f), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.8f, 0.3f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        spColor.color = spGrad;

        var spSize = _sparkleParticleSystem.sizeOverLifetime;
        spSize.enabled = true;
        AnimationCurve spSizeCurve = new AnimationCurve();
        spSizeCurve.AddKey(0.0f, 0.8f);
        spSizeCurve.AddKey(0.3f, 1.0f);
        spSizeCurve.AddKey(1.0f, 0.0f); // Fading out
        spSize.size = new ParticleSystem.MinMaxCurve(1.0f, spSizeCurve);

        var spRenderer = sparkleObj.GetComponent<ParticleSystemRenderer>();
        if (_sparkleMat != null)
        {
            spRenderer.material = _sparkleMat;
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
        emitParams.startSize = Random.Range(0.12f, 0.20f);
        emitParams.startLifetime = Random.Range(0.3f, 0.45f);
        emitParams.startColor = new Color(0.82f, 0.80f, 0.76f, Random.Range(0.35f, 0.5f));

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
            emitParams.startSize = Random.Range(0.15f, 0.25f);
            emitParams.startLifetime = Random.Range(0.35f, 0.5f);
            emitParams.startColor = new Color(0.82f, 0.80f, 0.76f, Random.Range(0.35f, 0.5f));

            _dustParticleSystem.Emit(emitParams, 1);
        }
    }

    public void EmitLampSparkle(Vector3 lampPosition)
    {
        if (_sparkleParticleSystem == null) return;

        // Lamp sparkle: 10-15 particles, additive gold, small (0.05-0.15), lifetime 0.3-0.5 s, fading out
        int count = Random.Range(10, 16);
        for (int i = 0; i < count; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y) + 0.35f; // Bias upward

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = lampPosition + Random.insideUnitSphere * 0.15f;
            emitParams.velocity = randomDir.normalized * Random.Range(1.6f, 2.8f);
            emitParams.startSize = Random.Range(0.05f, 0.15f);
            emitParams.startLifetime = Random.Range(0.3f, 0.5f);
            emitParams.startColor = Color.Lerp(new Color(1f, 0.90f, 0.35f, 1f), new Color(1f, 0.75f, 0.12f, 1f), Random.value);

            _sparkleParticleSystem.Emit(emitParams, 1);
        }
    }

    private void OnDestroy()
    {
        if (_dustMat != null) Destroy(_dustMat);
        if (_sparkleMat != null) Destroy(_sparkleMat);
        if (_softCircleTex != null) Destroy(_softCircleTex);
    }
}

