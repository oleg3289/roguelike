using Roguelike.Combat.Effects;
using Roguelike.Combat.Status;

namespace Roguelike.Combat.Entities
{
    /// <summary>
    /// Interface for any entity in combat (player, enemy).
    /// Extends ITargetable with additional combat capabilities.
    /// </summary>
    public interface IEntity : ITargetable
    {
        StatusManager StatusManager { get; }
        void ResetBlock();
        void Die();
    }
}
