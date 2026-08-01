using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Places the chem glassware kit in a clear, readable table layout.
    /// </summary>
    public static class LabFactory
    {
        static readonly Color AgNO3 = LabChemicals.AgNO3;
        static readonly Color NaCl = LabChemicals.NaCl;
        static readonly Color Fixer = LabChemicals.Fixer;
        static readonly Color CuSO4 = LabChemicals.CuSO4;
        static readonly Color Water = LabChemicals.ClearMix;
        static readonly Color EmptyTint = LabChemicals.ClearMix;

        public static GameObject CreateLabKit(Transform parent, Vector3 worldPos, Quaternion worldRot)
        {
            return CreateLabKit(parent, worldPos, worldRot, forExperiment: false);
        }

        public static GameObject CreateLabKit(Transform parent, Vector3 worldPos, Quaternion worldRot, bool forExperiment)
        {
            var root = new GameObject("LabKit");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(worldPos, worldRot);

            if (forExperiment)
                BuildClearExperimentLayout(root.transform);
            else
                BuildDisplayLayout(root.transform);

            if (root.transform.childCount == 0)
                CreatePrimitiveFallback(root.transform);

            return root;
        }

        /// <summary>
        /// Readable layout (camera looks from -Z toward +Z):
        ///   Back: spare glassware (no labels)
        ///   Mid:  A  B  C  D  in a wide row
        ///   Front: reaction beaker (MIX)
        /// </summary>
        static void BuildClearExperimentLayout(Transform root)
        {
            // Clean table: 4 reagent beakers + MIX. Accurate aqueous colors + inset fills.
            GlassBeakerBuilder.Create(root, new Vector3(-0.48f, 0f, 0.16f), "Bottle_A",
                ChemRole.SilverNitrate, LabChemicals.DisplayName(ChemRole.SilverNitrate), AgNO3, 0.88f, 0.2f, 0.048f);
            GlassBeakerBuilder.Create(root, new Vector3(-0.16f, 0f, 0.16f), "Bottle_B",
                ChemRole.SodiumChloride, LabChemicals.DisplayName(ChemRole.SodiumChloride), NaCl, 0.88f, 0.2f, 0.048f);
            GlassBeakerBuilder.Create(root, new Vector3(0.16f, 0f, 0.16f), "Bottle_C",
                ChemRole.Fixer, LabChemicals.DisplayName(ChemRole.Fixer), Fixer, 0.88f, 0.2f, 0.048f);
            GlassBeakerBuilder.Create(root, new Vector3(0.48f, 0f, 0.16f), "Bottle_D",
                ChemRole.Distractor, LabChemicals.DisplayName(ChemRole.Distractor), CuSO4, 0.88f, 0.2f, 0.048f);

            // Larger MIX beaker in front — starts empty
            GlassBeakerBuilder.Create(root, new Vector3(0f, 0f, -0.12f), "ReactionBeaker",
                ChemRole.ReactionBeaker, LabChemicals.DisplayName(ChemRole.ReactionBeaker), Water, 0f, 0.26f, 0.07f);
        }

        static void BuildDisplayLayout(Transform root)
        {
            PlaceProp("Beaker", root, new Vector3(-0.4f, 0f, 0f), 1f, EmptyTint, 0.05f);
            PlaceProp("Erlenmeyer", root, new Vector3(-0.15f, 0f, 0.05f), 1f, Water, 0.1f);
            PlaceProp("Florence", root, new Vector3(0.1f, 0f, 0f), 1f, Water, 0.12f);
            PlaceProp("GraduatedCylinder", root, new Vector3(0.35f, 0f, 0.05f), 1f, Water, 0.08f);
            PlaceProp("ReagentBottle", root, new Vector3(0.55f, 0f, 0.15f), 1f, Water, 0.07f);
        }

        public static GameObject CreateBeakerPrefabRoot()
        {
            var root = new GameObject("CrystalBeaker");
            if (PlaceProp("Beaker", root.transform, Vector3.zero, 1f, null, 0f) == null)
                CreatePrimitiveBeaker(root.transform, Vector3.zero);
            return root;
        }

        static void PlaceReagent(Transform parent, Vector3 localPos, Color liquid,
            ChemRole role, string label, string name, Color labelColor)
        {
            var bottle = PlaceProp("ReagentBottle", parent, localPos, 1.05f, liquid, 0.09f);
            if (bottle == null)
            {
                var body = Prim(parent, PrimitiveType.Cylinder, name,
                    localPos + new Vector3(0f, 0.16f, 0f), new Vector3(0.1f, 0.16f, 0.1f),
                    LabMaterials.Solid(EmptyTint, 0.85f));
                Prim(parent, PrimitiveType.Cylinder, name + "_Liquid",
                    localPos + new Vector3(0f, 0.12f, 0f), new Vector3(0.08f, 0.1f, 0.08f),
                    LabMaterials.Solid(liquid, 0.55f));
                bottle = body;
            }

            bottle.name = name;
            MakeClickable(bottle, role, label.Replace("\n", " "));
            // Identification is on-screen UI buttons (world TextMesh labels were unreadable)
        }

        static GameObject PlaceProp(string prefabName, Transform parent, Vector3 localPos, float scale,
            Color? liquid, float fillHeight)
        {
            var prefab = Resources.Load<GameObject>($"EduQuest/Prefabs/{prefabName}");
            if (prefab == null) return null;

            var go = Object.Instantiate(prefab, parent);
            go.name = prefabName;
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity; // upright — no auto-tilt
            go.transform.localScale = Vector3.one * scale;
            SitOnFloorLocal(go);

            // Liquids for experiment vessels are created by ChemVessel/LiquidVolume.
            // Props still get a simple fill for atmosphere.
            if (liquid.HasValue && fillHeight > 0.001f)
                LiquidVolume.Ensure(go.transform, liquid.Value, 0.7f, fillHeight + 0.04f, 0.06f * scale);

            return go;
        }

        /// <summary>Keep rotation identity; only lift so the mesh rests on y=0 local floor.</summary>
        static void SitOnFloorLocal(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            // Convert world bottom → local offset along parent up
            var parent = go.transform.parent;
            if (parent == null) return;
            var floorWorldY = parent.TransformPoint(go.transform.localPosition).y
                              - (go.transform.position.y - parent.position.y)
                              + parent.position.y;
            // Simpler: bottom should sit at parent.position.y + localPos.y contribution
            // We want world bounds.min.y == parent.TransformPoint(localPos with y=0).y
            var targetBottom = parent.TransformPoint(new Vector3(
                go.transform.localPosition.x, 0f, go.transform.localPosition.z)).y;
            var lift = targetBottom - bounds.min.y;
            go.transform.position += new Vector3(0f, lift, 0f);
        }

        /// <summary>Legacy helper used by prefab baker — identity upright only.</summary>
        public static void EnsureUpright(GameObject go)
        {
            if (go == null) return;
            go.transform.localRotation = Quaternion.identity;
            SitOnFloorLocal(go);
        }

        static void AddFill(Transform vessel, Color liquid, float height, float radius)
        {
            var liquidGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            liquidGo.name = "Substance";
            liquidGo.transform.SetParent(vessel, false);
            liquidGo.transform.localPosition = new Vector3(0f, Mathf.Max(0.07f, height), 0f);
            liquidGo.transform.localScale = new Vector3(
                Mathf.Max(0.09f, radius * 1.4f),
                Mathf.Max(0.07f, height),
                Mathf.Max(0.09f, radius * 1.4f));
            liquidGo.GetComponent<Renderer>().sharedMaterial = LabMaterials.Solid(liquid, 0.35f);
            StripCol(liquidGo);
        }

        static void MakeClickable(GameObject go, ChemRole role, string display)
        {
            if (go == null) return;

            var vessel = go.GetComponent<ChemVessel>();
            if (vessel == null) vessel = go.AddComponent<ChemVessel>();
            vessel.Configure(role, display);

            if (role == ChemRole.ReactionBeaker && go.GetComponent<BeakerMix>() == null)
                go.AddComponent<BeakerMix>();
        }

        static void CreatePrimitiveFallback(Transform parent)
        {
            CreatePrimitiveBeaker(parent, new Vector3(0f, 0f, -0.08f));
            CreatePrimitiveBottle(parent, new Vector3(-0.4f, 0f, 0.18f), AgNO3);
        }

        static void CreatePrimitiveBeaker(Transform parent, Vector3 localPos)
        {
            Prim(parent, PrimitiveType.Cylinder, "Beaker",
                localPos + new Vector3(0f, 0.2f, 0f), new Vector3(0.28f, 0.2f, 0.28f),
                LabMaterials.Solid(EmptyTint, 0.85f));
        }

        static void CreatePrimitiveBottle(Transform parent, Vector3 localPos, Color liquid)
        {
            Prim(parent, PrimitiveType.Cylinder, "Bottle_Body",
                localPos + new Vector3(0f, 0.16f, 0f), new Vector3(0.1f, 0.16f, 0.1f),
                LabMaterials.Solid(EmptyTint, 0.85f));
            Prim(parent, PrimitiveType.Cylinder, "Bottle_Liquid",
                localPos + new Vector3(0f, 0.12f, 0f), new Vector3(0.08f, 0.1f, 0.08f),
                LabMaterials.Solid(liquid, 0.65f));
        }

        static GameObject Prim(Transform parent, PrimitiveType type, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static void StripCol(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c == null) return;
            Object.DestroyImmediate(c);
        }
    }
}
