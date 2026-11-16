using System.Collections.Generic;

namespace Fish
{
    // One in-memory encounter. Retry never reads a changed team or enemy asset.
    public sealed class FishBattleSession
    {
        public enum SessionPhase { Loading, Battling, Returning, Closed }
        private readonly IReadOnlyList<FishBattleFighter> players;
        private readonly IReadOnlyList<FishBattleFighter> enemies;
        private readonly System.Action<string> onVictory;
        private readonly bool enemySpecialAtHalfHealth;
        public System.Action<IReadOnlyList<FishBattleFighter>> CommitHealth { private get; set; }
        public System.Action RecoverTeam { private get; set; }
        private bool committed;
        private void CommitResult()
        {
            if (committed || !Battle.IsFinished) return;
            committed = true;
            foreach (var fish in Battle.Players) fish.BeginTurn();
            foreach (var fish in Battle.Enemies) fish.BeginTurn();
            CommitHealth?.Invoke(Battle.Players);
        }
        public string EncounterId { get; }
        public SessionPhase Phase { get; private set; } = SessionPhase.Loading;
        public FishBattleRules Battle { get; private set; }
        public int Attempt { get; private set; }
        public int EnemyTurns { get; private set; }

        public FishBattleSession(string encounterId, IReadOnlyList<FishBattleFighter> playerTeam, IReadOnlyList<FishBattleFighter> enemyTeam,
            System.Action<string> victory = null, bool useEnemySpecialAtHalfHealth = false)
        {
            EncounterId = encounterId;
            onVictory = victory;
            enemySpecialAtHalfHealth = useEnemySpecialAtHalfHealth;
            var snapshot = new FishBattleRules(playerTeam, enemyTeam);
            players = snapshot.Players;
            enemies = snapshot.Enemies;
        }

        public bool TryBeginBattle()
        {
            if (Phase != SessionPhase.Loading) return false;
            StartAttempt();
            return true;
        }

        private void StartAttempt()
        {
            Battle = new FishBattleRules(players, enemies);
            EnemyTurns = 0;
            LastAction = null;
            committed = false;
            Attempt++;
            Phase = SessionPhase.Battling;
        }

        public bool TryRetry()
        {
            if (Phase != SessionPhase.Battling || Battle.State != FishBattleRules.BattleState.Defeat) return false;
            RecoverTeam?.Invoke();
            foreach (var fish in players) fish.InitialHealth = fish.MaxHealth;
            StartAttempt();
            return true;
        }

        public sealed class ActionResult
        {
            public bool ActorIsPlayer;
            public int ActorIndex;
            public int TargetIndex;
            public bool Offensive;
            public string Message;
        }
        public ActionResult LastAction { get; private set; }

        private bool Resolve(bool player, FishBattleRules.BattleAction action, int targetIndex, out string message)
        {
            message = "";
            if (Battle.IsFinished || (player && Battle.State != FishBattleRules.BattleState.PlayerTurn) ||
                (!player && Battle.State != FishBattleRules.BattleState.EnemyTurn)) return false;
            var actor = player ? Battle.ActivePlayer : Battle.ActiveEnemy;
            var record = new ActionResult {
                ActorIsPlayer = player,
                ActorIndex = player ? Battle.PlayerIndex : Battle.EnemyIndex,
                TargetIndex = targetIndex,
                Offensive = action == FishBattleRules.BattleAction.Attack ||
                    (action == FishBattleRules.BattleAction.Special && actor.SpecialEffect == FishSpecialEffect.StrongerAttack)
            };
            if (!Battle.TryAct(player, action, targetIndex, out message)) return false;
            record.Message = message;
            LastAction = record;
            CommitResult();
            return true;
        }

        public bool TryPlayerAction(FishBattleRules.BattleAction action, out string message) =>
            TryPlayerAction(action, Battle == null ? -1 : Battle.EnemyIndex, out message);

        public bool TryPlayerAction(FishBattleRules.BattleAction action, int targetIndex, out string message)
        {
            message = "";
            bool acted = Phase == SessionPhase.Battling && Resolve(true, action, targetIndex, out message);
            if (acted) CommitResult();
            // TryAct rejects all further actions after victory; retry is defeat-only.
            if (acted && Battle.State == FishBattleRules.BattleState.Victory) onVictory?.Invoke(EncounterId);
            return acted;
        }

        public bool TryEnemyTurn(out string message)
        {
            message = "";
            if (Phase != SessionPhase.Battling || Battle.State != FishBattleRules.BattleState.EnemyTurn) return false;

            var action = (EnemyTurns + 1) % 3 == 0 ? FishBattleRules.BattleAction.Defend : FishBattleRules.BattleAction.Attack;
            FishBattleFighter enemy = Battle.ActiveEnemy;
            if (enemySpecialAtHalfHealth && enemy.CanUseSpecial && enemy.Health <= enemy.MaxHealth / 2)
                action = FishBattleRules.BattleAction.Special;
            bool acted = Resolve(false, action, Battle.PlayerIndex, out message);
            if (acted) EnemyTurns++;
            if (acted) CommitResult();
            return acted;
        }

        public bool TryBeginReturn(bool abortFailedLoad = false)
        {
            if (Phase == SessionPhase.Returning || Phase == SessionPhase.Closed) return false;
            if (!abortFailedLoad && (Phase != SessionPhase.Battling || !Battle.IsFinished)) return false;
            Phase = SessionPhase.Returning;
            return true;
        }

        public bool CancelFailedReturn()
        {
            if (Phase != SessionPhase.Returning || Battle == null || !Battle.IsFinished) return false;
            Phase = SessionPhase.Battling;
            return true;
        }

        public bool CompleteReturn()
        {
            if (Phase != SessionPhase.Returning) return false;
            Phase = SessionPhase.Closed;
            return true;
        }
    }
}


