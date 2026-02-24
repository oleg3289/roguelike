namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Interface for card effects.
    /// </summary>
    public interface IEffect
    {
        EffectType EffectType { get; }
        int Value { get; set; }
        void Execute(ICombatContext context, ITargetable target);
    }
}
