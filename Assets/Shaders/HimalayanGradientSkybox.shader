Shader "Custom/HimalayanGradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color (Deep Blue)", Color) = (0.07, 0.26, 0.68, 1)
        _SkyMidColor ("Sky Mid Color (Morning Azure)", Color) = (0.36, 0.62, 0.90, 1)
        _HorizonColor ("Horizon Color (Warm Peach)", Color) = (0.98, 0.66, 0.46, 1)
        _BottomColor ("Bottom Color (Misty Valley)", Color) = (0.76, 0.82, 0.90, 1)
        _Exponent ("Sky Gradient Exponent", Float) = 1.3
        _HorizonOffset ("Horizon Offset", Float) = 0.02
        _PeachSharpness ("Peach Band Sharpness", Float) = 7.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _SkyMidColor;
            float4 _HorizonColor;
            float4 _BottomColor;
            float _Exponent;
            float _HorizonOffset;
            float _PeachSharpness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDir);
                float y = dir.y - _HorizonOffset;
                float4 col;
                if (y >= 0.0)
                {
                    // Warm peach is tightly confined near the horizon
                    float peachBlend = exp(-y * _PeachSharpness);
                    float skyT = saturate(pow(y, _Exponent));
                    float4 sky = lerp(_SkyMidColor, _TopColor, skyT);
                    col = lerp(sky, _HorizonColor, peachBlend);
                }
                else
                {
                    float t = saturate(pow(abs(y), 1.1));
                    col = lerp(_HorizonColor, _BottomColor, t);
                }
                return col;
            }
            ENDHLSL
        }
    }
}
