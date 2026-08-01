using UnityEngine;

namespace EduQuest
{
    /// <summary>World-space label over a vessel; faces the camera.</summary>
    public class BottleLabel : MonoBehaviour
    {
        TextMesh m_Mesh;
        TextMesh m_Shadow;
        Camera m_Cam;

        public static BottleLabel Create(Transform parent, string text, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0.48f, 0f);

            var label = go.AddComponent<BottleLabel>();
            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = 64;
            tm.characterSize = 0.024f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;

            var shadowGo = new GameObject("Shadow");
            shadowGo.transform.SetParent(go.transform, false);
            shadowGo.transform.localPosition = new Vector3(0.006f, -0.006f, 0.025f);
            var shadow = shadowGo.AddComponent<TextMesh>();
            shadow.fontSize = 64;
            shadow.characterSize = 0.024f;
            shadow.anchor = TextAnchor.LowerCenter;
            shadow.alignment = TextAlignment.Center;
            shadow.fontStyle = FontStyle.Bold;
            shadow.color = new Color(0f, 0f, 0f, 0.8f);

            label.m_Mesh = tm;
            label.m_Shadow = shadow;
            label.SetText(text, color);
            return label;
        }

        public void SetText(string text, Color color)
        {
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
