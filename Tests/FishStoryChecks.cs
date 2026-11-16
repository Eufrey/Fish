using System;
using Fish;
public static class FishStoryChecks
{
    private static int checks;
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    public static void Main()
    {
        var story = new FishStoryProgress();
        Check(story.TryPresentOpening() && !story.TryPresentOpening(), "Opening presentation once per session");
        Check(story.Stage == 0 && !story.TryPresentEnding(), "Presentation cannot advance or complete story");
        Check(!story.TryTakeNotice(out _), "No backstory introduction");
        Check(story.Objective == "Catch three fish." && !story.CanEnter(1), "Initial objective and lock");
        story.ObserveOwnedCount(2);
        Check(!story.CanEnter(1) && !story.ApplyVictory(1), "Two catches cannot unlock or win");
        story.ObserveOwnedCount(3);
        Check(story.CanEnter(1) && !story.CanEnter(2), "Three catches unlock only first");
        story.ObserveOwnedCount(10);
        Check(story.TryTakeNotice(out _) && !story.TryTakeNotice(out _), "Unlock notice once despite more catches");
        Check(!story.ApplyVictory(2), "Out of order victory rejected");
        int rewards = 0;
        var session = new FishBattleSession("1", new[] { new FishBattleFighter("P", 24, 50, 2) },
            new[] { new FishBattleFighter("E", 14, 4, 0) }, id => { rewards++; story.ApplyVictory(int.Parse(id)); });
        session.TryBeginBattle();
        Check(session.TryPlayerAction(FishBattleRules.BattleAction.Special, out _) == false && rewards == 0, "Rejected action no reward");
        session.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _);
        session.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _);
        Check(rewards == 1 && story.CanEnter(2) && !story.CanEnter(1), "Victory applied once and first completed");
        Check(!session.TryRetry() && !story.ApplyVictory(1), "Cannot retry or replay victory");
        session.TryBeginReturn(); session.CompleteReturn();
        Check(story.Stage == 2 && rewards == 1, "Return preserves progress without another reward");
        var losing = new FishBattleSession("2", new[] { new FishBattleFighter("P", 1, 1, 0) },
            new[] { new FishBattleFighter("E", 100, 100, 0) }, id => story.ApplyVictory(2));
        losing.TryBeginBattle(); losing.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _); losing.TryEnemyTurn(out _);
        Check(story.Stage == 2 && losing.TryRetry(), "Defeat and retry do not advance");
        losing.TryBeginReturn(true); losing.CompleteReturn();
        Check(story.Stage == 2, "Abort without winning does not advance");
        Check(story.ApplyVictory(2) && story.CanEnter(3), "Second unlocks boss");
        Check(story.ApplyVictory(3) && story.Stage == 4 && !story.CanEnter(3), "Boss completes story");
        Check(!story.ApplyVictory(3), "Ending cannot repeat");
        Check(story.TryPresentEnding() && !story.TryPresentEnding(), "Ending presentation consumed exactly once after completion");
        Check(!new FishStoryProgress().TryPresentEnding(), "New session cannot show ending early");
        int notices = 0; while (story.TryTakeNotice(out _)) notices++;
        Check(notices == 3, "Second, boss, ending each queued once");
        var boss = new FishBattleSession("3", new[] { new FishBattleFighter("P", 100, 10, 0) },
            new[] { new FishBattleFighter("Boss", 40, 1, 0, null, FishSpecialEffect.SelfHeal, 10) }, null, true);
        boss.TryBeginBattle(); boss.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _); boss.TryEnemyTurn(out _);
        Check(!boss.Battle.ActiveEnemy.SpecialUsed, "Boss cannot special above half");
        boss.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _); boss.TryEnemyTurn(out _);
        Check(boss.Battle.ActiveEnemy.SpecialUsed && boss.Battle.ActiveEnemy.Health == 30, "Boss heals at half and spends turn");
        boss.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _); boss.TryEnemyTurn(out _);
        Check(boss.Battle.ActiveEnemy.Health == 20, "Boss cannot heal twice");
        var failed = new FishTimingAttempt(); failed.Begin(0); failed.Tick(0, 1, false, false); failed.Tick(0, 2, true, true);
        Check(!failed.TryClaimCatch(), "Failed fishing cannot produce counted catch");
        failed.Cancel(); Check(!failed.TryClaimCatch(), "Cancellation cannot produce counted catch");
        Console.WriteLine("Passed " + checks + " story checks.");
    }
}

