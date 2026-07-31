#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EduQuest.EditorTools
{
    /// <summary>
    /// One-click performance pass used for the summative GDD evidence.
    /// Menu: EduQuest → Apply Performance Pass
    /// </summary>
    public static class PerformanceSetup
    {
        [MenuItem("EduQuest/Apply Performance Pass", priority = 20)]
        public static void Apply()
        {
            var report = new StringBuilder();
            report.AppendLine("EduQuest Performance Pass");
            report.AppendLine("FPS benchmark target: 60 FPS (desktop Editor / standalone)");
            report.AppendLine();

            // 1) Static batching candidates — mark non-animated mesh roots as Static
            var marked = 0;
            foreach (var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include))
            {
                if (renderer == null) continue;
                var go = renderer.gameObject;
                // Skip moving experiment parts
                var n = go.name;
                if (n is "Bob" or "Cord" or "Seed" or "Sprout" or "Leaves" or "Bubbles" or "Flame" or "Liquid")
                    continue;
                if (go.GetComponentInParent<ParticleSystem>() != null)
                    continue;

                GameObjectUtility.SetStaticEditorFlags(go,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.ContributeGI);
                marked++;
                EditorUtility.SetDirty(go);
            }

            report.AppendLine($"• Static batching flags set on {marked} mesh objects (pots/platform/static props).");
            report.AppendLine("• Dynamic objects (bob, sprout, particles) left non-static on purpose.");

            // 2) Mipmaps on textures
            var textures = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || path.Contains("TutorialInfo")) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                if (!importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = true;
                    importer.SaveAndReimport();
                    textures++;
                }
            }

            report.AppendLine(textures > 0
                ? $"• Enabled mipmaps on {textures} texture(s)."
                : "• Mipmaps: project textures already use Generate Mip Maps (or use procedurals/URP lit defaults).");

            // 3) Ensure runtime FPS target component
            var perf = Object.FindAnyObjectByType<LabPerformanceSettings>();
            if (perf == null)
            {
                var go = new GameObject("LabPerformanceSettings");
                go.AddComponent<LabPerformanceSettings>();
                Undo.RegisterCreatedObjectUndo(go, "Add LabPerformanceSettings");
                report.AppendLine("• Added LabPerformanceSettings (Application.targetFrameRate = 60).");
            }
            else
            {
                report.AppendLine("• LabPerformanceSettings already present (60 FPS target).");
            }

            // 4) Player settings reminders (cannot set all from here safely)
            report.AppendLine();
            report.AppendLine("Also verify in Project Settings:");
            report.AppendLine("• Player → Other → Static Batching = ON, Dynamic Batching = ON (or URP SRP Batcher).");
            report.AppendLine("• Player → Configuration → Scripting Backend = IL2CPP for device builds.");
            report.AppendLine("• Occlusion Culling = NOT used (tiny single-room lab; unjustified bake cost).");
            report.AppendLine();
            report.AppendLine("Profiler capture for GDD:");
            report.AppendLine("1) Window → Analysis → Profiler");
            report.AppendLine("2) Play → record CPU/GPU while switching all 3 labs");
            report.AppendLine("3) Screenshot → paste into GDD Performance Considerations");

            if (EditorSceneManager.GetActiveScene().isDirty)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log(report.ToString());
            EditorUtility.DisplayDialog("EduQuest Performance Pass", report.ToString(), "OK");
        }
    }
}
#endif
