using System.Linq;
using ProjectLove.Data;
using ProjectLove.VN;

namespace ProjectLove.Core
{
    // Access, content selection and reward frequency are independent policies.
    public sealed class CharacterInteractions
    {
        private readonly GameSession session;
        public RoomHotspot Target { get; private set; }
        public bool IsOpen => Target != null;
        public CharacterDataSO Character => Target?.character;
        public CharacterInteractions(GameSession session) { this.session = session; }

        public bool TryOpen(RoomHotspot target)
        {
            if (session.IsBusy || session.State.dayEnded || target == null || target.character == null ||
                target.kind != RoomInteractionKind.Character || session.CurrentRoom == null ||
                !session.CurrentRoom.hotspots.Contains(target) || !session.Config.characters.Contains(target.character) ||
                !target.placements.Any(p => p != null && p.visible && p.phase == session.State.phase)) return false;
            Target = target; session.NotifyChanged(); return true;
        }
        public bool TryClose()
        {
            if (!IsOpen || session.Dialogue.IsActive) return false;
            Target = null; session.NotifyChanged(); return true;
        }
        public bool RewardAvailable => IsOpen && session.State.FindCharacter(Character.id).lastTalkDay != session.State.day;
        public int TalkCost => RewardAvailable && session.Config.usesStamina ? Character.dailyTalkStaminaCost : 0;
        public DialogueNodeSO Select(ConversationVariant[] variants)
        {
            return IsOpen ? SelectFor(Character,variants) : null;
        }
        public bool MatchesVariant(CharacterDataSO character,ConversationVariant v)
        {
            var state=character==null?null:session.State.FindCharacter(character.id);if(state==null||v==null)return false;
            int affection=state.affection,stress=state.stress;
            return session.State.day >= v.startDay && session.State.day <= v.endDay &&
                (v.anyPhase || v.phase == session.State.phase) && affection >= v.minimumAffection && affection <= v.maximumAffection &&
                (session.Romance.Enabled || stress >= v.minimumStress && stress <= v.maximumStress) &&
                (!v.overtimeOnly || session.Production.HasOrder(character.id, StaffOrderKind.Overtime)) && session.Stats.Meets(v.conditions);
        }
        public DialogueNodeSO SelectFor(CharacterDataSO character,ConversationVariant[] variants)
        {
            var variant=variants?.Where(v=>MatchesVariant(character,v)&&v.Nodes.Any(DialogueRunner.IsPlayable))
                .OrderByDescending(v=>v.priority).ThenBy(v=>v.id,System.StringComparer.Ordinal).FirstOrDefault();
            if(variant==null)return null;var nodes=variant.Nodes.Where(DialogueRunner.IsPlayable).ToArray();
            // Stable context rotation: refresh/save/reopen cannot reroll content or touch game RNG.
            uint salt=2166136261;unchecked{foreach(char c in character.id+":"+variant.id)salt=(salt^c)*16777619;}
            return nodes[(int)((salt+(uint)(session.State.day-1)+(uint)session.State.phase)%(uint)nodes.Length)];
        }
        public DialogueNodeSO Greeting => Select(Character?.greetings);
        public DialogueNodeSO Conversation => session.Romance.Enabled && IsOpen && session.State.flags.Contains(session.Romance.Cast(Character.id).topicFlag) ? session.Romance.Cast(Character.id).focusTalk : Select(Character?.conversations) ?? Target?.conversation?.entryNode;
        public bool CanTalk => IsOpen && !session.State.dayEnded && !session.Dialogue.IsActive &&
            session.CanPay(0, TalkCost) && DialogueRunner.IsPlayable(Conversation);
        public bool TryTalk()
        {
            if (!CanTalk) return false;
            var character = Character;
            bool reward = RewardAvailable;
            return session.Dialogue.TryBeginInteraction(Conversation, TalkCost, () =>
            {
                if (!reward) { session.Romance.RecordTalk(character.id, 0); return; }
                var runtime = session.State.FindCharacter(character.id);
                int gain = System.Math.Min(100 - runtime.affection, character.dailyTalkAffection);
                runtime.affection += gain;
                session.Romance.RecordTalk(character.id, gain);
                runtime.lastTalkDay = session.State.day;
            });
        }
        public GiftPreference Preference(ItemDataSO item) => Character?.giftPreferences?.FirstOrDefault(p => p != null && p.item == item);
        public bool CanGift(ItemDataSO item) => IsOpen && !session.State.dayEnded && !session.Dialogue.IsActive && item != null &&
            item.giftable && session.Config.items.Contains(item) && session.ItemCount(item) > 0 &&
            DialogueRunner.IsPlayable(Preference(item)?.response ?? Character.neutralGiftResponse);
        public bool TryGift(ItemDataSO item)
        {
            if (!CanGift(item)) return false;
            var preference = Preference(item);
            var character = Character;
            int affection = preference?.affectionChange ?? character.neutralGiftAffection;
            var response = preference?.response ?? character.neutralGiftResponse;
            // Commit only after the reaction finishes. Back/defer restores the entire snapshot.
            return session.Dialogue.TryBeginInteraction(response, 0, () =>
            {
                var entry = session.State.inventory.First(e => e.itemId == item.id);
                if (--entry.quantity == 0) session.State.inventory.Remove(entry);
                var runtime = session.State.FindCharacter(character.id);
                int before = runtime.affection;
                runtime.affection = System.Math.Max(0, System.Math.Min(100, runtime.affection + affection));
                session.Romance.RecordGift(character.id, runtime.affection - before);
            });
        }
        internal void CloseForWork() { Target = null; }
        public bool CanWork => IsOpen && (session.Romance.Enabled ? session.Romance.Preview(RomanceActivity.WorkTogether, Character.id).allowed : session.Production.Enabled ?
            session.Production.Preview(ProjectLove.Simulation.ProductionAction.Help, Character.id).allowed :
            session.CanCompleteActivity(Character.workActivity, true, true));
        public bool TryWork()
        {
            if (!CanWork) return false;
            if (session.Romance.Enabled) return session.Romance.TryStart(RomanceActivity.WorkTogether, Character.id);
            if (session.Production.Enabled) return session.Production.TryAct(ProjectLove.Simulation.ProductionAction.Help, Character.id);
            var activity = Character.workActivity;
            // Close before committing the transaction so phase/day changes cannot leave a stale target.
            Target = null;
            return session.TryCompleteActivity(activity, true);
        }
    }
}
