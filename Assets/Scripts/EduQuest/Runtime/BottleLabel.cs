using UnityEngine;

namespace EduQuest
{
    /// <summary>World-space label just above a vessel rim; faces the camera.</summary>
    public class BottleLabel : MonoBehaviour
    {
        TextMesh m_Mesh;
        TextMesh m_Shadow;
        Camera m_Cam;

        public static BottleLabel Create(Transform parent, string text, Color color)
        {
            // Sit just above the glass rim (not floating half a meter up)
            float y = 0.26f;
            var rim = parent.Find("GlassRim");
            if (rim != null)
                y = rim.localPosition.y + 0.05f;
            else
            {
                var body = parent.Find("GlassBody");
                if (body != null)
                    y = body.localPosition.y + body.localScale.y + 0.05f;
            }

            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, y, 0f);

            var label = go.AddComponent<BottleLabel>();
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 48;
            tm.characterSize = 0.018f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            // Default font often lacks unicode subscripts — keep ASCII in display names

            var shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(go.transform, false);
            shadowGo.transform.localPosition = new Vector3(0.004f, -0.004f, 0.02f);
            var shadow = shadowGo.AddComponent<TextMesh>();
            shadow.fontSize = 48;
            shadow.characterSize = 0.018f;
            shadow.anchor = TextAnchor.LowerCenter;
            shadow.alignment = TextAlignment.Center;
            shadow.fontStyle = FontStyle.Bold;
            shadow.color = new Color(0f, 0f, 0f, 0.85f);

            label.m_Mesh = tm;
            label.m_Shadow = shadow;
            label.SetText(text, color);
            return label;
        }

        public void SetText(string text, Color color)
        {
            // Strip characters the default font may not draw
            text = (text ?? "")
                .Replace("₃", "3")
                .Replace("₄", "4")
                .Replace("·", " ")
                .Trim();

            if (m_Mesh != null)
            {
                m_Mesh.text = text;
                m_Mesh.color = color;
            }
            if (m_Shadow != null)
                m_Shadow.text = text;
        }

        void LateUpdate()
        {
            if (m_Cam == null) m_Cam = Camera.main;
            if (m_Cam == null) return;
            var toCam = m_Cam.transform.position - transform.position;
            if (toCam.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        }
    }
}
