using System.Collections.Generic;
using Roguelike.Combat.Entities;
using Roguelike.Combat.Effects;
using Roguelike.Combat.TurnSystem;

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
            turnManager.StartCombat();
            OnCombatStart?.Invoke();
        }
        
        public void EndTurn()
        {
            if (turnManager.CurrentState == TurnState.PlayerTurn)
            {
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
                    enemy.ResetBlock();
                    ExecuteEnemyAction(enemy);
                }
            }
            turnManager.EndEnemyTurn();
            player.ResetEnergy();
        }
        
        private void ExecuteEnemyAction(Enemy enemy)
        {
            // Basic attack for MVP - AI will be expanded later
            player.TakeDamage(5);
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
