using System.Collections.Generic;

namespace Roguelike.Map
{
    /// <summary>
    /// A single node on the map. Represents a single encounter.
    /// </summary>
    public class MapNode
    {
        private readonly int floorIndex;
        private readonly int nodeIndex;
        private readonly MapNodeType nodeType;
        private readonly List<MapNode> connections = new();
        
        private bool isVisited;
        private bool isAvailable;
        
        public int FloorIndex => floorIndex;
        public int NodeIndex => nodeIndex;
        public MapNodeType NodeType => nodeType;
        public IReadOnlyList<MapNode> Connections => connections;
        public bool IsVisited => isVisited;
        public bool IsAvailable => isAvailable;
        
        public MapNode(int floor, int index, MapNodeType type)
        {
            floorIndex = floor;
            nodeIndex = index;
            nodeType = type;
            isVisited = false;
            isAvailable = false;
        }
        
        public void AddConnection(MapNode node)
        {
            if (!connections.Contains(node))
            {
                connections.Add(node);
            }
        }
        
        public void MarkVisited()
        {
            isVisited = true;
        }
        
        public void SetAvailable(bool available)
        {
            isAvailable = available;
        }
    }
}
