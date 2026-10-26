using ProjectLove.Core;
using ProjectLove.Data;

namespace ProjectLove.VN
{
    public sealed class DialogueRunner
    {
        private readonly GameSession session;
        private EventDataSO activeEvent;
        private RuntimeGameState eventStart;
        private System.Action interactionComplete;
        public DialogueNodeSO Node { get; private set; }
        public int LineIndex { get; private set; }
        public bool IsActive => Node != null;
        public EventDataSO ActiveEvent => activeEvent;
        public bool ChoicesReady => IsActive && LineIndex >= Node.lines.Length && Node.choices.Length > 0;
        public bool HasAvailableChoice
        {
            get
            {
                if (!ChoicesReady) return false;
                foreach (var choice in Node.choices) if (CanChoose(choice)) return true;
                return false;
            }
        }
        public DialogueLine CurrentLine => IsActive && LineIndex < Node.lines.Length ? Node.lines[LineIndex] : null;

        public DialogueRunner(GameSession session) { this.session = session; }

        public static bool IsPlayable(DialogueNodeSO node)
        {
            if (node == null || node.lines == null || node.lines.Length == 0 || node.choices == null) return false;
            foreach (var line in node.lines)
                if (line == null || string.IsNullOrWhiteSpace(line.lineId) || string.IsNullOrWhiteSpace(line.SourceText)) return false;
            return true;
        }

        public bool TryBegin(EventDataSO data)
        {
            if (!session.CanStartEvent(data)) return false;
            eventStart = session.State.Clone();
            session.TryPay(0, data.staminaCost);
            activeEvent = data;
            Node = data.entryNode;
            LineIndex = 0;
            session.NotifyChanged();
            return true;
        }

        internal bool TryBeginInteraction(DialogueNodeSO node, int staminaCost, System.Action onComplete)
        {
            if (IsActive || !session.Interactions.IsOpen || !IsPlayable(node) || onComplete == null || !session.CanPay(0, staminaCost)) return false;
            eventStart = session.State.Clone();
            session.TryPay(0, staminaCost);
            interactionComplete = onComplete;
            Node = node; LineIndex = 0;
            session.NotifyChanged(); return true;
        }

        internal bool TryBeginScene(DialogueNodeSO node, System.Action onComplete)
        {
            if (session.BoardOpen || IsActive || session.State.dayEnded || (!session.Interactions.IsOpen && session.IsBusy) || !IsPlayable(node) || onComplete == null) return false;
            eventStart = session.State.Clone(); interactionComplete = onComplete; Node = node; LineIndex = 0;
            session.NotifyChanged(); return true;
        }

        public bool TryAdvance()
        {
            if (!IsActive || ChoicesReady) return false;
            var line = CurrentLine;
            if (line != null && !session.State.readLines.Contains(line.lineId)) session.State.readLines.Add(line.lineId);
            LineIndex++;
            if (LineIndex >= Node.lines.Length && Node.choices.Length == 0)
            {
                if (Node.nextNode != null)
                {
                    if (!IsPlayable(Node.nextNode)) { LineIndex--; return false; }
                    Node = Node.nextNode;
                    LineIndex = 0;
                }
                else Finish();
            }
            session.NotifyChanged();
            return true;
        }

        public bool CanChoose(DialogueChoice choice) => ChoicesReady && choice != null &&
            !string.IsNullOrWhiteSpace(choice.choiceId) && System.Array.IndexOf(Node.choices, choice) >= 0 &&
            session.Stats.Meets(choice.conditions) && (interactionComplete != null ||
                (session.CanPay(choice.actionPointCost, choice.staminaCost) && session.CanApplyTransaction(choice.effects, choice.flags) &&
                 session.Production.CanApplyEventWorkloads(choice.activateWorkloads) &&
                 (choice.workAction == DialogueWorkAction.None || session.Production.PreviewDialogueWork(choice).allowed))) &&
            (choice.nextNode == null || IsPlayable(choice.nextNode));

        public bool TryChoose(DialogueChoice choice)
        {
            if (!CanChoose(choice)) return false;
            // Ambient interaction rewards are committed once by their policy, never by repeatable choices.
            if (interactionComplete == null)
            {
                if (choice.workAction != DialogueWorkAction.None)
                { if (!session.Production.TryDialogueWork(choice)) return false; }
                else
                {
                    if (!session.TryCommit(choice.actionPointCost, choice.effects, choice.flags, choice.staminaCost)) return false;
                    session.Production.ApplyEventWorkloads(choice.activateWorkloads);
                }
            }
            if (!session.State.chosenOptions.Contains(choice.choiceId)) session.State.chosenOptions.Add(choice.choiceId);
            if (choice.nextNode == null) Finish();
            else { Node = choice.nextNode; LineIndex = 0; }
            session.NotifyChanged();
            return true;
        }

        // Deferral rolls back the entire event, including earlier choices, so retrying cannot farm rewards.
        public bool TryDefer()
        {
            if (!IsActive) return false;
            var deferred = activeEvent;
            session.State.CopyFrom(eventStart);
            session.Events.Defer(deferred);
            activeEvent = null;
            interactionComplete = null;
            eventStart = null;
            Node = null;
            LineIndex = 0;
            session.NotifyChanged();
            return true;
        }

        private void Finish()
        {
            if (activeEvent != null) session.CompleteEvent(activeEvent, eventStart);
            else interactionComplete?.Invoke();
            activeEvent = null;
            interactionComplete = null;
            eventStart = null;
            Node = null;
            LineIndex = 0;
        }
    }
}
