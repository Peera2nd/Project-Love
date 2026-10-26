using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class RoomSystemsTests
    {
        private readonly List<ScriptableObject> objects = new List<ScriptableObject>();
        private GameConfigSO config;
        private CharacterDataSO character;
        private ItemDataSO item;
        private RoomDataSO room, shop;
        private GameSession session;
        private string directory;
        private T Make<T>() where T : IdentifiedDataSO
        {
            var data = ScriptableObject.CreateInstance<T>(); data.id = Guid.NewGuid().ToString("N"); objects.Add(data); return data;
        }
        [SetUp] public void SetUp()
        {
            character = Make<CharacterDataSO>();
            item = Make<ItemDataSO>(); item.price = 5; item.maxStack = 2; item.staminaRecovery = 15;
            room = Make<RoomDataSO>(); shop = Make<RoomDataSO>(); shop.shopItems = new[] { item };
            config = Make<GameConfigSO>(); config.totalDays = 30; config.maxActionPointsPerDay = 4;
            config.usesStamina = true; config.startingMoney = 10; config.startingStamina = 100; config.maxStamina = 100; config.overnightRecovery = 40;
            config.characters = new[] { character }; config.items = new[] { item }; config.rooms = new[] { room, shop }; config.startingRoomId = room.id;
            session = new GameSession(config);
            directory = Path.GetFullPath(Path.Combine("Logs", "RoomSaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void TearDown()
        {
            foreach (var data in objects) UnityEngine.Object.DestroyImmediate(data); objects.Clear();
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
        private ActivityDataSO Work()
        {
            var data = Make<ActivityDataSO>(); data.staminaCost = 20; data.elapsedPhases = 1;
            data.successEffects = new[] { new StatModifier { stat = StatType.MainProgress, amount = 6 } }; return data;
        }
        private EventDataSO Conversation(bool daily = true)
        {
            var node = Make<DialogueNodeSO>();
            node.lines = new[] { new DialogueLine { lineId = "line", text = "test", speaker = character } };
            var data = Make<EventDataSO>(); data.startDay = 1; data.endDay = 30; data.anyPhase = true;
            data.repeatDaily = daily; data.elapsedPhases = 1; data.entryNode = node; return data;
        }
        [Test] public void WorkCommitsEnergyOutputAndExactlyOnePhase()
        {
            Assert.That(session.TryCompleteActivity(Work(), true), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(80)); Assert.That(session.State.actionPoints, Is.Zero);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(session.State.mainProgress, Is.EqualTo(6));
            Assert.That(config.startingStamina, Is.EqualTo(100));
        }
        [Test] public void RejectedCostsTargetsAndDurationLeaveAllStateUnchanged()
        {
            var work = Work(); session.State.stamina = 10; string before = JsonUtility.ToJson(session.State);
            Assert.That(session.TryCompleteActivity(work, true), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            session.State.stamina = 100; session.State.phase = DayPhase.Night; work.elapsedPhases = 2;
            before = JsonUtility.ToJson(session.State);
            Assert.That(session.TryCompleteActivity(work, true), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
            work.elapsedPhases = 1; work.successEffects = new[] { new StatModifier { stat = StatType.Affection, character = Make<CharacterDataSO>(), amount = 10 } };
            Assert.That(session.TryCompleteActivity(work, true), Is.False);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }
        [Test] public void ZeroEnergyRestRecoversAndAdvancesOnePhase()
        {
            session.State.stamina = 0; var rest = Make<ActivityDataSO>(); rest.staminaRecovery = 30; rest.elapsedPhases = 1;
            Assert.That(session.TryCompleteActivity(rest, true), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(30)); Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime));
        }
        [Test] public void FinalPhaseEndsDayWithoutSkippingAndRecoveryIsBounded()
        {
            session.State.phase = DayPhase.Night; session.State.stamina = 25;
            Assert.That(session.TryCompleteActivity(Work(), true), Is.True);
            Assert.That(session.State.dayEnded, Is.True); Assert.That(session.State.day, Is.EqualTo(1));
            Assert.That(session.TryCompleteActivity(Work(), true), Is.False);
            Assert.That(session.TryStartNextDay(), Is.True); Assert.That(session.State.stamina, Is.EqualTo(45));
            Assert.That(session.State.roomId, Is.EqualTo(room.id));
            session.State.day = 30; session.State.phase = DayPhase.Night;
            Assert.That(session.TryCompleteActivity(Work(), true), Is.True);
            Assert.That(session.Days.CampaignEnded, Is.True); Assert.That(session.TryStartNextDay(), Is.False);
        }
        [Test] public void DailyConversationRefreshesTomorrowWhileStoryCompletionRemainsOnceOnly()
        {
            var daily = Conversation(); session.Dialogue.TryBegin(daily); session.Dialogue.TryAdvance();
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime)); Assert.That(session.CanStartEvent(daily), Is.False);
            Assert.That(session.State.completedEvents, Is.Empty);
            session.TryEndDay(); session.TryStartNextDay(); Assert.That(session.CanStartEvent(daily), Is.True);
            var story = Conversation(false); session.Dialogue.TryBegin(story); session.Dialogue.TryAdvance();
            session.TryEndDay(); session.TryStartNextDay(); Assert.That(session.CanStartEvent(story), Is.False);
        }
        [Test] public void DeferringRefundsWholeConversationWithoutTimeOrDailyReward()
        {
            var data = Conversation(); data.staminaCost = 10;
            data.entryNode.choices = new[] { new DialogueChoice { choiceId = "choice", effects = new[] { new StatModifier { stat = StatType.Money, amount = 100 } },
                nextNode = data.entryNode } };
            string before = JsonUtility.ToJson(session.State);
            session.Dialogue.TryBegin(data); session.Dialogue.TryAdvance(); session.Dialogue.TryChoose(data.entryNode.choices[0]);
            Assert.That(session.State.money, Is.EqualTo(110)); Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before)); Assert.That(session.CanStartEvent(data), Is.True);
        }
        [Test] public void PickupIsOnceOnlyAndPurchaseRejectsWrongSceneNoMoneyAndFullStack()
        {
            var pickup = new RoomHotspot { id = "pickup", kind = RoomInteractionKind.Pickup, item = item };
            room.hotspots = new[] { pickup };
            Assert.That(session.TryBuy(item), Is.False);
            Assert.That(session.TryCollect(pickup), Is.True); Assert.That(session.TryCollect(pickup), Is.False);
            session.TryTravel(shop); Assert.That(session.TryBuy(item), Is.True); Assert.That(session.State.money, Is.EqualTo(5));
            Assert.That(session.TryBuy(item), Is.False); Assert.That(session.State.money, Is.EqualTo(5));
            session.State.stamina = 50; session.TryUse(item); session.State.money = 0;
            Assert.That(session.TryBuy(item), Is.False); Assert.That(session.ItemCount(item), Is.EqualTo(1));
        }
        [Test] public void ItemUseClampsRecoveryAndDoesNotConsumeAtFullEnergy()
        {
            session.TryTravel(shop); session.TryBuy(item);
            Assert.That(session.TryUse(item), Is.False); Assert.That(session.ItemCount(item), Is.EqualTo(1));
            session.State.stamina = 95; Assert.That(session.TryUse(item), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(100)); Assert.That(session.ItemCount(item), Is.Zero);
        }
        [Test] public void ModalDialogueRejectsTravelWorkPickupShopAndInventoryUse()
        {
            session.TryTravel(shop); session.TryBuy(item); session.State.stamina = 50;
            session.Dialogue.TryBegin(Conversation());
            string before = JsonUtility.ToJson(session.State);
            Assert.That(session.TryTravel(room), Is.False); Assert.That(session.TryBuy(item), Is.False);
            Assert.That(session.TryUse(item), Is.False); Assert.That(session.TryCompleteActivity(Work(), true), Is.False);
            Assert.That(session.TryEndDay(), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }
        [Test] public void SaveRestoresRoomInventoryDailyGateAndPickupAcrossReload()
        {
            var daily = Conversation(); session.Dialogue.TryBegin(daily); session.Dialogue.TryAdvance();
            var pickup = new RoomHotspot { id = "pickup", kind = RoomInteractionKind.Pickup, item = item }; room.hotspots = new[] { pickup };
            session.TryCollect(pickup); session.TryTravel(shop); var store = new SaveStore(directory);
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var fresh = new GameSession(config); Assert.That(store.Load(fresh, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            Assert.That(fresh.State.roomId, Is.EqualTo(shop.id)); Assert.That(fresh.ItemCount(item), Is.EqualTo(1));
            Assert.That(fresh.State.collectedPickups, Does.Contain("pickup")); Assert.That(fresh.CanStartEvent(daily), Is.False);
            fresh.TryEndDay(); fresh.TryStartNextDay(); Assert.That(fresh.CanStartEvent(daily), Is.True);
            Assert.That(fresh.TryCollect(pickup), Is.False);
        }
        [Test] public void InvalidNewSaveCollectionsAreRejectedWithoutMutation()
        {
            var bad = session.State.Clone(); bad.inventory.Add(new InventoryEntry { itemId = item.id, quantity = item.maxStack + 1 });
            string before = JsonUtility.ToJson(session.State); Assert.That(session.TryRestore(bad), Is.False);
            bad = session.State.Clone(); bad.roomId = "unknown"; Assert.That(session.TryRestore(bad), Is.False);
            bad = session.State.Clone(); bad.dailyInteractions.Add(new DailyInteractionState { eventId = "event", day = 31 });
            Assert.That(session.TryRestore(bad), Is.False); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(before));
        }
        [Test] public void VersionOneApSaveIsMigratedButNeverReinterpretedAsStamina()
        {
            config.usesStamina = false; config.rooms = new RoomDataSO[0]; config.startingRoomId = "";
            var legacy = new GameSession(config); var store = new SaveStore(directory);
            store.Save(legacy, SaveSlot.Manual); string path = store.PathFor(legacy, SaveSlot.Manual);
            File.WriteAllText(path, File.ReadAllText(path).Replace("\"version\": 3", "\"version\": 1"));
            Assert.That(store.Load(legacy, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            config.usesStamina = true; var newer = new GameSession(config);
            Assert.That(store.Load(newer, SaveSlot.Manual), Is.EqualTo(SaveResult.Incompatible));
        }
        [TestCase(2)] [TestCase(3)] public void NewerSavesCannotChangeResourceModelEvenAtZeroAp(int version)
        {
            config.usesStamina = false; config.rooms = new RoomDataSO[0]; config.startingRoomId = "";
            var old = new GameSession(config); old.ActionPoints.TrySpend(config.maxActionPointsPerDay);
            var store = new SaveStore(directory); Assert.That(store.Save(old, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            string path = store.PathFor(old, SaveSlot.Manual); File.WriteAllText(path, File.ReadAllText(path).Replace("\"version\": 3", "\"version\": " + version));
            config.usesStamina = true; var target = new GameSession(config); string before = JsonUtility.ToJson(target.State);
            Assert.That(store.Load(target, SaveSlot.Manual), Is.EqualTo(SaveResult.Incompatible));
            Assert.That(JsonUtility.ToJson(target.State), Is.EqualTo(before));
        }
        [Test] public void GeneratedRoomUsesActualSuppliedSpritesAndApprovedDuration()
        {
            var generated = AssetDatabase.LoadAssetAtPath<GameConfigSO>(IsometricPrototypeBuilder.ConfigPath);
            Assert.That(generated, Is.Not.Null); Assert.That(generated.totalDays, Is.EqualTo(30)); Assert.That(generated.usesStamina, Is.True);
            Assert.That(generated.rooms, Has.Length.EqualTo(2));
            foreach (var data in generated.rooms)
                foreach (var art in data.phases) Assert.That(art.background.rect.size, Is.EqualTo(new Vector2(1024, 1024)));
            Assert.That(FoundationValidator.ValidateProject(), Is.Empty);
        }
    }
}
