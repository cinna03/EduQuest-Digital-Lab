using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace EduQuest.AR
{
    /// <summary>
    /// Desktop: webcam luminance.
    /// Phone AR: AR Foundation light estimation (no second WebCamTexture fight).
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
        bool m_UseArLight;
        ARCameraManager m_ArCamera;

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
                string mode = m_UseArLight ? "AR" : "CAM";
                if (IsBright) return $"{mode} BRIGHT";
                if (IsDark) return $"{mode} DARK";
                return $"{mode} DIM";
            }
        }

        public IEnumerator StartSensor()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (TryStartArLight())
            {
                if (cameraPreview != null)
                {
                    cameraPreview.texture = null;
                    cameraPreview.color = new Color(0.08f, 0.12f, 0.16f, 0.9f);
                }
                yield break;
            }
#endif
            yield return StartWebcam();
        }

        bool TryStartArLight()
        {
            m_ArCamera = FindAnyObjectByType<ARCameraManager>();
            if (m_ArCamera == null || !m_ArCamera.gameObject.activeInHierarchy)
                return false;

            m_UseArLight = true;
            m_ArCamera.frameReceived += OnArFrame;
            m_Ready = true; // becomes meaningful when first estimation arrives
            m_Brightness = 0.35f;
            Debug.Log("WorldLightSensor: using AR light estimation.");
            return true;
        }

        void OnArFrame(ARCameraFrameEventArgs args)
        {
            float target = m_Brightness;
            var le = args.lightEstimation;
            if (le.averageBrightness.HasValue)
                target = Mathf.Clamp01(le.averageBrightness.Value);
            else if (le.averageIntensityInLumens.HasValue)
                target = Mathf.Clamp01(le.averageIntensityInLumens.Value / 1200f);
            else
                return;

            m_Brightness = Mathf.Lerp(m_Brightness, target, 1f - Mathf.Exp(-smooth * Time.deltaTime));
            m_Ready = true;
        }

        IEnumerator StartWebcam()
        {
            m_UseArLight = false;
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
            if (m_UseArLight || m_Webcam == null || !m_Webcam.isPlaying || !m_Webcam.didUpdateThisFrame)
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

                long sum = 0;
                int count = 0;
                int stepX = Mathf.Max(1, w / sampleSize);
                int stepY = Mathf.Max(1, h / sampleSize);
                for (int y = 0; y < h; y += stepY)
                {
                    for (int x = 0; x < w; x += stepX)
                    {
                        var c = m_Pixels[y * w + x];
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
            if (m_ArCamera != null)
                m_ArCamera.frameReceived -= OnArFrame;

            if (m_Webcam != null)
            {
                m_Webcam.Stop();
                Destroy(m_Webcam);
            }
        }

#if UNITY_EDITOR
        public void EditorDebugSetBrightness(float value)
        {
            m_Brightness = Mathf.Clamp01(value);
            m_Ready = true;
        }
#endif
    }
}
