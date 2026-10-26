using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class CharacterInteractionTests
    {
        private readonly List<ScriptableObject> assets = new List<ScriptableObject>();
        private GameSession session;
        private GameConfigSO config;
        private CharacterDataSO person;
        private RoomHotspot target;
        private ItemDataSO item;
        private DialogueNodeSO basic, special;
        private WorkStreamSO art, code;
        private string directory;
        private T Make<T>() where T : IdentifiedDataSO
        {
            var data = ScriptableObject.CreateInstance<T>(); data.id = Guid.NewGuid().ToString("N"); assets.Add(data); return data;
        }
        private DialogueNodeSO Node()
        {
            var node = Make<DialogueNodeSO>(); node.lines = new[] { new DialogueLine { lineId = node.id + "_line", text = "ข้อความทดสอบ" } }; return node;
        }
        [SetUp] public void Setup()
        {
            person = Make<CharacterDataSO>(); person.startingAffection = 10;
            basic = Node(); special = Node(); person.neutralGiftResponse = basic;
            person.greetings = new[] { new ConversationVariant { id = "greeting", node = basic } };
            person.conversations = new[] { new ConversationVariant { id = "default", node = basic } };
            item = Make<ItemDataSO>(); item.giftable = true; item.price = 1; item.maxStack = 9;
            art = Make<WorkStreamSO>(); code = Make<WorkStreamSO>();
            var work = Make<ActivityDataSO>(); work.staminaCost = 20; work.elapsedPhases = 1;
            work.successWork = new[] { new WorkOutput { stream = art, amount = 6 } }; person.workActivity = work;
            target = new RoomHotspot { id = "person", kind = RoomInteractionKind.Character, character = person,
                placements = new[] { new RoomPlacement { phase = DayPhase.Morning } } };
            var room = Make<RoomDataSO>(); room.hotspots = new[] { target }; room.shopItems = new[] { item };
            config = Make<GameConfigSO>(); config.totalDays = 30; config.usesStamina = true; config.maxActionPointsPerDay = 4;
            config.characters = new[] { person }; config.items = new[] { item }; config.rooms = new[] { room }; config.startingRoomId = room.id;
            config.workStreams = new[] { art, code }; config.startingMoney = 10;
            session = new GameSession(config);
            directory = Path.GetFullPath(Path.Combine("Logs", "SocialSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown()
        {
            foreach (var data in assets) UnityEngine.Object.DestroyImmediate(data); assets.Clear();
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        [Test] public void OpeningGreetingIsFreeAndBlocksWorldActionsAndSaves()
        {
            string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Interactions.TryOpen(target), Is.True);
            Assert.That(session.Interactions.Greeting, Is.SameAs(basic));
            Assert.That(session.TryEndDay(), Is.False); Assert.That(session.TryBuy(item), Is.False);
            Assert.That(session.TryCompleteActivity(person.workActivity, true), Is.False);
            Assert.That(new SaveStore(directory).Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Busy));
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            Assert.That(session.Interactions.TryClose(), Is.True);
        }
        [Test] public void FirstDailyTalkChargesOnceAndRepeatsFreeAtZeroEnergyWithoutTime()
        {
            session.Interactions.TryOpen(target); Assert.That(session.Interactions.TryTalk(), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(97)); session.Dialogue.TryAdvance();
            Assert.That(session.State.characters[0].affection, Is.EqualTo(12)); Assert.That(session.State.characters[0].lastTalkDay, Is.EqualTo(1));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning)); session.State.stamina = 0;
            for (int i = 0; i < 3; i++) { Assert.That(session.Interactions.TryTalk(), Is.True); session.Dialogue.TryAdvance(); }
            Assert.That(session.State.stamina, Is.Zero); Assert.That(session.State.characters[0].affection, Is.EqualTo(12));
            session.Interactions.TryClose(); session.TryEndDay(); session.TryStartNextDay(); session.Interactions.TryOpen(target);
            Assert.That(session.Interactions.RewardAvailable, Is.True); Assert.That(session.Interactions.TalkCost, Is.EqualTo(3));
        }
        [Test] public void DeferringTalkRestoresCostHistoryAndRewardGate()
        {
            session.Interactions.TryOpen(target); string before = JsonUtility.ToJson(session.State);
            session.Interactions.TryTalk(); session.Dialogue.TryDefer();
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(session.Interactions.RewardAvailable, Is.True);
        }
        [Test] public void DifferentCharactersHaveIndependentDailyRewardGates()
        {
            var other = Make<CharacterDataSO>(); other.conversations = person.conversations;
            var otherTarget = new RoomHotspot { id = "other", kind = RoomInteractionKind.Character, character = other, placements = target.placements };
            config.characters = new[] { person, other }; config.rooms[0].hotspots = new[] { target, otherTarget };
            session = new GameSession(config);
            session.Interactions.TryOpen(target); session.Interactions.TryTalk(); session.Dialogue.TryAdvance(); session.Interactions.TryClose();
            session.Interactions.TryOpen(otherTarget); Assert.That(session.Interactions.RewardAvailable, Is.True);
            session.Interactions.TryTalk(); session.Dialogue.TryAdvance();
            Assert.That(session.State.FindCharacter(person.id).affection, Is.EqualTo(12));
            Assert.That(session.State.FindCharacter(other.id).affection, Is.EqualTo(2)); Assert.That(session.State.stamina, Is.EqualTo(94));
        }
        [Test] public void ZeroEnergyFirstTalkIsRejectedButBackAndRestRemainAvailable()
        {
            session.State.stamina = 0; session.Interactions.TryOpen(target);
            Assert.That(session.Interactions.TryTalk(), Is.False); Assert.That(session.Interactions.TryWork(), Is.False);
            Assert.That(session.Interactions.TryClose(), Is.True);
            var rest = Make<ActivityDataSO>(); rest.staminaRecovery = 30; rest.elapsedPhases = 1;
            Assert.That(session.TryCompleteActivity(rest, true), Is.True); Assert.That(session.State.stamina, Is.EqualTo(30));
        }
        [TestCase(GiftReaction.Liked, 5)] [TestCase(GiftReaction.Neutral, 1)] [TestCase(GiftReaction.Disliked, -2)]
        public void GiftReactionsConsumeOneItemAndCommitOnlyTheirOwnAffection(GiftReaction reaction, int change)
        {
            if (reaction != GiftReaction.Neutral) person.giftPreferences = new[] { new GiftPreference { item = item, reaction = reaction, affectionChange = change, response = special } };
            session.TryBuy(item); session.Interactions.TryOpen(target); session.State.stamina = 0;
            Assert.That(session.Interactions.TryGift(item), Is.True);
            Assert.That(session.Dialogue.Node, Is.SameAs(reaction == GiftReaction.Neutral ? basic : special));
            Assert.That(session.ItemCount(item), Is.EqualTo(1)); session.Dialogue.TryAdvance();
            Assert.That(session.ItemCount(item), Is.Zero); Assert.That(session.State.characters[0].affection, Is.EqualTo(10 + change));
            Assert.That(session.State.stamina, Is.Zero); Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning));
            Assert.That(session.Interactions.RewardAvailable, Is.True); Assert.That(session.Interactions.TryGift(item), Is.False);
        }
        [Test] public void BackDuringGiftReturnsItemAndStateWithoutReward()
        {
            session.TryBuy(item); session.Interactions.TryOpen(target); string before = JsonUtility.ToJson(session.State);
            session.Interactions.TryGift(item); session.Dialogue.TryDefer();
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(session.Interactions.IsOpen, Is.True);
        }
        [Test] public void UnavailableAndForeignTargetsAndNonGiftableItemsAreRejected()
        {
            session.TryBuy(item); target.placements[0].visible = false;
            Assert.That(session.Interactions.TryOpen(target), Is.False); target.placements[0].visible = true;
            Assert.That(session.Interactions.TryOpen(new RoomHotspot { character = person }), Is.False);
            session.Interactions.TryOpen(target); item.giftable = false;
            Assert.That(session.Interactions.TryGift(item), Is.False);
        }
        [Test] public void VariantSelectionRespectsDayPhaseAffectionFlagsAndStoryCompletion()
        {
            var variant = new ConversationVariant { id = "special", node = special, priority = 10, startDay = 2,
                anyPhase = false, phase = DayPhase.Evening, minimumAffection = 30,
                conditions = new[] { new Condition { type = ConditionType.FlagSet, flagId = "ready" },
                    new Condition { type = ConditionType.EventCompleted, eventId = "story" } } };
            person.conversations = person.conversations.Concat(new[] { variant }).ToArray(); session.Interactions.TryOpen(target);
            Assert.That(session.Interactions.Conversation, Is.SameAs(basic));
            session.State.day = 2; session.State.phase = DayPhase.Evening; session.State.characters[0].affection = 30; session.State.flags.Add("ready");
            Assert.That(session.Interactions.Conversation, Is.SameAs(basic)); session.State.completedEvents.Add("story");
            Assert.That(session.Interactions.Conversation, Is.SameAs(special));
            session.State.phase = DayPhase.Night; Assert.That(session.Interactions.Conversation, Is.SameAs(basic));
        }
        [Test] public void GreetingPoolRotatesAcrossDaysAndStaysStableAcrossRefreshAndReopen()
        {
            var third=Node();person.greetings[0].pool=new[]{special,third};session.Interactions.TryOpen(target);
            var selected=new HashSet<DialogueNodeSO>();
            for(int d=1;d<=3;d++)
            {
                session.State.day=d;var node=session.Interactions.Greeting;selected.Add(node);Assert.That(session.Interactions.Greeting,Is.SameAs(node));
                session.Interactions.TryClose();session.Interactions.TryOpen(target);Assert.That(session.Interactions.Greeting,Is.SameAs(node));
            }
            Assert.That(selected,Is.EquivalentTo(new[]{basic,special,third}));
        }
        [Test] public void AffectionBandReplacesPoolAndUnwrittenAlternativesAreSkipped()
        {
            var empty=Make<DialogueNodeSO>();person.greetings[0].maximumAffection=29;person.greetings[0].pool=new[]{empty};
            person.greetings=person.greetings.Concat(new[]{new ConversationVariant{id="close",minimumAffection=30,node=special}}).ToArray();session.Interactions.TryOpen(target);
            session.State.characters[0].affection=29;Assert.That(session.Interactions.Greeting,Is.SameAs(basic));
            session.State.characters[0].affection=30;Assert.That(session.Interactions.Greeting,Is.SameAs(special));
        }
        [Test] public void RepeatableChoiceCannotFarmLegacyEffectsOrChargeHiddenResources()
        {
            basic.choices = new[] { new DialogueChoice { choiceId = "repeat", staminaCost = 999,
                effects = new[] { new StatModifier { stat = StatType.Money, amount = 100 } } } };
            session.Interactions.TryOpen(target);
            for (int i = 0; i < 3; i++)
            {
                session.Interactions.TryTalk(); session.Dialogue.TryAdvance();
                Assert.That(session.Dialogue.TryChoose(basic.choices[0]), Is.True);
            }
            Assert.That(session.State.money, Is.EqualTo(10)); Assert.That(session.State.stamina, Is.EqualTo(97));
            Assert.That(session.State.characters[0].affection, Is.EqualTo(12));
        }
        [Test] public void CharacterWorkTargetsItsOwnStreamAndCommitsResourceAndTimeTogether()
        {
            session.Interactions.TryOpen(target); Assert.That(session.Interactions.TryWork(), Is.True);
            Assert.That(session.Interactions.IsOpen, Is.False); Assert.That(session.WorkCompleted(art), Is.EqualTo(6));
            Assert.That(session.WorkCompleted(code), Is.Zero); Assert.That(session.State.stamina, Is.EqualTo(80));
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime));
        }
        [Test] public void InvalidWorkOutputRejectsWholeTransactionAndKeepsMenu()
        {
            person.workActivity.successWork[0].stream = Make<WorkStreamSO>();
            session.Interactions.TryOpen(target); string before = JsonUtility.ToJson(session.State);
            Assert.That(session.Interactions.TryWork(), Is.False); Assert.That(session.Interactions.IsOpen, Is.True);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }
        [Test] public void SaveV3PreservesDailyTalkAndWorkWhileInvalidExtendedSnapshotsAreRejected()
        {
            session.Interactions.TryOpen(target); session.Interactions.TryTalk(); session.Dialogue.TryAdvance();
            session.Interactions.TryWork(); var store = new SaveStore(directory);
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var restored = new GameSession(config); Assert.That(store.Load(restored, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(restored.State.characters[0].lastTalkDay, Is.EqualTo(1)); Assert.That(restored.WorkCompleted(art), Is.EqualTo(6));
            var invalid = restored.State.Clone(); invalid.characters[0].lastTalkDay = 2; Assert.That(restored.TryRestore(invalid), Is.False);
            invalid = restored.State.Clone(); invalid.workProgress.Add(new WorkProgressEntry { streamId = art.id }); Assert.That(restored.TryRestore(invalid), Is.False);
        }
        [Serializable] private class Envelope { public int version; public bool usesStamina; public string configId, payload, checksum; }
        [Test] public void V2MigrationPreservesAggregateWithoutInventingHistoricWorkAttribution()
        {
            var store = new SaveStore(directory); session.State.mainProgress = 40; store.Save(session, SaveSlot.Manual);
            string path = store.PathFor(session, SaveSlot.Manual); var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path));
            envelope.version = 2; envelope.payload = Regex.Replace(envelope.payload, ",\\\"workProgress\\\":\\[.*?\\]", "");
            using (var sha = SHA256.Create()) envelope.checksum = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(envelope.payload))).Replace("-", "").ToLowerInvariant();
            File.WriteAllText(path, JsonUtility.ToJson(envelope));
            var restored = new GameSession(config); Assert.That(store.Load(restored, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(restored.State.mainProgress, Is.EqualTo(40)); Assert.That(restored.WorkCompleted(art), Is.Zero);
            Assert.That(restored.State.characters[0].lastTalkDay, Is.Zero);
        }
    }
}
