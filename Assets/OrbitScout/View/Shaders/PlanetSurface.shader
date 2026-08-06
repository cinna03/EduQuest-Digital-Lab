Shader "OrbitScout/PlanetSurface"
{
    Properties
    {
        _Matcap ("Matcap", 2D) = "white" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Range(0, 1)) = 1
        _AtmosphereColor ("Atmosphere", Color) = (0.45, 0.72, 1, 1)
        _AtmospherePower ("Atmosphere Power", Float) = 3.5
        _AtmosphereStrength ("Atmosphere Strength", Range(0, 2)) = 0.45
        _LightInfluence ("Light Influence", Range(0, 1)) = 0.35
        _Ambient ("Ambient", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "PlanetForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_Matcap);
            SAMPLER(sampler_Matcap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Matcap_ST;
                half4 _Tint;
                half _Saturation;
                half4 _AtmosphereColor;
                half _AtmospherePower;
                half _AtmosphereStrength;
                half _LightInfluence;
                half _Ambient;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs norm = GetVertexNormalInputs(input.normalOS);
                output.positionCS = pos.positionCS;
                output.normalWS = norm.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                return output;
            }

            half3 ApplySaturation(half3 color, half sat)
            {
                half luma = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                return lerp(half3(luma, luma, luma), color, sat);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);

                // View-space matcap from the reference sphere render
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
                float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                half3 matcap = SAMPLE_TEXTURE2D(_Matcap, sampler_Matcap, matcapUV).rgb;

                half3 color = matcap * _Tint.rgb;
                color = ApplySaturation(color, _Saturation);

                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half lighting = lerp(1.0h, _Ambient + (1.0h - _Ambient) * ndotl, _LightInfluence);
                color *= lighting * mainLight.color;

                half fresnel = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _AtmospherePower);
                color += _AtmosphereColor.rgb * fresnel * _AtmosphereStrength * _Saturation;

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    // Built-in / editor fallback
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _Matcap;
            fixed4 _Tint;
            half _Saturation;
            fixed4 _AtmosphereColor;
            half _AtmospherePower;
            half _AtmosphereStrength;
            half _LightInfluence;
            half _Ambient;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normalWS = UnityObjectToWorldNormal(v.normal);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDirWS = normalize(_WorldSpaceCameraPos - worldPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 nVS = mul((float3x3)UNITY_MATRIX_V, n);
                float2 uv = nVS.xy * 0.5 + 0.5;
                fixed3 matcap = tex2D(_Matcap, uv).rgb;
                fixed3 color = matcap * _Tint.rgb;
                half luma = dot(color, half3(0.2126h, 0.7152h, 0.0722h));
                color = lerp(fixed3(luma, luma, luma), color, _Saturation);

                half ndotl = saturate(dot(n, _WorldSpaceLightPos0.xyz));
                half lighting = lerp(1.0h, _Ambient + (1.0h - _Ambient) * ndotl, _LightInfluence);
                color *= lighting * _LightColor0.rgb;

                half fresnel = pow(1.0h - saturate(dot(n, normalize(i.viewDirWS))), _AtmospherePower);
                color += _AtmosphereColor.rgb * fresnel * _AtmosphereStrength * _Saturation;
                return fixed4(color, 1);
            }
            ENDCG
        }
    }

    FallBack Off
}
