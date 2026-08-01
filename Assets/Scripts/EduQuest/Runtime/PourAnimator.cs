using System;
using System.Collections;
using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Levitates the source vessel over the target, tips, streams liquid in, then returns home.
    /// </summary>
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

            var srcT = source.transform;
            var dstT = target.transform;
            var parent = srcT.parent;

            var srcRestLocal = srcT.localPosition;
            var srcRestRot = Quaternion.identity;

            // Approach: hover just above the target mouth, slightly offset so the tip is readable
            var targetMouth = dstLiq != null ? dstLiq.GetMouthWorldPos() : dstT.position + Vector3.up * 0.22f;
            var from = srcT.position;
            var planar = from - dstT.position;
            planar.y = 0f;
            if (planar.sqrMagnitude < 0.0001f) planar = -srcT.forward;
            planar.Normalize();

            var approachWorld = targetMouth + planar * 0.09f + Vector3.up * 0.06f;
            var approachLocal = parent != null
                ? parent.InverseTransformPoint(approachWorld)
                : approachWorld;

            // Tip toward the target opening
            var tipDir = (dstT.position - approachWorld);
            tipDir.y = 0f;
            if (tipDir.sqrMagnitude < 0.0001f) tipDir = -planar;
            tipDir.Normalize();
            var tipAxis = Vector3.Cross(Vector3.up, tipDir);
            if (tipAxis.sqrMagnitude < 0.0001f) tipAxis = srcT.right;
            tipAxis.Normalize();
            var tipRot = Quaternion.AngleAxis(62f, tipAxis);

            // Short stream visual (vessels are close)
            var streamGo = new GameObject("PourStream");
            var lr = streamGo.AddComponent<LineRenderer>();
            lr.positionCount = 4;
            lr.startWidth = 0.014f;
            lr.endWidth = 0.008f;
            lr.numCapVertices = 6;
            lr.numCornerVertices = 4;
            lr.material = LabMaterials.Liquid(streamColor);
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;

            // 1) Levitate toward pour position
            float t = 0f;
            var liftLocal = srcRestLocal + Vector3.up * 0.1f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.22f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                srcT.localPosition = Vector3.Lerp(srcRestLocal, liftLocal, k);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.38f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                srcT.localPosition = Vector3.Lerp(liftLocal, approachLocal, k);
                yield return null;
            }
            srcT.localPosition = approachLocal;

            // 2) Tip into position
            t = 0f;
            lr.enabled = true;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.28f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                srcT.localRotation = Quaternion.Slerp(srcRestRot, tipRot, k);
                UpdateStream(lr, source, target, streamColor, k * 0.5f);
                yield return null;
            }

            // 3) Transfer fill levels while streaming
            t = 0f;
            const float pourDur = 0.75f;
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
                var w = 0.01f + Mathf.Sin(Time.time * 22f) * 0.0035f;
                lr.startWidth = w + 0.004f;
                lr.endWidth = w;
                yield return null;
            }

            srcLiq.SetFill(sourceFillAfter, instant: true);
            dstLiq.SetLiquid(targetColor, targetFillAfter, instant: true);

            // 4) Untip
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.26f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                srcT.localRotation = Quaternion.Slerp(tipRot, srcRestRot, k);
                UpdateStream(lr, source, target, streamColor, 1f - k);
                yield return null;
            }
            lr.enabled = false;
            srcT.localRotation = srcRestRot;

            // 5) Fly home
            t = 0f;
            var fromPos = srcT.localPosition;
            var midHome = Vector3.Lerp(fromPos, srcRestLocal, 0.5f) + Vector3.up * 0.05f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.42f;
                var k = Mathf.SmoothStep(0f, 1f, t);
                // slight arc home
                var a = Vector3.Lerp(fromPos, midHome, k);
                var b = Vector3.Lerp(midHome, srcRestLocal, k);
                srcT.localPosition = Vector3.Lerp(a, b, k);
                yield return null;
            }

            srcT.localPosition = srcRestLocal;
            srcT.localRotation = srcRestRot;
            if (streamGo != null) Destroy(streamGo);

            source.EndPourLock();
            target.EndPourLock();
            IsPouring = false;
            onComplete?.Invoke();
        }

        static void UpdateStream(LineRenderer lr, ChemVessel source, ChemVessel target, Color color, float strength)
        {
            if (lr == null) return;
            var a = TipLipWorld(source);
            var b = MouthWorld(target);

            // Short drip arc — vessels are already close
            var mid = Vector3.Lerp(a, b, 0.5f) + Vector3.down * 0.02f;
            var p1 = Vector3.Lerp(a, mid, 0.55f);
            var p2 = Vector3.Lerp(mid, b, 0.55f);
            lr.SetPosition(0, a);
            lr.SetPosition(1, p1);
            lr.SetPosition(2, p2);
            lr.SetPosition(3, b);

            var c = Color.Lerp(color, Color.white, 0.12f);
            c.a = Mathf.Clamp01(0.4f + strength * 0.6f);
            lr.startColor = c;
            lr.endColor = new Color(c.r, c.g, c.b, c.a * 0.7f);
        }

        /// <summary>Lowest pour-lip of the tipped rim (not a far laser from the bottle center).</summary>
        static Vector3 TipLipWorld(ChemVessel vessel)
        {
            var t = vessel.transform;
            var rim = t.Find("GlassRim");
            float topLocalY = rim != null ? rim.localPosition.y : 0.2f;
            var lipLocal = new Vector3(0f, topLocalY, 0f);

            // When tipped, shift to the downhill side of the rim (world-down on the rim plane)
            var downHillWorld = Vector3.ProjectOnPlane(Vector3.down, t.up);
            if (downHillWorld.sqrMagnitude > 0.0001f)
            {
                var downHillLocal = t.InverseTransformDirection(downHillWorld.normalized);
                lipLocal += downHillLocal * 0.045f;
            }
            return t.TransformPoint(lipLocal);
        }

        static Vector3 MouthWorld(ChemVessel vessel)
        {
            if (vessel.Liquid != null) return vessel.Liquid.GetMouthWorldPos();
            var rim = vessel.transform.Find("GlassRim");
            if (rim != null) return rim.position;
            return vessel.transform.position + Vector3.up * 0.22f;
        }
    }
}
