using System.Collections.Generic;
using Roguelike.Map;
using Roguelike.Relics;

namespace Roguelike.Run
{
    /// <summary>
    /// Manages the overall run lifecycle.
    /// </summary>
    public class RunManager : IMapContext
    {
        private readonly RunState state;
        private readonly MapGenerator mapGenerator;
        
        public RunState State => state;
        public int CurrentFloor => state.CurrentFloor;
        public MapNode CurrentNode => state.CurrentNode;
        
        public event System.Action<MapNode> OnNodeEntered;
        public event System.Action<int> OnFloorAdvanced;
        public event System.Action OnRunStarted;
        public event System.Action OnRunEnded;
        
        public RunManager(int totalFloors = 4, int? seed = null)
        {
            state = new RunState();
            mapGenerator = new MapGenerator(totalFloors, 3, 5, seed);
        }
        
        public void StartNewRun()
        {
            var floors = mapGenerator.GenerateMap();
            state.SetFloors(floors);
            OnRunStarted?.Invoke();
        }
        
        public void EnterNode(MapNode node)
        {
            node.MarkVisited();
            state.SetCurrentNode(node);
            
            // Make connected nodes available
            foreach (var connection in node.Connections)
            {
                connection.SetAvailable(true);
            }
            
            OnNodeEntered?.Invoke(node);
        }
        
        public void AdvanceFloor()
        {
            OnFloorAdvanced?.Invoke(state.CurrentFloor);
        }
        
        public void EndRun()
        {
            OnRunEnded?.Invoke();
        }
        
        public IReadOnlyList<MapNode> GetAvailableNodes()
        {
            var available = new List<MapNode>();
            
            if (CurrentNode == null)
            {
                // At start, return first floor nodes
                if (state.Floors.Count > 0)
                {
                    foreach (var node in state.Floors[0].Nodes)
                    {
                        if (node.IsAvailable)
                        {
                            available.Add(node);
                        }
                    }
                }
            }
            else
            {
                // Return connected available nodes
                foreach (var node in CurrentNode.Connections)
                {
                    if (node.IsAvailable && !node.IsVisited)
                    {
                        available.Add(node);
                    }
                }
            }
            
            return available;
        }
    }
}
