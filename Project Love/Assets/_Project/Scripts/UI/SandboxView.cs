using System;
using System.Collections;
using System.IO;
using ProjectLove.Simulation;
using ProjectLove.Core;
using ProjectLove.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    public sealed class SandboxView : MonoBehaviour
    {
        public GameConfigSO config;
        public SandboxPresentationSO presentation;
        public EventDataSO[] events = new EventDataSO[0];
        public ActivityDataSO[] activities = new ActivityDataSO[0];
        public TMP_Text titleText, subtitleText, dayText, statsText, characterText, speakerText, dialogueText, summaryText;
        public Image portrait;
        public Button continueButton, phaseButton, endDayButton, nextDayButton, restartButton;
        public Transform eventButtons, activityButtons, choiceButtons;
        public Button buttonTemplate;
        public Button saveButton, loadButton, loadAutoButton;
        public TMP_Text feedbackText;
        [Min(0)] public int prototypeDays;
        public SaveStore Saves { get; private set; }
        public EventScheduler Scheduler { get; private set; }
        private string feedback = "";
        public GameSession Session { get; private set; }
        public bool IsRevealing { get; private set; }
        private Coroutine reveal;
        private string displayedLineId;

        private void Start()
        {
            continueButton.onClick.AddListener(Continue);
            phaseButton.onClick.AddListener(() => Session.TryAdvancePhase());
            endDayButton.onClick.AddListener(() => { if (Session.TryEndDay()) Save(SaveSlot.Auto); });
            nextDayButton.onClick.AddListener(() =>
            {
                if (prototypeDays > 0 && Session.State.day >= prototypeDays) return;
                if (Session.TryStartNextDay()) Save(SaveSlot.Auto);
            });
            restartButton.onClick.AddListener(Restart);
            SetSaveDirectory(Path.Combine(Application.persistentDataPath, "PrototypeSaves"));
            if (saveButton != null) saveButton.onClick.AddListener(() => Save(SaveSlot.Manual));
            if (loadButton != null) loadButton.onClick.AddListener(() => Load(SaveSlot.Manual));
            if (loadAutoButton != null) loadAutoButton.onClick.AddListener(() => Load(SaveSlot.Auto));
            Restart();
        }

        private void OnDestroy()
        {
            if (Session != null) Session.Changed -= Refresh;
        }

        public void SetSaveDirectory(string directory) { Saves = new SaveStore(directory); }

        public SaveResult Save(SaveSlot slot)
        {
            var result = Saves.Save(Session, slot);
            feedback = result == SaveResult.Success ? presentation.savedText :
                result == SaveResult.Busy ? presentation.busyText : presentation.saveFailedText;
            Refresh();
            return result;
        }

        public SaveResult Load(SaveSlot slot)
        {
            var result = Saves.Load(Session, slot);
            feedback = result == SaveResult.Success ? presentation.loadedText :
                result == SaveResult.Busy ? presentation.busyText : presentation.loadFailedText;
            Refresh();
            return result;
        }

        public void Restart()
        {
            if (Session != null) Session.Changed -= Refresh;
            if (reveal != null) StopCoroutine(reveal);
            IsRevealing = false;
            displayedLineId = null;
            feedback = "";
            Session = new GameSession(config);
            Scheduler = new EventScheduler(Session, events);
            Session.Changed += Refresh;
            Refresh();
        }

        public void Continue()
        {
            if (IsRevealing) { RevealAll(); return; }
            if (Session.Dialogue.ChoicesReady || !Session.Dialogue.TryAdvance())
                Session.Dialogue.TryDefer();
        }

        public void RevealAll()
        {
            if (reveal != null) StopCoroutine(reveal);
            dialogueText.maxVisibleCharacters = int.MaxValue;
            IsRevealing = false;
        }

        private void Refresh()
        {
            var state = Session.State;
            titleText.text = presentation.title;
            subtitleText.text = presentation.subtitle;
            string phase = (int)state.phase < presentation.phaseNames.Length ? presentation.phaseNames[(int)state.phase] : state.phase.ToString();
            dayText.text = string.Format(presentation.dayFormat, state.day, config.totalDays, phase, state.actionPoints, config.maxActionPointsPerDay);
            statsText.text = string.Format(presentation.statsFormat, config.mainProgressLabel, state.mainProgress, state.money, state.skillPoints);
            if (config.characters.Length > 0)
            {
                var character = config.characters[0];
                var runtime = state.FindCharacter(character.id);
                characterText.text = string.Format(presentation.characterFormat, character.displayName, runtime.affection, runtime.stress);
            }
            SetButtonText(continueButton, Session.Dialogue.ChoicesReady ? presentation.deferLabel : presentation.continueLabel);
            if (saveButton != null) SetButtonText(saveButton, presentation.saveLabel);
            if (loadButton != null) SetButtonText(loadButton, presentation.loadLabel);
            if (loadAutoButton != null) SetButtonText(loadAutoButton, presentation.loadAutoLabel);
            bool safe = !Session.Dialogue.IsActive;
            if (saveButton != null) saveButton.interactable = safe;
            if (loadButton != null) loadButton.interactable = safe && Saves.Exists(Session, SaveSlot.Manual);
            if (loadAutoButton != null) loadAutoButton.interactable = safe && Saves.Exists(Session, SaveSlot.Auto);
            if (feedbackText != null) feedbackText.text = feedback;
            SetButtonText(phaseButton, presentation.nextPhaseLabel);
            SetButtonText(endDayButton, presentation.endDayLabel);
            SetButtonText(nextDayButton, presentation.nextDayLabel);
            SetButtonText(restartButton, presentation.restartLabel);
            phaseButton.interactable = !state.dayEnded && !Session.Dialogue.IsActive && state.phase != DayPhase.Night;
            endDayButton.interactable = !state.dayEnded && !Session.Dialogue.IsActive;
            bool prototypeEnded = prototypeDays > 0 && state.day >= prototypeDays && state.dayEnded;
            nextDayButton.gameObject.SetActive(state.dayEnded && !Session.Days.CampaignEnded && !prototypeEnded);
            summaryText.text = !state.dayEnded ? "" : prototypeEnded ? presentation.prototypeEndedText :
                Session.Days.CampaignEnded ? presentation.campaignEndedText :
                string.Format(presentation.dayEndedFormat, state.day, state.mainProgress, state.mainProgress - state.dayStartProgress);

            ClearButtons(eventButtons);
            foreach (var data in Scheduler.Available())
            {
                if (data == null) continue;
                var captured = data;
                AddButton(eventButtons, data.displayName, Session.CanStartEvent(data), () => Session.Dialogue.TryBegin(captured));
            }
            ClearButtons(activityButtons);
            foreach (var activity in activities)
            {
                if (activity == null) continue;
                var captured = activity;
                string label = string.Format(presentation.activityFormat, activity.displayName, activity.actionPointCost);
                AddButton(activityButtons, label + " · " + presentation.successLabel,
                    Session.CanCompleteActivity(activity, true), () => Session.TryCompleteActivity(captured, true));
                AddButton(activityButtons, label + " · " + presentation.failureLabel,
                    Session.CanCompleteActivity(activity, false), () => Session.TryCompleteActivity(captured, false));
            }
            ClearButtons(choiceButtons);
            if (Session.Dialogue.ChoicesReady)
            {
                foreach (var choice in Session.Dialogue.Node.choices)
                {
                    if (choice == null) continue;
                    var captured = choice;
                    bool enabled = Session.Dialogue.CanChoose(choice);
                    AddButton(choiceButtons, choice.text + (enabled ? "" : " · " + presentation.unavailableLabel), enabled,
                        () => Session.Dialogue.TryChoose(captured));
                }
            }
            continueButton.gameObject.SetActive(Session.Dialogue.IsActive);
            var line = Session.Dialogue.CurrentLine;
            speakerText.text = line != null && line.speaker != null ? line.speaker.displayName : presentation.narratorLabel;
            portrait.sprite = line != null && line.speaker != null ? line.speaker.GetSprite(line.expression) : null;
            portrait.enabled = portrait.sprite != null;
            if (line != null && displayedLineId != line.lineId)
            {
                if (reveal != null) StopCoroutine(reveal);
                displayedLineId = line.lineId;
                dialogueText.text = line.text;
                dialogueText.maxVisibleCharacters = 0;
                dialogueText.ForceMeshUpdate();
                reveal = StartCoroutine(Reveal());
            }
            else if (line == null)
            {
                RevealAll();
                displayedLineId = null;
                if (!Session.Dialogue.ChoicesReady) dialogueText.text = presentation.readyText;
                else if (!Session.Dialogue.HasAvailableChoice) dialogueText.text = presentation.blockedText;
            }
        }

        private IEnumerator Reveal()
        {
            int count = dialogueText.textInfo.characterCount;
            IsRevealing = true;
            if (presentation.charactersPerSecond <= 0) { RevealAll(); yield break; }
            float visible = 0;
            while (dialogueText.maxVisibleCharacters < count)
            {
                visible += presentation.charactersPerSecond * Time.unscaledDeltaTime;
                dialogueText.maxVisibleCharacters = Mathf.Min(count, Mathf.FloorToInt(visible));
                yield return null;
            }
            IsRevealing = false;
        }

        private Button AddButton(Transform parent, string text, bool enabled, Action action)
        {
            var button = Instantiate(buttonTemplate, parent);
            button.gameObject.SetActive(true);
            button.interactable = enabled;
            SetButtonText(button, text);
            button.onClick.AddListener(() => action());
            return button;
        }

        private static void SetButtonText(Button button, string value)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            text.enableAutoSizing = true;
            text.fontSizeMin = 18;
            text.fontSizeMax = 22;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.text = value;
        }
        private static void ClearButtons(Transform parent)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }
    }
}
