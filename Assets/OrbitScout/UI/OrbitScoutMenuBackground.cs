using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Paths and helpers for menu / briefing background art.
    /// </summary>
    public static class OrbitScoutMenuBackground
    {
        public const string MenuPanelBackgroundAssetPath = "Assets/OrbitScout/UI/Visuals/Planet_background.png";
        public const string PlanetBackgroundResourcePath = "OrbitScout/Planet_background";
        public const string PlanetBackgroundAssetPath = "Assets/OrbitScout/UI/Visuals/Planet_background.png";
        public const string LevelSelectBackgroundAssetPath = "Assets/OrbitScout/UI/Visuals/LevelSelect_starfield.png";
        public const string LevelSelectBackgroundResourcePath = "OrbitScout/LevelSelect_starfield";
        public const string WalkthroughBackgroundAssetPath = "Assets/OrbitScout/UI/Visuals/Walkthrough_starfield.png";
        public const string WalkthroughBackgroundResourcePath = "OrbitScout/Walkthrough_starfield";

        public static Sprite LoadMenuBackgroundSprite()
        {
            return Resources.Load<Sprite>(PlanetBackgroundResourcePath);
        }

        public static Sprite LoadLevelSelectBackgroundSprite()
        {
            return Resources.Load<Sprite>(LevelSelectBackgroundResourcePath);
        }

        public static void ApplyToPanel(GameObject panel)
        {
            if (panel == null)
                return;

            Sprite sprite = LoadMenuBackgroundSprite();
            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.AddComponent<Image>();

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }

            image.color = Color.white;
            image.raycastTarget = false;
        }

        public static void ApplyLevelSelectBackground(GameObject panel)
        {
            if (panel == null)
                return;

            Sprite sprite = LoadLevelSelectBackgroundSprite();
#if UNITY_EDITOR
            if (sprite == null)
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(LevelSelectBackgroundAssetPath);
#endif
            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.AddComponent<Image>();

            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
            }

            image.color = Color.white;
            image.raycastTarget = false;
        }
    }
}
