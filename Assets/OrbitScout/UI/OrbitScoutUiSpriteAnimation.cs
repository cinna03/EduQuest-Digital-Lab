using UnityEngine;
using UnityEngine.UI;

namespace OrbitScout.UI
{
    /// <summary>
    /// Simple UI flipbook player for sticker / GIF-style decorations.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class OrbitScoutUiSpriteAnimation : MonoBehaviour
    {
        [Header("Flipbook")]
        [SerializeField] Sprite[] frames;
        [SerializeField] float framesPerSecond = 12f;
        [SerializeField] bool loop = true;

        [Header("Idle motion (edit in Inspector)")]
        [SerializeField] float bobAmplitude;
        [SerializeField] float bobSpeed = 1.6f;
        [SerializeField] float pulseScale;

        Image image;
        RectTransform rect;
        Vector2 basePos;
        Vector3 baseScale;
        int frameIndex;
        float elapsed;
        float bobPhase;
        bool playing = true;

        public void Configure(Sprite[] animationFrames, float fps, float bob = 0f, float pulse = 0f)
        {
            frames = animationFrames;
            framesPerSecond = Mathf.Max(0.5f, fps);
            bobAmplitude = bob;
            pulseScale = pulse;
            frameIndex = 0;
            elapsed = 0f;
            EnsureRefs();
            if (frames != null && frames.Length > 0 && image != null)
                image.sprite = frames[0];
        }

        void Awake()
        {
            EnsureRefs();
        }

        void OnEnable()
        {
            EnsureRefs();
            if (rect != null)
            {
                basePos = rect.anchoredPosition;
                baseScale = rect.localScale;
            }
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
        }

        void EnsureRefs()
        {
            if (image == null)
                image = GetComponent<Image>();
            if (rect == null)
                rect = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (!playing)
                return;

            EnsureRefs();

            if (frames != null && frames.Length > 1 && image != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float frameTime = 1f / framesPerSecond;
                while (elapsed >= frameTime)
                {
                    elapsed -= frameTime;
                    frameIndex++;
                    if (frameIndex >= frames.Length)
                        frameIndex = loop ? 0 : frames.Length - 1;
                    image.sprite = frames[frameIndex];
                    if (!loop && frameIndex >= frames.Length - 1)
                        break;
                }
            }

            if (rect == null)
                return;

            if (bobAmplitude > 0.01f)
            {
                bobPhase += Time.unscaledDeltaTime * bobSpeed;
                rect.anchoredPosition = basePos + new Vector2(0f, Mathf.Sin(bobPhase) * bobAmplitude);
            }

            if (pulseScale > 0.001f)
            {
                float s = 1f + Mathf.Sin(bobPhase * 0.85f + 0.4f) * pulseScale;
                rect.localScale = baseScale * s;
            }
        }
    }
}
