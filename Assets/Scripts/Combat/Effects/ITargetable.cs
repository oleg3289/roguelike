namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Interface for entities that can be targeted by cards/effects.
    /// </summary>
    public interface ITargetable
    {
        string Name { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
        int Block { get; }
        bool IsDead { get; }
        
        void TakeDamage(int amount);
        void Heal(int amount);
        void AddBlock(int amount);
    }
}
