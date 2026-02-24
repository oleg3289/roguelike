namespace Roguelike.Map
{
    /// <summary>
    /// Context interface for map interactions.
    /// </summary>
    public interface IMapContext
    {
        int CurrentFloor { get; }
        MapNode CurrentNode { get; }
        
        void EnterNode(MapNode node);
        void AdvanceFloor();
    }
}
