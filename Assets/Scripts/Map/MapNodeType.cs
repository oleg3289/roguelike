namespace Roguelike.Map
{
    /// <summary>
    /// Types of nodes on the map.
    /// </summary>
    public enum MapNodeType
    {
        Start,      // Starting node
        Battle,     // Normal combat
        Elite,      // Elite enemy (harder, better rewards)
        Boss,       // Boss battle (ends floor)
        Shop,       // Buy/sell cards and relics
        Rest,       // Heal or upgrade card
        Event,      // Random event
        Treasure,   // Free relic/gold
        Unknown     // Hidden until visited (for future)
    }
}
