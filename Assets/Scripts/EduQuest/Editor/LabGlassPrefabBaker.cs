#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EduQuest.EditorTools
{
    /// <summary>
    /// Turns imported FBX glassware into Resources prefabs with phone-safe glass materials.
    /// Menu: EduQuest → Art → Bake Lab Glassware Prefabs
    /// </summary>
    public static class LabGlassPrefabBaker
    {
        const string ModelsDir = "Assets/Art/LabGlassware/Models";
        const string PrefabDir = "Assets/Resources/EduQuest/Prefabs";
        const string GlassMatPath = "Assets/Resources/EduQuest/Materials/LabGlass.mat";

        static readonly (string fbx, string prefab, float targetHeight)[] Items =
        {
            ("beaker.fbx", "Beaker", 0.22f),
            ("erlenmeyer.fbx", "Erlenmeyer", 0.24f),
            ("florence.fbx", "Florence", 0.32f),
            ("graduated_cylinder.fbx", "GraduatedCylinder", 0.28f),
            ("round_bottom.fbx", "RoundBottom", 0.26f),
            ("reagent_bottle.fbx", "ReagentBottle", 0.2f),
        };

        [MenuItem("EduQuest/Art/Bake Lab Glassware Prefabs", priority = 20)]
        public static void Bake()
        {
            var built = BakeQuiet();
            EditorUtility.DisplayDialog(
                "Lab Glassware Prefabs",
                $"Baked {built} prefabs into:\n{PrefabDir}\n\nClear glass, no printed labels.",
                "OK");
        }

        public static int BakeQuiet()
        {
            AssetDatabase.Refresh();
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory("Assets/Resources/EduQuest/Materials");

            var glass = EnsureGlassMaterial();
            var built = 0;

            foreach (var item in Items)
            {
                var modelPath = $"{ModelsDir}/{item.fbx}";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (model == null)
                {
                    Debug.LogWarning($"[EduQuest] Missing model: {modelPath}");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                if (instance == null)
                    instance = Object.Instantiate(model);

                instance.name = item.prefab;
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;

                ApplyGlass(instance, glass);
                NormalizeToTable(instance, item.targetHeight);
                StripColliders(instance);

                PrefabUtility.SaveAsPrefabAsset(instance, $"{PrefabDir}/{item.prefab}.prefab");
                Object.DestroyImmediate(instance);
                built++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return built;
        }

        static Material EnsureGlassMaterial()
        {
            var src = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Resources/EduQuest/Materials/LabLit.mat");
            if (src == null)
            {
                src = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/MobileARTemplateAssets/Materials/ObjectMaterial.mat");
            }

            var mat = AssetDatabase.LoadAssetAtPath<Material>(GlassMatPath);
            if (mat == null)
            {
                mat = src != null
                    ? new Material(src)
                    : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, GlassMatPath);
            }
            else if (src != null)
            {
                mat.shader = src.shader;
            }

            // Opaque glass look (phone-safe): pale cyan + high polish
            var color = new Color(0.78f, 0.9f, 0.96f, 1f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.92f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.08f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        static void ApplyGlass(GameObject root, Material glass)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                r.sharedMaterial = glass;
        }

        static void NormalizeToTable(GameObject root, float targetHeight)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            if (bounds.size.y < 0.0001f) return;

            var scale = targetHeight / bounds.size.y;
            root.transform.localScale *= scale;

            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var dy = -bounds.min.y;
            foreach (Transform child in root.transform)
                child.localPosition += new Vector3(0f, dy / Mathf.Max(0.0001f, root.transform.localScale.y), 0f);
        }

        static void StripColliders(GameObject root)
        {
            foreach (var c in root.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(c);
        }
    }
}
#endif
