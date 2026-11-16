using System;
using System.Collections.Generic;

namespace Fish
{
    public sealed class FishBattleRules
    {
        public enum BattleState { PlayerTurn, EnemyTurn, Victory, Defeat }
        public enum BattleAction { Attack, Defend, Special }
        private readonly FishBattleFighter[] players;
        private readonly FishBattleFighter[] enemies;
        public IReadOnlyList<FishBattleFighter> Players { get; }
        public IReadOnlyList<FishBattleFighter> Enemies { get; }
        public int PlayerIndex { get; private set; }
        public int EnemyIndex { get; private set; }
        public FishBattleFighter ActivePlayer => PlayerIndex < players.Length ? players[PlayerIndex] : null;
        public FishBattleFighter ActiveEnemy => EnemyIndex < enemies.Length ? enemies[EnemyIndex] : null;
        public BattleState State { get; private set; } = BattleState.PlayerTurn;
        public bool IsFinished => State == BattleState.Victory || State == BattleState.Defeat;

        public FishBattleRules(IReadOnlyList<FishBattleFighter> playerTeam, IReadOnlyList<FishBattleFighter> enemyTeam)
        {
            if (playerTeam == null || playerTeam.Count < 1 || playerTeam.Count > 3)
                throw new ArgumentException("Player team must contain 1-3 fish.");
            players = CopyTeam(playerTeam);
            enemies = CopyTeam(enemyTeam);
            Players = Array.AsReadOnly(players);
            Enemies = Array.AsReadOnly(enemies);
            while (PlayerIndex < players.Length && players[PlayerIndex].Health == 0) PlayerIndex++;
            if (PlayerIndex == players.Length) throw new ArgumentException("Recover at least one selected fish before battle.");
        }

        private static FishBattleFighter[] CopyTeam(IReadOnlyList<FishBattleFighter> source)
        {
            if (source == null || source.Count == 0) throw new ArgumentException("A battle team cannot be empty.");
            var copies = new FishBattleFighter[source.Count];
            for (int i = 0; i < copies.Length; i++)
                copies[i] = source[i]?.FreshCopy() ?? throw new ArgumentException("Battle fish cannot be null.");
            return copies;
        }

        public static int CalculateDamage(FishBattleFighter attacker, FishBattleFighter defender, int bonusAttack = 0)
        {
            // Bonus Attack uses exactly the normal defense/guard/minimum rules.
            long damage = Math.Max(1L, (long)attacker.Attack + Math.Max(0, bonusAttack) - defender.Defense);
            if (defender.IsDefending) damage = Math.Max(1L, damage / 2 + damage % 2);
            return (int)Math.Min(int.MaxValue, damage);
        }

        private static int NextLiving(FishBattleFighter[] team, int start)
        {
            for (int offset = 0; offset < team.Length; offset++)
            {
                int index = (start + offset) % team.Length;
                if (team[index].Health > 0) return index;
            }
            return team.Length;
        }

        public bool TryAct(bool isPlayer, BattleAction action, out string message) =>
            TryAct(isPlayer, action, isPlayer ? EnemyIndex : PlayerIndex, out message);

        public bool TryAct(bool isPlayer, BattleAction action, int targetIndex, out string message)
        {
            message = "";
            if (IsFinished || (isPlayer && State != BattleState.PlayerTurn) ||
                (!isPlayer && State != BattleState.EnemyTurn) ||
                (action != BattleAction.Attack && action != BattleAction.Defend && action != BattleAction.Special)) return false;

            FishBattleFighter actor = isPlayer ? ActivePlayer : ActiveEnemy;
            bool offensive = action == BattleAction.Attack || (action == BattleAction.Special && actor.SpecialEffect == FishSpecialEffect.StrongerAttack);
            var opponents = isPlayer ? enemies : players;
            if (offensive && (targetIndex < 0 || targetIndex >= opponents.Length || opponents[targetIndex].Health == 0)) return false;
            FishBattleFighter target = offensive ? opponents[targetIndex] : actor;
            if (action == BattleAction.Special && !actor.CanUseSpecial) return false;
            if (action == BattleAction.Defend)
            {
                actor.Defend();
                message = actor.Name + " defends until its next turn.";
            }
            else if (action == BattleAction.Special && actor.SpecialEffect == FishSpecialEffect.SelfHeal)
            {
                actor.SpendSpecial();
                int restored = actor.Heal(actor.SpecialPower);
                message = actor.Name + " uses " + actor.SpecialName + " and restores " + restored +
                    " HP (" + actor.Health + "/" + actor.MaxHealth + ").";
            }
            else
            {
                bool special = action == BattleAction.Special;
                if (special) actor.SpendSpecial();
                int damage = CalculateDamage(actor, target, special ? actor.SpecialPower : 0);
                target.TakeDamage(damage);
                message = actor.Name + (special ? " uses " + actor.SpecialName + " for " : " attacks for ") + damage + " damage.";
                if (target.Health == 0)
                {
                    message += " " + target.Name + " is defeated.";
                }
            }

            PlayerIndex = NextLiving(players, PlayerIndex + (isPlayer ? 1 : 0));
            EnemyIndex = NextLiving(enemies, EnemyIndex + (isPlayer ? 0 : 1));
            if (EnemyIndex == enemies.Length) { State = BattleState.Victory; return true; }
            if (PlayerIndex == players.Length) { State = BattleState.Defeat; return true; }
            State = isPlayer ? BattleState.EnemyTurn : BattleState.PlayerTurn;
            (isPlayer ? ActiveEnemy : ActivePlayer).BeginTurn();
            return true;
        }
    }
}


