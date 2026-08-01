using UnityEngine;
using UnityEngine.Rendering;

namespace EduQuest
{
    /// <summary>Phone-safe URP materials — transparent glass + solid inset liquids.</summary>
    public static class LabMaterials
    {
        static Material s_Lit;

        static Material LitTemplate()
        {
            if (s_Lit != null) return s_Lit;

            s_Lit = Resources.Load<Material>("EduQuest/Materials/LabLit");
#if UNITY_EDITOR
            if (s_Lit == null)
            {
                s_Lit = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/MobileARTemplateAssets/Materials/ObjectMaterial.mat");
            }
            if (s_Lit == null)
            {
                s_Lit = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Samples/XR Interaction Toolkit/3.3.0/Starter Assets/DemoSceneAssets/Materials/Lit White.mat");
            }
#endif
            if (s_Lit == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                             ?? Shader.Find("Sprites/Default");
                s_Lit = new Material(shader);
            }
            return s_Lit;
        }

        public static Material Solid(Color color, float smoothness = 0.45f)
        {
            var mat = new Material(LitTemplate());
            color.a = 1f;
            ApplyColor(mat, color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
            SetOpaque(mat);
            return mat;
        }

        /// <summary>See-through glass so the inset liquid reads as inside the beaker.</summary>
        public static Material GlassShell()
        {
            var mat = new Material(LitTemplate());
            var color = new Color(0.78f, 0.9f, 0.96f, 0.22f);
            ApplyColor(mat, color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
            SetTransparent(mat);
            return mat;
        }

        public static Material GlassRim()
        {
            var mat = new Material(LitTemplate());
            // Slightly denser rim/base so the beaker silhouette stays readable
            var color = new Color(0.72f, 0.86f, 0.94f, 0.55f);
            ApplyColor(mat, color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.9f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.08f);
            SetTransparent(mat);
            return mat;
        }

        /// <summary>
        /// Liquid body. Clear aqueous solutions (alpha &lt; ~0.92) stay translucent so they
        /// read as water-like inside glass; colored reagents stay opaque.
        /// </summary>
        public static Material Liquid(Color color)
        {
            var mat = new Material(LitTemplate());
            bool clear = color.a < 0.92f;
            var c = color;
            if (clear)
            {
                // Keep a readable watery body without looking like plastic paint
                c.a = Mathf.Clamp(color.a, 0.42f, 0.7f);
                ApplyColor(mat, c);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.85f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.02f);
                SetTransparent(mat);
            }
            else
            {
                c.a = 1f;
                ApplyColor(mat, c);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.55f);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
                SetOpaque(mat);
            }
            return mat;
        }

        /// <summary>Bright surface disc so fill height is obvious even for clear solutions.</summary>
        public static Material Meniscus(Color liquidColor)
        {
            var mat = new Material(LitTemplate());
            var c = Color.Lerp(liquidColor, Color.white, 0.55f);
            c.a = 0.85f;
            ApplyColor(mat, c);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.95f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.1f);
            SetTransparent(mat);
            return mat;
        }

        public static Material Glass()
        {
            var baked = Resources.Load<Material>("EduQuest/Materials/LabGlass");
            if (baked != null) return baked;
            return GlassShell();
        }

        static void ApplyColor(Material mat, Color color)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        }

        static void SetOpaque(Material mat)
        {
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 1f);
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.renderQueue = (int)RenderQueue.Geometry;
        }

        static void SetTransparent(Material mat)
        {
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f); // Alpha
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
