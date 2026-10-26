using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    public sealed partial class StudioSprintView
    {
        private RectTransform skinHud, workerMeters, roster;
        private void InitializeSkin()
        {
            if (!Presentation.useStudioSkin) return;
            view.hudText.gameObject.SetActive(false);
            skinHud = StudioUiWidgets.Rect("Studio HUD Metrics", view.hudText.transform.parent, .25f, .05f, .985f, .95f);
            workerMeters = StudioUiWidgets.Rect("Worker Meters", view.dialogueModal.transform, .04f, .41f, .55f, .50f);
            status.rectTransform.anchorMin = new Vector2(.04f, .505f); status.fontSizeMax = 22;
            roster = StudioUiWidgets.Rect("Studio Roster", view.panelTitle.transform.parent, .05f, .25f, .95f, .79f);
            StudioUiWidgets.Panel(view.panelTitle.transform.parent.GetComponent<Image>(), Presentation);
            StudioUiWidgets.Panel(view.speakerText.transform.parent.GetComponent<Image>(), Presentation);
            var back = Presentation.Icon("back");
            if (back != null)
            {
                var vector = view.deferButton.GetComponentInChildren<BackArrowGraphic>(true);
                if (vector != null) vector.gameObject.SetActive(false);
                StudioUiWidgets.Image("Back Icon", view.deferButton.transform, .16f, .16f, .84f, .84f, back, Color.white).preserveAspect = true;
            }
        }
        private static void ClearVisuals(Transform root)
        { foreach (Transform child in root) { child.gameObject.SetActive(false); Destroy(child.gameObject); } }
        private void RefreshSkin()
        {
            if (!Presentation.useStudioSkin || skinHud == null) return;
            ClearVisuals(skinHud); workerMeters.gameObject.SetActive(false);
            var state = Session.State;
            string phase = state.dayEnded ? T("dayEnded") : Presentation.phaseNames[(int)state.phase];
            StudioUiWidgets.Metric(Presentation, "Day", skinHud, 0, 0, .28f, 1,
                Presentation.Icon(state.phase == DayPhase.Night ? "night" : "day"), string.Format(T("hudDay"), state.day, Session.Config.totalDays, phase), -1, Color.white);
            StudioUiWidgets.Metric(Presentation, "Money", skinHud, .29f, 0, .44f, 1, Presentation.Icon("money"), string.Format(T("hudMoney"), state.money), -1, Color.white);
            StudioUiWidgets.Metric(Presentation, "Stamina", skinHud, .45f, 0, .72f, 1, Presentation.Icon("stamina"),
                string.Format(T("hudEnergy"), state.stamina, Session.Config.maxStamina), (float)state.stamina / Session.Config.maxStamina, Presentation.staminaTint);
            StudioUiWidgets.Metric(Presentation, "Main Progress", skinHud, .74f, 0, 1, 1, Presentation.Icon("progress"),
                string.Format(T("hudProgress"), state.mainProgress), state.mainProgress / 100f, Presentation.progressTint);
            StudioUiWidgets.DecorateButton(boardButton, Presentation, Presentation.Icon("sprint_board"));
            StudioUiWidgets.DecorateButton(phaseButton, Presentation, Presentation.Icon("time"));
            bool overview = view.IsOverviewPanel && Session.Events.DeferredGlobalEvents().Length == 0;
            roster.gameObject.SetActive(overview);
            if (!overview) return;
            ClearVisuals(roster);
            var workers = Work.Config.workers;
            for (int i = 0; i < workers.Length; i++)
            {
                var worker = workers[i]; var person = Session.State.FindCharacter(worker.character.id); var task = Work.Active(worker.character);
                float high = 1 - (float)i / workers.Length;
                var card = Panel("Team " + worker.character.id, roster, 0, high - 1f / workers.Length + .025f, 1, high, Color.white);
                StudioUiWidgets.Panel(card.GetComponent<Image>(), Presentation, true);
                var avatar = StudioUiWidgets.Image("Owner", card, .015f, .20f, .18f, .95f, worker.character.taskAvatar, Color.white); avatar.preserveAspect = true;
                StudioUiWidgets.Image("Avatar Frame", card, .015f, .20f, .18f, .95f, Presentation.avatarFrameSprite, worker.stream.tint).preserveAspect = true;
                StudioUiWidgets.Text(Presentation, "Worker", card, .21f, .72f, .98f, .98f, worker.character.displayName + " · " + worker.stream.displayName, 23);
                string progress = task == null ? T("noTask") : string.Format(T("dependencyProgress"), task.effort, Work.Definition(task).effort, TaskStatus(task));
                StudioUiWidgets.Text(Presentation, "Task", card, .21f, .40f, .98f, .72f, progress, 19);
                if (task != null) Bar("Team Work", card, .21f, .29f, .98f, .38f, (float)task.effort / Work.Definition(task).effort, worker.stream.tint);
                StudioUiWidgets.Image("Stress Icon", card, .21f, .02f, .27f, .24f, Presentation.Icon("stress"), Color.white).preserveAspect = true;
                StudioUiWidgets.Text(Presentation, "Stress", card, .29f, .02f, .58f, .25f, person.stress + "/100", 19);
                Bar("Team Stress", card, .60f, .06f, .98f, .17f, person.stress / 100f, Presentation.stressTint);
                StudioUiWidgets.Hint(card.gameObject, Presentation, task == null ? T("noTask") : Work.Definition(task).displayName + "\n" + progress + "\n" + Work.BlockReason(task));
                string orderIcon = Work.HasOrder(worker.character.id, StaffOrderKind.Rest) ? "rest" : Work.HasOrder(worker.character.id, StaffOrderKind.Overtime) ? "overtime" : null;
                if (orderIcon != null) StudioUiWidgets.Image("Roster Order", card, .90f, .73f, .985f, .98f, Presentation.Icon(orderIcon), Color.white).preserveAspect = true;
            }
            view.panelText.rectTransform.anchorMin = new Vector2(.05f, .025f); view.panelText.rectTransform.anchorMax = new Vector2(.95f, .23f);
            view.panelText.fontSizeMin = 16; view.panelText.fontSizeMax = 20;
            view.panelText.text = view.CurrentNotice + (string.IsNullOrEmpty(state.production.lastReport) ? "" : "\n" + state.production.lastReport);
            if (string.IsNullOrWhiteSpace(view.panelText.text)) view.panelText.text = T("overview");
        }
        private void RenderWorkerMeters(CharacterDataSO character, TaskState task, ProductionForecast forecast)
        {
            if (!Presentation.useStudioSkin || workerMeters == null) return;
            workerMeters.gameObject.SetActive(true); ClearVisuals(workerMeters);
            var person = Session.State.FindCharacter(character.id);
            StudioUiWidgets.Metric(Presentation, "Affection", workerMeters, 0, 0, .30f, 1, Presentation.Icon("heart"),
                string.Format(T("affectionShort"), person.affection), person.affection / 100f, Presentation.affectionTint);
            StudioUiWidgets.Metric(Presentation, "Stress", workerMeters, .34f, 0, .64f, 1, Presentation.Icon("stress"),
                string.Format(T("stressShort"), person.stress), person.stress / 100f, Presentation.stressTint);
            if (task != null)
            {
                var data = Work.Definition(task); var delta = forecast.allowed ? forecast.workers.First(w => w.characterId == character.id) : null;
                var metric = StudioUiWidgets.Metric(Presentation, "Task Forecast", workerMeters, .68f, 0, 1, 1, data.stream.icon,
                    string.Format(T("workShort"), task.effort, data.effort), (float)task.effort / data.effort, data.stream.tint,
                    delta == null ? -1 : (float)delta.after / data.effort);
                StudioUiWidgets.Hint(metric.gameObject, Presentation, T("sheetHint"));
            }
        }
    }
}
