using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using ProjectLove.Simulation;
using ProjectLove.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class OpeningStoryTests
    {
        GameConfigSO config;
        GameSession session;
        string saveDirectory;

        [SetUp] public void Setup()
        {
            config = AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);
            session = new GameSession(config);
            saveDirectory = Path.GetFullPath(Path.Combine("Logs", "OpeningStorySaves", Guid.NewGuid().ToString("N")));
        }
        [TearDown] public void Teardown()
        {
            if (Directory.Exists(saveDirectory)) Directory.Delete(saveDirectory, true);
        }
        EventDataSO Event(string id) => config.events.Single(e => e.id == id);
        void Finish(int option = 0)
        {
            int guard = 0;
            while (session.Dialogue.IsActive && guard++ < 100)
            {
                if (session.Dialogue.ChoicesReady)
                {
                    var choices = session.Dialogue.Node.choices.Where(session.Dialogue.CanChoose).ToArray();
                    Assert.That(choices.Length, Is.GreaterThan(option), session.Dialogue.Node.id);
                    Assert.That(session.Dialogue.TryChoose(choices[option]), Is.True);
                    option = 0;
                }
                else Assert.That(session.Dialogue.TryAdvance(), Is.True);
            }
            Assert.That(session.Dialogue.IsActive, Is.False, "Unfinished opening dialogue");
        }
        void Play(string id, int option = 0)
        {
            Assert.That(session.Dialogue.TryBegin(Event(id)), Is.True, id);
            Finish(option);
        }
        void Forced(int option = 0)
        {
            Assert.That(session.Events.TryBeginForced(), Is.True);
            Finish(option);
        }
        void Day(int day)
        {
            while (session.State.day < day)
            {
                while (!session.State.dayEnded)
                {
                    if (session.Events.HasForcedPending) Forced();
                    else Assert.That(session.TryEndDay(), Is.True);
                }
                Assert.That(session.TryStartNextDay(), Is.True);
                session.Romance.AcknowledgeReport();
            }
        }
        CharacterDataSO Person(string key) => config.characters.Single(c => c.id == "romance_" + key);
        void BeginCompanion(string key, int response = 0, int style = 0)
        {
            Forced(style);
            Assert.That(session.State.flags, Does.Contain("opening_advice_" + new[] { "direct", "listen", "natural" }[style]));
            Assert.That(session.Events.BubbleEvents().Count(e => e.id.StartsWith("opening_") && e.id.EndsWith("_personal")), Is.EqualTo(3));
            Play("opening_" + key + "_personal", response);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Daytime));
            Assert.That(session.State.flags, Does.Contain("opening_companion_" + key));
            Assert.That(session.Events.BubbleEvents().Any(e => e.id.EndsWith("_personal") && e.id.StartsWith("opening_")), Is.False);
        }

        [TestCase("grace", 0, 0, 0)]
        [TestCase("grace", 1, 1, 1)]
        [TestCase("plaifah", 0, 2, 0)]
        [TestCase("plaifah", 1, 0, 1)]
        [TestCase("ploy", 0, 1, 0)]
        [TestCase("ploy", 1, 2, 1)]
        public void CompanionMemoryAppointmentAndReload(string key, int response, int style, int slot)
        {
            BeginCompanion(key, response, style);
            Assert.That(session.State.FindCharacter(Person(key).id).affection, Is.EqualTo(2));
            Day(2);
            Assert.That(session.Events.NextForced.id, Is.EqualTo("opening_" + key + "_memory_" + response));
            Forced(slot);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning));
            Assert.That(session.State.flags, Does.Contain("opening_" + key + "_invited"));
            Assert.That(session.Events.NextForced, Is.Null);
            var store = new SaveStore(saveDirectory);
            Assert.That(store.Save(session, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            var loaded = new GameSession(config);
            Assert.That(store.Load(loaded, SaveSlot.Manual), Is.EqualTo(SaveResult.Success));
            session = loaded;
            Assert.That(session.Events.NextForced, Is.Null, "Memory must not replay after load");
            Day(3);
            Assert.That(session.Events.NextForced.id, Is.EqualTo("opening_demo_" + new[] { "direct", "listen", "natural" }[style]));
            Forced();
            var person = Person(key);
            var greeting = session.Interactions.SelectFor(person, person.greetings);
            Assert.That(greeting.id, Does.EndWith(slot == 0 ? "reminder_daytime" : "reminder_evening"));
            string appointment = "opening_" + key + "_appointment_" + (slot == 0 ? "daytime" : "evening");
            Assert.That(session.Events.BubbleEvents().Any(e => e.id == appointment), Is.False, "Appointment must respect agreed phase");
            for (int phase = 0; phase <= slot; phase++) Assert.That(session.TryAdvancePhase(), Is.True);
            Assert.That(session.Events.BubbleEvents().Any(e => e.id == appointment), Is.True);
            var before = session.State.phase;
            Play(appointment, response);
            Assert.That((int)session.State.phase, Is.EqualTo((int)before + 1));
            Assert.That(session.State.FindCharacter(person.id).affection, Is.EqualTo(5));
            Assert.That(session.State.flags, Does.Contain("opening_" + key + "_answer_" + response));
            Assert.That(session.Romance.Mood(person.id), Is.EqualTo(CharacterMood.Cheerful));
            Assert.That(session.Interactions.SelectFor(person, person.greetings).id, Does.EndWith("_after"));
            Assert.That(session.CanStartEvent(Event(appointment)), Is.False);
            Assert.That(session.CanRestore(session.State), Is.True);
        }

        [Test] public void BackRollsBackOpeningAndPersonalChoiceWithoutRewardsOrFocusLock()
        {
            Assert.That(session.Events.TryBeginForced(), Is.True);
            while (!session.Dialogue.ChoicesReady) session.Dialogue.TryAdvance();
            Assert.That(session.Dialogue.TryChoose(session.Dialogue.Node.choices[1]), Is.True);
            Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(session.State.flags, Does.Not.Contain("opening_advice_listen"));
            Play("romance_welcome");
            Play("opening_grace_personal");
            // Use another detached run to verify a mid-reply cancellation before commitment.
            session = new GameSession(config); Forced();
            Assert.That(session.Dialogue.TryBegin(Event("opening_plaifah_personal")), Is.True);
            while (!session.Dialogue.ChoicesReady) session.Dialogue.TryAdvance();
            session.Dialogue.TryChoose(session.Dialogue.Node.choices[0]);
            Assert.That(session.Dialogue.TryDefer(), Is.True);
            Assert.That(session.State.phase, Is.EqualTo(DayPhase.Morning));
            Assert.That(session.State.flags, Does.Not.Contain("opening_companion_plaifah"));
            Assert.That(session.State.FindCharacter(Person("plaifah").id).affection, Is.Zero);
            Assert.That(session.Events.BubbleEvents().Count(e => e.id.StartsWith("opening_") && e.id.EndsWith("_personal")), Is.EqualTo(3));
        }

        [Test] public void DeclineReturnLateVignetteAndMissedAppointmentHaveRepair()
        {
            Forced(); Day(3); Forced();
            Play("opening_ploy_personal", 1);
            Assert.That(session.Events.NextForced.id, Is.EqualTo("opening_ploy_memory_1"));
            Forced(2);
            Assert.That(session.State.flags, Does.Not.Contain("opening_ploy_invited"));
            Play("opening_ploy_reinvite", 2);
            Assert.That(session.CanStartEvent(Event("opening_ploy_reinvite")), Is.False);
            Day(4);
            Play("opening_ploy_reinvite");
            Assert.That(session.State.flags, Does.Contain("opening_ploy_invited"));
            Day(8);
            Assert.That(session.Events.BubbleEvents().Any(e => e.id == "opening_ploy_makeup"), Is.True);
            Play("opening_ploy_makeup", 1);
            Assert.That(session.State.flags, Does.Contain("opening_ploy_payoff"));
            Assert.That(session.CanStartEvent(Event("opening_ploy_makeup")), Is.False);
            Assert.That(session.State.FindCharacter(Person("ploy").id).affection, Is.EqualTo(5));
            Assert.That(session.CanRestore(session.State), Is.True);
        }

        [Test] public void AuthoredGraphIsDiscoverableAndOpeningChoicesRender()
        {
            Assert.That(EventScheduler.ValidateCatalog(config), Is.Empty);
            var entries = AuthoringCatalog.Entries(config);
            Assert.That(AuthoringCatalog.Search(entries, "ถือแฟ้ม").Any(e => e.node.id == "romance_welcome_scene"), Is.True);
            foreach (var e in config.events.Where(e => e.id.StartsWith("opening_")))
                Assert.That(entries.Any(n => n.story == e && n.node == e.entryNode), Is.True, e.id);
            using (var preview = new AuthoringPreview(config, AuthoringScreen.Conversation, story: Event("romance_welcome")))
            {
                while (!preview.Session.Dialogue.ChoicesReady) preview.Advance();
                preview.View.Refresh(); preview.View.RevealAll();
                var texture = preview.Render(1280, 720);
                Assert.That(texture, Is.Not.Null);
                foreach (var choice in preview.Session.Dialogue.Node.choices)
                {
                    var label = preview.View.GetComponentsInChildren<TMP_Text>().FirstOrDefault(t => t.text == choice.text);
                    Assert.That(label, Is.Not.Null, choice.text);
                    label.ForceMeshUpdate();
                    Assert.That(label.isTextOverflowing, Is.False, choice.text);
                }
                var target = RenderTexture.GetTemporary(1280, 720, 0);
                var old = RenderTexture.active;
                try
                {
                    Graphics.Blit(texture, target); RenderTexture.active = target;
                    var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                    try
                    {
                        pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); pixels.Apply();
                        Assert.That(pixels.GetPixels32().Count(c => c.r + c.g + c.b > 60), Is.GreaterThan(10000));
                        Directory.CreateDirectory("Logs"); File.WriteAllBytes("Logs/Session058_OpeningChoices.png", pixels.EncodeToPNG());
                    }
                    finally { UnityEngine.Object.DestroyImmediate(pixels); }
                }
                finally { RenderTexture.active = old; RenderTexture.ReleaseTemporary(target); }
            }
        }
    }
}
