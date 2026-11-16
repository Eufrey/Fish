using System;
using Fish;
namespace Fish { public sealed class FishSpecies { public int health = 24; } }
public static class FishHealthChecks
{
 static int n;
 static void Check(bool ok) { if(!ok) throw new Exception("Health check " + n); n++; }
 public static void Main() {
 var species=new FishSpecies(); var a=new FishOwnedCatch(species); var b=new FishOwnedCatch(species);
 a.SetHealth(5); Check(b.CurrentHP==24 && a.CurrentHP==5 && a.CatchId!=b.CatchId);
 a.SetHealth(-1); Check(a.CurrentHP==0); a.SetHealth(999); Check(a.CurrentHP==24 && species.health==24);
 var session=new FishBattleSession("test",new[]{new FishBattleFighter("zero",24,5,1,a.CatchId,currentHP:0),new FishBattleFighter("hurt",24,1,0,b.CatchId,currentHP:5)},new[]{new FishBattleFighter("enemy",100,10,0)});
 int commits=0, heals=0;
 session.CommitHealth=fish=>{ commits++; foreach(var f in fish) if(f.OwnedCatchId==b.CatchId)b.SetHealth(f.Health); };
 session.RecoverTeam=()=>{heals++;a.Recover();b.Recover();};
 session.TryBeginBattle(); Check(session.Battle.PlayerIndex==1 && session.Battle.ActivePlayer.Health==5);
 session.TryPlayerAction(FishBattleRules.BattleAction.Attack,out _);session.TryEnemyTurn(out _);
 Check(commits==1 && b.CurrentHP==0); session.TryEnemyTurn(out _);Check(commits==1);
 Check(session.TryRetry() && heals==1 && b.CurrentHP==24 && session.Battle.PlayerIndex==0 && session.Battle.ActivePlayer.Health==24);
 Check(!session.Battle.ActivePlayer.IsDefending && !session.Battle.ActivePlayer.SpecialUsed);
 var win=new FishBattleSession("win",new[]{new FishBattleFighter("p",24,100,0,b.CatchId,FishSpecialEffect.SelfHeal,10,currentHP:5)},new[]{new FishBattleFighter("e",2,1,0)});
 win.CommitHealth=fish=>b.SetHealth(fish[0].Health);win.TryBeginBattle();win.TryPlayerAction(FishBattleRules.BattleAction.Special,out _);win.TryEnemyTurn(out _);win.TryPlayerAction(FishBattleRules.BattleAction.Attack,out _);
 Check(b.CurrentHP==14);win.TryBeginReturn();win.CompleteReturn();Check(b.CurrentHP==14);
 var next=new FishBattleSession("next",new[]{new FishBattleFighter("p",24,10,0,b.CatchId,currentHP:b.CurrentHP)},new[]{new FishBattleFighter("e",5,1,0)});next.TryBeginBattle();Check(next.Battle.ActivePlayer.Health==14);
 Console.WriteLine("Passed "+n+" health checks.");
 }
}
