using System;
using Fish;

public static class FishBattleChecks
{
    private static int checks;
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }
    private static FishBattleFighter Fish(string name, int hp = 20, int attack = 7, int defense = 2) => new FishBattleFighter(name, hp, attack, defense);
    private static bool Act(FishBattleRules battle, bool player, FishBattleRules.BattleAction action) => battle.TryAct(player, action, out _);
    private static void Invalid(Action action, string message)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentException) { rejected = true; }
        Check(rejected, message);
    }

    public static void Main()
    {
        var lineup = new FishBattleRules(new[] { Fish("P1",100), Fish("P2",100), Fish("P3",100) }, new[] { Fish("E1",100), Fish("E2",100), Fish("E3",100) });
        string[] order = { "P1", "E1", "P2", "E2", "P3", "E3", "P1" };
        for (int i = 0; i < order.Length; i++)
        {
            bool player = i % 2 == 0;
            Check((player ? lineup.ActivePlayer : lineup.ActiveEnemy).Name == order[i], "Lineup rotation " + i);
            Act(lineup, player, FishBattleRules.BattleAction.Defend);
        }
        var targeted = new FishBattleRules(new[] { Fish("P",100,100) }, new[] { Fish("E1"), Fish("E2"), Fish("E3") });
        Check(!targeted.TryAct(true, FishBattleRules.BattleAction.Attack, -1, out _), "Invalid target rejected");
        Check(targeted.PlayerIndex == 0 && targeted.State == FishBattleRules.BattleState.PlayerTurn, "Invalid target preserves turn");
        Check(targeted.TryAct(true, FishBattleRules.BattleAction.Attack, 1, out _), "Non-active target accepted");
        Check(targeted.Enemies[1].Health == 0 && targeted.EnemyIndex == 0, "Pending enemy preserved after another target defeated");
        targeted.TryAct(false, FishBattleRules.BattleAction.Defend, out _);
        Check(targeted.EnemyIndex == 2, "Defeated slot skipped during cyclic rotation");
        Check(!targeted.TryAct(true, FishBattleRules.BattleAction.Attack, 1, out _), "Defeated target rejected");
        var playerSource = Fish("Player");
        var enemySource = Fish("Enemy");
        var battle = new FishBattleRules(new[] { playerSource }, new[] { enemySource });
        Check(battle.State == FishBattleRules.BattleState.PlayerTurn, "Player acts first");
        Check(battle.ActivePlayer.Health == 20 && battle.ActiveEnemy.Health == 20, "Starts at full HP");
        Check(!ReferenceEquals(playerSource, battle.ActivePlayer), "Battle fighter is an isolated copy");
        Check(!Act(battle, false, FishBattleRules.BattleAction.Attack), "Enemy cannot act first");
        Check(!Act(battle, true, (FishBattleRules.BattleAction)99), "Invalid action rejected");
        Check(Act(battle, true, FishBattleRules.BattleAction.Attack), "Player attack accepted");
        Check(battle.ActiveEnemy.Health == 15, "Attack minus defense damage");
        Check(enemySource.Health == 20, "Input snapshot not mutated");
        Check(battle.State == FishBattleRules.BattleState.EnemyTurn, "Player action gives enemy turn");
        Check(!Act(battle, true, FishBattleRules.BattleAction.Attack) && battle.ActiveEnemy.Health == 15, "Duplicate player action rejected");
        Check(Act(battle, false, FishBattleRules.BattleAction.Attack) && battle.ActivePlayer.Health == 15, "Enemy action then damage");
        Check(battle.State == FishBattleRules.BattleState.PlayerTurn, "Alternates back to player");

        var guard = new FishBattleRules(new[] { Fish("P") }, new[] { Fish("E") });
        Check(Act(guard, true, FishBattleRules.BattleAction.Defend) && guard.ActivePlayer.IsDefending, "Player guard starts");
        Check(!Act(guard, true, FishBattleRules.BattleAction.Defend), "Guard cannot stack through duplicate action");
        Check(FishBattleRules.CalculateDamage(guard.ActiveEnemy, guard.ActivePlayer) == 3, "Odd five damage halves upward to three");
        Act(guard, false, FishBattleRules.BattleAction.Attack);
        Check(guard.ActivePlayer.Health == 17, "Guard reduces incoming damage");
        Check(!guard.ActivePlayer.IsDefending, "Guard expires at defender next turn");
        Act(guard, true, FishBattleRules.BattleAction.Attack);
        Act(guard, false, FishBattleRules.BattleAction.Defend);
        Check(guard.ActiveEnemy.IsDefending, "Enemy can defend");
        Check(FishBattleRules.CalculateDamage(guard.ActivePlayer, guard.ActiveEnemy) == 3, "Enemy guard reduces player damage");
        Act(guard, true, FishBattleRules.BattleAction.Defend);
        Check(!guard.ActiveEnemy.IsDefending, "Unused guard expires even when opponent also defends");
        Act(guard, false, FishBattleRules.BattleAction.Defend);
        Check(!guard.ActivePlayer.IsDefending, "Player unused guard expires at next player turn");
        Act(guard, true, FishBattleRules.BattleAction.Defend);
        Check(FishBattleRules.CalculateDamage(guard.ActiveEnemy, guard.ActivePlayer) == 3, "Repeated-turn guard has same reduction, not stacking");

        var lowDamage = new FishBattleRules(new[] { Fish("P", 10, 0, 100) }, new[] { Fish("E", 10, 0, 100) });
        Act(lowDamage, true, FishBattleRules.BattleAction.Defend);
        Check(FishBattleRules.CalculateDamage(lowDamage.ActiveEnemy, lowDamage.ActivePlayer) == 1, "Guarded minimum damage is one");
        Act(lowDamage, false, FishBattleRules.BattleAction.Attack);
        Check(lowDamage.ActivePlayer.Health == 9, "Minimum damage applied");
        Act(lowDamage, true, FishBattleRules.BattleAction.Attack);
        Check(lowDamage.ActiveEnemy.Health == 9, "Unguarded minimum damage is one");
        Check(FishBattleRules.CalculateDamage(Fish("P", 1, int.MaxValue, 0), Fish("E", 1, 0, 0)) == int.MaxValue, "Large attack does not overflow");

        var victory = new FishBattleRules(new[] { Fish("P", 20, 100, 0) }, new[] { Fish("E1", 3, 2, 0), Fish("E2", 3, 2, 0) });
        Act(victory, true, FishBattleRules.BattleAction.Attack);
        Check(victory.Enemies[0].Health == 0, "Damage clamps defeated enemy to zero immediately");
        Check(victory.EnemyIndex == 1 && victory.ActiveEnemy.Name == "E2", "Next enemy automatically replaces defeated fish");
        Check(victory.State == FishBattleRules.BattleState.EnemyTurn, "Replacement enemy gets next alternating turn");
        Act(victory, false, FishBattleRules.BattleAction.Attack);
        Act(victory, true, FishBattleRules.BattleAction.Attack);
        Check(victory.State == FishBattleRules.BattleState.Victory && victory.ActiveEnemy == null, "Last enemy defeat produces immediate victory");
        int remainingHp = victory.ActivePlayer.Health;
        Check(!Act(victory, false, FishBattleRules.BattleAction.Attack) && victory.ActivePlayer.Health == remainingHp, "No retaliation after victory");
        Check(!Act(victory, true, FishBattleRules.BattleAction.Defend), "No action after terminal result");

        var defeat = new FishBattleRules(new[] { Fish("P1", 2, 1, 0), Fish("P2", 2, 1, 0) }, new[] { Fish("E", 99, 100, 0) });
        Act(defeat, true, FishBattleRules.BattleAction.Attack);
        Act(defeat, false, FishBattleRules.BattleAction.Attack);
        Check(defeat.Players[1].Health == 0 && defeat.PlayerIndex == 0, "Defeated player is replaced immediately");
        Check(defeat.ActivePlayer.Name == "P1" && defeat.State == FishBattleRules.BattleState.PlayerTurn, "Replacement player gets next turn");
        Act(defeat, true, FishBattleRules.BattleAction.Attack);
        Act(defeat, false, FishBattleRules.BattleAction.Attack);
        Check(defeat.State == FishBattleRules.BattleState.Defeat && defeat.ActivePlayer == null, "All player fish defeated means immediate defeat");
        Check(!Act(defeat, true, FishBattleRules.BattleAction.Attack), "Defeated team cannot act");
        var retry = new FishBattleRules(defeat.Players, defeat.Enemies);
        Check(retry.ActivePlayer.Health == retry.ActivePlayer.MaxHealth && retry.ActiveEnemy.Health == retry.ActiveEnemy.MaxHealth, "New battle restores full HP from injured/defeated snapshots");
        Check(retry.State == FishBattleRules.BattleState.PlayerTurn && !retry.ActivePlayer.IsDefending, "Retry resets turns and guard");
        var duplicate = Fish("Same species", 10);
        var distinct = new FishBattleRules(new[] { duplicate, duplicate, duplicate }, new[] { duplicate, duplicate });
        Check(!ReferenceEquals(distinct.Players[0], distinct.Players[1]) && !ReferenceEquals(distinct.Players[0], distinct.Enemies[0]), "Repeated species/snapshot creates independent battle instances");
        Check(distinct.Players.Count == 3 && distinct.Enemies.Count == 2, "Full team sizes supported");
        var clamped = Fish("Unset stats", 0, -5, -7);
        Check(clamped.Health == 1 && clamped.Attack == 0 && clamped.Defense == 0, "Invalid numeric stats safely clamped");
        Invalid(() => new FishBattleRules(new FishBattleFighter[0], new[] { duplicate }), "Empty player team rejected");
        Invalid(() => new FishBattleRules(new[] { duplicate }, new FishBattleFighter[0]), "Empty enemy team rejected");
        Invalid(() => new FishBattleRules(new[] { duplicate, duplicate, duplicate, duplicate }, new[] { duplicate }), "Player team maximum enforced");
        Invalid(() => new FishBattleRules(new[] { duplicate }, new FishBattleFighter[] { null }), "Missing fighter rejected");
        Console.WriteLine("Passed " + checks + " battle-rule checks.");
    }
}



