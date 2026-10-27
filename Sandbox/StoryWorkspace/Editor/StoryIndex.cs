using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLove.Core;
using ProjectLove.Data;
using ProjectLove.EditorTools;
using UnityEditor;

namespace ProjectLove.StorySandbox
{
    // Read-only legacy adapter. UI and draft storage never modify these source assets.
    public sealed class StoryEntry
    {
        public string title, kind, sourceProperty, gates;
        public IdentifiedDataSO owner;
        public DialogueNodeSO node;
        public EventDataSO story;
        public CharacterDataSO character;
        public ConversationVariant variant;
        public readonly HashSet<DialogueNodeSO> nodes = new HashSet<DialogueNodeSO>();
        public IEnumerable<CharacterDataSO> Speakers => nodes.SelectMany(n => n.lines)
            .Where(l => l.speaker != null).Select(l => l.speaker).Distinct();
    }

    public sealed class StoryEdge
    {
        public DialogueNodeSO from, to;
        public DialogueChoice choice;
        public string Label => choice == null ? "Continue" : choice.SourceText;
    }

    public sealed class ScriptRow
    {
        public DialogueNodeSO node;
        public DialogueLine line;
        public DialogueChoice choice;
        public string label;
        public int depth;
        public bool reference;
    }

    public sealed class StoryIndex
    {
        public readonly List<StoryEntry> entries = new List<StoryEntry>();
        public readonly List<StoryEdge> edges = new List<StoryEdge>();
        public readonly List<DialogueNodeSO> nodes;
        public StoryIndex(GameConfigSO game,IEnumerable<IdentifiedDataSO> extra=null)
        {
            var assets = AuthoringCatalog.Reachable(game).Concat(extra??Array.Empty<IdentifiedDataSO>()).Where(a=>a!=null).Distinct().ToList();
            nodes = assets.OfType<DialogueNodeSO>().ToList();
            foreach (var n in nodes)
            {
                // A node-level continuation is used only when there are no choices.
                if (n.choices.Length == 0 && n.nextNode != null)
                    edges.Add(new StoryEdge { from = n, to = n.nextNode });
                foreach (var c in n.choices)
                    edges.Add(new StoryEdge { from = n, to = c.nextNode, choice = c });
            }
            foreach (var owner in assets.Where(a => !(a is DialogueNodeSO)))
            {
                var so = new SerializedObject(owner);
                var p = so.GetIterator();
                while (p.NextVisible(true))
                {
                    if (p.propertyType != SerializedPropertyType.ObjectReference ||
                        !(p.objectReferenceValue is DialogueNodeSO n)) continue;
                    var e = new StoryEntry { owner = owner, node = n, sourceProperty = p.propertyPath,
                        title = Title(n), kind = "Activity", gates = "Selected by its caller" };
                    if (owner is EventDataSO story)
                    {
                        e.story = story; e.character = story.character; e.kind = "Event"; e.title=game.localization?.catalog?.Find(LocalizationService.Key(story,"displayName"))?.thai??Title(story);
                        e.gates = $"Days {story.startDay}–{story.endDay} · " +
                            (story.anyPhase ? "Any phase" : Phase(story.triggerPhase)) +
                            $" · {(story.room == null ? "Any room" : story.room.displayName)}\n" +
                            $"Priority {story.priority} · {Delivery(story.delivery)} · " +
                            (story.repeatDaily ? $"Repeat, gap {story.minimumRepeatDays} days" : "Once") +
                            (string.IsNullOrEmpty(story.exclusiveGroup) ? "" : " · Exclusive: " + story.exclusiveGroup) +
                            "\n" + Conditions(story.conditions);
                    }
                    else if (owner is CharacterDataSO character)
                    {
                        e.character = character;
                        e.title=character.displayName+" · "+Title(n);
                        bool greeting = p.propertyPath.StartsWith("greetings.");
                        bool talk = p.propertyPath.StartsWith("conversations.");
                        if (greeting || talk)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(p.propertyPath, @"data\[(\d+)\]");
                            if (match.Success)
                            {
                                var v = (greeting ? character.greetings : character.conversations)[int.Parse(match.Groups[1].Value)];
                                e.variant = v; e.kind = greeting ? "Greeting" : "Talk";
                                e.title += " · " + (string.IsNullOrEmpty(v.displayName) ? v.id : v.displayName);
                                e.gates = $"Days {v.startDay}–{v.endDay} · " + (v.anyPhase ? "Any phase" : Phase(v.phase)) +
                                    $"\nAffection {v.minimumAffection}–{v.maximumAffection} · Stress {v.minimumStress}–{v.maximumStress}" +
                                    $" · Priority {v.priority}" + (v.overtimeOnly ? " · Overtime required" : "") + "\n" + Conditions(v.conditions);
                            }
                        }
                    }
                    else if(owner is RomanceConfigSO romance)
                    {
                        var match=System.Text.RegularExpressions.Regex.Match(p.propertyPath,@"cast\.Array\.data\[(\d+)\]");
                        if(match.Success)e.character=romance.cast[int.Parse(match.Groups[1].Value)].character;
                        e.gates=p.propertyPath.EndsWith("sharedScene")?"Work Together · selected character":
                            p.propertyPath.EndsWith("focusTalk")?"Talk · unlocked topic overrides the regular pool":
                            p.propertyPath=="producerScene"?"Producer action":
                            "Personal Time · mood and affection select personal, comfort or warm block";
                    }
                    // Each serialized root reference is retained, including shared pool/context callers.
                    Visit(n, e.nodes); entries.Add(e);
                }
            }
            entries.Sort((a,b) => string.Compare(a.title,b.title,StringComparison.Ordinal));
        }
        public static string Title(IdentifiedDataSO a)
        {
            if (a == null) return "End";
            var p = new SerializedObject(a).FindProperty("displayName");
            if(p != null && !string.IsNullOrEmpty(p.stringValue))return p.stringValue;
            if(a is EventDataSO story && story.entryNode!=null)return Title(story.entryNode);
            if(a is DialogueNodeSO node)
            {
                var line=node.lines.FirstOrDefault(l=>!string.IsNullOrWhiteSpace(l.SourceText));
                if(line!=null)
                {
                    var elements=new System.Globalization.StringInfo(line.SourceText);
                    string snippet=elements.LengthInTextElements>38?elements.SubstringByTextElements(0,38)+"…":line.SourceText;
                    return (line.speaker?.displayName??"Narrator")+" · "+snippet;
                }
                return "Untitled Block";
            }
            return a.name;
        }
        static void Visit(DialogueNodeSO n, HashSet<DialogueNodeSO> seen)
        {
            if (n == null || !seen.Add(n)) return;
            if (n.choices.Length == 0) Visit(n.nextNode,seen);
            foreach(var c in n.choices) Visit(c.nextNode,seen);
        }
        public IEnumerable<StoryEntry> Callers(DialogueNodeSO n) => entries.Where(e => e.nodes.Contains(n));
        public IEnumerable<StoryEdge> Incoming(DialogueNodeSO n) => edges.Where(e => e.to == n);
        public IEnumerable<StoryEdge> Outgoing(DialogueNodeSO n) => edges.Where(e => e.from == n);
        public static bool Contains(string value,string query) => (value ?? "").IndexOf(query ?? "",StringComparison.OrdinalIgnoreCase) >= 0;
        public static bool Matches(DialogueNodeSO n,string query,StoryDraft draft=null)
        {
            return Contains(Title(n)+" "+n.id,query) || n.lines.Any(l =>
                Contains((draft?.Thai(l.localizedText,l.SourceText)??l.SourceText)+" "+(draft?.English(l.localizedText)??l.localizedText.catalog?.Find(l.localizedText.key)?.english)+" "+l.speaker?.displayName,query)) ||
                n.choices.Any(c => Contains((draft?.Thai(c.localizedText,c.SourceText)??c.SourceText)+" "+(draft?.English(c.localizedText)??c.localizedText.catalog?.Find(c.localizedText.key)?.english),query));
        }
        public List<ScriptRow> Manuscript(DialogueNodeSO start)
        {
            var rows = new List<ScriptRow>(); var seen = new HashSet<DialogueNodeSO>();
            void Walk(DialogueNodeSO n,int depth)
            {
                if(n == null) { rows.Add(new ScriptRow { label="End",depth=depth }); return; }
                if(!seen.Add(n)) { rows.Add(new ScriptRow { node=n,label="Join / Return: "+Title(n),depth=depth,reference=true });return; }
                rows.Add(new ScriptRow {node=n,label=Title(n),depth=depth});
                foreach(var l in n.lines) rows.Add(new ScriptRow {node=n,line=l,depth=depth});
                if(n.choices.Length==0) Walk(n.nextNode,depth);
                foreach(var c in n.choices)
                { rows.Add(new ScriptRow {node=n,choice=c,depth=depth});Walk(c.nextNode,depth+1); }
            }
            Walk(start,0); return rows;
        }
        public static string Phase(DayPhase p) => new[]{"Morning","Afternoon","Evening","Night"}[(int)p];
        public static string Delivery(EventDelivery d) => d == EventDelivery.Forced ? "Auto" : d == EventDelivery.CharacterBubble ? "Character" : "Manual";
        public static string Stat(StatType s) => new[]{"Progress","Money","Skill Points","Affection","Stress"}[(int)s];
        public static string Conditions(IEnumerable<Condition> conditions)
        {
            var rows = (conditions ?? Array.Empty<Condition>()).Select(c =>
            {
                if(c == null) return "Missing condition";
                string target = c.character == null ? "" : c.character.displayName+" · ";
                switch(c.type)
                {
                    case ConditionType.StatAtLeast:return target+Stat(c.stat)+" ≥ "+c.value;
                    case ConditionType.StatAtMost:return target+Stat(c.stat)+" ≤ "+c.value;
                    case ConditionType.FlagSet:return "Flag set: "+c.flagId;
                    case ConditionType.FlagUnset:return "Flag clear: "+c.flagId;
                    case ConditionType.EventCompleted:return "Event done: "+c.eventId;
                    case ConditionType.EventNotCompleted:return "Event pending: "+c.eventId;
                    case ConditionType.WorkloadActivated:return "Workload active: "+c.targetId;
                    case ConditionType.WorkloadNotActivated:return "Workload inactive: "+c.targetId;
                    case ConditionType.TaskAccepted:return "Task accepted: "+c.targetId;
                    case ConditionType.TaskNotAccepted:return "Task pending: "+c.targetId;
                    case ConditionType.CharacterOvertime:return target+"Overtime";
                    case ConditionType.CharacterCanVolunteer:return target+"Can volunteer";
                    case ConditionType.CharacterSharedNight:return target+"Shared night";
                    default:return "Unknown condition: "+c.type;
                }
            }).ToArray();
            return rows.Length == 0 ? "Always" : string.Join("\nAND ",rows);
        }
    }
}
