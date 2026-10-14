// Personajes, enemigos y objetos del remake: sprite sin iluminar (se leen siempre, como en Hollow Knight) con un
// contorno de tinta y un destello al recibir un golpe. _Outline en texels del sprite; _Flash de 0 a 1.
Shader "MazeOfKnight/SpriteOutlineFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _OutlineColor ("Contorno", Color) = (0.03, 0.03, 0.05, 1)
        _Outline ("Grosor del contorno (texels)", Float) = 1
        _FlashColor ("Destello", Color) = (1, 1, 1, 1)
        _Flash ("Destello (0-1)", Range(0, 1)) = 0
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
                half4 _OutlineColor;
                float _Outline;
                half4 _FlashColor;
                half _Flash;
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
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _Flash * c.a);
                if (c.a > 0.01 || _Outline <= 0)
                    return c;
                // Fuera del sprite: contorno si algún vecino (8 direcciones) es opaco.
                float2 d = _MainTex_TexelSize.xy * _Outline;
                half a = 0;
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( d.x, 0)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-d.x, 0)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0,  d.y)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -d.y)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + d).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv - d).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(d.x, -d.y)).a);
                a = max(a, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-d.x, d.y)).a);
                half4 o = _OutlineColor;
                o.rgb = lerp(o.rgb, _FlashColor.rgb, _Flash);
                o.a *= a * i.color.a;
                return o;
            }
            ENDHLSL
        }
    }
}
