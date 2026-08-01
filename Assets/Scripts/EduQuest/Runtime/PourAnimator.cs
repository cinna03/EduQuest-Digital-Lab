using System;
using System.Collections;
using UnityEngine;

namespace EduQuest
{
    /// <summary>Tips a source vessel and draws a liquid stream into a target.</summary>
    public class PourAnimator : MonoBehaviour
    {
        public static PourAnimator Ensure(GameObject host)
        {
            var p = host.GetComponent<PourAnimator>();
            if (p == null) p = host.AddComponent<PourAnimator>();
            return p;
        }

        public bool IsPouring { get; private set; }

        public void Play(
            ChemVessel source,
            ChemVessel target,
            Color streamColor,
            float sourceFillAfter,
            Color targetColor,
            float targetFillAfter,
            Action onComplete)
        {
            if (IsPouring)
            {
                onComplete?.Invoke();
                return;
            }
            StartCoroutine(PourRoutine(source, target, streamColor, sourceFillAfter, targetColor, targetFillAfter, onComplete));
        }

        IEnumerator PourRoutine(
            ChemVessel source,
            ChemVessel target,
            Color streamColor,
            float sourceFillAfter,
            Color targetColor,
            float targetFillAfter,
            Action onComplete)
        {
            IsPouring = true;
            source.BeginPourLock();
            target.BeginPourLock();

            var srcLiq = source.Liquid;
            var dstLiq = target.Liquid;
            if (srcLiq == null) srcLiq = LiquidVolume.Ensure(source.transform, streamColor, 0.85f);
            if (dstLiq == null) dstLiq = LiquidVolume.Ensure(target.transform, targetColor, 0.05f);

            float startSrcFill = srcLiq.Fill;
            float startDstFill = dstLiq.Fill;
            Color startDstColor = dstLiq.Color;

            // Raise source a bit for the pour
            var srcRest = source.transform.localPosition;
            var srcRaised = srcRest + Vector3.up * 0.12f;
            var srcRestRot = source.transform.localRotation;

            // Tip toward target
            var worldDir = target.transform.position - source.transform.position;
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.0001f) worldDir = source.transform.forward;
            worldDir.Normalize();
            var tipAxis = Vector3.Cross(Vector3.up, worldDir);
            if (tipAxis.sqrMagnitude < 0.0001f) tipAxis = source.transform.right;
            tipAxis.Normalize();
            var tipRot = Quaternion.AngleAxis(58f, tipAxis) * srcRestRot;

            // Stream visual
            var streamGo = new GameObject("PourStream");
            var lr = streamGo.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = 0.012f;
            lr.endWidth = 0.008f;
            lr.numCapVertices = 4;
            lr.material = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
            lr.startColor = streamColor;
            lr.endColor = new Color(streamColor.r, streamColor.g, streamColor.b, 0.55f);
            lr.textureMode = LineTextureMode.Stretch;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            // Arc mid control via extra point
            lr.positionCount = 3;

            // Lift
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.28f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                source.transform.localPosition = Vector3.Lerp(srcRest, srcRaised, k);
                yield return null;
            }

            // Tip + stream
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.35f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                source.transform.localRotation = Quaternion.Slerp(srcRestRot, tipRot, k);
                UpdateStream(lr, source, target, streamColor, k);
                yield return null;
            }

            // Pour transfer
            t = 0f;
            const float pourDur = 0.85f;
            while (t < 1f)
            {
                t += Time.deltaTime / pourDur;
                var k = Mathf.SmoothStep(0f, 1f, t);
                srcLiq.SetFill(Mathf.Lerp(startSrcFill, sourceFillAfter, k), instant: true);
                dstLiq.SetLiquid(
                    Color.Lerp(startDstColor, targetColor, k),
                    Mathf.Lerp(startDstFill, targetFillAfter, k),
                    instant: true);
                UpdateStream(lr, source, target, streamColor, 1f);
                // Pulse stream width
                var w = 0.01f + Mathf.Sin(Time.time * 24f) * 0.004f;
                lr.startWidth = w + 0.004f;
                lr.endWidth = w;
                yield return null;
            }

            srcLiq.SetFill(sourceFillAfter, instant: true);
            dstLiq.SetLiquid(targetColor, targetFillAfter, instant: true);

            // Untip
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                source.transform.localRotation = Quaternion.Slerp(tipRot, srcRestRot, k);
                lr.startColor = new Color(streamColor.r, streamColor.g, streamColor.b, 1f - k);
                lr.endColor = new Color(streamColor.r, streamColor.g, streamColor.b, 0.55f * (1f - k));
                UpdateStream(lr, source, target, streamColor, 1f - k);
                yield return null;
            }

            // Lower
            t = 0f;
            var fromPos = source.transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.25f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                source.transform.localPosition = Vector3.Lerp(fromPos, srcRest, k);
                yield return null;
            }

            source.transform.localPosition = srcRest;
            source.transform.localRotation = srcRestRot;
            if (streamGo != null) Destroy(streamGo);

            source.EndPourLock();
            target.EndPourLock();
            IsPouring = false;
            onComplete?.Invoke();
        }

        static void UpdateStream(LineRenderer lr, ChemVessel source, ChemVessel target, Color color, float strength)
        {
            if (lr == null) return;
            var a = source.Liquid != null ? source.Liquid.GetSpoutWorldPos() : source.transform.position + Vector3.up * 0.25f;
            var b = target.Liquid != null ? target.Liquid.GetMouthWorldPos() : target.transform.position + Vector3.up * 0.22f;
            // Offset spout in pour direction slightly
            var mid = Vector3.Lerp(a, b, 0.5f) + Vector3.down * 0.04f;
            lr.SetPosition(0, a);
            lr.SetPosition(1, mid);
            lr.SetPosition(2, b);
            var c = color;
            c.a = Mathf.Clamp01(strength);
            lr.startColor = c;
            lr.endColor = new Color(c.r, c.g, c.b, c.a * 0.5f);
        }
    }
}
