using System;
using Fish;

public static class FishSpecialChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }
    private static FishBattleFighter Fighter(FishSpecialEffect effect, int power = 5, int hp = 20, string id = "catch-a") =>
        new FishBattleFighter("Placeholder", hp, 7, 2, id, effect, power, "Fictional Test", "Fictional test description");
    private static bool Act(FishBattleRules battle, bool player, FishBattleRules.BattleAction action) => battle.TryAct(player, action, out _);

    public static void Main()
    {
        var source = Fighter(FishSpecialEffect.StrongerAttack);
        var battle = new FishBattleRules(new[] { source }, new[] { new FishBattleFighter("Enemy", 50, 9, 2) });
        Check(battle.ActivePlayer.CanUseSpecial && !battle.ActivePlayer.SpecialUsed, "Fresh fish special available");
        Check(battle.TryAct(true, FishBattleRules.BattleAction.Special, out string message), "Stronger attack resolves");
        Check(battle.ActiveEnemy.Health == 40, "Special adds five Attack before subtracting two Defense");
        Check(message.Contains("Fictional Test") && message.Contains("10 damage"), "Damage feedback includes ability and amount");
        Check(battle.ActivePlayer.SpecialUsed && !battle.ActivePlayer.CanUseSpecial, "Use tracked on battle fighter");
        Check(!source.SpecialUsed && source.Health == 20, "Definition/source snapshot not mutated");
        Check(battle.State == FishBattleRules.BattleState.EnemyTurn, "Special consumes turn");
        Check(!Act(battle, true, FishBattleRules.BattleAction.Special) && battle.ActiveEnemy.Health == 40, "Same-turn repeated click cannot apply again");
        Act(battle, false, FishBattleRules.BattleAction.Defend);
        Check(!Act(battle, true, FishBattleRules.BattleAction.Special) && battle.State == FishBattleRules.BattleState.PlayerTurn, "Spent special rejected on later turn without consuming it");
        Check(Act(battle, true, FishBattleRules.BattleAction.Attack), "Normal action remains available after spent special rejection");

        var guarded = new FishBattleRules(new[] { source }, new[] { new FishBattleFighter("Enemy", 50, 9, 2) });
        Act(guarded, true, FishBattleRules.BattleAction.Attack);
        Act(guarded, false, FishBattleRules.BattleAction.Defend);
        Act(guarded, true, FishBattleRules.BattleAction.Special);
        Check(guarded.ActiveEnemy.Health == 40, "Stronger attack respects guard: ten becomes five after prior five damage");
        Check(!guarded.ActiveEnemy.IsDefending, "Guard still expires at next defender turn");
        var minimum = new FishBattleRules(new[] { source }, new[] { new FishBattleFighter("Tank", 50, 0, 999) });
        Act(minimum, true, FishBattleRules.BattleAction.Attack);
        Act(minimum, false, FishBattleRules.BattleAction.Defend);
        Act(minimum, true, FishBattleRules.BattleAction.Special);
        Check(minimum.ActiveEnemy.Health == 48, "Normal and guarded special preserve minimum damage one");

        var healer = new FishBattleRules(new[] { Fighter(FishSpecialEffect.SelfHeal, 8) }, new[] { new FishBattleFighter("Enemy", 50, 9, 2) });
        Act(healer, true, FishBattleRules.BattleAction.Attack);
        Act(healer, false, FishBattleRules.BattleAction.Attack);
        Check(healer.ActivePlayer.Health == 13, "Healing fixture has missing HP");
        int enemyHp = healer.ActiveEnemy.Health;
        Check(healer.TryAct(true, FishBattleRules.BattleAction.Special, out message), "Self heal resolves");
        Check(healer.ActivePlayer.Health == 20 && healer.ActiveEnemy.Health == enemyHp, "Heal caps at own maximum and does not change opponent");
        Check(message.Contains("restores 7 HP") && message.Contains("20/20"), "Heal reports actual capped restoration");
        Check(healer.State == FishBattleRules.BattleState.EnemyTurn && healer.ActivePlayer.SpecialUsed, "Heal consumes turn and special");
        Act(healer, false, FishBattleRules.BattleAction.Defend);
        Check(!Act(healer, true, FishBattleRules.BattleAction.Special), "Heal cannot be used twice");
        var fullHealth = new FishBattleRules(new[] { Fighter(FishSpecialEffect.SelfHeal) }, new[] { new FishBattleFighter("E", 50, 1, 0) });
        fullHealth.TryAct(true, FishBattleRules.BattleAction.Special, out message);
        Check(fullHealth.ActivePlayer.Health == 20 && message.Contains("restores 0 HP"), "Full-health heal never exceeds maximum");
        Check(fullHealth.ActivePlayer.SpecialUsed && fullHealth.State == FishBattleRules.BattleState.EnemyTurn, "Full-health heal still spends its use/turn");

        var first = Fighter(FishSpecialEffect.SelfHeal, 5, 2, "catch-a");
        var second = Fighter(FishSpecialEffect.SelfHeal, 5, 2, "catch-b");
        var session = new FishBattleSession("marker", new[] { first, second }, new[] { new FishBattleFighter("Enemy", 1000, 100, 0) });
        session.TryBeginBattle();
        session.TryPlayerAction(FishBattleRules.BattleAction.Special, out _);
        session.Battle.TryAct(false, FishBattleRules.BattleAction.Attack, 0, out _);
        Check(session.Battle.PlayerIndex == 1 && session.Battle.Players[0].SpecialUsed, "Used fish can be defeated and replaced");
        Check(session.Battle.ActivePlayer.CanUseSpecial && session.Battle.ActivePlayer.OwnedCatchId == "catch-b", "Separate catch of same species gets its own use");
        session.TryPlayerAction(FishBattleRules.BattleAction.Special, out _);
        session.TryEnemyTurn(out _);
        Check(session.Battle.State == FishBattleRules.BattleState.Defeat && session.Battle.Players[1].SpecialUsed, "Each individual fish can use its own special");
        Check(session.TryRetry(), "Defeat permits retry after specials");
        foreach (FishBattleFighter fish in session.Battle.Players)
            Check(fish.CanUseSpecial && !fish.SpecialUsed && fish.Health == fish.MaxHealth, "Retry resets special availability and health for every fish");
        Check(session.Battle.ActivePlayer.SpecialName == "Fictional Test" && session.Battle.ActivePlayer.SpecialPower == 5 &&
            session.Battle.ActivePlayer.SpecialDescription == "Fictional test description", "Snapshot/retry retains configured definition");
        var fresh = new FishBattleRules(battle.Players, battle.Enemies);
        Check(fresh.ActivePlayer.CanUseSpecial && !fresh.ActivePlayer.SpecialUsed, "A separate new battle resets used specials");

        foreach (var invalid in new[] { Fighter(FishSpecialEffect.None), Fighter(FishSpecialEffect.StrongerAttack, 0), Fighter((FishSpecialEffect)999) })
        {
            var invalidBattle = new FishBattleRules(new[] { invalid }, new[] { new FishBattleFighter("E", 20, 1, 0) });
            Check(!invalidBattle.ActivePlayer.CanUseSpecial && !Act(invalidBattle, true, FishBattleRules.BattleAction.Special) &&
                invalidBattle.State == FishBattleRules.BattleState.PlayerTurn, "Unsupported/unconfigured special cannot consume action");
        }
        var knockout = new FishBattleRules(new[] { source }, new[] { new FishBattleFighter("E1", 1, 1, 0), new FishBattleFighter("E2", 1, 1, 0) });
        Act(knockout, true, FishBattleRules.BattleAction.Special);
        Check(knockout.EnemyIndex == 1 && knockout.State == FishBattleRules.BattleState.EnemyTurn, "Special KO replaces enemy immediately");
        var win = new FishBattleRules(new[] { source }, new[] { new FishBattleFighter("E", 1, 1, 0) });
        Act(win, true, FishBattleRules.BattleAction.Special);
        Check(win.State == FishBattleRules.BattleState.Victory && !Act(win, false, FishBattleRules.BattleAction.Attack), "Special last KO wins without retaliation");

        var enemyHealing = new FishBattleRules(new[] { new FishBattleFighter("P", 20, 5, 0) }, new[] { Fighter(FishSpecialEffect.SelfHeal, 8) });
        Act(enemyHealing, true, FishBattleRules.BattleAction.Attack);
        Act(enemyHealing, false, FishBattleRules.BattleAction.Special);
        Check(enemyHealing.ActiveEnemy.Health == 20 && enemyHealing.ActiveEnemy.SpecialUsed, "Configured effect rules also work on enemy side");
        var ai = new FishBattleSession("ai", new[] { new FishBattleFighter("P", 100, 0, 0) }, new[] { Fighter(FishSpecialEffect.SelfHeal, 8, 100) });
        ai.TryBeginBattle();
        for (int i = 0; i < 3; i++) { ai.TryPlayerAction(FishBattleRules.BattleAction.Attack, out _); ai.TryEnemyTurn(out _); }
        Check(!ai.Battle.ActiveEnemy.SpecialUsed && ai.Battle.ActiveEnemy.IsDefending, "Enemy AI still attacks twice then defends without specials");
        var huge = new FishBattleRules(new[] { new FishBattleFighter("P", 20, int.MaxValue, 0, null, FishSpecialEffect.StrongerAttack, int.MaxValue) },
            new[] { new FishBattleFighter("E", int.MaxValue, 0, 0) });
        Act(huge, true, FishBattleRules.BattleAction.Special);
        Check(huge.State == FishBattleRules.BattleState.Victory && huge.Enemies[0].Health == 0, "Large configured bonus cannot overflow damage or HP");
        var hugeHeal = new FishBattleRules(new[] { Fighter(FishSpecialEffect.SelfHeal, int.MaxValue) }, new[] { new FishBattleFighter("E", 50, 1, 0) });
        Act(hugeHeal, true, FishBattleRules.BattleAction.Special);
        Check(hugeHeal.ActivePlayer.Health == hugeHeal.ActivePlayer.MaxHealth, "Large heal cannot overflow max HP");
        Console.WriteLine("Passed " + checks + " special-ability checks.");
    }
}



