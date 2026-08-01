using UnityEngine;

namespace EduQuest
{
    /// <summary>Visible liquid inside a vessel — fill height + color.</summary>
    public class LiquidVolume : MonoBehaviour
    {
        [SerializeField] Transform liquid;
        [SerializeField] Renderer liquidRenderer;
        [SerializeField] float maxHeight = 0.11f;
        [SerializeField] float radius = 0.075f;
        [SerializeField] float baseY = 0.05f;

        float m_Fill = 1f;
        Color m_Color = new Color(0.7f, 0.85f, 1f);

        public float Fill => m_Fill;
        public Color Color => m_Color;
        public Transform Surface => liquid;

        public static LiquidVolume Ensure(Transform vessel, Color color, float fill, float maxHeight = 0.11f, float radius = 0.075f)
        {
            var lv = vessel.GetComponent<LiquidVolume>();
            if (lv == null) lv = vessel.gameObject.AddComponent<LiquidVolume>();
            lv.maxHeight = maxHeight;
            lv.radius = radius;
            lv.EnsureMesh();
            lv.SetLiquid(color, fill, instant: true);
            return lv;
        }

        public void EnsureMesh()
        {
            if (liquid != null && liquidRenderer != null) return;

            var existing = transform.Find("Substance") ?? transform.Find("MixLiquid");
            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                go.name = "Substance";
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name = "Substance";
                go.transform.SetParent(transform, false);
                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }

            liquid = go.transform;
            liquidRenderer = go.GetComponent<Renderer>();
            ApplyTransform();
        }

        public void SetLiquid(Color color, float fill, bool instant = false)
        {
            EnsureMesh();
            m_Color = color;
            m_Fill = Mathf.Clamp01(fill);

            if (liquidRenderer != null)
            {
                liquidRenderer.enabled = m_Fill > 0.02f;
                liquidRenderer.sharedMaterial = LabMaterials.Solid(m_Color, 0.45f);
            }

            if (instant)
                ApplyTransform();
        }

        public void SetFill(float fill, bool instant = false)
        {
            SetLiquid(m_Color, fill, instant);
        }

        public void SetColor(Color color, bool instant = false)
        {
            SetLiquid(color, m_Fill, instant);
        }

        void LateUpdate()
        {
            ApplyTransform();
        }

        void ApplyTransform()
        {
            if (liquid == null) return;
            var h = Mathf.Max(0.01f, maxHeight * Mathf.Max(0.05f, m_Fill));
            liquid.localPosition = new Vector3(0f, baseY + h * 0.5f, 0f);
            liquid.localRotation = Quaternion.identity;
            liquid.localScale = new Vector3(radius * 2f, h * 0.5f, radius * 2f);
        }

        public Vector3 GetSpoutWorldPos()
        {
            // Approximate spout: top rim of vessel
            var r = GetComponentsInChildren<Renderer>();
            if (r.Length == 0) return transform.position + Vector3.up * 0.25f;
            var b = r[0].bounds;
            for (var i = 1; i < r.Length; i++)
            {
                if (r[i].gameObject.name is "Substance" or "MixLiquid" or "Label" or "Shadow") continue;
                b.Encapsulate(r[i].bounds);
            }
            return new Vector3(b.center.x, b.max.y, b.center.z);
        }

        public Vector3 GetMouthWorldPos()
        {
            return GetSpoutWorldPos();
        }
    }
}
