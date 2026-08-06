using OrbitScout.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Star level node on the mission map. Hover/tap shows name; click starts the level if unlocked.
    /// </summary>
    public sealed class OrbitScoutLevelMapNode : OrbitScoutLevelCardButton,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler
    {
        [SerializeField] Image fillImage;
        [SerializeField] Image glowImage;
        [SerializeField] Image lockImage;
        [SerializeField] TMP_Text numeralText;

        OrbitScoutLevelMapController map;
        bool unlocked = true;

        public void Bind(
            OrbitScoutLevelMapController controller,
            LevelId levelId,
            Image fill,
            Image glow,
            Image lockIcon,
            TMP_Text numeral)
        {
            map = controller;
            level = levelId;
            fillImage = fill;
            glowImage = glow;
            lockImage = lockIcon;
            numeralText = numeral;
            if (numeralText != null)
                numeralText.text = OrbitScoutLevelBriefings.RomanNumeral(levelId);
        }

        public void SetUnlocked(bool isUnlocked)
        {
            unlocked = isUnlocked;

            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = true;
                button.enabled = true;
            }

            Image hit = GetComponent<Image>();
            if (hit != null)
                hit.raycastTarget = true;

            // Bright white/lavender sparkle when unlocked; muted purple when locked
            Color star = isUnlocked
                ? new Color(1f, 0.96f, 1f, 1f)
                : new Color(0.55f, 0.42f, 0.72f, 0.55f);
            Color glow = isUnlocked
                ? new Color(0.85f, 0.70f, 1f, 0.4f)
                : new Color(0.4f, 0.3f, 0.55f, 0.15f);
            Color numeral = isUnlocked
                ? new Color(0.42f, 0.22f, 0.62f, 1f)
                : new Color(0.55f, 0.45f, 0.65f, 0.75f);

            if (fillImage != null)
                fillImage.color = star;
            if (glowImage != null)
            {
                glowImage.gameObject.SetActive(true);
                glowImage.color = glow;
            }
            if (numeralText != null)
                numeralText.color = numeral;
            if (lockImage != null)
            {
                lockImage.gameObject.SetActive(!isUnlocked);
                lockImage.color = new Color(0.85f, 0.75f, 1f, 0.95f);
            }
        }

        public bool IsUnlocked => unlocked;

        public void OnPointerEnter(PointerEventData eventData)
        {
            map?.ShowHover(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            map?.HideHover(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            map?.ShowHover(this);
        }
    }
}
