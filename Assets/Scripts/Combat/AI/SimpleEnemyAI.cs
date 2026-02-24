using System;
using Roguelike.Combat.Entities;
using Roguelike.Combat.Status;

namespace Roguelike.Combat.AI
{
    /// <summary>
    /// Simple pattern-based enemy AI.
    /// </summary>
    public class SimpleEnemyAI : IEnemyAI
    {
        private readonly Random random = new();
        private readonly int attackDamage;
        private readonly int blockAmount;
        private readonly float attackChance;
        
        public SimpleEnemyAI(int attackDamage = 6, int blockAmount = 4, float attackChance = 0.7f)
        {
            this.attackDamage = attackDamage;
            this.blockAmount = blockAmount;
            this.attackChance = attackChance;
        }
        
        public EnemyAction DetermineNextAction(Enemy self, Player target, int turnNumber)
        {
            // Simple pattern: 70% attack, 30% defend
            float roll = (float)random.NextDouble();
            
            if (roll < attackChance)
            {
                int damage = attackDamage;
                if (self.StatusManager.HasStatus(StatusType.Strength))
                {
                    damage += self.StatusManager.GetStatusStacks(StatusType.Strength);
                }
                return new EnemyAction(EnemyActionType.Attack, damage, $"Attack {damage}");
            }
            else
            {
                return new EnemyAction(EnemyActionType.Defend, blockAmount, $"Block {blockAmount}");
            }
        }
        
        public void ResetIntent()
        {
            // No state to reset for simple AI
        }
    }
}
