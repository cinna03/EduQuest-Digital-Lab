using UnityEngine;
using UnityEngine.Rendering;

namespace EduQuest.Lab
{
    public static class LabGlassMaterials
    {
        public static Material MakeGlass(Color tint, float smoothness = 0.92f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            ApplyTransparent(mat, tint, smoothness);
            return mat;
        }

        public static Material MakeLiquid(Color tint)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            ApplyTransparent(mat, tint, 0.75f);
            return mat;
        }

        public static Material MakeSolid(Color tint, float smoothness = 0.4f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        static void ApplyTransparent(Material mat, Color tint, float smoothness)
        {
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);

            // URP transparent surface
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
}
