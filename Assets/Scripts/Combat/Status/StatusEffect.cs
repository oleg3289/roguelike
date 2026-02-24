using Roguelike.Combat.Entities;
using Roguelike.Combat.Effects;

namespace Roguelike.Combat.Status
{
    /// <summary>
    /// Base implementation of a status effect.
    /// </summary>
    public class StatusEffect : IStatusEffect
    {
        private int stacks;
        private readonly bool isPermanent;
        
        public StatusType StatusType { get; }
        public int Stacks => stacks;
        public bool IsPermanent => isPermanent;
        
        public StatusEffect(StatusType type, int initialStacks, bool permanent = false)
        {
            StatusType = type;
            stacks = initialStacks;
            isPermanent = permanent;
        }
        
        public void ApplyStacks(int amount)
        {
            stacks += amount;
            if (stacks < 0) stacks = 0;
        }
        
        public void RemoveStacks(int amount)
        {
            stacks -= amount;
            if (stacks < 0) stacks = 0;
        }
        
        public void OnTurnStart(IEntity owner, ICombatContext context)
        {
            // Override in derived classes for specific behaviors
        }
        
        public void OnTurnEnd(IEntity owner, ICombatContext context)
        {
            // Most statuses decay at end of turn
            if (!isPermanent && DecaysAtEndOfTurn())
            {
                RemoveStacks(1);
            }
        }
        
        private bool DecaysAtEndOfTurn()
        {
            return StatusType == StatusType.Weak 
                || StatusType == StatusType.Vulnerable
                || StatusType == StatusType.Frail
                || StatusType == StatusType.Entangle;
        }
    }
}
