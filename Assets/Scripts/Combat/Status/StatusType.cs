namespace Roguelike.Combat.Status
{
    /// <summary>
    /// Types of status effects in the game.
    /// </summary>
    public enum StatusType
    {
        Strength,     // Increase damage dealt
        Dexterity,    // Increase block gained
        Weak,         // Deal 25% less damage
        Vulnerable,   // Take 50% more damage
        Poison,       // Take damage at end of turn
        Regen,        // Heal at end of turn
        Frail,        // Gain 25% less block
        Entangle      // Cards cost 1 more energy
    }
}
