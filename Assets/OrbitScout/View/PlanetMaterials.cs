using System.Collections.Generic;
using UnityEngine;

namespace OrbitScout.View
{
    public static class PlanetMaterials
    {
        const string TextureFolder = "OrbitScout/Planets/";
        const string RingsTextureName = "SaturnRings";

        static readonly Dictionary<PlanetId, Material> BodyMaterials = new Dictionary<PlanetId, Material>();
        static Material ringMaterial;
        static Material sunMaterial;
        static Material sunGlowMaterial;
        static Mesh ringDiscMesh;

        static readonly int MatcapId = Shader.PropertyToID("_Matcap");
        static readonly int TintId = Shader.PropertyToID("_Tint");
        static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        static readonly int AtmosphereColorId = Shader.PropertyToID("_AtmosphereColor");
        static readonly int AtmosphereStrengthId = Shader.PropertyToID("_AtmosphereStrength");
        static readonly int AtmospherePowerId = Shader.PropertyToID("_AtmospherePower");
        static readonly int LightInfluenceId = Shader.PropertyToID("_LightInfluence");
        static readonly int AmbientId = Shader.PropertyToID("_Ambient");

        struct AtmosphereSpec
        {
            public Color Color;
            public float Strength;
            public float Power;
            public float LightInfluence;
            public float Ambient;
        }

        static AtmosphereSpec AtmosphereFor(PlanetId id)
        {
            return id switch
            {
                PlanetId.Mercury => new AtmosphereSpec
                {
                    Color = new Color(0.55f, 0.52f, 0.48f),
                    Strength = 0.05f,
                    Power = 4f,
                    LightInfluence = 0.28f,
                    Ambient = 0.62f
                },
                PlanetId.Venus => new AtmosphereSpec
                {
                    Color = new Color(1f, 0.78f, 0.35f),
                    Strength = 0.7f,
                    Power = 2.8f,
                    LightInfluence = 0.3f,
                    Ambient = 0.6f
                },
                PlanetId.Earth => new AtmosphereSpec
                {
                    Color = new Color(0.35f, 0.65f, 1f),
                    Strength = 0.85f,
                    Power = 3.2f,
                    LightInfluence = 0.32f,
                    Ambient = 0.58f
                },
                PlanetId.Mars => new AtmosphereSpec
                {
                    Color = new Color(0.95f, 0.55f, 0.35f),
                    Strength = 0.28f,
                    Power = 3.6f,
                    LightInfluence = 0.3f,
                    Ambient = 0.6f
                },
                PlanetId.Jupiter => new AtmosphereSpec
                {
                    Color = new Color(1f, 0.85f, 0.65f),
                    Strength = 0.4f,
                    Power = 3.4f,
                    LightInfluence = 0.28f,
                    Ambient = 0.62f
                },
                PlanetId.Saturn => new AtmosphereSpec
                {
                    Color = new Color(1f, 0.9f, 0.7f),
                    Strength = 0.35f,
                    Power = 3.4f,
                    LightInfluence = 0.28f,
                    Ambient = 0.62f
                },
                PlanetId.Uranus => new AtmosphereSpec
                {
                    Color = new Color(0.55f, 0.95f, 1f),
                    Strength = 0.55f,
                    Power = 3f,
                    LightInfluence = 0.3f,
                    Ambient = 0.6f
                },
                PlanetId.Neptune => new AtmosphereSpec
                {
                    Color = new Color(0.35f, 0.55f, 1f),
                    Strength = 0.65f,
                    Power = 3.1f,
                    LightInfluence = 0.3f,
                    Ambient = 0.58f
                },
                _ => new AtmosphereSpec
                {
                    Color = Color.white,
                    Strength = 0.2f,
                    Power = 3.5f,
                    LightInfluence = 0.3f,
                    Ambient = 0.6f
                }
            };
        }

        public static Color FallbackColor(PlanetId id)
        {
            return id switch
            {
                PlanetId.Mercury => new Color(0.72f, 0.70f, 0.68f),
                PlanetId.Venus => new Color(0.95f, 0.72f, 0.38f),
                PlanetId.Earth => new Color(0.22f, 0.48f, 0.92f),
                PlanetId.Mars => new Color(0.86f, 0.36f, 0.20f),
                PlanetId.Jupiter => new Color(0.86f, 0.66f, 0.42f),
                PlanetId.Saturn => new Color(0.90f, 0.80f, 0.55f),
                PlanetId.Uranus => new Color(0.55f, 0.85f, 0.90f),
                PlanetId.Neptune => new Color(0.28f, 0.40f, 0.95f),
                _ => Color.white
            };
        }

        public static Material GetBodyMaterial(PlanetId id)
        {
            if (BodyMaterials.TryGetValue(id, out Material existing) && existing != null)
                return existing;

            Shader shader = OrbitScoutShaderLibrary.PlanetSurface
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            Material mat = new Material(shader)
            {
                name = "Planet_" + id,
                hideFlags = HideFlags.DontSave
            };

            Texture2D tex = Resources.Load<Texture2D>(TextureFolder + id);
            AtmosphereSpec atm = AtmosphereFor(id);
            bool usesMatcap = mat.HasProperty(MatcapId);

            if (usesMatcap)
            {
                if (tex != null)
                    mat.SetTexture(MatcapId, tex);
                mat.SetColor(TintId, tex != null ? Color.white : FallbackColor(id));
                mat.SetFloat(SaturationId, 1f);
                mat.SetColor(AtmosphereColorId, atm.Color);
                mat.SetFloat(AtmosphereStrengthId, atm.Strength);
                mat.SetFloat(AtmospherePowerId, atm.Power);
                mat.SetFloat(LightInfluenceId, atm.LightInfluence);
                mat.SetFloat(AmbientId, atm.Ambient);
            }
            else
            {
                // Flat fallback if matcap shader/texture missing
                Color c = FallbackColor(id);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", c);
                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", 0.22f);
                if (mat.HasProperty("_Metallic"))
                    mat.SetFloat("_Metallic", 0f);
            }

            BodyMaterials[id] = mat;
            return mat;
        }

        public static void ApplyBody(Renderer renderer, PlanetId id)
        {
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetBodyMaterial(id);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;

            var block = new MaterialPropertyBlock();
            block.SetColor(TintId, Color.white);
            block.SetFloat(SaturationId, 1f);
            renderer.SetPropertyBlock(block);
        }

        public static Material GetSunMaterial()
        {
            if (sunMaterial != null)
                return sunMaterial;

            Shader shader = OrbitScoutShaderLibrary.PlanetSurface
                ?? Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");

            sunMaterial = new Material(shader)
            {
                name = "Planet_Sun",
                hideFlags = HideFlags.DontSave
            };

            Texture2D tex = Resources.Load<Texture2D>(TextureFolder + "Sun");
            if (sunMaterial.HasProperty(MatcapId))
            {
                if (tex != null)
                    sunMaterial.SetTexture(MatcapId, tex);
                sunMaterial.SetColor(TintId, tex != null ? Color.white : new Color(1f, 0.78f, 0.25f));
                sunMaterial.SetFloat(SaturationId, 1f);
                sunMaterial.SetColor(AtmosphereColorId, new Color(1f, 0.72f, 0.25f));
                sunMaterial.SetFloat(AtmosphereStrengthId, 1.35f);
                sunMaterial.SetFloat(AtmospherePowerId, 2.2f);
                sunMaterial.SetFloat(LightInfluenceId, 0.08f);
                sunMaterial.SetFloat(AmbientId, 0.95f);
            }
            else
            {
                Color c = new Color(1f, 0.78f, 0.25f);
                if (sunMaterial.HasProperty("_BaseColor"))
                    sunMaterial.SetColor("_BaseColor", c);
                if (sunMaterial.HasProperty("_Color"))
                    sunMaterial.SetColor("_Color", c);
                sunMaterial.EnableKeyword("_EMISSION");
                if (sunMaterial.HasProperty("_EmissionColor"))
                    sunMaterial.SetColor("_EmissionColor", c * 2.2f);
            }

            return sunMaterial;
        }

        public static void ApplySun(Renderer renderer)
        {
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetSunMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.SetPropertyBlock(null);
        }

        public static Material GetSunGlowMaterial()
        {
            if (sunGlowMaterial != null)
                return sunGlowMaterial;

            Shader shader = OrbitScoutShaderLibrary.PlanetRings
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");

            sunGlowMaterial = new Material(shader)
            {
                name = "Planet_SunGlow",
                hideFlags = HideFlags.DontSave
            };

            Texture2D tex = Resources.Load<Texture2D>(TextureFolder + "SunGlow");
            if (tex != null)
            {
                if (sunGlowMaterial.HasProperty("_MainTex"))
                    sunGlowMaterial.SetTexture("_MainTex", tex);
                if (sunGlowMaterial.HasProperty("_BaseMap"))
                    sunGlowMaterial.SetTexture("_BaseMap", tex);
            }

            if (sunGlowMaterial.HasProperty("_Tint"))
                sunGlowMaterial.SetColor("_Tint", new Color(1f, 0.9f, 0.65f, 1f));
            if (sunGlowMaterial.HasProperty("_Brightness"))
                sunGlowMaterial.SetFloat("_Brightness", 1.35f);

            return sunGlowMaterial;
        }

        public static void ApplySunGlow(Renderer renderer)
        {
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetSunGlowMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.SetPropertyBlock(null);
        }

        public static Material GetRingMaterial()
        {
            if (ringMaterial != null)
                return ringMaterial;

            Shader shader = OrbitScoutShaderLibrary.PlanetRings
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");

            ringMaterial = new Material(shader)
            {
                name = "Planet_SaturnRings",
                hideFlags = HideFlags.DontSave
            };

            Texture2D tex = Resources.Load<Texture2D>(TextureFolder + RingsTextureName);
            if (tex != null)
            {
                if (ringMaterial.HasProperty("_MainTex"))
                    ringMaterial.SetTexture("_MainTex", tex);
                if (ringMaterial.HasProperty("_BaseMap"))
                    ringMaterial.SetTexture("_BaseMap", tex);
            }

            if (ringMaterial.HasProperty("_Tint"))
                ringMaterial.SetColor("_Tint", Color.white);
            if (ringMaterial.HasProperty("_Brightness"))
                ringMaterial.SetFloat("_Brightness", 1.15f);

            return ringMaterial;
        }

        public static Mesh GetRingDiscMesh()
        {
            if (ringDiscMesh != null)
                return ringDiscMesh;

            const int segments = 64;
            var verts = new Vector3[segments + 1];
            var uvs = new Vector2[segments + 1];
            var tris = new int[segments * 3];

            verts[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                float x = Mathf.Cos(a);
                float z = Mathf.Sin(a);
                verts[i + 1] = new Vector3(x, 0f, z);
                uvs[i + 1] = new Vector2(x * 0.5f + 0.5f, z * 0.5f + 0.5f);

                int t = i * 3;
                tris[t] = 0;
                tris[t + 1] = i + 1;
                tris[t + 2] = i + 2 <= segments ? i + 2 : 1;
            }

            ringDiscMesh = new Mesh
            {
                name = "SaturnRingDisc",
                hideFlags = HideFlags.DontSave
            };
            ringDiscMesh.vertices = verts;
            ringDiscMesh.uv = uvs;
            ringDiscMesh.triangles = tris;
            ringDiscMesh.RecalculateNormals();
            ringDiscMesh.RecalculateBounds();
            return ringDiscMesh;
        }

        public static void ApplyRings(Renderer renderer)
        {
            if (renderer == null)
                return;

            renderer.sharedMaterial = GetRingMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            renderer.SetPropertyBlock(null);
        }

        public static void SetBodyDisplay(Renderer renderer, Color tint, float saturation)
        {
            if (renderer == null)
                return;

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(TintId, tint);
            block.SetFloat(SaturationId, Mathf.Clamp01(saturation));

            // Keep flat-color fallbacks in sync when matcap shader isn't present
            block.SetColor("_BaseColor", tint);
            block.SetColor("_Color", tint);
            renderer.SetPropertyBlock(block);
        }
    }
}
