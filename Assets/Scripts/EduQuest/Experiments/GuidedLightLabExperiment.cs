using System.Collections;
using EduQuest.AR;
using UnityEngine;
using UnityEngine.UI;

namespace EduQuest.Experiments
{
    /// <summary>
    /// AR-native biology lab: photosynthesis needs REAL light from the room/camera;
    /// night mode needs REAL darkness. A UI slider cannot finish this experiment.
    /// </summary>
    public class GuidedLightLabExperiment : MonoBehaviour, ILabExperiment
    {
        public enum Step
        {
            Place = 0,
            SeekLight = 1,
            Photosynthesis = 2,
            SeekDark = 3,
            NightMode = 4,
            Complete = 5
        }

        [SerializeField] WorldLightSensor lightSensor;
        [SerializeField] Text stepLabel;
        [SerializeField] Text guideTitle;
        [SerializeField] Text guideBody;
        [SerializeField] Text reactionText;
        [SerializeField] Text meterLabel;
        [SerializeField] Image meterFill;
        [SerializeField] Button placeButton;
        [SerializeField] Button confirmButton;
        [SerializeField] Button hintButton;

        [SerializeField] GameObject plantRoot;
        [SerializeField] Transform sprout;
        [SerializeField] Transform leaves;
        [SerializeField] Renderer soil;
        [SerializeField] ParticleSystem oxygenBurst;
        [SerializeField] Light plantGlow;
        [SerializeField] GameObject placementRing;

        Step m_Step = Step.Place;
        bool m_Placed;
        float m_HoldTimer;
        float m_Energy; // built only in real light
        string m_Status = "This lab uses your camera as the light sensor.";

        const float BrightHold = 2.5f;
        const float DarkHold = 2.5f;

        public string Title => "Biology · Light & Life (AR)";
        public string Prompt => "Why must a plant experience real light — not a button — for photosynthesis to make sense in AR?";
        public string Status => m_Status;
        public GameObject Root => gameObject;

        public void Bind(
            WorldLightSensor sensor,
            Text step,
            Text title,
            Text body,
            Text reaction,
            Text meter,
            Image fill,
            Button place,
            Button confirm,
            Button hint,
            GameObject plant,
            Transform sproutTf,
            Transform leavesTf,
            Renderer soilRend,
            ParticleSystem oxygen,
            Light glow,
            GameObject ring)
        {
            lightSensor = sensor;
            stepLabel = step;
            guideTitle = title;
            guideBody = body;
            reactionText = reaction;
            meterLabel = meter;
            meterFill = fill;
            placeButton = place;
            confirmButton = confirm;
            hintButton = hint;
            plantRoot = plant;
            sprout = sproutTf;
            leaves = leavesTf;
            soil = soilRend;
            oxygenBurst = oxygen;
            plantGlow = glow;
            placementRing = ring;

            if (placeButton != null)
            {
                placeButton.onClick.RemoveAllListeners();
                placeButton.onClick.AddListener(PlacePlant);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(ConfirmStep);
            }

            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(RefreshGuide);
            }
        }

        public void Enter()
        {
            gameObject.SetActive(true);
            ResetExperiment();
            if (lightSensor != null)
                StartCoroutine(BootCamera());
        }

        public void Exit()
        {
            gameObject.SetActive(false);
        }

        IEnumerator BootCamera()
        {
            ShowReaction("Starting camera — point it at your real environment.");
            yield return lightSensor.StartSensor();
            if (!lightSensor.IsReady)
                ShowReaction("Camera unavailable. Allow webcam access — this lab cannot run on UI alone.");
            else
                ShowReaction("Camera live. Your room’s light now controls the plant.");
            RefreshGuide();
        }

        public void ResetExperiment()
        {
            m_Step = Step.Place;
            m_Placed = false;
            m_HoldTimer = 0f;
            m_Energy = 0f;
            if (plantRoot) plantRoot.SetActive(false);
            if (placementRing) placementRing.SetActive(true);
            if (plantGlow) plantGlow.enabled = false;
            if (oxygenBurst) oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ApplyPlantVisuals();
            ShowReaction("Place the seedling on your table, then use real light and darkness.");
            RefreshGuide();
            RefreshControls();
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            UpdateMeter();
            RunStepLogic();
            ApplyPlantVisuals();
        }

        void UpdateMeter()
        {
            float b = lightSensor != null ? lightSensor.Brightness : 0f;
            if (meterFill != null)
            {
                meterFill.fillAmount = b;
                meterFill.color = lightSensor != null && lightSensor.IsBright
                    ? new Color(1f, 0.9f, 0.3f)
                    : lightSensor != null && lightSensor.IsDark
                        ? new Color(0.25f, 0.35f, 0.7f)
                        : new Color(0.55f, 0.6f, 0.65f);
            }

            if (meterLabel != null)
            {
                string cam = lightSensor != null && lightSensor.IsReady ? lightSensor.Label : "NO CAMERA";
                meterLabel.text = $"World light: {cam}  ({b:0%})";
            }
        }

        void RunStepLogic()
        {
            if (lightSensor == null || !lightSensor.IsReady)
            {
                if (m_Step > Step.Place)
                    m_Status = "Waiting for camera… AR light sensing required.";
                return;
            }

            switch (m_Step)
            {
                case Step.SeekLight:
                case Step.Photosynthesis:
                    if (lightSensor.IsBright)
                    {
                        m_HoldTimer += Time.deltaTime;
                        m_Energy = Mathf.Clamp01(m_Energy + Time.deltaTime * 0.12f);
                        if (plantGlow != null)
                        {
                            plantGlow.enabled = true;
                            plantGlow.intensity = 0.6f + m_Energy;
                            plantGlow.color = new Color(0.55f, 1f, 0.45f);
                        }

                        if (m_Step == Step.SeekLight && m_HoldTimer >= BrightHold)
                        {
                            Advance(Step.Photosynthesis, "Bright light detected — photosynthesis starting (energy rising).");
                            if (oxygenBurst != null && !oxygenBurst.isPlaying) oxygenBurst.Play();
                        }
                        else if (m_Step == Step.Photosynthesis && m_Energy >= 0.55f)
                        {
                            Advance(Step.SeekDark, "Energy stored. Now move into DARKNESS (cover lens or dim the room).");
                        }
                        else
                        {
                            ShowReaction($"Holding in light… {m_HoldTimer:0.0}s / energy {m_Energy:0%}");
                        }
                    }
                    else
                    {
                        m_HoldTimer = Mathf.Max(0f, m_HoldTimer - Time.deltaTime);
                        if (plantGlow) plantGlow.intensity = Mathf.Lerp(plantGlow.intensity, 0.15f, Time.deltaTime);
                        ShowReaction("Too dim. Point the camera at a window, lamp, or bright surface.");
                    }
                    break;

                case Step.SeekDark:
                case Step.NightMode:
                    if (lightSensor.IsDark)
                    {
                        m_HoldTimer += Time.deltaTime;
                        if (plantGlow != null)
                        {
                            plantGlow.enabled = true;
                            plantGlow.color = new Color(0.25f, 0.35f, 0.8f);
                            plantGlow.intensity = 0.35f;
                        }

                        if (oxygenBurst != null && oxygenBurst.isPlaying)
                            oxygenBurst.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                        if (m_Step == Step.SeekDark && m_HoldTimer >= DarkHold)
                        {
                            Advance(Step.NightMode, "Darkness detected — plant shifts toward night / respiration mode.");
                        }
                        else if (m_Step == Step.NightMode && m_HoldTimer >= DarkHold + 1.2f)
                        {
                            Advance(Step.Complete, "You controlled the lab with real light & dark. That’s why this needs AR/camera.");
                        }
                        else
                            ShowReaction($"Holding in darkness… {m_HoldTimer:0.0}s");
                    }
                    else
                    {
                        m_HoldTimer = Mathf.Max(0f, m_HoldTimer - Time.deltaTime * 1.5f);
                        ShowReaction("Still too bright. Cup your hand over the camera or face a dark corner.");
                    }
                    break;
            }
        }

        public void PlacePlant()
        {
            m_Placed = true;
            if (plantRoot) plantRoot.SetActive(true);
            if (placementRing) placementRing.SetActive(false);
            ShowReaction("Seedling placed on your table surface.");
            Advance(Step.SeekLight, "Now find REAL light with the camera — no button can replace the Sun.");
        }

        public void ConfirmStep()
        {
            // Confirm only acknowledges; light/dark still required
            if (m_Step == Step.Place && !m_Placed)
            {
                PlacePlant();
                return;
            }

            if (m_Step == Step.Complete)
            {
                ShowReaction("Open Reflect and write why the camera mattered.");
                return;
            }

            ShowReaction("Keep going — the meter must reach BRIGHT or DARK using your environment.");
            RefreshGuide();
        }

        void Advance(Step step, string reaction)
        {
            if ((int)step < (int)m_Step) return;
            m_Step = step;
            m_HoldTimer = 0f;
            if (!string.IsNullOrEmpty(reaction)) ShowReaction(reaction);
            RefreshGuide();
            RefreshControls();
        }

        void ShowReaction(string msg)
        {
            m_Status = msg;
            if (reactionText != null) reactionText.text = "Reaction: " + msg;
        }

        void RefreshGuide()
        {
            if (stepLabel != null)
                stepLabel.text = m_Step == Step.Complete ? "Done" : $"Step {(int)m_Step + 1} / 5";

            string title;
            string body;
            switch (m_Step)
            {
                case Step.Place:
                    title = "1 · Place on your table";
                    body = "In AR this anchors to a real surface.\n\nPress Place seedling.\n\nWhy AR? The plant lives in YOUR room — next you’ll use that room’s light.";
                    break;
                case Step.SeekLight:
                    title = "2 · Find real LIGHT";
                    body = "Point the camera at a bright window or lamp.\nHold until the meter says BRIGHT.\n\nThere is no “Add light” cheat — the physical world is the control.";
                    break;
                case Step.Photosynthesis:
                    title = "3 · Photosynthesis";
                    body = "Keep the bright view. Energy builds and oxygen particles appear.\n\nIf you turn away from the light, progress stalls.";
                    break;
                case Step.SeekDark:
                    title = "4 · Find real DARKNESS";
                    body = "Cover the lens or move to a dark corner.\nHold until the meter says DARK.\n\nSame plant, opposite environment → opposite reaction.";
                    break;
                case Step.NightMode:
                    title = "5 · Night mode";
                    body = "In darkness the plant stops the light-driven burst and shifts mood.\nHold a moment to finish.";
                    break;
                default:
                    title = "Complete";
                    body = "You proved the lab needs the real world:\nlight → energy, dark → night mode.\n\nPress Reflect.";
                    break;
            }

            if (guideTitle != null) guideTitle.text = title;
            if (guideBody != null) guideBody.text = body;
        }

        void RefreshControls()
        {
            if (placeButton != null)
                placeButton.interactable = m_Step == Step.Place;
            if (confirmButton != null)
                confirmButton.interactable = true;
        }

        void ApplyPlantVisuals()
        {
            float grow = Mathf.Lerp(0.15f, 1f, m_Energy);
            if (sprout != null)
            {
                sprout.localScale = new Vector3(0.08f, 0.15f + grow * 0.55f, 0.08f);
                sprout.localPosition = new Vector3(0f, sprout.localScale.y * 0.5f + 0.05f, 0f);
            }

            if (leaves != null)
            {
                float open = m_Step >= Step.Photosynthesis ? Mathf.Lerp(0.15f, 0.45f, m_Energy) : 0.12f;
                if (m_Step >= Step.NightMode) open *= 0.7f; // slight droop cue
                leaves.localScale = Vector3.one * open;
                leaves.localPosition = new Vector3(0f, (sprout != null ? sprout.localPosition.y : 0.4f) + 0.2f, 0f);
                var r = leaves.GetComponent<Renderer>();
                if (r != null)
                {
                    var block = new MaterialPropertyBlock();
                    r.GetPropertyBlock(block);
                    var day = new Color(0.2f, 0.75f, 0.3f);
                    var night = new Color(0.15f, 0.35f, 0.28f);
                    var c = m_Step >= Step.NightMode ? night : Color.Lerp(new Color(0.35f, 0.45f, 0.25f), day, m_Energy);
                    block.SetColor("_BaseColor", c);
                    block.SetColor("_Color", c);
                    r.SetPropertyBlock(block);
                }
            }

            if (soil != null)
            {
                var block = new MaterialPropertyBlock();
                soil.GetPropertyBlock(block);
                var c = new Color(0.35f, 0.25f, 0.14f);
                block.SetColor("_BaseColor", c);
                block.SetColor("_Color", c);
                soil.SetPropertyBlock(block);
            }
        }
    }
}
