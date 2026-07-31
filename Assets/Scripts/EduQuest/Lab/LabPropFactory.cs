using EduQuest.Experiments;
using EduQuest.UI;
using UnityEngine;

namespace EduQuest.Lab
{
    public enum GlasswareKind
    {
        GriffinBeaker,
        Erlenmeyer,
        GraduatedCylinder,
        RoundFlask,
        ReagentBottle
    }

    /// <summary>
    /// Professional chem-feel glassware from primitives:
    /// Griffin beaker, Erlenmeyer, graduated cylinder, round flask, reagent bottle.
    /// </summary>
    public static class LabPropFactory
    {
        static readonly Color GlassTint = new Color(0.82f, 0.9f, 0.96f, 0.18f);
        static readonly Color GlassRim = new Color(0.9f, 0.95f, 1f, 0.4f);
        static readonly Color Frost = new Color(0.95f, 0.96f, 0.97f, 0.55f);
        static readonly Color Tick = new Color(0.12f, 0.12f, 0.14f, 0.9f);

        public static GameObject CreateCrystalBeakerRoot()
        {
            var root = new GameObject("CrystalBeaker");
            var beaker = root.AddComponent<CrystalBeaker>();

            // Low-form Griffin beaker body
            var glass = CreateCylinder(root.transform, "Glass", new Vector3(0f, 0.22f, 0f), new Vector3(0.34f, 0.22f, 0.34f),
                LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(glass);

            // Slightly wider flared rim
            var rim = CreateCylinder(root.transform, "Rim", new Vector3(0f, 0.45f, 0f), new Vector3(0.37f, 0.018f, 0.37f),
                LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(rim);

            // Pouring spout
            var spout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spout.name = "Spout";
            spout.transform.SetParent(root.transform, false);
            spout.transform.localPosition = new Vector3(0.18f, 0.44f, 0f);
            spout.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            spout.transform.localScale = new Vector3(0.08f, 0.03f, 0.06f);
            spout.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeGlass(GlassRim);
            StripCollider(spout);

            // Frosted writing patch (DURAN-style matte area)
            AddFrostPatch(root.transform, new Vector3(-0.14f, 0.26f, 0.165f), new Vector3(0.12f, 0.1f, 0.01f), "600 ml");

            // Graduation ticks (APPROX. VOL. feel)
            AddGraduationTicks(root.transform, 0.14f, 0.12f, 0.38f, 5, 0.165f);

            var liquid = CreateCylinder(root.transform, "Liquid", new Vector3(0f, 0.14f, 0f), new Vector3(0.28f, 0.08f, 0.28f),
                LabGlassMaterials.MakeLiquid(new Color(0.85f, 0.9f, 0.95f, 0.4f)));
            StripCollider(liquid);
            liquid.SetActive(false);

            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "Crystal";
            crystal.transform.SetParent(root.transform, false);
            crystal.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            crystal.transform.localRotation = Quaternion.Euler(35f, 25f, 15f);
            crystal.transform.localScale = Vector3.one * 0.13f;
            StripCollider(crystal);
            crystal.GetComponent<Renderer>().sharedMaterial =
                LabGlassMaterials.MakeSolid(new Color(0.85f, 0.92f, 1f), 0.95f);
            crystal.SetActive(false);

            var hit = root.AddComponent<CapsuleCollider>();
            hit.center = new Vector3(0f, 0.26f, 0f);
            hit.radius = 0.2f;
            hit.height = 0.55f;

            var smoke = CreateParticles(root.transform, "Smoke", new Vector3(0f, 0.52f, 0f),
                new Color(0.85f, 0.9f, 0.95f, 0.4f), 0.12f);
            var sparkle = CreateParticles(root.transform, "Sparkle", new Vector3(0f, 0.4f, 0f),
                new Color(0.75f, 0.9f, 1f, 0.9f), 0.03f);

            var glowGo = new GameObject("Glow");
            glowGo.transform.SetParent(root.transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.32f, 0f);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 2f;
            glow.enabled = false;

            var floatLabel = new GameObject("BeakerLabel").AddComponent<FloatingGlassLabel>();
            floatLabel.transform.SetParent(root.transform, false);
            floatLabel.Configure(root.transform, "Reaction Beaker · 600 ml", new Vector3(0f, 0.68f, 0f));

            beaker.Configure(
                liquid.GetComponent<Renderer>(), liquid.transform, crystal.transform,
                smoke, sparkle, glow);

            return root;
        }

        public static ChemicalBottle CreateBottle(
            Transform parent,
            ChemId id,
            string shortLabel,
            string fullName,
            Color liquidColor,
            Vector3 position,
            GlasswareKind kind = GlasswareKind.ReagentBottle)
        {
            var root = new GameObject("Vessel_" + shortLabel);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            Renderer bodyRend;
            Renderer liquidRend;
            float labelHeight;
            float hitHeight;
            float hitRadius;

            switch (kind)
            {
                case GlasswareKind.GriffinBeaker:
                    BuildGriffin(root.transform, liquidColor, out bodyRend, out liquidRend, out labelHeight, out hitHeight, out hitRadius);
                    AddFrostPatch(root.transform, new Vector3(-0.08f, 0.18f, 0.1f), new Vector3(0.08f, 0.07f, 0.008f), shortLabel);
                    AddGraduationTicks(root.transform, 0.08f, 0.08f, 0.28f, 4, 0.1f);
                    break;
                case GlasswareKind.Erlenmeyer:
                    BuildErlenmeyer(root.transform, liquidColor, out bodyRend, out liquidRend, out labelHeight, out hitHeight, out hitRadius);
                    AddFrostPatch(root.transform, new Vector3(-0.06f, 0.14f, 0.1f), new Vector3(0.08f, 0.06f, 0.008f), shortLabel);
                    break;
                case GlasswareKind.GraduatedCylinder:
                    BuildGradCylinder(root.transform, liquidColor, out bodyRend, out liquidRend, out labelHeight, out hitHeight, out hitRadius);
                    AddGraduationTicks(root.transform, 0.045f, 0.1f, 0.42f, 6, 0.04f);
                    break;
                case GlasswareKind.RoundFlask:
                    BuildRoundFlask(root.transform, liquidColor, out bodyRend, out liquidRend, out labelHeight, out hitHeight, out hitRadius);
                    AddFrostPatch(root.transform, new Vector3(-0.05f, 0.2f, 0.12f), new Vector3(0.07f, 0.05f, 0.008f), shortLabel);
                    break;
                default:
                    BuildReagentBottle(root.transform, liquidColor, out bodyRend, out liquidRend, out labelHeight, out hitHeight, out hitRadius);
                    AddFrostPatch(root.transform, new Vector3(-0.05f, 0.18f, 0.07f), new Vector3(0.07f, 0.08f, 0.008f), shortLabel);
                    break;
            }

            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, hitHeight * 0.5f, 0f);
            col.radius = hitRadius;
            col.height = hitHeight;

            var labelGo = new GameObject("HoverLabel");
            labelGo.transform.SetParent(root.transform, false);
            var floating = labelGo.AddComponent<FloatingGlassLabel>();

            var bottle = root.AddComponent<ChemicalBottle>();
            bottle.Setup(id, fullName, liquidColor, floating, liquidRend, bodyRend);
            floating.Configure(root.transform, fullName, new Vector3(0f, labelHeight, 0f));
            return bottle;
        }

        /// <summary>Standalone measuring cylinder prop for the bench (chem feel).</summary>
        public static GameObject CreateBenchGradCylinder(Transform parent, Vector3 position)
        {
            var root = new GameObject("Bench_GraduatedCylinder");
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            BuildGradCylinder(root.transform, new Color(0.7f, 0.85f, 1f, 0.35f),
                out _, out _, out var labelH, out var hitH, out var hitR);
            AddGraduationTicks(root.transform, 0.05f, 0.1f, 0.45f, 8, 0.045f);
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, hitH * 0.5f, 0f);
            col.radius = hitR;
            col.height = hitH;
            var label = new GameObject("HoverLabel").AddComponent<FloatingGlassLabel>();
            label.transform.SetParent(root.transform, false);
            label.Configure(root.transform, "Measuring Cylinder · 50 ml", new Vector3(0f, labelH, 0f));
            return root;
        }

        static void BuildGriffin(Transform t, Color liquid, out Renderer body, out Renderer liq, out float labelH, out float hitH, out float hitR)
        {
            var glass = CreateCylinder(t, "Glass", new Vector3(0f, 0.16f, 0f), new Vector3(0.2f, 0.16f, 0.2f), LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(glass);
            var rim = CreateCylinder(t, "Rim", new Vector3(0f, 0.33f, 0f), new Vector3(0.22f, 0.012f, 0.22f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(rim);
            var spout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spout.name = "Spout";
            spout.transform.SetParent(t, false);
            spout.transform.localPosition = new Vector3(0.11f, 0.32f, 0f);
            spout.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
            spout.transform.localScale = new Vector3(0.05f, 0.02f, 0.04f);
            spout.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeGlass(GlassRim);
            StripCollider(spout);
            var liquidGo = CreateCylinder(t, "Liquid", new Vector3(0f, 0.12f, 0f), new Vector3(0.16f, 0.09f, 0.16f), LabGlassMaterials.MakeLiquid(liquid));
            StripCollider(liquidGo);
            body = glass.GetComponent<Renderer>();
            liq = liquidGo.GetComponent<Renderer>();
            labelH = 0.52f;
            hitH = 0.42f;
            hitR = 0.12f;
        }

        static void BuildErlenmeyer(Transform t, Color liquid, out Renderer body, out Renderer liq, out float labelH, out float hitH, out float hitR)
        {
            // Approximate cone with stacked scaled cylinders
            var baseC = CreateCylinder(t, "Base", new Vector3(0f, 0.08f, 0f), new Vector3(0.22f, 0.07f, 0.22f), LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(baseC);
            var mid = CreateCylinder(t, "Mid", new Vector3(0f, 0.18f, 0f), new Vector3(0.14f, 0.06f, 0.14f), LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(mid);
            var neck = CreateCylinder(t, "Neck", new Vector3(0f, 0.3f, 0f), new Vector3(0.06f, 0.08f, 0.06f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(neck);
            var lip = CreateCylinder(t, "Lip", new Vector3(0f, 0.39f, 0f), new Vector3(0.07f, 0.012f, 0.07f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(lip);
            var liquidGo = CreateCylinder(t, "Liquid", new Vector3(0f, 0.1f, 0f), new Vector3(0.16f, 0.07f, 0.16f), LabGlassMaterials.MakeLiquid(liquid));
            StripCollider(liquidGo);
            body = mid.GetComponent<Renderer>();
            liq = liquidGo.GetComponent<Renderer>();
            labelH = 0.55f;
            hitH = 0.48f;
            hitR = 0.13f;
        }

        static void BuildGradCylinder(Transform t, Color liquid, out Renderer body, out Renderer liq, out float labelH, out float hitH, out float hitR)
        {
            var foot = CreateCylinder(t, "Foot", new Vector3(0f, 0.02f, 0f), new Vector3(0.16f, 0.015f, 0.16f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(foot);
            var tube = CreateCylinder(t, "Tube", new Vector3(0f, 0.26f, 0f), new Vector3(0.07f, 0.24f, 0.07f), LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(tube);
            var spout = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spout.name = "Spout";
            spout.transform.SetParent(t, false);
            spout.transform.localPosition = new Vector3(0.04f, 0.5f, 0f);
            spout.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            spout.transform.localScale = new Vector3(0.035f, 0.015f, 0.03f);
            spout.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeGlass(GlassRim);
            StripCollider(spout);
            var liquidGo = CreateCylinder(t, "Liquid", new Vector3(0f, 0.18f, 0f), new Vector3(0.055f, 0.12f, 0.055f), LabGlassMaterials.MakeLiquid(liquid));
            StripCollider(liquidGo);
            body = tube.GetComponent<Renderer>();
            liq = liquidGo.GetComponent<Renderer>();
            labelH = 0.68f;
            hitH = 0.55f;
            hitR = 0.09f;
        }

        static void BuildRoundFlask(Transform t, Color liquid, out Renderer body, out Renderer liq, out float labelH, out float hitH, out float hitR)
        {
            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Bulb";
            bulb.transform.SetParent(t, false);
            bulb.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            bulb.transform.localScale = new Vector3(0.24f, 0.22f, 0.24f);
            bulb.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeGlass(GlassTint);
            StripCollider(bulb);
            var neck = CreateCylinder(t, "Neck", new Vector3(0f, 0.32f, 0f), new Vector3(0.055f, 0.1f, 0.055f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(neck);
            var liquidGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            liquidGo.name = "Liquid";
            liquidGo.transform.SetParent(t, false);
            liquidGo.transform.localPosition = new Vector3(0f, 0.13f, 0f);
            liquidGo.transform.localScale = new Vector3(0.18f, 0.14f, 0.18f);
            liquidGo.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeLiquid(liquid);
            StripCollider(liquidGo);
            body = bulb.GetComponent<Renderer>();
            liq = liquidGo.GetComponent<Renderer>();
            labelH = 0.55f;
            hitH = 0.48f;
            hitR = 0.14f;
        }

        static void BuildReagentBottle(Transform t, Color liquid, out Renderer body, out Renderer liq, out float labelH, out float hitH, out float hitR)
        {
            var bodyGo = CreateCylinder(t, "Body", new Vector3(0f, 0.16f, 0f), new Vector3(0.12f, 0.16f, 0.12f), LabGlassMaterials.MakeGlass(GlassTint));
            StripCollider(bodyGo);
            var shoulder = CreateCylinder(t, "Shoulder", new Vector3(0f, 0.33f, 0f), new Vector3(0.1f, 0.03f, 0.1f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(shoulder);
            var neck = CreateCylinder(t, "Neck", new Vector3(0f, 0.4f, 0f), new Vector3(0.05f, 0.05f, 0.05f), LabGlassMaterials.MakeGlass(GlassRim));
            StripCollider(neck);
            var cap = CreateCylinder(t, "Cap", new Vector3(0f, 0.47f, 0f), new Vector3(0.06f, 0.03f, 0.06f),
                LabGlassMaterials.MakeSolid(new Color(0.15f, 0.16f, 0.18f), 0.4f));
            StripCollider(cap);
            var liquidGo = CreateCylinder(t, "Liquid", new Vector3(0f, 0.14f, 0f), new Vector3(0.095f, 0.11f, 0.095f), LabGlassMaterials.MakeLiquid(liquid));
            StripCollider(liquidGo);
            body = bodyGo.GetComponent<Renderer>();
            liq = liquidGo.GetComponent<Renderer>();
            labelH = 0.62f;
            hitH = 0.52f;
            hitR = 0.1f;
        }

        static void AddFrostPatch(Transform parent, Vector3 localPos, Vector3 scale, string mark)
        {
            var patch = GameObject.CreatePrimitive(PrimitiveType.Quad);
            patch.name = "FrostLabel";
            patch.transform.SetParent(parent, false);
            patch.transform.localPosition = localPos;
            patch.transform.localScale = scale;
            StripCollider(patch);
            patch.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeSolid(Frost, 0.15f);

            var textGo = new GameObject("Mark");
            textGo.transform.SetParent(patch.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            textGo.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            var tm = textGo.AddComponent<TextMesh>();
            tm.text = mark;
            tm.fontSize = 32;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);
            tm.fontStyle = FontStyle.Bold;
        }

        static void AddGraduationTicks(Transform parent, float x, float yMin, float yMax, int count, float z)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0f : i / (float)(count - 1);
                float y = Mathf.Lerp(yMin, yMax, t);
                float w = i % 2 == 0 ? 0.045f : 0.028f;
                var tick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tick.name = "Tick_" + i;
                tick.transform.SetParent(parent, false);
                tick.transform.localPosition = new Vector3(x, y, z);
                tick.transform.localScale = new Vector3(w, 0.004f, 0.004f);
                tick.GetComponent<Renderer>().sharedMaterial = LabGlassMaterials.MakeSolid(Tick, 0.1f);
                StripCollider(tick);
            }
        }

        static GameObject CreateCylinder(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static ParticleSystem CreateParticles(Transform parent, string name, Vector3 localPos, Color color, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = size;
            main.startSpeed = 0.25f;
            main.startLifetime = 1.8f;
            main.startColor = color;
            main.gravityModifier = -0.04f;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        static void StripCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(c);
            else Object.Destroy(c);
#else
            Object.Destroy(c);
#endif
        }
    }
}
