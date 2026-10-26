using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectLove.Core
{
    public enum SaveSlot { Manual, Auto }
    public enum SaveResult { Success, Busy, Missing, Invalid, Incompatible, IoError }

    // Only safe, non-dialogue snapshots are persisted. Definitions are never serialized into runtime saves.
    public sealed class SaveStore
    {
        [Serializable] private sealed class Envelope
        {
            public int version;
            public bool usesStamina;
            public string configId, payload, checksum;
        }
        private readonly string directory;
        public SaveStore(string directory) { this.directory = directory; }
        public string PathFor(GameSession session, SaveSlot slot) => PathFor(session, slot, session.Config.localization != null);
        private string PathFor(GameSession session, SaveSlot slot, bool localized) => Path.Combine(directory,
            Digest((session.Config.id ?? "") + (session.Production.Enabled ? "_production_v5" : "") +
                (session.Events.Enabled ? "_events_v6" : "") + (session.Production.PlanningEnabled ? "_planning_v7" : "") + (session.Romance.Enabled ? localized ? "_romance_v9" : "_romance_v8" : "")) + "_" + slot.ToString().ToLowerInvariant() + ".json");
        public bool Exists(GameSession session, SaveSlot slot) => File.Exists(PathFor(session, slot)) || session.Config.localization != null && File.Exists(PathFor(session, slot, false));

        public SaveResult Save(GameSession session, SaveSlot slot)
        {
            if (session.IsBusy) return SaveResult.Busy;
            if (string.IsNullOrWhiteSpace(session.Config.id) || !session.CanRestore(session.State)) return SaveResult.Invalid;
            string path = PathFor(session, slot), temp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(directory);
                string payload = JsonUtility.ToJson(session.State);
                var envelope = new Envelope { version = session.Romance.Enabled ? session.Config.localization != null ? 9 : 8 : session.Production.PlanningEnabled ? 7 : session.Events.Enabled ? 6 : session.Production.Enabled ? 5 : 3, usesStamina = session.Config.usesStamina, configId = session.Config.id, payload = payload, checksum = Digest(payload) };
                byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(envelope, true));
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                return SaveResult.Success;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { return SaveResult.IoError; }
        }

        public SaveResult Load(GameSession session, SaveSlot slot)
        {
            if (session.HasActiveModal) return SaveResult.Busy;
            string path = PathFor(session, slot);
            bool importLegacy = !File.Exists(path) && session.Config.localization != null;
            if (importLegacy) path = PathFor(session, slot, false);
            if (!File.Exists(path)) return SaveResult.Missing;
            try
            {
                if (new FileInfo(path).Length > 4 * 1024 * 1024) return SaveResult.Invalid;
                var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path, Encoding.UTF8));
                if (envelope == null || string.IsNullOrEmpty(envelope.payload) ||
                    envelope.checksum != Digest(envelope.payload)) return SaveResult.Invalid;
                if ((envelope.version != 9 && envelope.version != 8 && envelope.version != 7 && envelope.version != 6 && envelope.version != 5 && envelope.version != 3 && envelope.version != 2 && envelope.version != 1) ||
                    (session.Romance.Enabled != (envelope.version >= 8)) ||
                    (envelope.version == 9 && session.Config.localization == null) ||
                    (session.Config.localization != null && envelope.version != (importLegacy ? 8 : 9)) ||
                    (envelope.version < 8 && session.Production.PlanningEnabled != (envelope.version == 7)) ||
                    (envelope.version < 8 && envelope.version != 7 && session.Events.Enabled != (envelope.version == 6)) ||
                    (envelope.version < 8 && envelope.version != 7 && envelope.version != 6 && session.Production.Enabled != (envelope.version == 5)) ||
                    (envelope.version == 1 && session.Config.usesStamina) || envelope.configId != session.Config.id ||
                    (envelope.version >= 2 && envelope.usesStamina != session.Config.usesStamina)) return SaveResult.Incompatible;
                var restored = JsonUtility.FromJson<RuntimeGameState>(envelope.payload);
                if (importLegacy)
                {
                    var mapping = session.Config.localization;
                    if (restored?.romance == null || restored.romance.catalog != mapping.legacyRomanceFingerprint ||
                        session.Romance.LegacyFingerprint() != mapping.legacyRomanceFingerprint ||
                        LocalizedGameplayFingerprint.Compute(session.Config) != mapping.migrationGameplayFingerprint)
                        return SaveResult.Incompatible;
                    restored.romance.catalog = session.State.romance.catalog;
                }
                if (restored != null && envelope.version < 8) restored.romance = null;
                if (restored?.production != null && envelope.version < 7) restored.production.planning = null;
                if (restored != null && (envelope.version < 6 || !session.Events.Enabled)) restored.eventSchedule = null;
                if (restored != null && (envelope.version < 4 || !session.Production.Enabled)) restored.production = null;
                if (envelope.version == 1 && restored != null)
                {
                    restored.inventory = new System.Collections.Generic.List<InventoryEntry>();
                    restored.dailyInteractions = new System.Collections.Generic.List<DailyInteractionState>();
                    restored.collectedPickups = new System.Collections.Generic.List<string>();
                    restored.roomId = session.Config.startingRoomId;
                }
                if (envelope.version < 3 && restored != null)
                {
                    // Earlier saves have no attribution by stream. Keep their aggregate; do not invent a split.
                    restored.workProgress = new System.Collections.Generic.List<WorkProgressEntry>();
                    foreach (var stream in session.Config.workStreams)
                        restored.workProgress.Add(new WorkProgressEntry { streamId = stream.id });
                    if (restored.characters != null) foreach (var character in restored.characters)
                        if (character != null) character.lastTalkDay = 0;
                }
                return session.TryRestore(restored) ? SaveResult.Success : SaveResult.Invalid;
            }
            catch (ArgumentException) { return SaveResult.Invalid; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { return SaveResult.IoError; }
        }

        private static string Digest(string value)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-", "").ToLowerInvariant();
        }
    }
}
