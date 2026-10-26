using UnityEngine;

// Creates particle/effect materials from template assets in Assets/Resources/FX.
// Materials made only in code don't make Unity include the shader's transparent
// variants in a build, so on the web the soft effects were drawn as solid squares.
// The templates are saved assets, so their variants are always included.
public static class FxMaterials
{
    public static Material New(bool additive, Shader fallbackShader)
    {
        Material template = Resources.Load<Material>(additive ? "FX/FX_ParticleAdditive" : "FX/FX_ParticleAlpha");
        if (template != null) return new Material(template);
        return fallbackShader != null ? new Material(fallbackShader) : null;
    }
}
