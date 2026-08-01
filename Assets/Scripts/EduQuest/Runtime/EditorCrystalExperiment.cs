using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace EduQuest
{
    /// <summary>
    /// Editor crystal experiment:
    /// hover → label; click → select (glow + levitate); click again → deselect;
    /// click another while selected → pour selected into target.
    /// Wrong pour → fail, press RESET.
    /// </summary>
    public class EditorCrystalExperiment : MonoBehaviour
    {
        const float SettleSeconds = 5f;
        const float TargetAg = 10f;
        const float TargetCl = 10f;
        const float TargetFix = 5f;

        [SerializeField] ExperimentHud ui;
        [SerializeField] Transform kitRoot;
        [SerializeField] Light roomKeyLight;
        [SerializeField] Light roomFillLight;
        [SerializeField] Camera rayCamera;

        BeakerMix m_Beaker;
        ChemVessel m_Selected;
        ChemVessel m_Hover;
        bool m_Dark = true;
        bool m_Ended;
        float m_Ag, m_Cl, m_Fix;
        bool m_Precipitate, m_Settled, m_Stabilized;
        float m_SettleTimer;
        string m_Action = "";

        public void Configure(ExperimentHud hud, Transform kit, Light key, Light fill, Camera cam)
        {
            ui = hud;
            kitRoot = kit;
            roomKeyLight = key;
            roomFillLight = fill;
            rayCamera = cam;
        }

        public void Configure(GuideHud guide, Transform kit, Light key, Light fill, Camera cam)
        {
            kitRoot = kit;
            roomKeyLight = key;
            roomFillLight = fill;
            rayCamera = cam;
        }

        void Start()
        {
            if (FindAnyObjectByType<EditorLabApp>() != null) return;
            Begin();
        }

        void OnDestroy() => UnhookUi();

        public void Begin()
        {
            if (kitRoot == null) kitRoot = GameObject.Find("LabKit")?.transform;
            if (rayCamera == null) rayCamera = Camera.main;

            WireVessels();
            EnsureBeakerVisuals();
            HookUi();
            ResetRun();
            Debug.Log("[EduQuest] Hover for labels · click to select · click again to deselect · click target to pour");
        }

        void HookUi()
        {
            if (ui == null) ui = FindAnyObjectByType<ExperimentHud>();
            if (ui == null) return;
            UnhookUi();
            ui.DarkRequested += SetDark;
            ui.ResetRequested += ResetRun;
        }

        void UnhookUi()
        {
            if (ui == null) return;
            ui.DarkRequested -= SetDark;
            ui.ResetRequested -= ResetRun;
        }

        void Update()
        {
            HandleKeys();
            UpdateHover();
            HandleClick();

            if (!m_Ended && m_Precipitate && !m_Settled && !m_Stabilized)
            {
                m_SettleTimer += Time.deltaTime;
                if (m_SettleTimer >= SettleSeconds)
                {
                    m_Settled = true;
                    m_Beaker?.SetLook(BeakerMix.Look.RawCrystal);
                    m_Action = "Settled. Select Fixer (C), then click the MIX beaker to pour.";
                    RefreshUi();
                }
                else if (Time.frameCount % 10 == 0)
                {
                    m_Action = $"Precipitate settling… {Mathf.Clamp01(m_SettleTimer / SettleSeconds):0%} — stay dark";
                    RefreshUi();
                }
            }

            if (!m_Ended && !m_Dark && m_Precipitate && !m_Stabilized)
                Fail("Light hit the crystal before fixer.");
        }

        void HandleKeys()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.dKey.wasPressedThisFrame) SetDark(true);
            if (kb.lKey.wasPressedThisFrame) SetDark(false);
            if (kb.rKey.wasPressedThisFrame) ResetRun();
        }

        void UpdateHover()
        {
            var vessel = RaycastVessel();
            if (vessel == m_Hover) return;

            if (m_Hover != null) m_Hover.SetHover(false);
            m_Hover = vessel;
            if (m_Hover != null) m_Hover.SetHover(true);
        }

        void HandleClick()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var vessel = RaycastVessel();
            if (vessel == null) return;

            if (m_Ended)
            {
                m_Action = "Run finished — press RESET.";
                RefreshUi();
                return;
            }

            // Click same selected → deselect
            if (m_Selected != null && vessel == m_Selected)
            {
                ClearSelection();
                m_Action = "Deselected. Click a bottle to pick it up.";
                RefreshUi();
                return;
            }

            // Nothing selected → select
            if (m_Selected == null)
            {
                Select(vessel);
                m_Action = $"Picked up {vessel.DisplayName}. Click MIX beaker to pour — or click it again to put down.";
                RefreshUi();
                return;
            }

            // Selected + click other → pour
            TryPour(m_Selected, vessel);
        }

        ChemVessel RaycastVessel()
        {
            if (rayCamera == null) return null;
            var mouse = Mouse.current;
            if (mouse == null) return null;

            var ray = rayCamera.ScreenPointToRay(mouse.position.ReadValue());
            var hits = Physics.RaycastAll(ray, 50f);
            ChemVessel best = null;
            var bestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.gameObject.name is "Substance" or "MixLiquid" or "Label" or "Shadow")
                    continue;

                var v = hit.collider.GetComponent<ChemVessel>()
                        ?? hit.collider.GetComponentInParent<ChemVessel>();
                if (v == null) continue;
                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    best = v;
                }
            }
            return best;
        }

        void Select(ChemVessel vessel)
        {
            if (m_Selected != null && m_Selected != vessel)
                m_Selected.SetSelected(false);
            m_Selected = vessel;
            m_Selected.SetSelected(true);
        }

        void ClearSelection()
        {
            if (m_Selected != null)
                m_Selected.SetSelected(false);
            m_Selected = null;
        }

        void TryPour(ChemVessel source, ChemVessel target)
        {
            var src = source.Role;
            var dst = target.Role;

            // Free pouring: any vessel can receive another — wrong mixes ruin the batch.
            if (src == ChemRole.ReactionBeaker)
            {
                Spoil(target, $"Dumped MIX into {target.DisplayName} — reaction wasted.");
                return;
            }

            // Correct path: reagent → MIX beaker (chemistry steps)
            if (dst == ChemRole.ReactionBeaker)
            {
                if (src == ChemRole.Distractor)
                {
                    Spoil(target, "Poured CuSO₄ (D) into MIX — contaminated.");
                    return;
                }

                var consumed = src switch
                {
                    ChemRole.SilverNitrate => PourA(),
                    ChemRole.SodiumChloride => PourB(),
                    ChemRole.Fixer => PourC(),
                    _ => true
                };

                if (consumed)
                    ClearSelection();
                RefreshUi();
                return;
            }

            // Reagent → another bottle (or any non-MIX vessel): allowed, but wrong for this experiment
            Spoil(target,
                $"Mixed {source.DisplayName} into {target.DisplayName}. Wrong combination — restart.");
        }

        void Spoil(ChemVessel target, string reason)
        {
            if (target != null)
            {
                if (target.Role == ChemRole.ReactionBeaker)
                    m_Beaker?.SetLook(BeakerMix.Look.Contaminated);
                else
                    target.ShowContamination(new Color(0.25f, 0.45f, 0.2f)); // dirty green mix
            }

            Fail(reason);
            ClearSelection();
            RefreshUi();
        }

        bool PourA()
        {
            if (m_Fix > 0f && m_Ag <= 0f)
            {
                Fail("Fixer was added first — no crystal can form.");
                return true;
            }

            m_Ag = TargetAg;
            m_Beaker?.SetLook(BeakerMix.Look.ClearSolution);
            m_Action = m_Dark
                ? "Poured A (AgNO₃) into MIX. Next: pick B, pour into MIX."
                : "Poured A — room is LIGHT. Press DARK, then pour B.";
            TryFormPrecipitate();
            return true;
        }

        bool PourB()
        {
            if (m_Ag <= 0f)
            {
                m_Action = "Need A in the MIX beaker first. Keep holding B, or put down and pick A.";
                return false;
            }

            m_Cl = TargetCl;
            m_Action = "Poured B (NaCl) into MIX.";
            TryFormPrecipitate();
            return true;
        }

        void TryFormPrecipitate()
        {
            if (m_Precipitate || m_Ag < TargetAg || m_Cl < TargetCl) return;
            m_Precipitate = true;
            m_SettleTimer = 0f;
            m_Beaker?.SetLook(BeakerMix.Look.WhitePrecipitate);
            m_Action = m_Dark
                ? "White AgCl formed! Wait ~5s in the dark, then pour C into MIX."
                : "Precipitate formed — PRESS DARK now or it will burn.";
        }

        bool PourC()
        {
            if (!m_Precipitate)
            {
                m_Action = "Need A + B precipitate in MIX first.";
                return false;
            }
            if (!m_Settled)
            {
                m_Action = "Still settling — wait for 100%, then pour C.";
                return false;
            }
            if (!m_Dark)
            {
                Fail("Tried to fix while light was ON.");
                return true;
            }

            m_Fix = TargetFix;
            m_Stabilized = true;
            m_Beaker?.SetLook(BeakerMix.Look.Stabilized);
            m_Action = "Poured C (Fixer). Stabilized! Press LIGHT to activate.";
            return true;
        }

        void SetDark(bool dark)
        {
            m_Dark = dark;
            if (roomKeyLight != null) roomKeyLight.intensity = dark ? 0.12f : 1.15f;
            if (roomFillLight != null) roomFillLight.intensity = dark ? 0.05f : 0.35f;

            if (!m_Ended && !dark && m_Stabilized && m_Precipitate)
            {
                m_Ended = true;
                m_Beaker?.SetLook(BeakerMix.Look.GlowSuccess);
                m_Action = "SUCCESS — silver-blue glow! Press RESET to run again.";
                ClearSelection();
                Debug.Log("[EduQuest] EXPERIMENT SUCCESS");
            }
            else if (!m_Ended)
            {
                m_Action = dark ? "Room is DARK." : "Room is LIGHT.";
            }

            RefreshUi();
        }

        void Fail(string reason)
        {
            if (m_Ended) return;
            m_Ended = true;
            if (m_Beaker != null && m_Beaker.Current != BeakerMix.Look.Contaminated)
                m_Beaker.SetLook(BeakerMix.Look.BurntResidue);
            m_Action = "FAILED — " + reason + "  Press RESET.";
            ClearSelection();
            Debug.LogWarning("[EduQuest] " + m_Action);
            RefreshUi();
        }

        void ResetRun()
        {
            m_Ended = false;
            m_Ag = m_Cl = m_Fix = 0f;
            m_Precipitate = m_Settled = m_Stabilized = false;
            m_SettleTimer = 0f;
            m_Beaker?.SetLook(BeakerMix.Look.Empty);
            ClearSelection();
            foreach (var v in kitRoot != null ? kitRoot.GetComponentsInChildren<ChemVessel>(true) : System.Array.Empty<ChemVessel>())
                v.ResetVisual();
            SetDark(true);
            m_Action = "Ready. Pour freely — but only A→MIX, B→MIX, C→MIX wins.";
            RefreshUi();
        }

        void RefreshUi()
        {
            string step, title, body;
            var tone = GuideHud.Tone.Normal;

            if (m_Ended && m_Beaker != null && m_Beaker.Current == BeakerMix.Look.GlowSuccess)
            {
                step = "SUCCESS";
                title = "Crystal activated!";
                body = "You poured A then B into MIX, waited, poured C, then lit the room.";
                tone = GuideHud.Tone.Success;
            }
            else if (m_Ended)
            {
                step = "FAILED";
                title = "Batch ruined — press RESET";
                body = "You can pour into any container, but only\nA → MIX, then B → MIX, then C → MIX wins.\nWrong mixes (C into A, D anywhere, etc.) fail.";
                tone = GuideHud.Tone.Fail;
            }
            else if (!m_Precipitate)
            {
                step = "Step 1 · Mix";
                title = "Pour A then B into MIX";
                body = "Free pour: pick up any bottle, click any other to pour.\nWinning path: A → MIX, then B → MIX.\nWrong mixes spoil the batch — press RESET.";
            }
            else if (!m_Settled)
            {
                step = "Step 2 · Wait";
                title = "Let the precipitate settle";
                body = "Stay DARK. Don't pour yet.";
                tone = GuideHud.Tone.Warn;
            }
            else if (!m_Stabilized)
            {
                step = "Step 3 · Fix";
                title = "Pour Fixer (C) into MIX";
                body = "Pick up C, click the MIX beaker.";
            }
            else
            {
                step = "Step 4 · Light";
                title = "Activate with LIGHT";
                body = "Press LIGHT — MIX should glow silver-blue.";
                tone = GuideHud.Tone.Warn;
            }

            var selected = m_Selected != null ? m_Selected.DisplayName + " (click again to put down)" : "none";
            ui?.Show(step, title, body, m_Action, selected, BeakerStateText(), tone);
        }

        string BeakerStateText()
        {
            if (m_Beaker == null) return "unknown";
            return m_Beaker.Current switch
            {
                BeakerMix.Look.Empty => "empty",
                BeakerMix.Look.ClearSolution => "clear (has AgNO₃)",
                BeakerMix.Look.WhitePrecipitate => "white AgCl precipitate",
                BeakerMix.Look.RawCrystal => "raw crystal — ready for fixer",
                BeakerMix.Look.Stabilized => "stabilized — ready for light",
                BeakerMix.Look.GlowSuccess => "GLOWING — success",
                BeakerMix.Look.BurntResidue => "burnt residue",
                BeakerMix.Look.Contaminated => "contaminated",
                _ => m_Beaker.Current.ToString()
            };
        }

        void WireVessels()
        {
            if (kitRoot == null) return;

            // Ensure every ChemClickable has a ChemVessel
            foreach (var click in kitRoot.GetComponentsInChildren<ChemClickable>(true))
            {
                var vessel = click.GetComponent<ChemVessel>();
                if (vessel == null) vessel = click.gameObject.AddComponent<ChemVessel>();
                vessel.Configure(click.Role, string.IsNullOrEmpty(click.DisplayName) ? click.Role.ToString() : click.DisplayName);
                vessel.RecacheRestFromCurrent();
            }

            foreach (var v in kitRoot.GetComponentsInChildren<ChemVessel>(true))
            {
                if (v.Role == ChemRole.ReactionBeaker)
                    m_Beaker = v.GetComponent<BeakerMix>() ?? v.gameObject.AddComponent<BeakerMix>();
            }
        }

        void EnsureBeakerVisuals()
        {
            if (m_Beaker == null)
            {
                var t = kitRoot != null ? kitRoot.Find("ReactionBeaker") : null;
                if (t != null)
                    m_Beaker = t.GetComponent<BeakerMix>() ?? t.gameObject.AddComponent<BeakerMix>();
            }
            if (m_Beaker == null) return;

            var liquidTf = m_Beaker.transform.Find("MixLiquid");
            GameObject liquidGo;
            if (liquidTf == null)
            {
                liquidGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                liquidGo.name = "MixLiquid";
                liquidGo.transform.SetParent(m_Beaker.transform, false);
                liquidGo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
                liquidGo.transform.localScale = new Vector3(0.14f, 0.07f, 0.14f);
                Object.DestroyImmediate(liquidGo.GetComponent<Collider>());
            }
            else liquidGo = liquidTf.gameObject;

            var glowTf = m_Beaker.transform.Find("CrystalGlow");
            Light glow;
            if (glowTf == null)
            {
                var g = new GameObject("CrystalGlow");
                g.transform.SetParent(m_Beaker.transform, false);
                g.transform.localPosition = new Vector3(0f, 0.18f, 0f);
                glow = g.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.range = 1.2f;
                glow.enabled = false;
            }
            else glow = glowTf.GetComponent<Light>();

            m_Beaker.Bind(liquidGo.GetComponent<Renderer>(), glow);

            foreach (var t in kitRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t.name is not ("Substance" or "MixLiquid")) continue;
                var col = t.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
            }
        }
    }
}
