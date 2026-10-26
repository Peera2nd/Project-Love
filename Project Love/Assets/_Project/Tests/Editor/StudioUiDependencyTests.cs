using System;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using ProjectLove.Simulation;
using UnityEditor;
using UnityEngine;

namespace ProjectLove.Tests
{
    public sealed class StudioUiDependencyTests
    {
        private GameConfigSO config;
        private GameSession session;
        [SetUp] public void Setup()
        {
            config = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath));
            config.events = Array.Empty<EventDataSO>(); session = new GameSession(config);
        }
        [TearDown] public void Teardown() => UnityEngine.Object.DestroyImmediate(config);
        private WorkTaskSO Task(string id) => config.production.tasks.Single(t => t.id == id);
        [Test] public void SuppliedIconsAvatarsPanelsAndSharedSheetAreBoundToRealSprites()
        {
            var style = config.production.presentation; Assert.That(style.useStudioSkin, Is.True); Assert.That(style.uiContentVersion, Is.EqualTo(1));
            Assert.That(style.icons.Length, Is.GreaterThanOrEqualTo(36)); Assert.That(style.icons.All(i => i.sprite != null), Is.True);
            Assert.That(config.characters.All(c => c.taskAvatar != null), Is.True); Assert.That(config.workStreams.All(s => s.icon != null), Is.True);
            Assert.That(new[] { style.panelSprite, style.taskCardSprite, style.avatarFrameSprite, style.selectionRingSprite }.All(s => s != null), Is.True);
            var bars = new[] { style.barTrack, style.barFill, style.barFrame, style.barForecast };
            Assert.That(bars.All(b => b != null && b.rect.width == 256 && b.rect.height == 64 && b.border == Vector4.one * 16), Is.True);
            Assert.That(bars.Select(b => b.texture).Distinct().Count(), Is.EqualTo(1));
            for (int i = 0; i < bars.Length; i++) Assert.That(bars[i].rect.x, Is.EqualTo(i * 256));
            Assert.That(style.panelSprite.border, Is.EqualTo(Vector4.one * 24));
        }
        [Test] public void FirstSprintHasDesignArtCodeChainAndIndependentAlternatives()
        {
            var brief = Task("studio_w1_ploy_1"); var art = Task("studio_w1_plaifah_2"); var code = Task("studio_w1_grace_2");
            Assert.That(art.prerequisites, Does.Contain(brief)); Assert.That(code.prerequisites, Does.Contain(art));
            Assert.That(Task("studio_w1_grace_1").prerequisites, Is.Empty); Assert.That(Task("studio_w1_plaifah_1").prerequisites, Is.Empty);
            Assert.That(Task("studio_w1_ploy_2").prerequisites, Is.Empty);
            Assert.That(session.Production.IsReady(session.Production.Find(art.id)), Is.False);
        }
        [Test] public void ReadyReviewDoesNotUnlockArtUntilAcceptedAndAcceptanceDoesNotConsumeTime()
        {
            var work = session.Production; var brief = Task("studio_w1_ploy_1"); var art = Task("studio_w1_plaifah_2");
            work.Find(brief.id).effort = brief.effort;
            Assert.That(work.Column(work.Find(brief.id)), Is.EqualTo(TaskColumn.Review)); Assert.That(work.IsReady(work.Find(art.id)), Is.False);
            int stamina = session.State.stamina; int phases = session.State.production.resolvedPhases;
            Assert.That(work.TryReview(), Is.True); Assert.That(work.IsReady(work.Find(art.id)), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(stamina)); Assert.That(session.State.production.resolvedPhases, Is.EqualTo(phases));
            string accepted = JsonUtility.ToJson(session.State);
            Assert.That(work.TryReview(), Is.True); Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(accepted));
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void ArtAcceptanceStillRequiresProducerDecisionBeforeCodeIntegration()
        {
            var work = session.Production; var brief = Task("studio_w1_ploy_1"); var art = Task("studio_w1_plaifah_2"); var code = Task("studio_w1_grace_2");
            work.Find(brief.id).effort = brief.effort; work.TryReview(); work.Find(art.id).effort = art.effort; work.TryReview();
            Assert.That(work.IsReady(work.Find(code.id)), Is.False);
            Assert.That(work.TryAct(ProductionAction.Clarify, code.id), Is.True); Assert.That(work.IsReady(work.Find(code.id)), Is.True);
            Assert.That(work.TryPrioritize(code.id), Is.True); Assert.That(work.Preview(ProductionAction.Help, code.owner.id).allowed, Is.True);
            Assert.That(session.CanRestore(session.State), Is.True);
        }
        [Test] public void BlockedPriorityCanBeChangedToIndependentWorkWithoutTimeOrResourceCost()
        {
            var work = session.Production; var blocked = Task("studio_w1_grace_2"); var free = Task("studio_w1_grace_1");
            string definition = JsonUtility.ToJson(blocked); Assert.That(work.TryPrioritize(blocked.id), Is.True);
            Assert.That(work.Preview(ProductionAction.Help, blocked.owner.id).allowed, Is.False);
            string state = JsonUtility.ToJson(session.State); var choices = work.ReadyTasks(blocked.owner);
            Assert.That(JsonUtility.ToJson(session.State), Is.EqualTo(state)); Assert.That(choices.Any(t => t.taskId == free.id), Is.True);
            Assert.That(choices.Any(t => t.taskId == blocked.id), Is.False); Assert.That(work.TryPrioritize(free.id), Is.True);
            Assert.That(session.State.stamina, Is.EqualTo(100)); Assert.That(session.State.production.resolvedPhases, Is.Zero);
            Assert.That(work.Preview(ProductionAction.Help, free.owner.id).allowed, Is.True); Assert.That(JsonUtility.ToJson(blocked), Is.EqualTo(definition));
        }
        [Test] public void OneTimeBindingRerunPreservesAuthoredDefinitionsAndSpriteIds()
        {
            var style = config.production.presentation;
            string before = JsonUtility.ToJson(style); var tasks = config.production.tasks.Select(JsonUtility.ToJson).ToArray();
            var ids = new[] { style.barTrack, style.barFill, style.barFrame, style.barForecast }.Select(s => GlobalObjectId.GetGlobalObjectIdSlow(s).ToString()).ToArray();
            StudioUiUpdate.Apply(); Assert.That(JsonUtility.ToJson(style), Is.EqualTo(before));
            Assert.That(config.production.tasks.Select(JsonUtility.ToJson), Is.EqualTo(tasks));
            Assert.That(new[] { style.barTrack, style.barFill, style.barFrame, style.barForecast }.Select(s => GlobalObjectId.GetGlobalObjectIdSlow(s).ToString()), Is.EqualTo(ids));
        }
    }
}
