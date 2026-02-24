using System.Collections.Generic;
using Roguelike.Map;
using Roguelike.Relics;

namespace Roguelike.Run
{
    /// <summary>
    /// Represents the state of a single run.
    /// </summary>
    public class RunState
    {
        private int gold;
        private int currentFloor;
        private MapNode currentNode;
        private readonly List<MapFloor> floors = new();
        private readonly RelicManager relicManager = new();
        
        public int Gold => gold;
        public int CurrentFloor => currentFloor;
        public MapNode CurrentNode => currentNode;
        public IReadOnlyList<MapFloor> Floors => floors;
        public RelicManager RelicManager => relicManager;
        
        public void AddGold(int amount)
        {
            gold += amount;
            if (gold < 0) gold = 0;
        }
        
        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                return true;
            }
            return false;
        }
        
        public void SetFloors(List<MapFloor> mapFloors)
        {
            floors.Clear();
            floors.AddRange(mapFloors);
            currentFloor = 0;
        }
        
        public void SetCurrentNode(MapNode node)
        {
            currentNode = node;
            currentFloor = node.FloorIndex;
        }
    }
}
