using System.Collections.Generic;

namespace Roguelike.Map
{
    /// <summary>
    /// A single floor of the map, containing multiple nodes.
    /// </summary>
    public class MapFloor
    {
        private readonly int floorIndex;
        private readonly List<MapNode> nodes = new();
        
        public int FloorIndex => floorIndex;
        public IReadOnlyList<MapNode> Nodes => nodes;
        
        public MapFloor(int index)
        {
            floorIndex = index;
        }
        
        public void AddNode(MapNode node)
        {
            nodes.Add(node);
        }
        
        public MapNode GetNode(int index)
        {
            return index >= 0 && index < nodes.Count ? nodes[index] : null;
        }
    }
}
