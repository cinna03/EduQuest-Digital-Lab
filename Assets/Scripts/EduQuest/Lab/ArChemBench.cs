using EduQuest.Experiments;
using UnityEngine;

namespace EduQuest.Lab
{
    /// <summary>
    /// Spawns the six reagent vessels around a placed AR beaker (phone AR).
    /// Desktop preview keeps bottles in the scene already.
    /// </summary>
    public class ArChemBench : MonoBehaviour
    {
        Transform m_Root;

        public void Clear()
        {
            if (m_Root != null)
                Destroy(m_Root.gameObject);
            m_Root = null;
        }

        public void SpawnAround(Transform beaker)
        {
            Clear();
            if (beaker == null) return;

            m_Root = new GameObject("AR_ReagentBench").transform;
            m_Root.SetParent(beaker, false);
            m_Root.localPosition = Vector3.zero;
            m_Root.localRotation = Quaternion.identity;

            // Arc in front of the beaker on the table
            Spawn(ChemId.SilverNitrate, "AgNO₃", "A · Silver Nitrate",
                new Color(0.85f, 0.9f, 1f, 0.55f), new Vector3(-0.35f, 0f, 0.28f), GlasswareKind.ReagentBottle);
            Spawn(ChemId.SodiumChloride, "NaCl", "B · Sodium Chloride",
                new Color(0.92f, 0.95f, 0.98f, 0.5f), new Vector3(-0.18f, 0f, 0.32f), GlasswareKind.Erlenmeyer);
            Spawn(ChemId.SodiumThiosulfate, "Fixer", "C · Sodium Thiosulfate",
                new Color(0.65f, 0.85f, 0.95f, 0.55f), new Vector3(0f, 0f, 0.34f), GlasswareKind.GraduatedCylinder);
            Spawn(ChemId.DistilledWater, "H₂O", "D · Distilled Water",
                new Color(0.55f, 0.75f, 0.95f, 0.4f), new Vector3(0.18f, 0f, 0.32f), GlasswareKind.GriffinBeaker);
            Spawn(ChemId.SodiumCarbonate, "Na₂CO₃", "E · Sodium Carbonate",
                new Color(0.75f, 0.7f, 0.55f, 0.55f), new Vector3(0.35f, 0f, 0.28f), GlasswareKind.RoundFlask);
            Spawn(ChemId.CopperSulfate, "CuSO₄", "F · Copper Sulfate",
                new Color(0.15f, 0.5f, 0.9f, 0.7f), new Vector3(0.45f, 0f, 0.12f), GlasswareKind.Erlenmeyer);

            LabPropFactory.CreateBenchGradCylinder(m_Root, beaker.TransformPoint(new Vector3(-0.45f, 0f, 0.05f)));
        }

        void Spawn(ChemId id, string shortLabel, string fullName, Color liquid, Vector3 localPos, GlasswareKind kind)
        {
            var world = m_Root.TransformPoint(localPos);
            LabPropFactory.CreateBottle(m_Root, id, shortLabel, fullName, liquid, world, kind);
        }
    }
}
