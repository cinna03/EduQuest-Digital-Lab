using System.Collections;
using System.Collections.Generic;
using OrbitScout.Core;
using UnityEngine;

namespace OrbitScout.View
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlanetBody : MonoBehaviour
    {
        public PlanetId planetId;

        Renderer bodyRenderer;
        Color baseColor;
        Vector3 baseLocalScale;
        int saveSteps;
        int crackSteps;
        bool exploded;
        Coroutine flashRoutine;
        Coroutine explodeRoutine;
        Transform crackRoot;
        readonly List<GameObject> crackPieces = new List<GameObject>();

        public bool IsExploded => exploded;
        public int SaveSteps => saveSteps;
        public int CrackSteps => crackSteps;

        public void Initialize(Color color)
        {
            bodyRenderer = GetComponent<Renderer>();
            baseColor = color;
            baseLocalScale = transform.localScale;
            ApplyColorDisplay();
        }

        public void ResetForLevelStart(LevelVisualMode mode)
        {
            saveSteps = 0;
            crackSteps = 0;
            exploded = false;
            StopAllPlanetRoutines();
            ClearCrackDecor();
            gameObject.SetActive(true);
            transform.localScale = baseLocalScale;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = true;

            if (mode == LevelVisualMode.Level2Greyscale)
                ApplyLevel2Visual();
            else
                ApplyColorDisplay();
        }

        public void ApplyLevel2Progress(int save, int cracks, bool isExploded)
        {
            saveSteps = Mathf.Clamp(save, 0, 3);
            crackSteps = Mathf.Clamp(cracks, 0, 3);
            exploded = isExploded;

            if (exploded)
            {
                if (explodeRoutine == null && gameObject.activeInHierarchy)
                    explodeRoutine = StartCoroutine(ExplodeThenHide());
                return;
            }

            transform.localScale = baseLocalScale * (1f - crackSteps * 0.035f);
            ApplyLevel2Visual();
            RebuildCrackDecor(crackSteps);
        }

        void ApplyLevel2Visual()
        {
            if (bodyRenderer == null)
                return;

            // Greyscale when unrestored; color returns as the planet is saved
            float saturation = saveSteps / 3f;
            Color tint = Color.white;

            if (crackSteps > 0)
            {
                float crackStrength = crackSteps switch
                {
                    1 => 0.28f,
                    2 => 0.45f,
                    _ => 0.65f
                };
                tint = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.28f), crackStrength);
            }

            PlanetMaterials.SetBodyDisplay(bodyRenderer, tint, saturation);
        }

        void RebuildCrackDecor(int cracks)
        {
            ClearCrackDecor();
            if (cracks <= 0)
                return;

            EnsureCrackRoot();
            int lineCount = cracks == 1 ? 2 : 4;
            float length = 0.35f + cracks * 0.08f;

            for (int i = 0; i < lineCount; i++)
            {
                float angle = i * (360f / lineCount) + crackSteps * 17f;
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crack.name = "Crack";
                crack.transform.SetParent(crackRoot, false);
                crack.transform.localPosition = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 0.42f;
                crack.transform.localRotation = Quaternion.Euler(35f, angle, 20f);
                crack.transform.localScale = new Vector3(0.04f, length, 0.02f);

                Collider crackCol = crack.GetComponent<Collider>();
                if (crackCol != null)
                    Destroy(crackCol);

                Renderer r = crack.GetComponent<Renderer>();
                if (r != null)
                    r.material.color = new Color(0.12f, 0.05f, 0.05f, 1f);

                crackPieces.Add(crack);
            }
        }

        void EnsureCrackRoot()
        {
            if (crackRoot != null)
                return;

            GameObject root = new GameObject("CrackRoot");
            crackRoot = root.transform;
            crackRoot.SetParent(transform, false);
        }

        void ClearCrackDecor()
        {
            foreach (GameObject piece in crackPieces)
            {
                if (piece != null)
                    Destroy(piece);
            }

            crackPieces.Clear();
        }

        IEnumerator ExplodeThenHide()
        {
            RebuildCrackDecor(3);
            ApplyLevel2Visual();

            float t = 0f;
            Vector3 startScale = transform.localScale;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float pulse = 1f + t * 0.6f;
                transform.localScale = startScale * pulse;
                if (bodyRenderer != null)
                {
                    float sat = Mathf.Lerp(saveSteps / 3f, 0f, t * 2f);
                    Color tint = Color.Lerp(Color.white, Color.black, t * 2f);
                    PlanetMaterials.SetBodyDisplay(bodyRenderer, tint, sat);
                }
                yield return null;
            }

            ClearCrackDecor();
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;

            gameObject.SetActive(false);
            explodeRoutine = null;
        }

        void ApplyColorDisplay()
        {
            if (bodyRenderer != null)
                PlanetMaterials.SetBodyDisplay(bodyRenderer, Color.white, 1f);
        }

        public void FlashCorrect()
        {
            StartFlash(new Color(0.45f, 1f, 0.6f));
        }

        public void FlashWrong()
        {
            StartFlash(new Color(1f, 0.4f, 0.4f));
        }

        void StartFlash(Color color)
        {
            if (!isActiveAndEnabled)
                return;

            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        IEnumerator FlashRoutine(Color color)
        {
            if (bodyRenderer != null)
                PlanetMaterials.SetBodyDisplay(bodyRenderer, color, 1f);

            yield return new WaitForSeconds(0.18f);

            if (bodyRenderer != null)
            {
                if (saveSteps > 0 || crackSteps > 0)
                    ApplyLevel2Visual();
                else
                    ApplyColorDisplay();
            }

            flashRoutine = null;
        }

        void StopAllPlanetRoutines()
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (explodeRoutine != null)
            {
                StopCoroutine(explodeRoutine);
                explodeRoutine = null;
            }
        }
    }
}
