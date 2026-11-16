using System;
using Fish;

public static class FishBattleSessionChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }
    private static void Lose(FishBattleSession session)
    {
        while (!session.Battle.IsFinished)
        {
            session.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _);
            session.TryEnemyTurn(out _);
        }
    }

    public static void Main()
    {
        var players = new[] { new FishBattleFighter("First", 2, 1, 0, "catch-a"), new FishBattleFighter("Second", 2, 1, 0, "catch-b") };
        var enemies = new[] { new FishBattleFighter("Same encounter enemy", 1000, 100, 0) };
        var session = new FishBattleSession("marker-one", players, enemies);
        Check(session.Phase == FishBattleSession.SessionPhase.Loading && session.Attempt == 0, "New session awaits scene load");
        Check(!session.TryRetry() && !session.TryBeginReturn(), "Cannot retry/return before battle");
        Check(!session.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _) && !session.TryEnemyTurn(out _), "No action during loading");
        Check(session.TryBeginBattle(), "Loaded scene begins once");
        Check(!session.TryBeginBattle() && session.Attempt == 1, "Repeated scene initialization rejected");
        Check(!session.TryBeginReturn() && !session.TryRetry(), "No return or retry during running battle");
        Check(session.TryPlayerAction(FishBattleRules.BattleAction.Defend, out _), "Temporary guard can be set");
        Check(session.Battle.Players[0].IsDefending, "Guard exists before retry fixture");
        Lose(session);
        Check(session.Battle.State == FishBattleRules.BattleState.Defeat, "Fixture loses battle");
        Check(!session.TryPlayerAction(FishBattleRules.BattleAction.Special, 0, out _), "Special after defeat is rejected safely");
        Check(session.EnemyTurns > 0 && session.Battle.PlayerIndex == 2, "Defeat has progressed turns and active index");
        FishBattleRules oldBattle = session.Battle;
        // Mutating a caller's arrays must not replace the captured encounter/team.
        players[0] = new FishBattleFighter("Changed selection", 999, 999, 999, "other-catch");
        enemies[0] = new FishBattleFighter("Changed encounter", 1, 0, 0);
        Check(session.TryRetry(), "Defeat permits retry");
        Check(session.Attempt == 2 && !ReferenceEquals(oldBattle, session.Battle), "Retry creates a fresh battle, not another scene/session");
        Check(session.EncounterId == "marker-one" && session.Battle.ActiveEnemy.Name == "Same encounter enemy", "Retry keeps same encounter snapshot");
        Check(session.Battle.Players[0].OwnedCatchId == "catch-a" && session.Battle.Players[1].OwnedCatchId == "catch-b", "Retry keeps selected catch IDs and order");
        Check(session.Battle.ActivePlayer.Name == "First" && session.Battle.ActivePlayer.MaxHealth == 2, "Caller changes cannot replace captured team stats");
        foreach (FishBattleFighter fish in session.Battle.Players)
            Check(fish.Health == fish.MaxHealth && !fish.IsDefending, "Every player restored to full HP without guard");
        foreach (FishBattleFighter fish in session.Battle.Enemies)
            Check(fish.Health == fish.MaxHealth && !fish.IsDefending, "Every enemy restored to full HP without guard");
        Check(session.EnemyTurns == 0 && session.Battle.PlayerIndex == 0 && session.Battle.EnemyIndex == 0 &&
            session.Battle.State == FishBattleRules.BattleState.PlayerTurn, "Retry resets AI/turn/index state and player acts first");
        Check(!session.TryRetry() && session.Attempt == 2, "Repeated retry click cannot restart again");
        session.TryPlayerAction(FishBattleRules.BattleAction.Defend, out _);
        session.TryEnemyTurn(out _);
        Check(session.EnemyTurns == 1, "AI restarts its first turn on retry");
        Lose(session);
        Check(session.TryBeginReturn(), "Defeat permits return");
        Check(!session.TryBeginReturn() && !session.TryRetry(), "Return transition blocks duplicate returns and retry");
        Check(!session.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _) && !session.TryEnemyTurn(out _), "Returning session cannot act");
        Check(session.CancelFailedReturn(), "A failed unload can release only the return lock for retry");
        Check(session.Phase == FishBattleSession.SessionPhase.Battling && session.Battle.IsFinished, "Failed unload keeps terminal encounter state");
        Check(session.TryBeginReturn() && session.CompleteReturn(), "Return can complete after retrying unload");
        Check(session.Phase == FishBattleSession.SessionPhase.Closed && !session.CompleteReturn(), "Completion is single-use");
        Check(!session.TryRetry() && !session.TryBeginReturn() && !session.TryBeginBattle(), "Closed session cannot restart or transition");

        var victory = new FishBattleSession("win-marker", new[] { new FishBattleFighter("P", 10, 100, 0, "kept-catch") },
            new[] { new FishBattleFighter("E", 1, 0, 0) });
        victory.TryBeginBattle();
        victory.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _);
        Check(victory.Battle.State == FishBattleRules.BattleState.Victory && !victory.TryRetry(), "Victory does not offer retry");
        Check(victory.TryBeginReturn() && victory.CompleteReturn(), "Victory permits Return to Shore");
        Check(victory.Battle.Players[0].OwnedCatchId == "kept-catch", "Return never rewrites catch identity");
        var failedLoad = new FishBattleSession("bad-load", new[] { new FishBattleFighter("P", 1, 1, 1) }, new[] { new FishBattleFighter("E", 1, 1, 1) });
        Check(failedLoad.TryBeginReturn(true) && failedLoad.CompleteReturn(), "Failed scene load can return without starting battle");
        Check(!failedLoad.TryBeginBattle(), "Aborted loading session cannot later begin");
        Console.WriteLine("Passed " + checks + " battle-session checks.");
    }
}


