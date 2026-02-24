using System.Collections.Generic;
using Roguelike.Combat.Effects;

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
        
        public string Name => name;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int Block => block;
        public bool IsDead => currentHealth <= 0;
        
        public Enemy(string enemyName, int health)
        {
            name = enemyName;
            maxHealth = health;
            currentHealth = health;
            block = 0;
        }
        
        public void TakeDamage(int amount)
        {
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
