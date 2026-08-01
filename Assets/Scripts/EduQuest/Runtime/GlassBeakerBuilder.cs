using UnityEngine;

namespace EduQuest
{
    /// <summary>
    /// Builds a normal-looking beaker: transparent glass shell + clearly inset liquid fill.
    /// </summary>
    public static class GlassBeakerBuilder
    {
        public static GameObject Create(
            Transform parent,
            Vector3 localPos,
            string name,
            ChemRole role,
            string displayName,
            Color liquidColor,
            float fill,
            float height = 0.22f,
            float radius = 0.055f)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            // Outer glass wall (transparent — liquid must stay smaller so it reads "inside")
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "GlassBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            var bodyR = body.GetComponent<Renderer>();
            bodyR.sharedMaterial = LabMaterials.GlassShell();
            bodyR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.DestroyImmediate(body.GetComponent<Collider>());

            // Thick rim
            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "GlassRim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localPosition = new Vector3(0f, height * 0.97f, 0f);
            rim.transform.localScale = new Vector3(radius * 2.12f, height * 0.035f, radius * 2.12f);
            rim.GetComponent<Renderer>().sharedMaterial = LabMaterials.GlassRim();
            Object.DestroyImmediate(rim.GetComponent<Collider>());

            // Glass floor disk
            var baseDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseDisk.name = "GlassBase";
            baseDisk.transform.SetParent(root.transform, false);
            baseDisk.transform.localPosition = new Vector3(0f, height * 0.03f, 0f);
            baseDisk.transform.localScale = new Vector3(radius * 1.95f, height * 0.03f, radius * 1.95f);
            baseDisk.GetComponent<Renderer>().sharedMaterial = LabMaterials.GlassRim();
            Object.DestroyImmediate(baseDisk.GetComponent<Collider>());

            // Click collider on root
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, height * 0.5f, 0f);
            box.size = new Vector3(radius * 2.35f, height, radius * 2.35f);

            // Liquid clearly inside the glass — wide enough to read, still inset from the wall
            var liquidMaxH = height * 0.72f;
            var liquidRadius = radius * 0.7f;
            var lv = LiquidVolume.Ensure(root.transform, liquidColor, fill, liquidMaxH, liquidRadius);
            lv.SetBaseY(height * 0.06f);

            var vessel = root.AddComponent<ChemVessel>();
            vessel.Configure(role, displayName);

            if (role == ChemRole.ReactionBeaker)
            {
                var mix = root.AddComponent<BeakerMix>();
                var g = new GameObject("CrystalGlow");
                g.transform.SetParent(root.transform, false);
                g.transform.localPosition = new Vector3(0f, height * 0.55f, 0f);
                var light = g.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 0.9f;
                light.enabled = false;
                mix.Bind(lv.Surface != null ? lv.Surface.GetComponent<Renderer>() : null, light);
            }

            return root;
        }
    }
}
