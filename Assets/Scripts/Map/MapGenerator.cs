using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Map
{
    /// <summary>
    /// Generates procedural maps with branching paths.
    /// </summary>
    public class MapGenerator
    {
        private readonly System.Random random;
        private readonly int totalFloors;
        private readonly int minNodesPerFloor;
        private readonly int maxNodesPerFloor;
        
        public MapGenerator(int floors = 4, int minNodes = 3, int maxNodes = 5, int? seed = null)
        {
            totalFloors = floors;
            minNodesPerFloor = minNodes;
            maxNodesPerFloor = maxNodes;
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }
        
        public List<MapFloor> GenerateMap()
        {
            var floors = new List<MapFloor>();
            
            for (int i = 0; i < totalFloors; i++)
            {
                var floor = GenerateFloor(i);
                floors.Add(floor);
            }
            
            // Connect floors
            for (int i = 0; i < floors.Count - 1; i++)
            {
                ConnectFloors(floors[i], floors[i + 1]);
            }
            
            // Mark starting nodes as available
            foreach (var node in floors[0].Nodes)
            {
                node.SetAvailable(true);
            }
            
            return floors;
        }
        
        private MapFloor GenerateFloor(int floorIndex)
        {
            var floor = new MapFloor(floorIndex);
            int nodeCount = random.Next(minNodesPerFloor, maxNodesPerFloor + 1);
            
            for (int i = 0; i < nodeCount; i++)
            {
                MapNodeType type = DetermineNodeType(floorIndex, i, nodeCount);
                var node = new MapNode(floorIndex, i, type);
                floor.AddNode(node);
            }
            
            return floor;
        }
        
        private MapNodeType DetermineNodeType(int floorIndex, int nodeIndex, int totalNodes)
        {
            // First floor is always start
            if (floorIndex == 0)
            {
                return MapNodeType.Start;
            }
            
            // Last floor is always boss
            if (floorIndex == totalFloors - 1)
            {
                return MapNodeType.Boss;
            }
            
            // Second to last floor has elite before boss
            if (floorIndex == totalFloors - 2)
            {
                return MapNodeType.Elite;
            }
            
            // Random node type for middle floors
            double roll = random.NextDouble();
            
            if (roll < 0.45) return MapNodeType.Battle;
            if (roll < 0.55) return MapNodeType.Elite;
            if (roll < 0.70) return MapNodeType.Event;
            if (roll < 0.80) return MapNodeType.Shop;
            if (roll < 0.90) return MapNodeType.Rest;
            return MapNodeType.Treasure;
        }
        
        private void ConnectFloors(MapFloor currentFloor, MapFloor nextFloor)
        {
            var currentNodes = currentFloor.Nodes.ToList();
            var nextNodes = nextFloor.Nodes.ToList();
            
            foreach (var node in currentNodes)
            {
                // Each node connects to 1-2 nodes on the next floor
                int connections = random.Next(1, 3);
                
                for (int i = 0; i < connections; i++)
                {
                    int targetIndex = random.Next(0, nextNodes.Count);
                    var targetNode = nextNodes[targetIndex];
                    node.AddConnection(targetNode);
                }
            }
            
            // Ensure every node in the next floor is reachable
            foreach (var nextNode in nextNodes)
            {
                bool hasIncoming = currentNodes.Any(n => n.Connections.Contains(nextNode));
                if (!hasIncoming && currentNodes.Count > 0)
                {
                    int sourceIndex = random.Next(0, currentNodes.Count);
                    currentNodes[sourceIndex].AddConnection(nextNode);
                }
            }
        }
    }
}
