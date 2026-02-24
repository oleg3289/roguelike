namespace Roguelike.Combat.TurnSystem
{
    /// <summary>
    /// Represents the current state in a turn.
    /// </summary>
    public enum TurnState
    {
        PlayerTurn,
        PlayerPlaying,
        EnemyTurn,
        EnemyPlaying,
        TurnEnd,
        CombatEnd
    }
}
