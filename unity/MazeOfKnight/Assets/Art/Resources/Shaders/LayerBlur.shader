// Capas del ambiente (fondos y primer plano) del remake: sprite sin iluminar, teñido por el color del renderer, con un
// desenfoque por distancia (_Blur, en texels): los planos lejanos y el primer plano se ven desenfocados y el plano de
// juego nítido, como en Hollow Knight. 13 muestras en dos anillos; con _Blur = 0 es un sprite normal.
Shader "MazeOfKnight/LayerBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Blur ("Desenfoque (texels)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "CanUseSpriteAtlas" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
                float _Blur;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                if (_Blur <= 0.01)
                    return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                float2 d = _MainTex_TexelSize.xy * _Blur;
                // Centro, anillo interior (radio 0,5) y anillo exterior (radio 1), girado 30 grados.
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half total = 1;
                const float2 ring[6] = { float2(1, 0), float2(0.5, 0.866), float2(-0.5, 0.866), float2(-1, 0), float2(-0.5, -0.866), float2(0.5, -0.866) };
                const float2 ring2[6] = { float2(0.866, 0.5), float2(0, 1), float2(-0.866, 0.5), float2(-0.866, -0.5), float2(0, -1), float2(0.866, -0.5) };
                [unroll]
                for (int k = 0; k < 6; k++)
                {
                    c += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ring[k] * d * 0.5);
                    c += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + ring2[k] * d) * 0.7;
                    total += 1.7;
                }
                return c / total * i.color;
            }
            ENDHLSL
        }
    }
}
