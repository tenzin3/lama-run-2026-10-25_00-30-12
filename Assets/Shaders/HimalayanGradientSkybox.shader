Shader "Custom/HimalayanGradientSkybox"
{
    Properties
    {
        _TopColor ("Top Color (Deep Blue)", Color) = (0.09, 0.22, 0.46, 1)
        _HorizonColor ("Horizon Color (Warm Peach)", Color) = (0.96, 0.68, 0.50, 1)
        _BottomColor ("Bottom Color (Misty Peach)", Color) = (0.86, 0.74, 0.70, 1)
        _Exponent ("Exponent", Float) = 1.6
        _HorizonOffset ("Horizon Offset", Float) = 0.04
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
            float4 _HorizonColor;
            float4 _BottomColor;
            float _Exponent;
            float _HorizonOffset;

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
                    float t = saturate(pow(abs(y), _Exponent));
                    col = lerp(_HorizonColor, _TopColor, t);
                }
                else
                {
                    float t = saturate(pow(abs(y), _Exponent * 1.2));
                    col = lerp(_HorizonColor, _BottomColor, t);
                }
                return col;
            }
            ENDHLSL
        }
    }
}
