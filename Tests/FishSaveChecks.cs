using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Fish;
namespace UnityEngine {
 public enum RuntimeInitializeLoadType { SubsystemRegistration }
 public class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) {} }
 public static class Application { public static string persistentDataPath; }
 public static class Resources { public static T[] FindObjectsOfTypeAll<T>() { return new T[]{(T)(object)Checks.Species}; } }
 public static class JsonUtility {
  static JsonSerializerOptions options = new JsonSerializerOptions { IncludeFields = true };
  public static string ToJson(object value, bool pretty) => JsonSerializer.Serialize(value, options);
  public static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json, options);
 }
}
namespace Fish {
 public class FishSpecies { public string speciesId = "test-species"; public int health = 24; }
 public static class FishBattleEncounter { public static bool HasActiveEncounter, ResultReady; }
 public class FishCollection {
  public List<FishOwnedCatch> Catches = new List<FishOwnedCatch>();
  public int Count => Catches.Count;
  public FishStoryProgress Story = new FishStoryProgress();
  public void Restore(IEnumerable<FishOwnedCatch> fish,int stage,bool opening,bool ending) { Catches.Clear(); Catches.AddRange(fish); Story.Restore(stage,opening,ending); }
 }
 public class FishBattleTeam { public FishTeamSelection Selection; public FishBattleTeam(FishCollection c) { Selection = new FishTeamSelection(id=>c.Catches.Exists(f=>f.CatchId==id)); } }
}
class Checks {
 public static FishSpecies Species = new FishSpecies();
 static int count;
 static void Check(bool value,string message) { if(!value) throw new Exception(message); count++; }
 static void Main() {
  string dir = Path.Combine(Path.GetTempPath(), "FISH-save-test-"+Guid.NewGuid().ToString("N"));
  UnityEngine.Application.persistentDataPath = dir;
  try {
   var c = new FishCollection(); var t = new FishBattleTeam(c);
   Check(!FishSave.CanContinue,"missing save");
   Check(FishSave.NewGame(c,t),"new game");
   c.Catches.Add(new FishOwnedCatch(Species)); c.Catches.Add(new FishOwnedCatch(Species));
   c.Catches[0].SetHealth(0); c.Catches[1].SetHealth(7);
   string first=c.Catches[0].CatchId, second=c.Catches[1].CatchId;
   t.Selection.TryAdd(second); t.Selection.TryAdd(first);
   c.Story.TryPresentOpening(); FishSave.Manual(c,t);
   Check(File.Exists(FishSave.Location+".bak"),"backup");
   c.Catches.Clear(); Check(FishSave.Load(c,t),"roundtrip");
   Check(c.Count==2 && c.Catches[0].CatchId==first && c.Catches[1].CatchId==second,"identity");
   Check(c.Catches[0].CurrentHP==0 && c.Catches[1].CurrentHP==7,"no healing");
   Check(t.Selection.Members[0]==second && t.Selection.Members[1]==first,"order");
   Check(c.Story.OpeningShown,"intro");
   Check(FishSave.Load(c,t) && c.Count==2,"repeat load");
   string valid=File.ReadAllText(FishSave.Location);
   File.WriteAllText(FishSave.Location,"broken");
   FishSave.Manual(c,t); Check(File.ReadAllText(FishSave.Location)=="broken","protect corrupt");
   Check(FishSave.CanContinue && FishSave.Load(c,t),"backup load");
   File.Delete(FishSave.Location+".bak"); Check(!FishSave.CanContinue,"all corrupt");
   File.WriteAllText(FishSave.Location,valid.Replace("\"version\":1","\"version\":99"));
   Check(!FishSave.CanContinue,"unsupported version");
   File.WriteAllText(FishSave.Location,"{\"version\":1}"); Check(!FishSave.CanContinue,"incomplete");
   File.WriteAllText(FishSave.Location,valid);
   var data=UnityEngine.JsonUtility.FromJson<FishSaveData>(valid);
   var map=new Dictionary<string,FishSpecies>{{Species.speciesId,Species}};
   data.catches[0].hp=999;
   try { FishSave.Validate(data,map); throw new Exception("HP accepted"); } catch(InvalidDataException) { count++; }
   data.catches[0].hp=0; data.team.Add(data.team[0]);
   try { FishSave.Validate(data,map); throw new Exception("duplicate team accepted"); } catch(InvalidDataException) { count++; }
   FishBattleEncounter.HasActiveEncounter=true;
   Check(!FishSave.Load(c,t) && !FishSave.NewGame(c,t),"battle gate");
   FishBattleEncounter.HasActiveEncounter=false;
   File.Delete(FishSave.Location); Directory.CreateDirectory(FishSave.Location+".tmp");
   Check(!FishSave.NewGame(c,t),"write failure");
   Console.WriteLine("Passed "+count+" save checks in isolated temporary directory.");
  } finally { if(Directory.Exists(dir)) Directory.Delete(dir,true); }
 }
}
