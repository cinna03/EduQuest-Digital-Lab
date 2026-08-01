using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Interactive glassware: hover label, select (glow + levitate), liquid, pour.
    /// </summary>
    public class ChemVessel : MonoBehaviour
    {
        public ChemRole Role = ChemRole.None;
        public string DisplayName = "";

        [SerializeField] float levitateHeight = 0.08f;
        [SerializeField] float moveSpeed = 8f;

        Vector3 m_RestLocalPos;
        Quaternion m_RestLocalRot;
        bool m_RestCached;
        bool m_Selected;
        bool m_Hover;
        bool m_PourLocked;
        BottleLabel m_Label;
        Light m_SelectGlow;
        Renderer[] m_Renderers;
        Color[] m_BaseColors;
        bool m_ColorsCached;
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
            CacheRest();
            SetLabelVisible(false);
            SetSelected(false, instant: true);
        }

        void Awake()
        {
            m_Renderers = GetComponentsInChildren<Renderer>(true);
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
                    m_SelectGlow.intensity = 1.6f + Mathf.Sin(Time.time * 6f) * 0.35f;
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
            // Snap back to rest basis after animation
            if (m_RestCached)
            {
                transform.localPosition = m_RestLocalPos + (m_Selected ? Vector3.up * levitateHeight : Vector3.zero);
                transform.localRotation = m_RestLocalRot;
            }
            ApplySelectTint(m_Selected);
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

            ApplySelectTint(selected);
        }

        public void ResetVisual()
        {
            m_Hover = false;
            m_PourLocked = false;
            SetSelected(false, instant: true);
            SetLabelVisible(false);
            if (m_Liquid != null)
                m_Liquid.SetLiquid(m_DefaultLiquidColor, m_DefaultFill, instant: true);
            ApplySelectTint(false);
        }

        public void ShowContamination(Color tint)
        {
            if (m_Liquid == null) EnsureLiquid();
            m_Liquid.SetLiquid(tint, Mathf.Max(0.55f, m_Liquid.Fill), instant: true);
        }

        public Color PourStreamColor() => m_Liquid != null ? m_Liquid.Color : m_DefaultLiquidColor;

        void EnsureLiquid()
        {
            DefaultLiquidForRole(Role, out m_DefaultLiquidColor, out m_DefaultFill, out var h, out var rad);
            m_Liquid = LiquidVolume.Ensure(transform, m_DefaultLiquidColor, m_DefaultFill, h, rad);
        }

        static void DefaultLiquidForRole(ChemRole role, out Color color, out float fill, out float height, out float radius)
        {
            height = 0.11f;
            radius = 0.07f;
            switch (role)
            {
                case ChemRole.SilverNitrate:
                    color = new Color(0.95f, 0.95f, 0.88f);
                    fill = 0.88f;
                    break;
                case ChemRole.SodiumChloride:
                    color = new Color(0.72f, 0.9f, 1f);
                    fill = 0.88f;
                    break;
                case ChemRole.Fixer:
                    color = new Color(1f, 0.68f, 0.12f);
                    fill = 0.88f;
                    break;
                case ChemRole.Distractor:
                    color = new Color(0.12f, 0.4f, 1f);
                    fill = 0.88f;
                    break;
                case ChemRole.ReactionBeaker:
                    color = new Color(0.75f, 0.88f, 0.95f);
                    fill = 0f; // starts empty
                    height = 0.13f;
                    radius = 0.09f;
                    break;
                default:
                    color = new Color(0.7f, 0.85f, 1f);
                    fill = 0.5f;
                    break;
            }
        }

        void EnsureCollider()
        {
            var col = GetComponent<Collider>();
            if (col != null) return;
            var box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.22f, 0.38f, 0.22f);
            box.center = new Vector3(0f, 0.16f, 0f);
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
            go.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            m_SelectGlow = go.AddComponent<Light>();
            m_SelectGlow.type = LightType.Point;
            m_SelectGlow.range = 0.55f;
            m_SelectGlow.color = new Color(1f, 0.95f, 0.55f);
            m_SelectGlow.intensity = 1.8f;
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

        void ApplySelectTint(bool selected)
        {
            if (m_Renderers == null || m_Renderers.Length == 0)
                m_Renderers = GetComponentsInChildren<Renderer>(true);

            if (!m_ColorsCached)
            {
                m_BaseColors = new Color[m_Renderers.Length];
                for (var i = 0; i < m_Renderers.Length; i++)
                {
                    var mat = m_Renderers[i] != null ? m_Renderers[i].material : null;
                    if (mat != null && mat.HasProperty("_BaseColor"))
                        m_BaseColors[i] = mat.GetColor("_BaseColor");
                    else if (mat != null && mat.HasProperty("_Color"))
                        m_BaseColors[i] = mat.GetColor("_Color");
                    else
                        m_BaseColors[i] = Color.white;
                }
                m_ColorsCached = true;
            }

            for (var i = 0; i < m_Renderers.Length; i++)
            {
                var r = m_Renderers[i];
                if (r == null) continue;
                if (r.gameObject.name is "Substance" or "MixLiquid") continue;
                var mat = r.material;
                var c = selected
                    ? Color.Lerp(m_BaseColors[i], new Color(1f, 0.92f, 0.4f), 0.45f)
                    : m_BaseColors[i];
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            }
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
