using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Fish
{
    [Serializable] public sealed class FishSaveData
    {
        public int version;
        public List<FishSaveCatch> catches;
        public List<string> team;
        public List<string> discovered;
        public int totalCatches, stage;
        public bool introduction, ending;
    }
    [Serializable] public sealed class FishSaveCatch { public string id, species; public int hp, maximumHP; }
    public static class FishSave
    {
        public static string Location => Path.Combine(Application.persistentDataPath, "fish-save.json");
        public static string Feedback { get; private set; } = "Use P for Continue, Save and New Game.";
        private static bool authorized;
        private static string lastSnapshot;
        private static bool requested;
        public static void Request() => requested = true;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { authorized = false; lastSnapshot = null; requested = false; }
        private static Dictionary<string, FishSpecies> Species()
        {
            var result = new Dictionary<string, FishSpecies>();
            foreach (var species in Resources.FindObjectsOfTypeAll<FishSpecies>())
                if (!string.IsNullOrEmpty(species.speciesId)) result[species.speciesId] = species;
            return result;
        }
        public static void Validate(FishSaveData data, Dictionary<string, FishSpecies> species)
        {
            if (data == null || data.version != 1 || data.catches == null || data.team == null || data.discovered == null)
                throw new InvalidDataException("Unsupported or incomplete save.");
            var ids = new HashSet<string>();
            var discovered = new HashSet<string>();
            foreach (var fish in data.catches)
            {
                if (fish == null || !Guid.TryParse(fish.id, out _) || !ids.Add(fish.id) || fish.species == null || !species.ContainsKey(fish.species) ||
                    fish.maximumHP < 1 || fish.hp < 0 || fish.hp > fish.maximumHP)
                    throw new InvalidDataException("Invalid catch, species or HP.");
                discovered.Add(fish.species);
            }
            if (data.team.Count > 3 || data.team.Distinct().Count() != data.team.Count || data.team.Any(id => !ids.Contains(id)) ||
                data.discovered.Count != discovered.Count || !discovered.SetEquals(data.discovered) || data.totalCatches != data.catches.Count ||
                data.stage < 0 || data.stage > 4 || (data.stage > 0 && data.totalCatches < 3) || (data.stage == 0 && data.totalCatches >= 3) || (data.ending && data.stage != 4))
                throw new InvalidDataException("Invalid team or progression.");
        }
        private static FishSaveData Read(string path)
        {
            var data = JsonUtility.FromJson<FishSaveData>(File.ReadAllText(path));
            Validate(data, Species());
            return data;
        }
        private static FishSaveData ReadAvailable()
        {
            try { return Read(Location); }
            catch { return Read(Location + ".bak"); }
        }
        public static bool CanContinue { get { try { ReadAvailable(); return true; } catch { return false; } } }
        public static bool Safe => !FishBattleEncounter.HasActiveEncounter;
        public static void Poll(FishCollection collection, FishBattleTeam team)
        {
            if ((!Safe && !FishBattleEncounter.ResultReady) || collection == null || team == null) return;
            if (!authorized && (File.Exists(Location) || File.Exists(Location + ".bak"))) return;
            if (!requested && !FishBattleEncounter.ResultReady) return;
            requested = false;
            var data = Capture(collection, team);
            string json = JsonUtility.ToJson(data, true);
            if (json != lastSnapshot) { lastSnapshot = json; Write(data, false); }
        }
        private static FishSaveData Empty() => new FishSaveData { version = 1, catches = new List<FishSaveCatch>(), team = new List<string>(), discovered = new List<string>() };
        private static FishSaveData Capture(FishCollection collection, FishBattleTeam team)
        {
            var data = Empty(); data.totalCatches = collection.Count; data.stage = collection.Story.Stage;
                data.introduction = collection.Story.OpeningShown; data.ending = collection.Story.EndingShown;
            foreach (var fish in collection.Catches)
                data.catches.Add(new FishSaveCatch { id = fish.CatchId, species = fish.Species.speciesId, hp = fish.CurrentHP, maximumHP = fish.MaximumHP });
            data.team.AddRange(team.Selection.Members);
            data.discovered.AddRange(data.catches.Select(f => f.species).Distinct());
            return data;
        }
        private static bool Write(FishSaveData data, bool replace)
        {
            try
            {
                Validate(data, Species());
                if (!replace && File.Exists(Location)) Read(Location);
                string json = JsonUtility.ToJson(data, true);
                Directory.CreateDirectory(Path.GetDirectoryName(Location));
                using (var stream = new FileStream(Location + ".tmp", FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream)) { writer.Write(json); writer.Flush(); stream.Flush(true); }
                if (File.Exists(Location))
                {
                    bool valid = false;
                    try { Read(Location); valid = true; } catch { }
                    File.Replace(Location + ".tmp", Location, valid ? Location + ".bak" : null);
                }
                else File.Move(Location + ".tmp", Location);
                lastSnapshot = json; authorized = true; Feedback = "Progress saved."; return true;
            }
            catch (Exception error) { Feedback = "Save failed. " + error.Message; return false; }
        }
        public static void Manual(FishCollection collection, FishBattleTeam team)
        {
            if (!Safe || collection == null || team == null) return;
            if (!authorized && (File.Exists(Location) || File.Exists(Location + ".bak"))) { Feedback = "Continue or confirm New Game before saving."; return; }
            Write(Capture(collection, team), false);
        }
        public static bool Load(FishCollection collection, FishBattleTeam team)
        {
            if (!Safe || collection == null || team == null) return false;
            try
            {
                var data = ReadAvailable();
                Apply(data, collection, team);
                authorized = true; lastSnapshot = JsonUtility.ToJson(data, true);
                try { Read(Location); Feedback = "Progress loaded. Stored fish HP restored."; }
                catch { Feedback = "Backup loaded. Main save unreadable. Automatic saving is blocked to protect it."; }
                return true;
            }
            catch (Exception error) { Feedback = "Load failed. " + error.Message; return false; }
        }
        public static bool NewGame(FishCollection collection, FishBattleTeam team)
        {
            if (!Safe || collection == null || team == null) return false;
            var data = Empty();
            if (!Write(data, true)) return false;
            Apply(data, collection, team); return true;
        }
        private static void Apply(FishSaveData data, FishCollection collection, FishBattleTeam team)
        {
            var species = Species();
            var restored = data.catches.Select(f => new FishOwnedCatch(species[f.species], f.id, f.hp, f.maximumHP)).ToList();
            while (team.Selection.Members.Count > 0) team.Selection.RemoveAt(0);
            collection.Restore(restored, data.stage, data.introduction, data.ending);
            foreach (string id in data.team) team.Selection.TryAdd(id);
        }
    }
}




