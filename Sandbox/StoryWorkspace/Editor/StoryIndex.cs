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
        public string Label => choice == null ? "อ่านต่อ" : choice.SourceText;
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
        public StoryIndex(GameConfigSO game)
        {
            var assets = AuthoringCatalog.Reachable(game);
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
                        title = Title(n), kind = "กิจกรรม / ของขวัญ", gates = "ขึ้นกับระบบที่เรียกใช้ · ดูแหล่งอ้างอิง" };
                    if (owner is EventDataSO story)
                    {
                        e.story = story; e.character = story.character; e.kind = "อีเว้น"; e.title=Title(story);
                        e.gates = $"วันที่ {story.startDay}–{story.endDay} · " +
                            (story.anyPhase ? "ทุกช่วงเวลา" : Phase(story.triggerPhase)) +
                            $" · {(story.room == null ? "ทุกห้อง" : story.room.displayName)}\n" +
                            $"ลำดับ {story.priority} · {Delivery(story.delivery)} · " +
                            (story.repeatDaily ? $"เล่นซ้ำ เว้น {story.minimumRepeatDays} วัน" : "จบครั้งเดียว") +
                            (string.IsNullOrEmpty(story.exclusiveGroup) ? "" : " · กลุ่มเลือกหนึ่ง: " + story.exclusiveGroup) +
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
                                e.variant = v; e.kind = greeting ? "ทักทาย" : "คุยเล่น";
                                e.title += " · " + (string.IsNullOrEmpty(v.displayName) ? v.id : v.displayName);
                                e.gates = $"วันที่ {v.startDay}–{v.endDay} · " + (v.anyPhase ? "ทุกช่วงเวลา" : Phase(v.phase)) +
                                    $"\nความสนิท {v.minimumAffection}–{v.maximumAffection} · ความเครียด {v.minimumStress}–{v.maximumStress}" +
                                    $" · ลำดับ {v.priority}" + (v.overtimeOnly ? " · ต้องยืนยัน OT" : "") + "\n" + Conditions(v.conditions);
                            }
                        }
                    }
                    // Each serialized root reference is retained, including shared pool/context callers.
                    Visit(n, e.nodes); entries.Add(e);
                }
            }
            entries.Sort((a,b) => string.Compare(a.title,b.title,StringComparison.Ordinal));
        }
        public static string Title(IdentifiedDataSO a)
        {
            if (a == null) return "จบบท";
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
                    return (line.speaker?.displayName??"คำบรรยาย")+" · "+snippet;
                }
                return "ช่วงบทที่ยังไม่มีชื่อหรือข้อความ";
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
        public static bool Matches(DialogueNodeSO n,string query,StoryDraft draft)
        {
            return Contains(Title(n)+" "+n.id,query) || n.lines.Any(l =>
                Contains(draft.Thai(l.localizedText,l.SourceText)+" "+draft.English(l.localizedText)+" "+l.speaker?.displayName,query)) ||
                n.choices.Any(c => Contains(draft.Thai(c.localizedText,c.SourceText)+" "+draft.English(c.localizedText),query));
        }
        public List<ScriptRow> Manuscript(DialogueNodeSO start)
        {
            var rows = new List<ScriptRow>(); var seen = new HashSet<DialogueNodeSO>();
            void Walk(DialogueNodeSO n,int depth)
            {
                if(n == null) { rows.Add(new ScriptRow { label="จบบท",depth=depth }); return; }
                if(!seen.Add(n)) { rows.Add(new ScriptRow { node=n,label="↪ จุดรวม / ย้อนกลับ: "+Title(n),depth=depth,reference=true });return; }
                rows.Add(new ScriptRow {node=n,label=Title(n),depth=depth});
                foreach(var l in n.lines) rows.Add(new ScriptRow {node=n,line=l,depth=depth});
                if(n.choices.Length==0) Walk(n.nextNode,depth);
                foreach(var c in n.choices)
                { rows.Add(new ScriptRow {node=n,choice=c,depth=depth});Walk(c.nextNode,depth+1); }
            }
            Walk(start,0); return rows;
        }
        public static string Phase(DayPhase p) => new[]{"เช้า","บ่าย","เย็น","กลางคืน"}[(int)p];
        public static string Delivery(EventDelivery d) => d == EventDelivery.Forced ? "อัตโนมัติ" : d == EventDelivery.CharacterBubble ? "บับเบิลตัวละคร" : "เรียกจากระบบ";
        public static string Stat(StatType s) => new[]{"ความคืบหน้างาน","เงิน","แต้มทักษะ","ความสนิท","ความเครียด"}[(int)s];
        public static string Conditions(IEnumerable<Condition> conditions)
        {
            var rows = (conditions ?? Array.Empty<Condition>()).Select(c =>
            {
                if(c == null) return "เงื่อนไขว่าง (ต้องตรวจข้อมูล)";
                string target = c.character == null ? "" : c.character.displayName+" · ";
                switch(c.type)
                {
                    case ConditionType.StatAtLeast:return target+Stat(c.stat)+" ≥ "+c.value;
                    case ConditionType.StatAtMost:return target+Stat(c.stat)+" ≤ "+c.value;
                    case ConditionType.FlagSet:return "มีธง: "+c.flagId;
                    case ConditionType.FlagUnset:return "ไม่มีธง: "+c.flagId;
                    case ConditionType.EventCompleted:return "จบอีเว้นแล้ว: "+c.eventId;
                    case ConditionType.EventNotCompleted:return "ยังไม่จบอีเว้น: "+c.eventId;
                    case ConditionType.WorkloadActivated:return "เปิดงานแล้ว: "+c.targetId;
                    case ConditionType.WorkloadNotActivated:return "ยังไม่เปิดงาน: "+c.targetId;
                    case ConditionType.TaskAccepted:return "รับการ์ดงานแล้ว: "+c.targetId;
                    case ConditionType.TaskNotAccepted:return "ยังไม่รับการ์ดงาน: "+c.targetId;
                    case ConditionType.CharacterOvertime:return target+"ยืนยัน OT";
                    case ConditionType.CharacterCanVolunteer:return target+"อาสาช่วยงานได้";
                    case ConditionType.CharacterSharedNight:return target+"อยู่ทำงานกลางคืนด้วยกัน";
                    default:return "เงื่อนไขที่ยังไม่มีคำอธิบาย: "+c.type;
                }
            }).ToArray();
            return rows.Length == 0 ? "ไม่มีเงื่อนไขเสริม" : string.Join("\nและ ",rows);
        }
    }
}
