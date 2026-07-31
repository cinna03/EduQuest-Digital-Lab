using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Places the chem glassware kit. Prefers real FBX prefabs from Resources;
    /// falls back to primitives if prefabs are not baked yet.
    /// </summary>
    public static class LabFactory
    {
        public static GameObject CreateLabKit(Transform parent, Vector3 worldPos, Quaternion worldRot)
        {
            var root = new GameObject("LabKit");
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(worldPos, worldRot);

            // Reference lineup (left → right): beaker, erlenmeyer, florence, cylinder, beaker
            // Plus reagent bottles for the AgCl experiment mix later.
            Place("Beaker", root.transform, new Vector3(-0.38f, 0f, 0.05f), 1f);
            Place("Erlenmeyer", root.transform, new Vector3(-0.18f, 0f, 0.08f), 1f);
            Place("Florence", root.transform, new Vector3(0.02f, 0f, 0.02f), 1f);
            Place("GraduatedCylinder", root.transform, new Vector3(0.2f, 0f, 0.1f), 1f);
            Place("Beaker", root.transform, new Vector3(0.38f, 0f, 0.05f), 0.9f);

            PlaceTintedBottle(root.transform, new Vector3(-0.28f, 0f, 0.28f), new Color(0.9f, 0.95f, 1f));
            PlaceTintedBottle(root.transform, new Vector3(-0.12f, 0f, 0.32f), new Color(0.95f, 0.97f, 1f));
            PlaceTintedBottle(root.transform, new Vector3(0.05f, 0f, 0.34f), new Color(0.45f, 0.8f, 0.95f));
            PlaceTintedBottle(root.transform, new Vector3(0.2f, 0f, 0.3f), new Color(0.55f, 0.8f, 1f));
            Place("RoundBottom", root.transform, new Vector3(0.36f, 0f, 0.26f), 0.85f);

            if (root.transform.childCount == 0)
                CreatePrimitiveFallback(root.transform);

            return root;
        }

        public static GameObject CreateBeakerPrefabRoot()
        {
            var root = new GameObject("CrystalBeaker");
            if (!Place("Beaker", root.transform, Vector3.zero, 1f))
                CreatePrimitiveBeaker(root.transform, Vector3.zero);
            return root;
        }

        static bool Place(string prefabName, Transform parent, Vector3 localPos, float scale)
        {
            var prefab = Resources.Load<GameObject>($"EduQuest/Prefabs/{prefabName}");
            if (prefab == null) return false;

            var go = Object.Instantiate(prefab, parent);
            go.name = prefabName;
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * scale;
            return true;
        }

        static void PlaceTintedBottle(Transform parent, Vector3 localPos, Color liquid)
        {
            if (Place("ReagentBottle", parent, localPos, 1f))
            {
                // Soft liquid cue inside (no labels)
                var liquidGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                liquidGo.name = "Liquid";
                liquidGo.transform.SetParent(parent, false);
                liquidGo.transform.localPosition = localPos + new Vector3(0f, 0.06f, 0f);
                liquidGo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                liquidGo.GetComponent<Renderer>().sharedMaterial = LabMaterials.Solid(liquid, 0.65f);
                StripCol(liquidGo);
                return;
            }

            CreatePrimitiveBottle(parent, localPos, liquid);
        }

        static void CreatePrimitiveFallback(Transform parent)
        {
            CreatePrimitiveBeaker(parent, new Vector3(-0.2f, 0f, 0f));
            CreatePrimitiveBottle(parent, new Vector3(0f, 0f, 0.2f), new Color(0.7f, 0.85f, 0.95f));
            CreatePrimitiveCylinder(parent, new Vector3(0.2f, 0f, 0.1f), new Color(0.45f, 0.8f, 0.95f));
        }

        static void CreatePrimitiveBeaker(Transform parent, Vector3 localPos)
        {
            Prim(parent, PrimitiveType.Cylinder, "Beaker",
                localPos + new Vector3(0f, 0.2f, 0f), new Vector3(0.28f, 0.2f, 0.28f),
                LabMaterials.Solid(new Color(0.75f, 0.88f, 0.95f), 0.85f));
            Prim(parent, PrimitiveType.Cylinder, "Rim",
                localPos + new Vector3(0f, 0.41f, 0f), new Vector3(0.3f, 0.02f, 0.3f),
                LabMaterials.Solid(new Color(0.85f, 0.92f, 1f), 0.9f));
        }

        static void CreatePrimitiveBottle(Transform parent, Vector3 localPos, Color liquid)
        {
            Prim(parent, PrimitiveType.Cylinder, "Bottle_Body",
                localPos + new Vector3(0f, 0.16f, 0f), new Vector3(0.1f, 0.16f, 0.1f),
                LabMaterials.Solid(new Color(0.8f, 0.9f, 0.95f), 0.85f));
            Prim(parent, PrimitiveType.Cylinder, "Bottle_Neck",
                localPos + new Vector3(0f, 0.36f, 0f), new Vector3(0.04f, 0.06f, 0.04f),
                LabMaterials.Solid(new Color(0.85f, 0.9f, 0.95f), 0.85f));
            Prim(parent, PrimitiveType.Cylinder, "Bottle_Liquid",
                localPos + new Vector3(0f, 0.12f, 0f), new Vector3(0.08f, 0.1f, 0.08f),
                LabMaterials.Solid(liquid, 0.65f));
        }

        static void CreatePrimitiveCylinder(Transform parent, Vector3 localPos, Color liquid)
        {
            Prim(parent, PrimitiveType.Cylinder, "Cylinder_Body",
                localPos + new Vector3(0f, 0.2f, 0f), new Vector3(0.07f, 0.2f, 0.07f),
                LabMaterials.Solid(new Color(0.8f, 0.9f, 0.95f), 0.85f));
            Prim(parent, PrimitiveType.Cylinder, "Cylinder_Liquid",
                localPos + new Vector3(0f, 0.14f, 0f), new Vector3(0.055f, 0.12f, 0.055f),
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
            StripCol(go);
            return go;
        }

        static void StripCol(GameObject go)
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
