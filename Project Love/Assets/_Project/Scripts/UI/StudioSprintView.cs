using System;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;

namespace ProjectLove.UI
{
    [DefaultExecutionOrder(10)]
    public sealed partial class StudioSprintView : MonoBehaviour
    {
        private IsometricRoomView view;
        private RectTransform board;
        private TMP_Text status;
        private Button boardButton, phaseButton;
        public bool IsOpen => board != null && board.gameObject.activeSelf;
        public RectTransform Board => board;
        private GameSession Session => view.Session;
        private ProductionSystem Work => Session.Production;
        private RoomPresentationSO Presentation => view.presentation;
        private string T(string key) => Presentation.Get(key);
        private void Start()
        {
            view = GetComponent<IsometricRoomView>();
            if (Session.Production.PlanningEnabled) { gameObject.AddComponent<PlanningBoardView>(); return; }
            boardButton = Button("Sprint Board", transform, .03f, .82f, .17f, .88f, T("board"), ShowBoard);
            phaseButton = Button("Pass Phase", transform, .18f, .82f, .32f, .88f, T("passPhase"), () => Session.TryAdvancePhase());
            var speech = (RectTransform)view.speakerText.transform.parent;
            speech.anchorMin = new Vector2(.04f, .65f); speech.anchorMax = new Vector2(.55f, .93f);
            view.dialogueText.fontSizeMax = 28;
            var choicesViewport = (RectTransform)view.choiceButtons.parent;
            choicesViewport.anchorMin = new Vector2(.04f, .08f); choicesViewport.anchorMax = new Vector2(.55f, .40f);
            status = Text("Worker Status", view.dialogueModal.transform, .04f, .41f, .55f, .64f, 24);
            status.alignment = TextAlignmentOptions.TopLeft;
            // New toolbar stays beneath the existing full-screen dialogue raycast blocker.
            boardButton.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());
            phaseButton.transform.SetSiblingIndex(view.dialogueModal.transform.GetSiblingIndex());
            InitializeSkin();
            Refresh();
        }
        public string SprintSummary()
        {
            if (view == null) view = GetComponent<IsometricRoomView>();
            var sprint = Work.CurrentSprint;
            return string.Format(T("sprintHeader"), sprint.displayName, sprint.startDay, sprint.endDay,
                string.Join(" · ", Work.Config.workers.Select(w => w.stream).Distinct().Select(s => string.Format(T("quota"), s.displayName, Work.Completed(s), Work.Required(s)))));
        }
        public string HelpLabel(CharacterDataSO character)
        {
            if (Work.PlanningEnabled) return string.Format(T("planningShared"), Work.Config.helpCost,
                Session.State.production.planning.shared.Any(s=>s.characterId==character.id && s.cursor/4==Session.State.day-1) ? 0 : Math.Min(100-Session.State.FindCharacter(character.id).affection, Work.Config.planning.sharedAffection));
            if (Session.State.phase == DayPhase.Night && Work.HasOrder(character.id, StaffOrderKind.Overtime))
                return string.Format(T("nightHelpButton"), Work.Config.helpCost,
                    Math.Min(100 - Session.State.FindCharacter(character.id).affection, Work.Config.sharedNightAffection));
            return string.Format(T("workButton"), Work.Config.helpCost);
        }
        public void Refresh()
        {
            if (view == null) view = GetComponent<IsometricRoomView>();
            if (Session.Production.PlanningEnabled) { GetComponent<PlanningBoardView>()?.Refresh(); return; }
            if (status == null) return;
            boardButton.interactable = !Session.IsBusy;
            phaseButton.interactable = !Session.IsBusy && !Session.State.dayEnded;
            RefreshSkin();
            var character = Session.Interactions.IsOpen ? Session.Interactions.Character : Session.Dialogue.ActiveEvent?.character;
            var worker = Work.Config.workers.FirstOrDefault(w => w.character == character);
            status.gameObject.SetActive(character != null && worker != null);
            if (character == null || worker == null) return;
            var runtime = Session.State.FindCharacter(character.id);
            var task = Work.Active(character);
            var stream = worker.stream;
            string taskInfo = task == null ? T("noTask") : string.Format(T("taskStatus"), Work.Definition(task).displayName, task.effort, Work.Definition(task).effort);
            status.text = string.Format(T("characterStatus"), runtime.affection, runtime.stress, taskInfo,
                string.Format(T("quota"), stream.displayName, Work.Completed(stream), Work.Required(stream)));
            var forecast = Work.Preview(ProductionAction.Help, character.id);
            if (forecast.allowed)
            {
                var delta = forecast.workers.First(w => w.characterId == character.id);
                status.text += "\n" + string.Format(T("helpForecast"), delta.help, delta.team, delta.after, Work.Definition(task).effort,
                    forecast.cost, delta.stressBefore, delta.stressAfter);
            }
            else if (!Session.Dialogue.IsActive)
            {
                if (Work.HasOrder(character.id, StaffOrderKind.Overtime)) status.text += "\n" + T("overtimeOrdered");
                else if (Work.HasOrder(character.id, StaffOrderKind.Rest)) status.text += "\n" + T("restOrdered");
                else if (Session.State.phase == DayPhase.Night)
                {
                    var order = Work.PreviewOrder(character.id, StaffOrderKind.Overtime);
                    status.text += "\n" + (order.allowed ? string.Format(T("otForecast"), order.effort, order.stressBefore, order.stressAfter, order.moneyCost) : order.reason);
                }
                else status.text += "\n" + forecast.reason;
            }
            RenderWorkerMeters(character, task, forecast);
        }
        public void ShowBoard()
        {
            if (Session.Production.PlanningEnabled) { GetComponent<PlanningBoardView>()?.ShowBoard(); return; }
            if (Session.IsBusy) return;
            focusedTask = null;
            BuildBoard();
        }
        private void BuildBoard()
        {
            Session.BoardOpen = true;
            view.Refresh();
            if (board != null) { board.gameObject.SetActive(false); Destroy(board.gameObject); }
            board = Panel("Sprint Modal", transform, 0, 0, 1, 1, new Color(.025f, .045f, .06f, .99f));
            StudioUiWidgets.Panel(board.GetComponent<Image>(), Presentation);
            RenderGoalBars();
            Text("Sprint Summary", board, .025f, .88f, .80f, .98f, 28).text = SprintSummary() + "\n" + view.hudText.text;
            Text("Board Hint", board, .025f, .825f, .76f, .85f, 20).text = T("boardHint");
            Button("Close Board", board, .84f, .91f, .975f, .975f, T("close"), CloseBoard);
            Button("Review Ready", board, .59f, .765f, .77f, .82f, T("review"), () => FromBoard(() => Work.TryReview()));
            var latest = Session.State.production.reviews.LastOrDefault();
            string review = latest == null ? T("noReview") : string.Format(T("reviewResult"),
                Work.Config.sprints.First(s => s.id == latest.sprintId).displayName,
                latest.quotas.All(q => q.completed >= q.required) ? T("passed") : T("missed"),
                string.Join(" · ", latest.quotas.Select(q => string.Format(T("quota"), Session.Config.workStreams.First(s => s.id == q.streamId).displayName, q.completed, q.required))), latest.carryoverCount);
            Text("Last Review", board, .025f, .735f, .57f, .825f, 20).text = review;
            string[] keys = { "backlog", "inProgress", "reviewColumn", "done" };
            for (int col = 0; col < 4; col++)
            {
                int index = col;
                float x = .025f + col * .188f;
                var panel = Panel("Column " + col, board, x, .04f, x + .178f, .72f, new Color(.07f, .11f, .14f));
                Text("Column Title", panel, .04f, .91f, .96f, .99f, 24).text = T(keys[col]);
                var list = ScrollList("Cards", panel, .035f, .02f, .965f, .90f);
                foreach (var task in Session.State.production.tasks.OrderBy(t => t.priority))
                {
                    var column = Work.Column(task);
                    bool belongs = index == 0 ? column == TaskColumn.Backlog : index == 1 ? column == TaskColumn.InProgress || column == TaskColumn.Blocked :
                        index == 2 ? column == TaskColumn.Review : column == TaskColumn.Done;
                    // Future tasks remain data-driven but do not flood the current board.
                    if (!belongs || !Work.IsReleased(task)) continue;
                    RenderTaskCard(list, task, column);
                }
            }
            if (focusedTask == null) RenderProducer(); else RenderTaskDetails();
            FocusCard();
        }
        public void CloseBoard()
        {
            if (Session.Production.PlanningEnabled) { GetComponent<PlanningBoardView>()?.CloseBoard(); return; }
            Session.BoardOpen = false;
            if (board != null) board.gameObject.SetActive(false);
            view.Refresh();
        }
        private void FromBoard(Func<bool> command)
        {
            CloseBoard(); command(); ShowBoard();
        }
        private void RenderProducer()
        {
            var panel = Panel("Producer Panel", board, .79f, .04f, .975f, .88f, new Color(.06f, .10f, .12f));
            StudioUiWidgets.Panel(panel.GetComponent<Image>(), Presentation);
            Text("Producer Title", panel, .04f, .94f, .96f, .995f, 26).text = T("producer");
            var list = ScrollList("Producer Actions", panel, .04f, .02f, .96f, .93f);
            var c = Work.Config;
            if (c.useProducerStages)
            {
                var stage = Work.ProducerStage;
                AddText(list, stage == null ? T("producerFinished") : string.Format(T("producerStage"), stage.displayName, Work.ProducerCompleted, stage.producerGoal), 80);
                var progress = Rect("Producer Progress", list, 0, 0, 1, 1); progress.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;
                Bar("Producer Fill", progress, 0, 0, 1, 1, stage == null ? 1 : (float)Work.ProducerCompleted / stage.producerGoal, Presentation.producerTint);
                AddAction(list, "Producer Work", string.Format(T("producerWork"), c.producerCost, c.producerMoney, c.producerEffort, c.producerTeamBonus), ProductionAction.Producer, null);
            }
            AddText(list, string.Format(T("coordinateHint"), c.coordinateBonus, c.coordinateTicks, Session.State.production.coordinationTicks), 96);
            AddAction(list, "Coordinate", string.Format(T("coordinate"), c.coordinateCost), ProductionAction.Coordinate, null);
            foreach (var task in Session.State.production.tasks.Where(t => Work.IsReleased(t) && Work.Definition(t).decisionBlocked && !t.decisionCleared))
                AddAction(list, "Clarify " + task.taskId, string.Format(T("clarify"), Work.Definition(task).displayName, c.clarifyCost), ProductionAction.Clarify, task.taskId);
            foreach (var worker in c.workers)
            {
                var runtime = Session.State.FindCharacter(worker.character.id);
                AddText(list, string.Format(T("teamStatus"), worker.character.displayName, runtime.stress, Work.Rate(worker.character)), 56);
            }
            foreach (var data in c.events.Where(e => !e.automatic))
                AddListButton(list, "Event " + data.id, string.Format(T("eventButton"), data.displayName), () => FromBoard(() => Work.TryActivate(data)),
                    !Session.State.dayEnded && !Session.State.production.events.Any(r => r.eventId == data.id) && Session.State.day >= data.earliestDay && Session.State.day <= data.latestDay);
            if (!string.IsNullOrEmpty(Session.State.production.lastReport)) AddText(list, Session.State.production.lastReport, 220);
        }
        private void AddAction(Transform parent, string name, string label, ProductionAction action, string target)
        {
            var forecast = Work.Preview(action, target);
            AddListButton(parent, name, label, () => FromBoard(() => Work.TryAct(action, target)), forecast.allowed);
            if (forecast.allowed && action == ProductionAction.Producer)
                AddText(parent, string.Join(" · ", forecast.workers.Select(w => Session.Config.characters.First(c => c.id == w.characterId).roleTitle + " +" + w.producer)), 65);
            if (!forecast.allowed) AddText(parent, forecast.reason, 58);
        }
        private void AddText(Transform parent, string text, int height)
        { var t = Text("Info", parent, 0, 0, 1, 1, 20); t.gameObject.AddComponent<LayoutElement>().preferredHeight = height; t.text = text; }
        private void AddListButton(Transform parent, string name, string label, Action command, bool enabled)
        { var b = Button(name, parent, 0, 0, 1, 1, label, command, enabled); b.gameObject.AddComponent<LayoutElement>().preferredHeight = 140; }
        private RectTransform ScrollList(string name, Transform parent, float x0, float y0, float x1, float y1)
        {
            var viewport = Panel(name, parent, x0, y0, x1, y1, new Color(0, 0, 0, .03f));
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect("Content", viewport, 0, 1, 1, 1); content.pivot = new Vector2(.5f, 1);
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>(); layout.spacing = 10; layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped; return content;
        }
        private static RectTransform Rect(string name, Transform parent, float x0, float y0, float x1, float y1)
        { var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false); r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = r.offsetMax = Vector2.zero; return r; }
        private static RectTransform Panel(string name, Transform parent, float x0, float y0, float x1, float y1, Color color)
        { var r = Rect(name, parent, x0, y0, x1, y1); r.gameObject.AddComponent<Image>().color = color; return r; }
        private void Bar(string name, Transform parent, float x0, float y0, float x1, float y1, float fraction, Color tint)
        {
            StudioUiWidgets.Bar(Presentation, name, parent, x0, y0, x1, y1, fraction, tint);
        }
        private TMP_Text Text(string name, Transform parent, float x0, float y0, float x1, float y1, float size)
        {
            var r = Rect(name, parent, x0, y0, x1, y1); var t = r.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = Presentation.font; t.fontSize = size; t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = size;
            t.color = new Color(.93f, .96f, .98f); t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.Normal;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.fontFeatures = new System.Collections.Generic.List<OTL_FeatureTag> {
                OTL_FeatureTag.kern, OTL_FeatureTag.mark, OTL_FeatureTag.mkmk };
            return t;
        }
        private Button Button(string name, Transform parent, float x0, float y0, float x1, float y1, string label, Action command, bool enabled = true)
        {
            var r = Panel(name, parent, x0, y0, x1, y1, new Color(.14f, .28f, .32f)); var b = r.gameObject.AddComponent<Button>(); b.interactable = enabled;
            var colors = b.colors; colors.disabledColor = new Color(.22f, .24f, .25f); b.colors = colors;
            var t = Text("Label", r, .025f, .035f, .975f, .965f, 23); t.alignment = TextAlignmentOptions.Center; t.text = label;
            b.onClick.AddListener(() => command());
            string icon = name.StartsWith("Prioritize ") ? "priority" : name.StartsWith("Inspect ") || name.StartsWith("Dependency ") ? "dependency" :
                name == "Producer Work" ? "producer" : name == "Review Ready" ? "review" : name == "Close Board" ? "close" :
                name == "Coordinate" ? "producer" : name.StartsWith("Clarify ") ? "unlock" : name.StartsWith("Event ") ? "bug" : "info";
            StudioUiWidgets.DecorateButton(b, Presentation, Presentation.Icon(icon), label);
            return b;
        }
    }
}
