namespace Roguelike.Combat.AI
{
    /// <summary>
    /// Represents a single action an enemy intends to take.
    /// </summary>
    public class EnemyAction
    {
        public EnemyActionType ActionType { get; }
        public int Value { get; }
        public string Description { get; }
        
        public EnemyAction(EnemyActionType type, int value, string description = "")
        {
            ActionType = type;
            Value = value;
            Description = description;
        }
    }
}
