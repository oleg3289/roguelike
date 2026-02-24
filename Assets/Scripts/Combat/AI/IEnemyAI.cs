using Roguelike.Combat.Entities;

namespace Roguelike.Combat.AI
{
    /// <summary>
    /// Interface for enemy AI behavior.
    /// </summary>
    public interface IEnemyAI
    {
        EnemyAction DetermineNextAction(Enemy self, Player target, int turnNumber);
        void ResetIntent();
    }
}
