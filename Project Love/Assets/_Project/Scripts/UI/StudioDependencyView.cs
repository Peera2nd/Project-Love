using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectLove.UI
{
    public sealed partial class StudioSprintView
    {
        private string focusedTask;
        public string FocusedTaskId => focusedTask;
        public void InspectTask(string id)
        {
            if (!Session.BoardOpen || Work.Find(id) == null) return;
            focusedTask = id; BuildBoard();
        }
        private string TaskStatus(TaskState task)
        {
            if (task.acceptedDay > 0) return T("dependencyAccepted");
            if (task.effort == Work.Definition(task).effort) return T("dependencyReview");
            if (!Work.IsReleased(task)) return T("dependencyStage");
            if (Work.Definition(task).decisionBlocked && !task.decisionCleared) return T("dependencyProducer");
            return Work.IsReady(task) ? T("dependencyReady") : T("dependencyWaiting");
        }
        private string TaskStatusIcon(TaskState task) => task.acceptedDay > 0 ? "done" : task.effort == Work.Definition(task).effort ? "review" : Work.IsReady(task) ? "work" : "locked";
        private string DependencySummary(WorkTaskSO data)
        {
            var missing = data.prerequisites.Where(p => Work.Find(p.id).acceptedDay == 0).ToArray();
            return missing.Length > 0 ? string.Join("\n", missing.Select(p => string.Format(T("dependencyNeeds"),
                Session.Config.characters.First(c => c.id == Work.Find(p.id).ownerId).displayName, p.displayName))) :
                data.prerequisites.Length > 0 ? T("dependencyDone") : T("dependencyIndependent");
        }
        private void RenderTaskCard(Transform list, TaskState task, TaskColumn column)
        {
            var data = Work.Definition(task); var person = Session.Config.characters.First(c => c.id == task.ownerId);
            var card = Panel("Card " + task.taskId, list, 0, 0, 1, 1, new Color(.12f, .20f, .24f));
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 380;
            StudioUiWidgets.Panel(card.GetComponent<Image>(), Presentation, true);
            if (focusedTask == task.taskId)
            { var outline = card.gameObject.AddComponent<Outline>(); outline.effectColor = data.stream.tint; outline.effectDistance = new Vector2(2, 2); }
            Panel("Discipline Stripe", card, 0, 0, .025f, 1, data.stream.tint).GetComponent<Image>().raycastTarget = false;
            StudioUiWidgets.Image("Discipline Icon", card, .05f, .90f, .14f, .98f, data.stream.icon, Color.white).preserveAspect = true;
            Text("Discipline", card, .16f, .90f, .53f, .98f, 20).text = data.stream.displayName;
            var stateText = Text("Task State", card, .48f, .88f, .96f, .995f, 18); stateText.fontSizeMin = 16; stateText.text = TaskStatus(task);
            var title = Text("Card Text", card, .05f, .69f, .96f, .88f, 22); title.fontSizeMin = 16; title.text = data.displayName;
            var avatar = StudioUiWidgets.Image("Owner Avatar", card, .77f, .56f, .97f, .68f,
                person.taskAvatar != null ? person.taskAvatar : person.GetSprite(CharacterExpression.Normal), Color.white); avatar.preserveAspect = true;
            if (Presentation.useStudioSkin) StudioUiWidgets.Image("Avatar Frame", card, .77f, .56f, .97f, .68f, Presentation.avatarFrameSprite, data.stream.tint).preserveAspect = true;
            Text("Card Owner", card, .05f, .56f, .75f, .68f, 20).text = person.displayName;
            Bar("Task Progress", card, .05f, .49f, .95f, .535f, (float)task.effort / data.effort, data.stream.tint);
            Text("Effort", card, .05f, .41f, .95f, .49f, 19).text = string.Format(T("workShort"), task.effort, data.effort);
            StudioUiWidgets.Image("Dependency Icon", card, .04f, .27f, .10f, .40f, Presentation.Icon(data.prerequisites.Length > 0 ? "dependency" : TaskStatusIcon(task)), Color.white).preserveAspect = true;
            Text("Dependency Summary", card, .12f, .21f, .96f, .40f, 19).text = DependencySummary(data);
            StudioUiWidgets.Hint(card.gameObject, Presentation, data.displayName + "\n" + TaskStatus(task) + "\n" + DependencySummary(data));
            bool editable = column != TaskColumn.Done && column != TaskColumn.Review;
            Button("Inspect " + task.taskId, card, .04f, .035f, editable ? .49f : .96f, .18f, T("dependencyInspect"), () => InspectTask(task.taskId));
            if (editable) Button("Prioritize " + task.taskId, card, .51f, .035f, .96f, .18f, T("priority"),
                () => FromBoard(() => Work.TryPrioritize(task.taskId)), !Session.State.dayEnded);
            var alternatives = Work.Config.workers.Where(w => w.stream == data.stream && w.character.id != task.ownerId).ToArray();
            if (alternatives.Length > 0 && column != TaskColumn.Done)
                Button("Assign " + task.taskId, card, .04f, .59f, .75f, .67f, string.Format(T("assignment"), alternatives[0].character.displayName),
                    () => FromBoard(() => Work.TryAssign(task.taskId, alternatives[0].character)), !Session.State.dayEnded);
        }
        private void RenderGoalBars()
        {
            var streams = Work.Config.workers.Select(w => w.stream).Distinct().ToArray();
            for (int i = 0; i < streams.Length; i++)
            {
                float left = .025f + i * .745f / streams.Length;
                float right = left + .745f / streams.Length - .015f;
                Bar("Sprint Goal " + streams[i].id, board, left, .858f, right, .878f,
                    (float)Work.Completed(streams[i]) / Mathf.Max(1, Work.Required(streams[i])), streams[i].tint);
            }
        }
        private void RenderTaskDetails()
        {
            var task = Work.Find(focusedTask); if (task == null) { focusedTask = null; RenderProducer(); return; }
            var data = Work.Definition(task);
            var panel = Panel("Task Details", board, .79f, .04f, .975f, .88f, new Color(.06f, .10f, .12f));
            StudioUiWidgets.Panel(panel.GetComponent<Image>(), Presentation);
            Text("Details Title", panel, .04f, .94f, .96f, .995f, 24).text = T("dependencyTitle");
            var list = ScrollList("Dependency Details", panel, .04f, .02f, .96f, .93f);
            AddListButton(list, "Close Task Details", T("dependencyFocus"), () => { focusedTask = null; BuildBoard(); }, true);
            AddText(list, data.displayName, 95);
            AddText(list, string.Format(T("dependencyOwner"), Session.Config.characters.First(c => c.id == task.ownerId).displayName, task.effort, data.effort) + "\n" + TaskStatus(task), 85);
            if (task.effort == data.effort && task.acceptedDay == 0)
                AddListButton(list, "Detail Review", T("review"), () => FromBoard(() => Work.TryReview()), !Session.State.dayEnded);
            AddText(list, T("dependencyReviewHint"), 85);
            AddText(list, T("dependencyBefore"), 50);
            if (data.prerequisites.Length == 0) AddText(list, T("dependencyIndependent"), 70);
            foreach (var before in data.prerequisites) DependencyButton(list, "Before", Work.Find(before.id));
            if (data.decisionBlocked && !task.decisionCleared)
                AddAction(list, "Clarify " + task.taskId, string.Format(T("clarify"), data.displayName, Work.Config.clarifyCost), ProductionAction.Clarify, task.taskId);
            AddText(list, T("dependencyAfter"), 50);
            var after = Work.Config.tasks.Where(t => t.prerequisites.Contains(data)).ToArray();
            if (after.Length == 0) AddText(list, T("dependencyNoneAfter"), 65);
            foreach (var successor in after) DependencyButton(list, "After", Work.Find(successor.id));
            var owner = Session.Config.characters.First(c => c.id == task.ownerId);
            var available = Work.ReadyTasks(owner).Where(t => t.taskId != task.taskId).ToArray();
            AddText(list, string.Format(T("dependencyChoose"), available.Length), 50);
            foreach (var ready in available) DependencyButton(list, "Ready", ready);
        }
        private void DependencyButton(Transform parent, string relation, TaskState task)
        {
            var data = Work.Definition(task); string owner = Session.Config.characters.First(c => c.id == task.ownerId).displayName;
            string label = string.Format(T("dependencyJump"), data.displayName) + "\n" + owner + " · " + task.effort + "/" + data.effort + " · " + TaskStatus(task);
            var button = Button("Dependency " + relation + " " + task.taskId, parent, 0, 0, 1, 1, label, () => InspectTask(task.taskId));
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 140;
        }
        private void FocusCard()
        {
            if (focusedTask == null) return;
            var card = board.GetComponentsInChildren<RectTransform>().FirstOrDefault(r => r.name == "Card " + focusedTask);
            if (card == null) return;
            Canvas.ForceUpdateCanvases();
            var scroll = card.GetComponentInParent<ScrollRect>();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, card);
            float y = Mathf.Clamp(-scroll.viewport.rect.height * .5f - bounds.center.y, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height));
            scroll.content.anchoredPosition = new Vector2(0, y);
        }
    }
}
