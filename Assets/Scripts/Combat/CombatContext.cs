using System.Collections.Generic;
using Roguelike.Combat.Entities;
using Roguelike.Combat.Effects;
using Roguelike.Combat.TurnSystem;
using Roguelike.Combat.Status;

namespace Roguelike.Combat
{
    /// <summary>
    /// Central combat state manager. Implements ICombatContext.
    /// </summary>
    public class CombatContext : ICombatContext
    {
        private readonly TurnManager turnManager;
        private readonly Player player;
        private readonly List<Enemy> enemies = new();
        
        public TurnManager TurnManager => turnManager;
        public Player Player => player;
        public IReadOnlyList<Enemy> Enemies => enemies;
        
        public event System.Action OnCombatStart;
        public event System.Action OnCombatEnd;
#pragma warning disable CS0067 // Event is never used - reserved for future use
        public event System.Action<IEntity> OnEntityDied;
#pragma warning restore CS0067
        
        public CombatContext(Player playerEntity)
        {
            player = playerEntity;
            turnManager = new TurnManager();
        }
        
        public void AddEnemy(Enemy enemy)
        {
            enemies.Add(enemy);
        }
        
        public void StartCombat()
        {
            player.ResetEnergy();
            player.ResetBlock();
            player.StatusManager.ApplyStatus(StatusType.Strength, 0); // Initialize
            turnManager.StartCombat();
            
            foreach (var enemy in enemies)
            {
                enemy.DetermineNextAction(player, turnManager.TurnNumber);
            }
            
            OnCombatStart?.Invoke();
        }
        
        public void EndTurn()
        {
            if (turnManager.CurrentState == TurnState.PlayerTurn)
            {
                // Process end of turn status effects for player
                player.StatusManager.ProcessTurnEnd(player, this);
                player.ResetBlock();
                turnManager.EndPlayerTurn();
                
                // Process enemy turns
                ProcessEnemyTurns();
            }
        }
        
        private void ProcessEnemyTurns()
        {
            turnManager.StartEnemyTurn();
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.ExecuteAction(player);
                    enemy.StatusManager.ProcessTurnEnd(enemy, this);
                    enemy.ResetBlock();
                }
            }
            
            // Process start of turn for player
            player.StatusManager.ProcessTurnStart(player, this);
            player.ResetEnergy();
            
            // Determine next enemy actions
            turnManager.EndEnemyTurn();
            
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.DetermineNextAction(player, turnManager.TurnNumber);
                }
            }
            
            CheckCombatEnd();
        }
        
        public void CheckCombatEnd()
        {
            if (player.IsDead)
            {
                turnManager.EndCombat();
                OnCombatEnd?.Invoke();
                return;
            }
            
            bool allEnemiesDead = true;
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    allEnemiesDead = false;
                    break;
                }
            }
            
            if (allEnemiesDead)
            {
                turnManager.EndCombat();
                OnCombatEnd?.Invoke();
            }
        }
    }
}
