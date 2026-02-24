using System.Collections.Generic;
using Roguelike.Combat.Effects;
using Roguelike.Combat.Status;
using Roguelike.Combat.AI;

namespace Roguelike.Combat.Entities
{
    /// <summary>
    /// Enemy entity in combat.
    /// </summary>
    public class Enemy : IEntity
    {
        private readonly string name;
        private readonly int maxHealth;
        private int currentHealth;
        private int block;
        private readonly StatusManager statusManager = new();
        private readonly IEnemyAI ai;
        private EnemyAction nextAction;
        
        public string Name => name;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int Block => block;
        public bool IsDead => currentHealth <= 0;
        public StatusManager StatusManager => statusManager;
        public EnemyAction NextAction => nextAction;
        
        public Enemy(string enemyName, int health, IEnemyAI enemyAI = null)
        {
            name = enemyName;
            maxHealth = health;
            currentHealth = health;
            block = 0;
            ai = enemyAI ?? new SimpleEnemyAI();
        }
        
        public void DetermineNextAction(Player target, int turnNumber)
        {
            nextAction = ai.DetermineNextAction(this, target, turnNumber);
        }
        
        public void ExecuteAction(Player target)
        {
            if (nextAction == null) return;
            
            switch (nextAction.ActionType)
            {
                case EnemyActionType.Attack:
                    int damage = nextAction.Value;
                    damage = statusManager.ModifyOutgoingDamage(damage);
                    target.TakeDamage(damage);
                    break;
                    
                case EnemyActionType.Defend:
                    int blockAmount = nextAction.Value;
                    blockAmount = statusManager.ModifyBlock(blockAmount);
                    AddBlock(blockAmount);
                    break;
            }
            
            nextAction = null;
        }
        
        public void TakeDamage(int amount)
        {
            amount = statusManager.ModifyIncomingDamage(amount);
            
            if (block >= amount)
            {
                block -= amount;
                return;
            }
            
            amount -= block;
            block = 0;
            currentHealth -= amount;
            
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
        }
        
        public void AddBlock(int amount)
        {
            block += amount;
        }
        
        public void ResetBlock()
        {
            block = 0;
        }
        
        public void Die()
        {
            // Enemy death - will be handled by combat context
        }
    }
}
