#if UNITY_EDITOR
using UnityEditor;

namespace EduQuest.EditorTools
{
    /// <summary>Ensures glass PNGs import as Sprites for the UI.</summary>
    public static class GlassSpriteImporter
    {
        [MenuItem("EduQuest/Reimport Glass UI Sprites")]
        public static void Reimport()
        {
            Import("Assets/UI/Glass/glass_label.png");
            Import("Assets/UI/Glass/glass_panel.png");
            Import("Assets/Resources/UI/Glass/glass_label.png");
            Import("Assets/Resources/UI/Glass/glass_panel.png");
            AssetDatabase.Refresh();
        }

        static void Import(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        [InitializeOnLoadMethod]
        static void AutoImport()
        {
            // Soft auto-fix on load if still Default texture type
            TrySoft("Assets/Resources/UI/Glass/glass_label.png");
            TrySoft("Assets/Resources/UI/Glass/glass_panel.png");
        }

        static void TrySoft(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType == TextureImporterType.Sprite) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
#endif
