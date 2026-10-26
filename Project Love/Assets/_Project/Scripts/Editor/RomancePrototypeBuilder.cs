using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ProjectLove.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ProjectLove.EditorTools
{
    public static class RomancePrototypeBuilder
    {
        public const string ScenePath="Assets/_Project/Scenes/Gameplay/StudioRomancePrototype.unity";
        public const string ConfigPath="Assets/_Project/Data/Romance/RomanceCampaign.asset";
        const string Root="Assets/_Project/Data/Romance/";
        [Serializable] sealed class CastSeed
        {
            public string sourceId,id,workTitle,topicTitle,topicFlag,greeting,talk;
            public int warmAffection;public CharacterMood initialMood,sharedMood,personalMood;
            public string[] shared,personal,comfort,warm,focusTalk;
        }
        [Serializable] sealed class MoodSeed {public int day;public string characterId;public CharacterMood mood;}
        [Serializable] sealed class ChoiceSeed { public string text,flag,reply;public int affection;public CharacterMood mood; }
        [Serializable] sealed class StorySeed { public string id,characterId,displayName;public int startDay,endDay,preferredDay,priority,elapsedPhases;public EventDelivery delivery;public string[] lines;public ChoiceSeed[] choices; }
        [Serializable] sealed class Seed { public string configId,rulesId,presentationId;public string[] producerLines;public CastSeed[] cast;public PresentationText[] text;public StorySeed[] stories;public MoodSeed[] moodSchedule; }
        [MenuItem("Tools/Project Love/Open Romance Prototype")]
        public static void BuildOrOpen()
        {
            if(EditorApplication.isPlaying||!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            ArtistWorkspaceBuilder.BuildFromCommandLine();EditorSceneManager.OpenScene(ScenePath);
        }
        public static void BuildFromCommandLine()
        {
            Directory.CreateDirectory(Root+"Dialogues");AssetDatabase.Refresh();
            var game=AssetDatabase.LoadAssetAtPath<GameConfigSO>(ConfigPath);
            if(game==null)
            {
                var original=AssetDatabase.LoadAssetAtPath<GameConfigSO>(StudioPrototypeBuilder.ConfigPath);
                if(original==null)throw new InvalidOperationException("Create the existing Studio prototype before the romance fixture.");
                string json=File.ReadAllText("Assets/_Project/Data/Settings/RomanceSeed.json");var seed=JsonUtility.FromJson<Seed>(json);
                T Save<T>(T value,string path,string id) where T:IdentifiedDataSO {value.id=id;AssetDatabase.CreateAsset(value,Root+path+".asset");return value;}
                var style=Save(UnityEngine.Object.Instantiate(original.production.presentation),"RomancePresentation",seed.presentationId);
                style.text=style.text.Where(t=>!seed.text.Any(s=>s.key==t.key)).Concat(seed.text).ToArray();style.charactersPerSecond=32;style.uiContentVersion=41;EditorUtility.SetDirty(style);
                var rules=ScriptableObject.CreateInstance<RomanceConfigSO>();JsonUtility.FromJsonOverwrite(json,rules);rules.presentation=style;Save(rules,"RomanceRules",seed.rulesId);
                var people=seed.cast.ToDictionary(c=>c.id,c=>{var p=UnityEngine.Object.Instantiate(original.characters.First(o=>o.id==c.sourceId));p.startingStress=0;p.dailyTalkStaminaCost=0;p.dailyTalkAffection=1;p.workActivity=null;return Save(p,c.id,c.id);});
                DialogueNodeSO Node(string id,CharacterDataSO person,string[] lines)=>Save(NodeValue(id,person,lines),"Dialogues/"+id,id);
                DialogueNodeSO Gift(DialogueNodeSO source,CharacterDataSO person)
                {
                    return Node(person.id+"_"+source.id,person,source.lines.Select(l=>l.text).ToArray());
                }
                rules.cast=seed.cast.Select(s=>
                {
                    var person=people[s.id];var greeting=Node(s.id+"_greeting",person,new[]{s.greeting});var talk=Node(s.id+"_talk",person,new[]{s.talk});
                    person.greetings=new[]{new ConversationVariant{id="default",endDay=original.totalDays,node=greeting}};
                    person.conversations=new[]{new ConversationVariant{id="default",endDay=original.totalDays,node=talk}};
                    person.neutralGiftResponse=Gift(person.neutralGiftResponse,person);
                    person.giftPreferences=person.giftPreferences.Select(g=>new GiftPreference{item=g.item,reaction=g.reaction,affectionChange=g.affectionChange,response=Gift(g.response,person)}).ToArray();
                    EditorUtility.SetDirty(person);
                    var c=new RomanceCast();JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(s),c);c.character=person;
                    c.sharedScene=Node(s.id+"_shared",person,s.shared);c.personalScene=Node(s.id+"_personal",person,s.personal);c.comfortScene=Node(s.id+"_comfort",person,s.comfort);
                    c.warmScene=Node(s.id+"_warm",person,s.warm);c.focusTalk=Node(s.id+"_focus_talk",person,s.focusTalk);return c;
                }).ToArray();
                rules.producerScene=Node("romance_producer",null,seed.producerLines);
                rules.moodSchedule=seed.moodSchedule.Select(m=>new MoodSchedule{day=m.day,character=people[m.characterId],mood=m.mood}).ToArray();
                game=UnityEngine.Object.Instantiate(original);game.production=null;game.romance=rules;game.usesStamina=false;game.startingStamina=0;game.overnightRecovery=0;game.startingSkillPoints=0;game.workStreams=Array.Empty<WorkStreamSO>();
                game.characters=people.Values.ToArray();
                var rooms=original.rooms.ToDictionary(r=>r.id,r=>Save(UnityEngine.Object.Instantiate(r),"romance_"+r.id,"romance_"+r.id));
                foreach(var room in rooms.Values)
                {
                    room.entryPhaseCost=0;
                    room.hotspots=room.hotspots.Where(h=>h.kind!=RoomInteractionKind.Producer&&h.kind!=RoomInteractionKind.Activity).ToArray();
                    foreach(var h in room.hotspots.Where(h=>h.character!=null))
                    {
                        var old=h.character;h.character=people[seed.cast.First(c=>c.sourceId==old.id).id];
                        var ambient=ScriptableObject.CreateInstance<EventDataSO>();ambient.entryNode=h.character.conversations[0].node;ambient.startDay=1;ambient.endDay=game.totalDays;ambient.preferredDay=1;ambient.anyPhase=true;
                        h.conversation=Save(ambient,"ambient_"+h.character.id,"ambient_"+h.character.id);
                    }
                    EditorUtility.SetDirty(room);
                }
                game.rooms=rooms.Values.ToArray();game.startingRoomId=rooms[original.startingRoomId].id;
                var outcomes=new List<MoodOutcome>();
                game.events=seed.stories.Select(s=>
                {
                    var e=ScriptableObject.CreateInstance<EventDataSO>();JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(s),e);e.character=people[s.characterId];e.room=rooms[original.startingRoomId];e.anyPhase=true;
                    var n=Node(s.id+"_scene",e.character,s.lines);
                    n.choices=s.choices.Select((c,i)=>
                    {
                        outcomes.Add(new MoodOutcome{eventId=s.id,requiredFlag=c.flag,character=e.character,mood=c.mood});
                        return new DialogueChoice{choiceId=s.id+"_choice_"+i,text=c.text,effects=new[]{new StatModifier{stat=StatType.Affection,character=e.character,amount=c.affection}},flags=new[]{new FlagChange{flagId=c.flag,value=true}},nextNode=Node(s.id+"_reply_"+i,e.character,new[]{c.reply})};
                    }).ToArray();EditorUtility.SetDirty(n);e.entryNode=n;return Save(e,s.id,s.id);
                }).ToArray();
                rules.moodOutcomes=outcomes.ToArray();EditorUtility.SetDirty(rules);Save(game,"RomanceCampaign",seed.configId);AssetDatabase.SaveAssets();
            }
            var currentStyle=game.romance.presentation;
            if(currentStyle.uiContentVersion<41)
            {
                var textSeed=JsonUtility.FromJson<Seed>(File.ReadAllText("Assets/_Project/Data/Settings/RomanceSeed.json"));
                currentStyle.text=currentStyle.text.Where(t=>!textSeed.text.Any(s=>s.key==t.key)).Concat(textSeed.text).ToArray();
                currentStyle.uiContentVersion=41;EditorUtility.SetDirty(currentStyle);
            }
            if(!File.Exists(ScenePath))IsometricPrototypeBuilder.BuildScene(game,game.romance.presentation,ScenePath);
            var existing=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToArray();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)}.Concat(existing).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Romance prototype ready: "+ScenePath);
        }
        static DialogueNodeSO NodeValue(string id,CharacterDataSO person,string[] lines)
        {
            var n=ScriptableObject.CreateInstance<DialogueNodeSO>();n.lines=lines.Select((text,i)=>new DialogueLine{lineId=id+"_line_"+i,speaker=person,text=text,expression=CharacterExpression.Normal}).ToArray();return n;
        }
    }
}
