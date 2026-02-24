namespace Roguelike.Combat.Effects
{
    /// <summary>
    /// Types of effects a card can have.
    /// </summary>
    public enum EffectType
    {
        Damage,
        Block,
        Heal,
        ApplyStatus,
        DrawCards,
        GainEnergy,
        Discard,
        Exhaust,
        Custom
    }
}
