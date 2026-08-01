using UnityEngine;
using UnityEngine.Rendering;

namespace EduQuest
{
    /// <summary>Shared URP materials for arena props and enemies.</summary>
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
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.05f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 1f);
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType", "Opaque");
            mat.renderQueue = (int)RenderQueue.Geometry;
            return mat;
        }
    }
}
