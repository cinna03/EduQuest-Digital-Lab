using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Imports trial fonts under Assets/OrbitScout/UI/Fonts and builds TMP Font Assets for testing.
/// </summary>
public static class OrbitScoutFontImportTools
{
    const string FontsFolder = "Assets/OrbitScout/UI/Fonts";
    const string TmpFolder = "Assets/OrbitScout/UI/Fonts/TMP";

    [MenuItem("Orbit Scout/Fonts/Create TMP Font Assets From Imported Fonts")]
    public static void CreateTmpFontAssets()
    {
        EnsureFolder(TmpFolder);

        string[] guids = AssetDatabase.FindAssets("t:Font", new[] { FontsFolder });
        int created = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith(TmpFolder))
                continue;

            Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (font == null)
                continue;

            string fileName = Path.GetFileNameWithoutExtension(path);
            string assetPath = TmpFolder + "/" + fileName + " SDF.asset";

            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                Debug.Log("TMP font already exists: " + assetPath);
                continue;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                font,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic);

            if (fontAsset == null)
            {
                Debug.LogWarning("Failed to create TMP font for " + path);
                continue;
            }

            AssetDatabase.CreateAsset(fontAsset, assetPath);

            if (fontAsset.material != null)
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            if (fontAsset.atlasTextures != null)
            {
                foreach (Texture2D atlas in fontAsset.atlasTextures)
                {
                    if (atlas != null)
                        AssetDatabase.AddObjectToAsset(atlas, fontAsset);
                }
            }

            EditorUtility.SetDirty(fontAsset);
            created++;
            Debug.Log("Created TMP font asset: " + assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Orbit Scout Fonts",
            "Created " + created + " TMP font asset(s) in:\n" + TmpFolder +
            "\n\nTo test: select any TextMeshProUGUI in the HUD → Font Asset → pick one from Fonts/TMP.",
            "OK");
    }

    [MenuItem("Orbit Scout/Fonts/Reveal Fonts Folder")]
    public static void RevealFontsFolder()
    {
        Object folder = AssetDatabase.LoadAssetAtPath<Object>(FontsFolder);
        if (folder != null)
        {
            Selection.activeObject = folder;
            EditorGUIUtility.PingObject(folder);
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
