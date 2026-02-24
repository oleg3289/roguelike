using Roguelike.Combat.Entities;
using Roguelike.Combat.Effects;

namespace Roguelike.Combat.Status
{
    /// <summary>
    /// Interface for status effects.
    /// </summary>
    public interface IStatusEffect
    {
        StatusType StatusType { get; }
        int Stacks { get; }
        bool IsPermanent { get; }
        
        void ApplyStacks(int amount);
        void RemoveStacks(int amount);
        void OnTurnStart(IEntity owner, ICombatContext context);
        void OnTurnEnd(IEntity owner, ICombatContext context);
    }
}
