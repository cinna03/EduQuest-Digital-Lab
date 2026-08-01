using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Interactive glassware: hover label, select (glow + levitate), liquid, pour.
    /// Selection does NOT recolor the glass — only light + lift.
    /// </summary>
    public class ChemVessel : MonoBehaviour
    {
        public ChemRole Role = ChemRole.None;
        public string DisplayName = "";

        [SerializeField] float levitateHeight = 0.07f;
        [SerializeField] float moveSpeed = 8f;

        Vector3 m_RestLocalPos;
        Quaternion m_RestLocalRot = Quaternion.identity;
        bool m_RestCached;
        bool m_Selected;
        bool m_Hover;
        bool m_PourLocked;
        BottleLabel m_Label;
        Light m_SelectGlow;
        LiquidVolume m_Liquid;
        Color m_DefaultLiquidColor;
        float m_DefaultFill;

        public bool IsSelected => m_Selected;
        public bool IsPourLocked => m_PourLocked;
        public LiquidVolume Liquid => m_Liquid;
        public ChemClickable Clickable { get; private set; }

        public void Configure(ChemRole role, string displayName)
        {
            Role = role;
            DisplayName = displayName;

            Clickable = GetComponent<ChemClickable>();
            if (Clickable == null) Clickable = gameObject.AddComponent<ChemClickable>();
            Clickable.Configure(role, displayName);

            EnsureCollider();
            EnsureLabel();
            EnsureGlow();
            EnsureLiquid();
            ForceGlassMaterials();
            CacheRest();
            SetLabelVisible(false);
            SetSelected(false, instant: true);
        }

        void LateUpdate()
        {
            if (m_PourLocked) return;
            if (!m_RestCached) CacheRest();

            var target = m_RestLocalPos + (m_Selected ? Vector3.up * levitateHeight : Vector3.zero);
            transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * moveSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_RestLocalRot, Time.deltaTime * moveSpeed);

            if (m_SelectGlow != null)
            {
                m_SelectGlow.enabled = m_Selected;
                if (m_Selected)
                    m_SelectGlow.intensity = 1.8f + Mathf.Sin(Time.time * 7f) * 0.4f;
            }

            SetLabelVisible(m_Hover || m_Selected);
        }

        public void BeginPourLock()
        {
            m_PourLocked = true;
            if (m_SelectGlow != null) m_SelectGlow.enabled = false;
        }

        public void EndPourLock()
        {
            m_PourLocked = false;
            if (m_RestCached)
            {
                transform.localPosition = m_RestLocalPos + (m_Selected ? Vector3.up * levitateHeight : Vector3.zero);
                transform.localRotation = m_RestLocalRot;
            }
        }

        public void SetHover(bool hover) => m_Hover = hover;

        public void SetSelected(bool selected, bool instant = false)
        {
            m_Selected = selected;
            if (!m_RestCached) CacheRest();

            if (instant && !m_PourLocked)
            {
                transform.localPosition = m_RestLocalPos + (selected ? Vector3.up * levitateHeight : Vector3.zero);
                transform.localRotation = m_RestLocalRot;
            }

            if (m_SelectGlow != null)
                m_SelectGlow.enabled = selected && !m_PourLocked;
        }

        public void ResetVisual()
        {
            m_Hover = false;
            m_PourLocked = false;
            SetSelected(false, instant: true);
            SetLabelVisible(false);
            if (m_Liquid != null)
                m_Liquid.SetLiquid(m_DefaultLiquidColor, m_DefaultFill, instant: true);
        }

        public void ShowContamination(Color tint)
        {
            if (m_Liquid == null) EnsureLiquid();
            m_Liquid.SetLiquid(tint, Mathf.Max(0.55f, m_Liquid.Fill), instant: true);
        }

        public Color PourStreamColor() => m_Liquid != null ? m_Liquid.Color : m_DefaultLiquidColor;

        void EnsureLiquid()
        {
            DefaultLiquidForRole(Role, out m_DefaultLiquidColor, out m_DefaultFill);

            m_Liquid = GetComponent<LiquidVolume>();
            if (m_Liquid != null)
            {
                // Keep builder dimensions; only set color/fill
                m_Liquid.SetLiquid(m_DefaultLiquidColor, m_DefaultFill, instant: true);
                return;
            }

            m_Liquid = LiquidVolume.Ensure(transform, m_DefaultLiquidColor, m_DefaultFill, 0.14f, 0.04f);
        }

        static void DefaultLiquidForRole(ChemRole role, out Color color, out float fill)
        {
            switch (role)
            {
                case ChemRole.SilverNitrate:
                    color = new Color(0.93f, 0.94f, 0.9f); // near-clear
                    fill = 0.75f;
                    break;
                case ChemRole.SodiumChloride:
                    color = new Color(0.55f, 0.82f, 1f);
                    fill = 0.75f;
                    break;
                case ChemRole.Fixer:
                    color = new Color(1f, 0.72f, 0.15f);
                    fill = 0.75f;
                    break;
                case ChemRole.Distractor:
                    color = new Color(0.15f, 0.4f, 0.95f);
                    fill = 0.75f;
                    break;
                case ChemRole.ReactionBeaker:
                    color = new Color(0.75f, 0.88f, 0.95f);
                    fill = 0f;
                    break;
                default:
                    color = new Color(0.7f, 0.85f, 1f);
                    fill = 0.5f;
                    break;
            }
        }

        void ForceGlassMaterials()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                if (r.gameObject.name is "Substance" or "MixLiquid") continue;
                if (r.gameObject.name == "GlassRim" || r.gameObject.name == "GlassBase")
                    r.sharedMaterial = LabMaterials.GlassRim();
                else
                    r.sharedMaterial = LabMaterials.GlassShell();
            }
        }

        void EnsureCollider()
        {
            var col = GetComponent<Collider>();
            if (col != null) return;
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.14f, 0.24f, 0.14f);
            box.center = new Vector3(0f, 0.12f, 0f);
        }

        void EnsureLabel()
        {
            if (m_Label != null) return;
            m_Label = GetComponentInChildren<BottleLabel>(true);
            if (m_Label == null)
                m_Label = BottleLabel.Create(transform, DisplayName, LabelColor(Role));
            else
                m_Label.SetText(DisplayName, LabelColor(Role));
        }

        void EnsureGlow()
        {
            var t = transform.Find("SelectGlow");
            if (t != null)
            {
                m_SelectGlow = t.GetComponent<Light>();
                return;
            }

            var go = new GameObject("SelectGlow");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            m_SelectGlow = go.AddComponent<Light>();
            m_SelectGlow.type = LightType.Point;
            m_SelectGlow.range = 0.45f;
            m_SelectGlow.color = new Color(1f, 0.95f, 0.6f);
            m_SelectGlow.intensity = 2f;
            m_SelectGlow.enabled = false;
        }

        void CacheRest()
        {
            m_RestLocalPos = transform.localPosition;
            m_RestLocalRot = Quaternion.identity;
            if (m_Selected)
                m_RestLocalPos -= Vector3.up * levitateHeight;
            m_RestCached = true;
        }

        public void RecacheRestFromCurrent()
        {
            m_RestLocalPos = transform.localPosition;
            m_RestLocalRot = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
            m_RestCached = true;
        }

        void SetLabelVisible(bool on)
        {
            if (m_Label == null) return;
            m_Label.gameObject.SetActive(on);
        }

        static Color LabelColor(ChemRole role) => role switch
        {
            ChemRole.SilverNitrate => Color.white,
            ChemRole.SodiumChloride => new Color(0.7f, 0.9f, 1f),
            ChemRole.Fixer => new Color(1f, 0.85f, 0.25f),
            ChemRole.Distractor => new Color(0.45f, 0.75f, 1f),
            ChemRole.ReactionBeaker => new Color(0.7f, 1f, 0.75f),
            _ => Color.white
        };
    }
}
