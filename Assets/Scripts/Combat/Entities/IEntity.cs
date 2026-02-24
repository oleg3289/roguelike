using Roguelike.Combat.Effects;

namespace Roguelike.Combat.Entities
{
    /// <summary>
    /// Interface for any entity in combat (player, enemy).
    /// Extends ITargetable with additional combat capabilities.
    /// </summary>
    public interface IEntity : ITargetable
    {
        void ResetBlock();
        void Die();
    }
}
