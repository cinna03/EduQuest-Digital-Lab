using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.AR
{
    /// <summary>
    /// Reads real-world brightness from the device camera (AR light proxy).
    /// On phone builds this is the same idea as AR Foundation light estimation:
    /// the physical room controls the lab — not a UI slider.
    /// </summary>
    public class WorldLightSensor : MonoBehaviour
    {
        [SerializeField] RawImage cameraPreview;
        [SerializeField] int sampleSize = 48;
        [SerializeField] float smooth = 8f;

        WebCamTexture m_Webcam;
        Color32[] m_Pixels;
        float m_Brightness;
        bool m_Ready;

        /// <summary>0 = dark, 1 = very bright (smoothed).</summary>
        public float Brightness => m_Brightness;
        public bool IsReady => m_Ready;
        public bool IsBright => m_Brightness >= 0.55f;
        public bool IsDark => m_Brightness <= 0.22f;

        public void SetPreview(RawImage preview) => cameraPreview = preview;
        public string Label
        {
            get
            {
                if (!m_Ready) return "Camera starting…";
                if (IsBright) return "BRIGHT";
                if (IsDark) return "DARK";
                return "DIM";
            }
        }

        public IEnumerator StartSensor()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogWarning("Webcam permission denied — light lab needs the camera.");
                yield break;
            }

            var devices = WebCamTexture.devices;
            if (devices == null || devices.Length == 0)
            {
                Debug.LogWarning("No camera found.");
                yield break;
            }

            // Prefer back camera on phones (world-facing = AR view)
            string deviceName = devices[0].name;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    deviceName = devices[i].name;
                    break;
                }
            }

            m_Webcam = new WebCamTexture(deviceName, 640, 480, 30);
            m_Webcam.Play();

            if (cameraPreview != null)
            {
                cameraPreview.texture = m_Webcam;
                cameraPreview.color = Color.white;
            }

            // Wait until frames arrive
            float timeout = 5f;
            while (m_Webcam != null && !m_Webcam.didUpdateThisFrame && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            m_Ready = m_Webcam != null && m_Webcam.isPlaying;
        }

        void Update()
        {
            if (m_Webcam == null || !m_Webcam.isPlaying || !m_Webcam.didUpdateThisFrame)
                return;

            float raw = SampleBrightness();
            m_Brightness = Mathf.Lerp(m_Brightness, raw, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        }

        float SampleBrightness()
        {
            try
            {
                int w = m_Webcam.width;
                int h = m_Webcam.height;
                if (w < 16 || h < 16) return m_Brightness;

                if (m_Pixels == null || m_Pixels.Length != w * h)
                    m_Pixels = new Color32[w * h];

                m_Webcam.GetPixels32(m_Pixels);

                // Sample a grid across the frame (center-weighted world view)
                long sum = 0;
                int count = 0;
                int stepX = Mathf.Max(1, w / sampleSize);
                int stepY = Mathf.Max(1, h / sampleSize);
                for (int y = 0; y < h; y += stepY)
                {
                    for (int x = 0; x < w; x += stepX)
                    {
                        var c = m_Pixels[y * w + x];
                        // Perceived luminance
                        sum += (long)(0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b);
                        count++;
                    }
                }

                if (count == 0) return m_Brightness;
                return Mathf.Clamp01(sum / (count * 255f));
            }
            catch
            {
                return m_Brightness;
            }
        }

        void OnDestroy()
        {
            if (m_Webcam != null)
            {
                m_Webcam.Stop();
                Destroy(m_Webcam);
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor-only test hook when no webcam is available.</summary>
        public void EditorDebugSetBrightness(float value)
        {
            m_Brightness = Mathf.Clamp01(value);
            m_Ready = true;
        }
#endif
    }
}
