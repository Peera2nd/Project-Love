using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.VN;
namespace ProjectLove.Simulation
{
    public sealed class RomancePreview
    {
        public bool allowed;
        public int project, affection, money;
        public string topicFlag;
        public CharacterMood moodAfter;
        public DialogueNodeSO scene;
    }
    // The casual path has one clock and explicit rewards. Mood is authored state, never Stress in disguise.
    public sealed class RomanceSystem
    {
        readonly GameSession session;
        public RomanceConfigSO Config => session.Config.romance;
        public bool Enabled => Config!=null;
        RomanceState State => session.State.romance;
        readonly string fingerprint;
        public RomanceSystem(GameSession session)
        {
            this.session=session;
            if(!Enabled)return;
            if(session.Config.production!=null||session.Config.usesStamina||session.Config.workStreams.Length!=0||Config.daysPerFocus<1||
                Config.cast==null||Config.cast.Length!=session.Config.characters.Length||Config.cast.Any(c=>c==null||c.character==null||!session.Config.characters.Contains(c.character)||
                !DialogueRunner.IsPlayable(c.sharedScene)||!DialogueRunner.IsPlayable(c.personalScene)||!DialogueRunner.IsPlayable(c.comfortScene)||!DialogueRunner.IsPlayable(c.warmScene)||!DialogueRunner.IsPlayable(c.focusTalk)||string.IsNullOrWhiteSpace(c.topicFlag))||
                Config.cast.Select(c=>c.character.id).Distinct().Count()!=Config.cast.Length||Config.cast.Select(c=>c.topicFlag).Distinct().Count()!=Config.cast.Length||
                !DialogueRunner.IsPlayable(Config.producerScene)||Config.moods==null||Config.moods.Length!=4||Config.moods.Select(m=>m.mood).Distinct().Count()!=4||
                new[]{Config.teamDailyProgress,Config.sharedProgress,Config.sharedAffection,Config.personalAffection,Config.producerProgress,Config.producerMoney,Config.focusAffection}.Any(n=>n<0))
                throw new ArgumentException("Invalid romance rules.");
            bool ValidMood(CharacterMood mood)=>Enum.IsDefined(typeof(CharacterMood),mood);
            if(Config.cast.Any(c=>!ValidMood(c.initialMood)||!ValidMood(c.sharedMood)||!ValidMood(c.personalMood)||c.warmAffection<0||c.warmAffection>100||c.character.startingStress!=0||c.character.dailyTalkStaminaCost!=0)||
                Config.moods.Any(m=>m==null||!ValidMood(m.mood)||string.IsNullOrWhiteSpace(m.label))||Config.milestones==null||Config.moodSchedule==null||Config.moodOutcomes==null||Config.dailyHints==null||
                Config.moodSchedule.Any(m=>m==null||m.day<1||m.day>session.Config.totalDays||!session.Config.characters.Contains(m.character)||!ValidMood(m.mood))||
                Config.moodSchedule.GroupBy(m=>m.day+":"+m.character.id).Any(g=>g.Count()!=1)||
                Config.moodOutcomes.Any(m=>m==null||!session.Config.events.Any(e=>e.id==m.eventId)||!session.Config.characters.Contains(m.character)||!ValidMood(m.mood))||
                Config.milestones.Any(m=>m==null||m.startDay<1||m.endDay<m.startDay||m.endDay>session.Config.totalDays||m.target<1||m.target>100)||
                Enumerable.Range(1,session.Config.totalDays).Any(day=>Config.milestones.Count(m=>m.startDay<=day&&m.endDay>=day)!=1))throw new ArgumentException("Invalid romance mood/calendar content.");
            var visited=new HashSet<DialogueNodeSO>();
            void SafeScene(DialogueNodeSO n)
            {
                if(n==null||!visited.Add(n))return;
                if(!DialogueRunner.IsPlayable(n)||n.choices.Any(c=>c==null||c.actionPointCost!=0||c.staminaCost!=0||c.workAction!=DialogueWorkAction.None||c.effects.Any(e=>e.stat==StatType.Stress)))throw new ArgumentException("Romance scenes cannot charge AP/energy or change Stress.");
                foreach(var c in n.choices)SafeScene(c.nextNode);SafeScene(n.nextNode);
            }
            foreach(var e in session.Config.events){if(e.staminaCost!=0)throw new ArgumentException("Romance events use time only.");SafeScene(e.entryNode);}
            foreach(var c in Config.cast){SafeScene(c.sharedScene);SafeScene(c.personalScene);SafeScene(c.comfortScene);SafeScene(c.warmScene);SafeScene(c.focusTalk);}
            SafeScene(Config.producerScene);
            fingerprint=session.Config.localization!=null?LocalizedGameplayFingerprint.Compute(session.Config):LegacyFingerprint();
            session.State.romance=new RomanceState{catalog=fingerprint};
            foreach(var c in Config.cast)
            {session.State.FindCharacter(c.character.id).stress=0;State.moods.Add(new CharacterMoodState{characterId=c.character.id,mood=c.initialMood});}
            StartDay();
        }
        public int Week => (session.State.day-1)/Config.daysPerFocus+1;
        public RomanceCast Cast(string id)=>Config.cast.FirstOrDefault(c=>c.character.id==id);
        public CharacterMood Mood(string id)=>State.moods.First(m=>m.characterId==id).mood;
        public MoodDefinition MoodInfo(string id)=>Config.moods.First(m=>m.mood==Mood(id));
        public WeeklyFocusState Focus=>State.focuses.FirstOrDefault(f=>f.week==Week);
        public RomanceMilestone Milestone=>Config.milestones.FirstOrDefault(m=>session.State.day>=m.startDay&&session.State.day<=m.endDay);
        public string TodayHint=>Config.dailyHints.Length==0?"":session.Localization.Content(Config,"day/"+(Math.Min(session.State.day,Config.dailyHints.Length))+"/hint",Config.dailyHints[Math.Min(session.State.day-1,Config.dailyHints.Length-1)]);
        public RomanceDayReport PendingReport=>State.reports.LastOrDefault(r=>(r.day<session.State.day||session.Days.CampaignEnded)&&r.day>State.acknowledgedDay);
        public void AcknowledgeReport(){if(PendingReport!=null)State.acknowledgedDay=PendingReport.day;}
        public bool TryFocus(string id)
        {
            if(!Enabled||session.IsBusy||session.State.dayEnded||Focus!=null||Cast(id)==null)return false;
            State.focuses.Add(new WeeklyFocusState{week=Week,characterId=id});session.NotifyChanged();return true;
        }
        public RomancePreview Preview(RomanceActivity activity,string id=null)
        {
            var f=new RomancePreview();
            if(!Enabled||session.State.dayEnded||session.BoardOpen||session.Dialogue.IsActive||!session.Days.CanConsumePhases(1)||!Enum.IsDefined(typeof(RomanceActivity),activity))return f;
            var cast=id==null?null:Cast(id);
            if(activity==RomanceActivity.Producer)
            {
                if(session.IsBusy)return f;
                f.scene=Config.producerScene;f.project=Config.producerProgress;f.money=Config.producerMoney;
            }
            else
            {
                if(cast==null||!session.Interactions.IsOpen||session.Interactions.Character!=cast.character)return f;
                bool shared=activity==RomanceActivity.WorkTogether;
                f.scene=shared?cast.sharedScene:(Mood(id)==CharacterMood.NeedsSupport||Mood(id)==CharacterMood.Tired)?cast.comfortScene:
                    session.State.FindCharacter(id).affection>=cast.warmAffection?cast.warmScene:cast.personalScene;
                f.project=shared?Config.sharedProgress:0;f.affection=shared?Config.sharedAffection:Config.personalAffection;
                f.moodAfter=shared?cast.sharedMood:cast.personalMood;
                if(shared&&Focus!=null&&Focus.characterId==id&&!Focus.claimed)
                {f.affection+=Config.focusAffection;if(!session.State.flags.Contains(cast.topicFlag))f.topicFlag=cast.topicFlag;}
                f.affection=Math.Min(100-session.State.FindCharacter(id).affection,f.affection);
            }
            f.project=Math.Min(100-session.State.mainProgress,f.project);
            f.money=(int)Math.Min(int.MaxValue-(long)session.State.money,f.money);
            f.allowed=DialogueRunner.IsPlayable(f.scene);return f;
        }
        public bool TryStart(RomanceActivity activity,string id=null)
        {
            var f=Preview(activity,id);if(!f.allowed)return false;
            return session.Dialogue.TryBeginScene(f.scene,()=>
            {
                session.State.mainProgress+=f.project;session.State.money+=f.money;
                if(id!=null)
                {
                    session.State.FindCharacter(id).affection+=f.affection;
                    State.moods.First(m=>m.characterId==id).mood=f.moodAfter;
                    if(!string.IsNullOrEmpty(f.topicFlag))session.State.flags.Add(f.topicFlag);
                    if(activity==RomanceActivity.WorkTogether&&Focus!=null&&Focus.characterId==id)Focus.claimed=true;
                }
                State.receipt=new RomanceReceipt{title=Config.presentation.Get(activity.ToString()),titleKey=LocalizationService.Key(Config.presentation,"ui/"+activity),characterId=id,project=f.project,affection=f.affection,money=f.money,topicFlag=f.topicFlag};
                session.Interactions.CloseForWork();Consume(1);
            });
        }
        public void RecordGift(string id,int affection)
        {if(Enabled)State.receipt=new RomanceReceipt{title=Config.presentation.Get("gift"),titleKey=LocalizationService.Key(Config.presentation,"ui/gift"),characterId=id,affection=affection};}
        public void RecordTalk(string id,int affection)
        {if(Enabled)State.receipt=new RomanceReceipt{title=Config.presentation.Get("talk"),titleKey=LocalizationService.Key(Config.presentation,"ui/talk"),characterId=id,affection=affection};}
        public void CompleteStory(EventDataSO data,RuntimeGameState before)
        {
            if(!Enabled)return;
            foreach(var o in Config.moodOutcomes.Where(o=>o.eventId==data.id&&(string.IsNullOrEmpty(o.requiredFlag)||session.State.flags.Contains(o.requiredFlag))))
                State.moods.First(m=>m.characterId==o.character.id).mood=o.mood;
            int affection=before==null||data.character==null?0:session.State.FindCharacter(data.character.id).affection-before.FindCharacter(data.character.id).affection;
            State.receipt=before==null||data.elapsedPhases==0&&affection==0?null:new RomanceReceipt{title=data.displayName,titleKey=LocalizationService.Key(data,"displayName"),characterId=data.character?.id,affection=affection,project=session.State.mainProgress-before.mainProgress,money=Math.Max(0,session.State.money-before.money)};
        }
        public bool Consume(int count)
        {
            if(!session.Days.CanConsumePhases(count))return false;
            session.Days.TryConsumePhases(count);
            if(session.State.dayEnded&&!State.reports.Any(r=>r.day==session.State.day))
            {
                int gain=Math.Min(100-session.State.mainProgress,Config.teamDailyProgress);session.State.mainProgress+=gain;
                State.reports.Add(new RomanceDayReport{day=session.State.day,projectBefore=session.State.dayStartProgress,projectAfter=session.State.mainProgress,teamGain=gain,
                    moneyBefore=State.dayStartMoney,moneyAfter=session.State.money,affectionBefore=State.dayStartAffection,affectionAfter=Affections(),
                    newTopics=session.State.flags.Where(f=>Config.cast.Any(c=>c.topicFlag==f)&&!State.dayStartTopics.Contains(f)).ToList()});
            }
            return true;
        }
        public void StartDay()
        {
            session.State.actionPoints=0;session.State.stamina=0;State.receipt=null;
            State.dayStartMoney=session.State.money;State.dayStartAffection=Affections();State.dayStartTopics=session.State.flags.Where(f=>Config.cast.Any(c=>c.topicFlag==f)).ToList();
            foreach(var change in Config.moodSchedule.Where(c=>c.day==session.State.day))State.moods.First(m=>m.characterId==change.character.id).mood=change.mood;
        }
        List<AffectionSnapshot> Affections()=>session.State.characters.Select(c=>new AffectionSnapshot{characterId=c.characterId,value=c.affection}).ToList();
        public bool CanRestore(RuntimeGameState s)
        {
            if(!Enabled)return s.romance==null;
            var r=s.romance;
            bool AffectionsValid(List<AffectionSnapshot> a)=>a!=null&&a.Count==Config.cast.Length&&a.All(v=>v!=null&&Cast(v.characterId)!=null&&v.value>=0&&v.value<=100)&&a.Select(v=>v.characterId).Distinct().Count()==a.Count;
            return r!=null&&r.catalog==fingerprint&&s.actionPoints==0&&s.stamina==0&&s.characters.All(c=>c.stress==0)&&
                r.dayStartMoney>=0&&r.acknowledgedDay>=0&&r.acknowledgedDay<=(s.dayEnded&&s.day==session.Config.totalDays?s.day:s.day-1)&&r.moods!=null&&r.moods.Count==Config.cast.Length&&
                r.moods.All(m=>m!=null&&Cast(m.characterId)!=null&&Enum.IsDefined(typeof(CharacterMood),m.mood))&&r.moods.Select(m=>m.characterId).Distinct().Count()==r.moods.Count&&
                r.focuses!=null&&r.focuses.All(f=>f!=null&&f.week>=1&&f.week<=(s.day-1)/Config.daysPerFocus+1&&Cast(f.characterId)!=null&&(!f.claimed||s.flags.Contains(Cast(f.characterId).topicFlag)))&&
                r.focuses.Select(f=>f.week).Distinct().Count()==r.focuses.Count&&AffectionsValid(r.dayStartAffection)&&r.dayStartTopics!=null&&r.dayStartTopics.All(t=>Config.cast.Any(c=>c.topicFlag==t))&&
                r.reports!=null&&r.reports.Count==s.day-1+(s.dayEnded?1:0)&&r.reports.Select(v=>v?.day).SequenceEqual(Enumerable.Range(1,r.reports.Count).Select(v=>(int?)v))&&
                r.reports.All(v=>v!=null&&v.projectBefore>=0&&v.projectAfter>=v.projectBefore&&v.projectAfter<=100&&v.teamGain>=0&&v.teamGain<=Config.teamDailyProgress&&
                    v.teamGain<=v.projectAfter-v.projectBefore&&v.moneyBefore>=0&&v.moneyAfter>=0&&AffectionsValid(v.affectionBefore)&&AffectionsValid(v.affectionAfter)&&v.newTopics!=null&&v.newTopics.All(t=>Config.cast.Any(c=>c.topicFlag==t)))&&
                (r.receipt==null||r.receipt.project>=0&&r.receipt.project<=100&&r.receipt.affection>=-100&&r.receipt.affection<=100&&r.receipt.money>=0&&(string.IsNullOrEmpty(r.receipt.characterId)||Cast(r.receipt.characterId)!=null));
        }
        public string LegacyFingerprint()
        {
            var b=new StringBuilder();void Add(params object[] xs){foreach(var x in xs){string v=x?.ToString()??"";b.Append(v.Length).Append(':').Append(v);}}
            Add(Config.id,Config.teamDailyProgress,Config.sharedProgress,Config.sharedAffection,Config.personalAffection,Config.producerProgress,Config.producerMoney,Config.focusAffection,Config.daysPerFocus);
            var seen=new HashSet<DialogueNodeSO>();void Node(DialogueNodeSO n){if(n==null||!seen.Add(n))return;Add(n.id);foreach(var l in n.lines)Add(l.lineId,l.text,l.speaker?.id,l.expression);foreach(var c in n.choices){Add(c.choiceId,c.text);Node(c.nextNode);}Node(n.nextNode);}
            Node(Config.producerScene);
            foreach(var c in Config.cast){Add(c.character.id,c.character.startingAffection,c.character.dailyTalkAffection,c.topicFlag,c.warmAffection,c.initialMood,c.sharedMood,c.personalMood);Node(c.sharedScene);Node(c.personalScene);Node(c.comfortScene);Node(c.warmScene);Node(c.focusTalk);foreach(var v in c.character.conversations)foreach(var n in v.Nodes)Node(n);foreach(var v in c.character.greetings)foreach(var n in v.Nodes)Node(n);foreach(var p in c.character.giftPreferences){Add(p.item.id,p.affectionChange);Node(p.response);}Node(c.character.neutralGiftResponse);}
            foreach(var m in Config.moodSchedule)Add(m.day,m.character.id,m.mood);
            foreach(var m in Config.moodOutcomes)Add(m.eventId,m.requiredFlag,m.character.id,m.mood);
            foreach(var m in Config.milestones)Add(m.startDay,m.endDay,m.target);
            using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(b.ToString())));
        }
    }
}
