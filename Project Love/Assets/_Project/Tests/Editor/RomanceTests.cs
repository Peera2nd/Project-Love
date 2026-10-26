using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;
using UnityEngine;
namespace ProjectLove.Tests
{
    public sealed class RomanceTests
    {
        GameConfigSO config;GameSession s;string directory;
        [SetUp]public void Setup(){config=AssetDatabase.LoadAssetAtPath<GameConfigSO>(RomancePrototypeBuilder.ConfigPath);s=new GameSession(config);directory=Path.GetFullPath(Path.Combine("Logs","RomanceSaves",Guid.NewGuid().ToString("N")));Welcome();}
        [TearDown]public void Teardown(){if(Directory.Exists(directory))Directory.Delete(directory,true);}
        void Welcome(){Assert.That(s.Events.TryBeginForced(),Is.True);Finish();}
        void Finish(){int guard=0;while(s.Dialogue.IsActive&&guard++<30){if(s.Dialogue.ChoicesReady)s.Dialogue.TryChoose(s.Dialogue.Node.choices.First(s.Dialogue.CanChoose));else s.Dialogue.TryAdvance();}Assert.That(s.Dialogue.IsActive,Is.False);}
        CharacterDataSO Person=>config.characters[0];
        void Open(CharacterDataSO p=null){var c=p??Person;Assert.That(s.Interactions.TryOpen(s.CurrentRoom.hotspots.First(h=>h.character==c)),Is.True);}
        void Activity(RomanceActivity a,CharacterDataSO p=null){if(a!=RomanceActivity.Producer)Open(p);Assert.That(s.Romance.TryStart(a,a==RomanceActivity.Producer?null:(p??Person).id),Is.True);Finish();}
        [Test]public void NoStressEnergyOrProductionSimulationExistsInCasualSession()
        {Assert.That(s.Production.Enabled,Is.False);Assert.That(s.State.production,Is.Null);Assert.That(s.State.actionPoints,Is.Zero);Assert.That(s.State.stamina,Is.Zero);Assert.That(s.Stats.TryGet(StatType.Stress,Person,out _),Is.False);Assert.That(s.CanPay(1,0),Is.False);Assert.That(s.CanPay(0,1),Is.False);Assert.That(s.CanRestore(s.State),Is.True);}
        [Test]public void SharedSceneCommitsPreviewOnceAndBackRollsEverythingBack()
        {
            Open();var f=s.Romance.Preview(RomanceActivity.WorkTogether,Person.id);string before=JsonUtility.ToJson(s.State);Assert.That(s.Interactions.TryWork(),Is.True);s.Dialogue.TryAdvance();Assert.That(s.State.mainProgress,Is.Zero);Assert.That(s.Dialogue.TryDefer(),Is.True);Assert.That(JsonUtility.ToJson(s.State),Is.EqualTo(before));
            Assert.That(s.Interactions.TryWork(),Is.True);Finish();Assert.That(s.State.mainProgress,Is.EqualTo(f.project));Assert.That(s.State.FindCharacter(Person.id).affection,Is.EqualTo(f.affection));Assert.That(s.State.phase,Is.EqualTo(DayPhase.Daytime));Assert.That(s.Dialogue.TryAdvance(),Is.False);Assert.That(s.CanRestore(s.State),Is.True);
        }
        [Test]public void PersonalTimeGivesMoreAffectionWithoutProjectBonusOrMoodOutputMultiplier()
        {Open();var work=s.Romance.Preview(RomanceActivity.WorkTogether,Person.id);var time=s.Romance.Preview(RomanceActivity.PersonalTime,Person.id);Assert.That(time.affection,Is.GreaterThan(work.affection));Assert.That(time.project,Is.Zero);s.Romance.TryStart(RomanceActivity.PersonalTime,Person.id);Finish();Assert.That(s.State.mainProgress,Is.Zero);Assert.That(s.Romance.Mood(Person.id),Is.EqualTo(CharacterMood.Cheerful));Open();Assert.That(s.Romance.Preview(RomanceActivity.WorkTogether,Person.id).project,Is.EqualTo(work.project));}
        [Test]public void ProducerHasOneImmediateRewardWithoutStageGatesOrDepartmentBuffs()
        {var f=s.Romance.Preview(RomanceActivity.Producer);Activity(RomanceActivity.Producer);Assert.That(s.State.mainProgress,Is.EqualTo(f.project));Assert.That(s.State.money,Is.EqualTo(config.startingMoney+f.money));Assert.That(s.State.romance.receipt.money,Is.EqualTo(f.money));Assert.That(s.State.workProgress,Is.Empty);}
        [Test]public void BaselineOccursOnceAtDayEndAndRecapHonorsSeparateSources()
        {Activity(RomanceActivity.WorkTogether);Activity(RomanceActivity.PersonalTime);Assert.That(s.TryEndDay(),Is.True);int end=s.State.mainProgress;Assert.That(s.TryEndDay(),Is.False);Assert.That(s.State.mainProgress,Is.EqualTo(end));var report=s.State.romance.reports.Single();Assert.That(report.teamGain,Is.EqualTo(config.romance.teamDailyProgress));Assert.That(report.projectAfter-report.projectBefore-report.teamGain,Is.EqualTo(config.romance.sharedProgress));Assert.That(s.TryStartNextDay(),Is.True);Assert.That(s.Romance.PendingReport,Is.Not.Null);s.Romance.AcknowledgeReport();Assert.That(s.Romance.PendingReport,Is.Null);Assert.That(s.CanRestore(s.State),Is.True);}
        [Test]public void DailyTalkFreeRepeatAndNewDayRewardRemainIndependentOfMajorTime()
        {Open();s.Interactions.TryTalk();Finish();int love=s.State.FindCharacter(Person.id).affection;s.Interactions.TryTalk();Finish();Assert.That(s.State.FindCharacter(Person.id).affection,Is.EqualTo(love));Assert.That(s.State.phase,Is.EqualTo(DayPhase.Morning));s.Interactions.TryClose();s.TryEndDay();s.TryStartNextDay();Open();Assert.That(s.Interactions.RewardAvailable,Is.True);s.Interactions.TryTalk();Finish();Assert.That(s.State.FindCharacter(Person.id).affection,Is.EqualTo(2*love));}
        [Test]public void FocusIsOptionalOnePerWeekAndBonusCannotBeFarmedAcrossDaysOrWeeks()
        {
            Assert.That(s.Romance.TryFocus(Person.id),Is.True);Assert.That(s.Romance.TryFocus(config.characters[1].id),Is.False);Open();var f=s.Romance.Preview(RomanceActivity.WorkTogether,Person.id);s.Interactions.TryWork();Finish();Assert.That(s.State.flags,Does.Contain(config.romance.cast[0].topicFlag));Open();Assert.That(s.Romance.Preview(RomanceActivity.WorkTogether,Person.id).affection,Is.EqualTo(config.romance.sharedAffection));s.Interactions.TryClose();
            while(s.State.day<8){s.TryEndDay();s.TryStartNextDay();}Assert.That(s.Romance.TryFocus(Person.id),Is.True);Activity(RomanceActivity.WorkTogether);Open();Assert.That(s.Romance.Preview(RomanceActivity.WorkTogether,Person.id).affection,Is.EqualTo(config.romance.sharedAffection));Assert.That(s.State.flags.Count(t=>t==config.romance.cast[0].topicFlag),Is.EqualTo(1));Assert.That(s.CanRestore(s.State),Is.True);
        }
        [Test]public void MoodChoosesComfortAndAuthoredEventOutcomeWithoutStress()
        {
            s.TryEndDay();s.TryStartNextDay();Assert.That(s.Romance.Mood(Person.id),Is.EqualTo(CharacterMood.Tired));Open();Assert.That(s.Romance.Preview(RomanceActivity.PersonalTime,Person.id).scene,Is.EqualTo(config.romance.cast[0].comfortScene));s.Interactions.TryClose();
            var bubble=s.Events.BubbleEvents().First(e=>e.character==Person);Assert.That(s.Dialogue.TryBegin(bubble),Is.True);string before=JsonUtility.ToJson(s.State);s.Dialogue.TryAdvance();s.Dialogue.TryChoose(s.Dialogue.Node.choices[0]);Assert.That(s.Dialogue.TryDefer(),Is.True);Assert.That(s.Romance.Mood(Person.id),Is.EqualTo(CharacterMood.Tired));Assert.That(s.State.flags,Does.Not.Contain("romance_coffee_quiet"));s.Dialogue.TryBegin(bubble);Finish();Assert.That(s.Romance.Mood(Person.id),Is.EqualTo(CharacterMood.Calm));Assert.That(s.State.phase,Is.EqualTo(DayPhase.Daytime));Assert.That(s.CanRestore(s.State),Is.True);
        }
        [Test]public void SaveReloadPreservesMoodFocusRecapAndDoesNotReplayRewards()
        {
            s.Romance.TryFocus(Person.id);Activity(RomanceActivity.WorkTogether);s.TryEndDay();s.TryStartNextDay();var store=new SaveStore(directory);string before=JsonUtility.ToJson(s.State);Assert.That(store.Save(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Activity(RomanceActivity.Producer);Assert.That(store.Load(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Assert.That(JsonUtility.ToJson(s.State),Is.EqualTo(before));Assert.That(s.Romance.PendingReport,Is.Not.Null);s.Romance.AcknowledgeReport();Assert.That(store.Save(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Assert.That(store.Load(s,SaveSlot.Manual),Is.EqualTo(SaveResult.Success));Assert.That(s.Romance.PendingReport,Is.Null);
        }
        [Test]public void InvalidMoodDuplicateFocusAndHiddenStressCannotBeRestored()
        {var bad=s.State.Clone();bad.romance.moods[0].mood=(CharacterMood)99;Assert.That(s.CanRestore(bad),Is.False);bad=s.State.Clone();bad.characters[0].stress=10;Assert.That(s.CanRestore(bad),Is.False);bad=s.State.Clone();bad.romance.focuses.Add(new WeeklyFocusState{week=1,characterId=Person.id});bad.romance.focuses.Add(new WeeklyFocusState{week=1,characterId=Person.id});Assert.That(s.CanRestore(bad),Is.False);}
        [TestCase(0,90)][TestCase(1,100)][TestCase(2,100)]public void ThirtyDayPoliciesRemainPlayableWithoutBoardEnergyOrOt(int policy,int expected)
        {
            for(int day=1;day<=config.totalDays;day++)
            {
                for(int phase=0;phase<4;phase++)Activity(policy==1&&phase==0?RomanceActivity.WorkTogether:policy==2&&day<=3&&phase==0?RomanceActivity.Producer:RomanceActivity.PersonalTime,config.characters[(day+phase)%3]);
                Assert.That(s.CanRestore(s.State),Is.True,"Day "+day);Assert.That(s.State.characters.All(c=>c.stress==0),Is.True);
                if(day<config.totalDays)Assert.That(s.TryStartNextDay(),Is.True);
            }
            Assert.That(s.State.mainProgress,Is.EqualTo(expected));Assert.That(s.Days.CampaignEnded,Is.True);Assert.That(s.State.romance.reports.Count,Is.EqualTo(30));Assert.That(s.TryStartNextDay(),Is.False);Assert.That(s.Romance.PendingReport.day,Is.EqualTo(30));s.Romance.AcknowledgeReport();Assert.That(s.Romance.PendingReport,Is.Null);Assert.That(s.CanRestore(s.State),Is.True);
        }
    }
}
