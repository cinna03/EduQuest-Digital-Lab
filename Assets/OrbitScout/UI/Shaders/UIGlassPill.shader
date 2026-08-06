Shader "OrbitScout/UI/GlassPill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)

        _GlassColor ("Glass Color", Color) = (0.82, 0.68, 0.98, 0.42)
        _GlassCore ("Glass Core", Color) = (0.93, 0.88, 1.0, 0.62)
        _RimColor ("Rim Color", Color) = (0.70, 0.48, 0.95, 0.78)
        _SpecColor ("Spec Color", Color) = (1.0, 0.98, 1.0, 0.85)
        _ShadowColor ("Shadow Color", Color) = (0.35, 0.12, 0.55, 0.35)

        _Aspect ("Aspect (W/H)", Float) = 4.5
        _RimWidth ("Rim Width", Range(0.02, 0.25)) = 0.11
        _SpecStrength ("Spec Strength", Range(0, 2)) = 1.15
        _GlowStrength ("Glow Strength", Range(0, 2)) = 1.0
        _EdgeSoftness ("Edge Softness", Range(0.001, 0.05)) = 0.01

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            fixed4 _GlassColor;
            fixed4 _GlassCore;
            fixed4 _RimColor;
            fixed4 _SpecColor;
            fixed4 _ShadowColor;
            float _Aspect;
            float _RimWidth;
            float _SpecStrength;
            float _GlowStrength;
            float _EdgeSoftness;

            float CapsuleSDF(float2 uv, float aspect)
            {
                float2 p = float2((uv.x - 0.5) * aspect, uv.y - 0.5);
                float radius = 0.5;
                float halfLen = max(aspect * 0.5 - radius, 0.0);
                float2 closest = float2(clamp(p.x, -halfLen, halfLen), 0.0);
                return length(p - closest) - radius;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float aspect = max(_Aspect, 1.0);

                float2 shadowUv = uv + float2(0.0, 0.05);
                float shadowSdf = CapsuleSDF(shadowUv, aspect);
                float shadowMask = (1.0 - smoothstep(-0.02, 0.14, shadowSdf)) * smoothstep(0.2, 0.7, uv.y);
                float4 shadow = float4(_ShadowColor.rgb, _ShadowColor.a * shadowMask);

                float sdf = CapsuleSDF(uv, aspect);
                float aa = max(_EdgeSoftness, fwidth(sdf) * 1.5);
                float pill = 1.0 - smoothstep(0.0, aa, sdf);

                float inside = max(-sdf, 0.0);
                float rim = saturate(1.0 - inside / max(_RimWidth, 0.001));
                rim = rim * rim * (3.0 - 2.0 * rim);

                float2 p = float2((uv.x - 0.5) * aspect, uv.y - 0.5);
                float2 glowCenter = float2(0.0, 0.05);
                float glow = exp(-dot(p - glowCenter, p - glowCenter) * 3.6) * _GlowStrength;
                float vert = saturate(uv.y * 0.85 + 0.15);

                float4 glass = lerp(_GlassColor, _GlassCore, saturate(glow * 0.9 + vert * 0.2));
                glass.rgb = lerp(glass.rgb, _RimColor.rgb, rim * 0.8);
                glass.a = lerp(glass.a, max(glass.a, _RimColor.a), rim * 0.5);
                glass.a *= pill;

                float topBand = saturate(1.0 - abs((inside / max(_RimWidth, 0.001)) - 0.55) * 3.0);
                float topY = saturate(1.0 - (uv.y - 0.16) / 0.24);
                float sideFade = 1.0 - saturate(abs(uv.x - 0.5) * 1.65);
                float spec = topBand * topY * sideFade * _SpecStrength * pill;
                glass.rgb = glass.rgb + _SpecColor.rgb * spec * _SpecColor.a;
                glass.a = saturate(glass.a + spec * 0.22);

                float endL = saturate(1.0 - abs(uv.x - 0.07) / 0.09);
                float endR = saturate(1.0 - abs(uv.x - 0.93) / 0.09);
                float endSpec = (endL + endR) * rim * 0.6 * pill;
                glass.rgb += _SpecColor.rgb * endSpec * 0.5;
                glass.a = saturate(glass.a + endSpec * 0.1);

                float bottomShade = saturate((uv.y - 0.55) / 0.4) * (1.0 - rim) * pill;
                glass.rgb *= 1.0 - bottomShade * 0.2;

                // Standard alpha composite: glass over soft shadow
                float outA = glass.a + shadow.a * (1.0 - glass.a);
                float3 outRgb = glass.rgb * glass.a + shadow.rgb * shadow.a * (1.0 - glass.a);
                outRgb = outA > 0.001 ? outRgb / outA : outRgb;

                float4 result = float4(outRgb, outA) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDCG
        }
    }
}
