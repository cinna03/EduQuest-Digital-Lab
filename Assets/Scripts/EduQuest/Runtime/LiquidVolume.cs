using UnityEngine;

namespace EduQuest
{
    /// <summary>Inset liquid cylinder + meniscus disc so fill level is always readable.</summary>
    public class LiquidVolume : MonoBehaviour
    {
        [SerializeField] Transform liquid;
        [SerializeField] Transform meniscus;
        [SerializeField] Renderer liquidRenderer;
        [SerializeField] Renderer meniscusRenderer;
        [SerializeField] float maxHeight = 0.14f;
        [SerializeField] float radius = 0.04f;
        [SerializeField] float baseY = 0.03f;

        float m_Fill = 1f;
        Color m_Color = new Color(0.7f, 0.85f, 1f);

        public float Fill => m_Fill;
        public Color Color => m_Color;
        public Transform Surface => liquid;

        public static LiquidVolume Ensure(Transform vessel, Color color, float fill, float maxHeight = 0.14f, float radius = 0.04f)
        {
            var lv = vessel.GetComponent<LiquidVolume>();
            bool created = lv == null;
            if (created) lv = vessel.gameObject.AddComponent<LiquidVolume>();

            if (created || maxHeight < lv.maxHeight)
                lv.maxHeight = maxHeight;
            if (created || radius < lv.radius)
                lv.radius = radius;

            lv.EnsureMesh();
            lv.SetLiquid(color, fill, instant: true);
            return lv;
        }

        public void SetBaseY(float y) => baseY = y;

        public void EnsureMesh()
        {
            var existing = transform.Find("Substance");
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "Substance";
                go.transform.SetParent(transform, false);
                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }

            var oldMix = transform.Find("MixLiquid");
            if (oldMix != null) oldMix.gameObject.SetActive(false);

            liquid = go.transform;
            liquidRenderer = go.GetComponent<Renderer>();
            if (liquidRenderer != null)
                liquidRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            EnsureMeniscus();
            ApplyTransform();
        }

        void EnsureMeniscus()
        {
            var existing = transform.Find("Meniscus");
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "Meniscus";
                go.transform.SetParent(transform, false);
                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }

            meniscus = go.transform;
            meniscusRenderer = go.GetComponent<Renderer>();
            if (meniscusRenderer != null)
                meniscusRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public void SetLiquid(Color color, float fill, bool instant = false)
        {
            EnsureMesh();
            m_Color = color;
            m_Fill = Mathf.Clamp01(fill);

            bool visible = m_Fill > 0.02f;
            if (liquidRenderer != null)
            {
                liquidRenderer.enabled = visible;
                liquidRenderer.sharedMaterial = LabMaterials.Liquid(m_Color);
            }

            if (meniscusRenderer != null)
            {
                meniscusRenderer.enabled = visible;
                meniscusRenderer.sharedMaterial = LabMaterials.Meniscus(m_Color);
            }

            ApplyTransform();
        }

        public void SetFill(float fill, bool instant = false) => SetLiquid(m_Color, fill, instant);

        public void SetColor(Color color, bool instant = false) => SetLiquid(color, m_Fill, instant);

        void LateUpdate() => ApplyTransform();

        void ApplyTransform()
        {
            if (liquid == null) return;

            // Visible column height tracks fill (empty = hidden via renderer)
            var h = Mathf.Max(0.01f, maxHeight * Mathf.Max(0.05f, m_Fill));
            liquid.localPosition = new Vector3(0f, baseY + h * 0.5f, 0f);
            liquid.localRotation = Quaternion.identity;
            liquid.localScale = new Vector3(radius * 2f, h * 0.5f, radius * 2f);

            if (meniscus != null)
            {
                // Thin bright disc on the liquid surface — makes level changes obvious
                meniscus.localPosition = new Vector3(0f, baseY + h + 0.0015f, 0f);
                meniscus.localRotation = Quaternion.identity;
                meniscus.localScale = new Vector3(radius * 2.05f, 0.0025f, radius * 2.05f);
            }
        }

        public Vector3 GetSpoutWorldPos()
        {
            var body = transform.Find("GlassRim") ?? transform.Find("GlassBody");
            if (body != null)
            {
                var r = body.GetComponent<Renderer>();
                if (r != null)
                    return new Vector3(r.bounds.center.x, r.bounds.max.y, r.bounds.center.z);
            }
            return transform.position + Vector3.up * (baseY + maxHeight + 0.04f);
        }

        public Vector3 GetMouthWorldPos() => GetSpoutWorldPos();
    }
}