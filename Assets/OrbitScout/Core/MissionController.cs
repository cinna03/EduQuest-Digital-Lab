using System;
using System.Collections.Generic;
using OrbitScout.View;
using UnityEngine;

namespace OrbitScout.Core
{
    public enum Level4Phase
    {
        None,
        Reading,
        Answering
    }

    public struct LevelRunResult
    {
        public LevelId Level;
        public bool PassedUnlock;
        public int Score;
        public int CorrectCount;
        public int TotalQuestions;
        public string Summary;
    }

    public class MissionController : MonoBehaviour
    {
        public static MissionController Instance { get; private set; }

        public LevelId ActiveLevel { get; private set; }
        public bool IsPlaying { get; private set; }
        public int SessionScore { get; private set; }
        public Level4Phase Level4Phase { get; private set; }

        public float TimeRemaining { get; private set; }
        public int QuestionIndex { get; private set; }
        public int TotalQuestions { get; private set; }
        public int CorrectQuestions { get; private set; }

        List<LevelCatalog.FactQuestion> factRun;
        List<LevelCatalog.MultiQuestion> multiRun;
        HashSet<PlanetId> multiSelected;
        HashSet<PlanetId> multiRequired;
        int level2SavedCount;

        readonly Dictionary<PlanetId, int> saveSteps = new Dictionary<PlanetId, int>();
        readonly Dictionary<PlanetId, int> crackSteps = new Dictionary<PlanetId, int>();
        readonly HashSet<PlanetId> exploded = new HashSet<PlanetId>();

        float level4PhaseTimer;
        int level4QuestionIndex;

        public event Action<LevelId> OnLevelStarted;
        public event Action<LevelRunResult> OnLevelEnded;
        public event Action<string, string> OnQuestionChanged;
        public event Action<string> OnFeedback;
        public event Action<int> OnScoreChanged;
        public event Action<float> OnTimerTick;
        public event Action<Level4Phase> OnLevel4PhaseChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitPlanetStateMaps();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (!IsPlaying)
                return;

            if (ActiveLevel == LevelId.Level3)
            {
                TimeRemaining -= Time.deltaTime;
                OnTimerTick?.Invoke(TimeRemaining);
                if (TimeRemaining <= 0f)
                    EndLevel(false, "Time's up!");
            }

            if (ActiveLevel == LevelId.Level4 && Level4Phase != Level4Phase.None)
            {
                level4PhaseTimer -= Time.deltaTime;
                OnTimerTick?.Invoke(level4PhaseTimer);
                if (level4PhaseTimer <= 0f)
                    AdvanceLevel4Phase();
            }
        }

        void InitPlanetStateMaps()
        {
            saveSteps.Clear();
            crackSteps.Clear();
            foreach (PlanetId id in AllPlanets())
            {
                saveSteps[id] = 0;
                crackSteps[id] = 0;
            }
        }

        static IEnumerable<PlanetId> AllPlanets()
        {
            yield return PlanetId.Mercury;
            yield return PlanetId.Venus;
            yield return PlanetId.Earth;
            yield return PlanetId.Mars;
            yield return PlanetId.Jupiter;
            yield return PlanetId.Saturn;
            yield return PlanetId.Uranus;
            yield return PlanetId.Neptune;
        }

        public void StartLevel(LevelId level)
        {
            ActiveLevel = level;
            IsPlaying = true;
            SessionScore = 0;
            QuestionIndex = 0;
            CorrectQuestions = 0;
            Level4Phase = Level4Phase.None;
            level4QuestionIndex = 0;
            level2SavedCount = 0;
            exploded.Clear();
            InitPlanetStateMaps();
            PlanetRegistry.ResetAllForLevel(
                level == LevelId.Level2 ? LevelVisualMode.Level2Greyscale : LevelVisualMode.FullColor);

            factRun = null;
            multiRun = null;
            multiSelected = new HashSet<PlanetId>();
            multiRequired = new HashSet<PlanetId>();

            switch (level)
            {
                case LevelId.Level1:
                    factRun = LevelCatalog.BuildLevel1Run();
                    TotalQuestions = factRun.Count;
                    TimeRemaining = -1f;
                    break;
                case LevelId.Level2:
                    factRun = LevelCatalog.BuildLevel2Run();
                    TotalQuestions = factRun.Count;
                    TimeRemaining = -1f;
                    RefreshAllPlanetVisuals();
                    break;
                case LevelId.Level3:
                    multiRun = LevelCatalog.BuildLevel3Run();
                    TotalQuestions = multiRun.Count;
                    TimeRemaining = 600f;
                    break;
                case LevelId.Level4:
                    multiRun = LevelCatalog.BuildLevel4Run();
                    TotalQuestions = multiRun.Count;
                    TimeRemaining = -1f;
                    break;
            }

            OnLevelStarted?.Invoke(level);
            OnScoreChanged?.Invoke(SessionScore);
            PushNextQuestion();
        }

        public void StopLevel()
        {
            IsPlaying = false;
            Level4Phase = Level4Phase.None;
        }

        public bool CanAcceptPlanetTap()
        {
            if (!IsPlaying)
                return false;

            if (ActiveLevel == LevelId.Level4 && Level4Phase != Level4Phase.Answering)
                return false;

            return true;
        }

        public bool SubmitPlanet(PlanetId planetId)
        {
            if (!CanAcceptPlanetTap())
                return false;

            if (exploded.Contains(planetId))
                return false;

            switch (ActiveLevel)
            {
                case LevelId.Level1:
                    return HandleLevel1Tap(planetId);
                case LevelId.Level2:
                    return HandleLevel2Tap(planetId);
                case LevelId.Level3:
                    return HandleMultiTap(planetId, failOnWrong: true, showCount: true);
                case LevelId.Level4:
                    return HandleMultiTap(planetId, failOnWrong: true, showCount: false);
            }

            return false;
        }

        public void SubmitNoMatchingPlanet()
        {
            if (!IsPlaying || ActiveLevel != LevelId.Level4 || Level4Phase != Level4Phase.Answering)
                return;

            if (multiRequired.Count == 0)
                CompleteMultiQuestion(true);
            else
                FailMultiQuestion("That wasn't empty — at least one planet matches.");
        }

        bool HandleLevel1Tap(PlanetId planetId)
        {
            LevelCatalog.FactQuestion q = factRun[QuestionIndex];
            if (planetId == q.Planet)
            {
                CorrectQuestions++;
                AddScore(100);
                PlanetRegistry.Get(planetId)?.FlashCorrect();
                OnFeedback?.Invoke(MissionBanter.FormatCorrect("Level 1 fact cleared.", 1));
                AdvanceQuestion();
                return true;
            }

            PlanetRegistry.Get(planetId)?.FlashWrong();
            OnFeedback?.Invoke(MissionBanter.GetWrongTap(planetId, q.Planet));
            AdvanceQuestion();
            return false;
        }

        bool HandleLevel2Tap(PlanetId planetId)
        {
            LevelCatalog.FactQuestion q = factRun[QuestionIndex];
            if (planetId == q.Planet)
            {
                CorrectQuestions++;
                AddScore(120);
                ApplyLevel2Correct(planetId);
                PlanetRegistry.Get(planetId)?.FlashCorrect();
                OnFeedback?.Invoke("Saved! +" + saveSteps[planetId] + "/3 facts for " + planetId);
                AdvanceQuestion();
                return true;
            }

            ApplyLevel2Wrong(planetId);
            OnFeedback?.Invoke(MissionBanter.GetWrongTap(planetId, q.Planet) + " (Crack " + crackSteps[planetId] + "/3 on " + planetId + ")");
            AdvanceQuestion();
            return false;
        }

        void ApplyLevel2Correct(PlanetId planetId)
        {
            if (exploded.Contains(planetId))
                return;

            if (crackSteps[planetId] > 0)
                crackSteps[planetId]--;

            int before = saveSteps[planetId];
            if (saveSteps[planetId] < 3)
                saveSteps[planetId]++;

            if (before < 3 && saveSteps[planetId] >= 3)
            {
                level2SavedCount++;
                AddScore(400);
                OnFeedback?.Invoke(planetId + " fully restored! +400");
            }

            RefreshPlanetVisual(planetId);
        }

        void ApplyLevel2Wrong(PlanetId planetId)
        {
            if (exploded.Contains(planetId))
                return;

            crackSteps[planetId]++;
            if (saveSteps[planetId] > 0)
                saveSteps[planetId]--;

            if (crackSteps[planetId] >= 3)
            {
                exploded.Add(planetId);
                OnFeedback?.Invoke(planetId + " couldn't take more mistakes — lost!");
                RemoveRemainingQuestionsForPlanet(planetId);
            }

            RefreshPlanetVisual(planetId);
        }

        /// <summary>
        /// Level 2: once a planet is destroyed, drop any not-yet-asked clues for that planet.
        /// </summary>
        void RemoveRemainingQuestionsForPlanet(PlanetId planetId)
        {
            if (factRun == null || ActiveLevel != LevelId.Level2)
                return;

            for (int i = factRun.Count - 1; i > QuestionIndex; i--)
            {
                if (factRun[i].Planet == planetId)
                    factRun.RemoveAt(i);
            }

            TotalQuestions = factRun.Count;
        }

        void RefreshPlanetVisual(PlanetId id)
        {
            PlanetBody body = PlanetRegistry.Get(id);
            if (body == null)
                return;

            body.ApplyLevel2Progress(saveSteps[id], crackSteps[id], exploded.Contains(id));
        }

        void RefreshAllPlanetVisuals()
        {
            foreach (PlanetId id in AllPlanets())
                RefreshPlanetVisual(id);
        }

        bool HandleMultiTap(PlanetId planetId, bool failOnWrong, bool showCount)
        {
            // Level 4 empty clue: any planet tap is wrong; correct move is to wait out the timer.
            if (ActiveLevel == LevelId.Level4 && multiRequired.Count == 0)
            {
                PlanetRegistry.Get(planetId)?.FlashWrong();
                FailMultiQuestion("No planet matched that clue — you shouldn't tap any.");
                return false;
            }

            if (multiRequired.Contains(planetId))
            {
                if (multiSelected.Contains(planetId))
                    return true;

                multiSelected.Add(planetId);
                PlanetRegistry.Get(planetId)?.FlashCorrect();
                if (multiSelected.Count >= multiRequired.Count)
                    CompleteMultiQuestion(true);
                else
                    OnFeedback?.Invoke("Good — keep going (" + multiSelected.Count + "/" + multiRequired.Count + ").");

                return true;
            }

            if (failOnWrong)
            {
                PlanetRegistry.Get(planetId)?.FlashWrong();
                // Level 4: wrong pick fails this question but the run continues through all 5.
                FailMultiQuestion(ActiveLevel == LevelId.Level4
                    ? "Wrong pick — that question failed. Next clue coming…"
                    : "Wrong pick — question failed.");
                return false;
            }

            return false;
        }

        void FailMultiQuestion(string message)
        {
            OnFeedback?.Invoke(message);
            AdvanceQuestion();
        }

        void CompleteMultiQuestion(bool success)
        {
            if (success)
            {
                CorrectQuestions++;
                AddScore(ActiveLevel == LevelId.Level4 ? 200 : 150);
            }

            AdvanceQuestion();
        }

        void PushNextQuestion()
        {
            multiSelected.Clear();
            multiRequired.Clear();

            if (ActiveLevel == LevelId.Level1 || ActiveLevel == LevelId.Level2)
            {
                if (ActiveLevel == LevelId.Level2)
                {
                    while (QuestionIndex < factRun.Count && exploded.Contains(factRun[QuestionIndex].Planet))
                        QuestionIndex++;
                }

                if (QuestionIndex >= factRun.Count)
                {
                    FinishLevel();
                    return;
                }

                LevelCatalog.FactQuestion q = factRun[QuestionIndex];
                string header = "Q " + (QuestionIndex + 1) + "/" + TotalQuestions;
                OnQuestionChanged?.Invoke(header, q.Prompt);
                return;
            }

            if (ActiveLevel == LevelId.Level3)
            {
                if (QuestionIndex >= multiRun.Count)
                {
                    FinishLevel();
                    return;
                }

                LevelCatalog.MultiQuestion q = multiRun[QuestionIndex];
                foreach (PlanetId p in q.Planets)
                    multiRequired.Add(p);

                OnQuestionChanged?.Invoke("Q " + (QuestionIndex + 1) + "/" + TotalQuestions, q.Prompt);
                return;
            }

            if (ActiveLevel == LevelId.Level4)
            {
                if (level4QuestionIndex >= multiRun.Count)
                {
                    FinishLevel();
                    return;
                }

                LevelCatalog.MultiQuestion q = multiRun[level4QuestionIndex];
                foreach (PlanetId p in q.Planets)
                    multiRequired.Add(p);

                QuestionIndex = level4QuestionIndex;
                BeginLevel4Question(q);
            }
        }

        void BeginLevel4Question(LevelCatalog.MultiQuestion q)
        {
            multiSelected.Clear();
            Level4Phase = Level4Phase.Reading;
            level4PhaseTimer = 10f;
            OnLevel4PhaseChanged?.Invoke(Level4Phase.Reading);
            OnQuestionChanged?.Invoke(
                "Q " + (level4QuestionIndex + 1) + "/" + TotalQuestions + " · READ",
                q.Prompt);
            OnFeedback?.Invoke(string.Empty);
        }

        void AdvanceLevel4Phase()
        {
            if (Level4Phase == Level4Phase.Reading)
            {
                Level4Phase = Level4Phase.Answering;
                level4PhaseTimer = 10f;
                OnLevel4PhaseChanged?.Invoke(Level4Phase.Answering);
                OnQuestionChanged?.Invoke(
                    "Q " + (level4QuestionIndex + 1) + "/" + TotalQuestions + " · ANSWER",
                    multiRun[level4QuestionIndex].Prompt);
                OnFeedback?.Invoke(string.Empty);
                return;
            }

            if (Level4Phase == Level4Phase.Answering)
            {
                // Empty-answer questions: not tapping any planet is the correct response.
                if (multiRequired.Count == 0)
                    CompleteMultiQuestion(true);
                else
                    FailMultiQuestion("Time's up — you needed to tap the matching planet(s).");
            }
        }

        void AdvanceQuestion()
        {
            QuestionIndex++;

            if (ActiveLevel == LevelId.Level4)
            {
                level4QuestionIndex = QuestionIndex;
                Level4Phase = Level4Phase.None;
                OnLevel4PhaseChanged?.Invoke(Level4Phase.None);
            }

            PushNextQuestion();
        }

        void AddScore(int points)
        {
            SessionScore += points;
            OnScoreChanged?.Invoke(SessionScore);
        }

        void FinishLevel()
        {
            bool passed = EvaluatePass(out string summary);
            GameProgress.TrySetLevelHighScore(ActiveLevel, SessionScore);

            if (passed)
            {
                if (ActiveLevel == LevelId.Level1)
                    GameProgress.UnlockLevel(LevelId.Level2);
                else if (ActiveLevel == LevelId.Level2)
                    GameProgress.UnlockLevel(LevelId.Level3);
                else if (ActiveLevel == LevelId.Level3)
                    GameProgress.UnlockLevel(LevelId.Level4);
            }

            GameProgress.Save();
            EndLevel(passed, summary);
        }

        bool EvaluatePass(out string summary)
        {
            switch (ActiveLevel)
            {
                case LevelId.Level1:
                    bool l1 = CorrectQuestions >= 5;
                    summary = l1
                        ? "Passed! " + CorrectQuestions + "/8 — Level 2 unlocked."
                        : "Need 5/8 correct. Got " + CorrectQuestions + "/8.";
                    return l1;

                case LevelId.Level2:
                    bool l2 = level2SavedCount >= 3;
                    summary = l2
                        ? "Passed! Saved " + level2SavedCount + " planets — Level 3 unlocked."
                        : "Need 3 fully saved planets. Saved " + level2SavedCount + ".";
                    return l2;

                case LevelId.Level3:
                    bool l3 = CorrectQuestions >= 7;
                    summary = l3
                        ? "Passed! " + CorrectQuestions + "/10 — Level 4 unlocked."
                        : "Need 7/10 correct in 10 minutes. Got " + CorrectQuestions + "/10.";
                    return l3;

                case LevelId.Level4:
                    bool l4 = CorrectQuestions >= 5;
                    summary = l4
                        ? "Perfect gauntlet! 5/5 — Orbit Scout mastered."
                        : "Need 5/5. Got " + CorrectQuestions + "/5.";
                    return l4;

                default:
                    summary = string.Empty;
                    return false;
            }
        }

        void EndLevel(bool passedUnlock, string summary)
        {
            IsPlaying = false;
            Level4Phase = Level4Phase.None;

            var result = new LevelRunResult
            {
                Level = ActiveLevel,
                PassedUnlock = passedUnlock,
                Score = SessionScore,
                CorrectCount = CorrectQuestions,
                TotalQuestions = TotalQuestions,
                Summary = summary
            };

            OnLevelEnded?.Invoke(result);
        }

        public int CountLevel2SavedPlanets()
        {
            return level2SavedCount;
        }
    }
}
