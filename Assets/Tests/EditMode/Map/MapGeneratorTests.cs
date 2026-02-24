using NUnit.Framework;
using Roguelike.Map;
using System.Collections.Generic;

namespace Roguelike.Tests.Map
{
    [TestFixture]
    public class MapGeneratorTests
    {
        [Test]
        public void GenerateMap_ReturnsCorrectNumberOfFloors()
        {
            var generator = new MapGenerator(floors: 4, seed: 12345);
            var floors = generator.GenerateMap();
            
            Assert.AreEqual(4, floors.Count);
        }
        
        [Test]
        public void GenerateMap_FirstFloorIsStart()
        {
            var generator = new MapGenerator(floors: 4, seed: 12345);
            var floors = generator.GenerateMap();
            
            foreach (var node in floors[0].Nodes)
            {
                Assert.AreEqual(MapNodeType.Start, node.NodeType);
            }
        }
        
        [Test]
        public void GenerateMap_LastFloorIsBoss()
        {
            var generator = new MapGenerator(floors: 4, seed: 12345);
            var floors = generator.GenerateMap();
            
            foreach (var node in floors[floors.Count - 1].Nodes)
            {
                Assert.AreEqual(MapNodeType.Boss, node.NodeType);
            }
        }
        
        [Test]
        public void GenerateMap_FloorsHaveConnections()
        {
            var generator = new MapGenerator(floors: 4, seed: 12345);
            var floors = generator.GenerateMap();
            
            for (int i = 0; i < floors.Count - 1; i++)
            {
                foreach (var node in floors[i].Nodes)
                {
                    Assert.Greater(node.Connections.Count, 0);
                }
            }
        }
        
        [Test]
        public void GenerateMap_StartNodesAreAvailable()
        {
            var generator = new MapGenerator(floors: 4, seed: 12345);
            var floors = generator.GenerateMap();
            
            foreach (var node in floors[0].Nodes)
            {
                Assert.IsTrue(node.IsAvailable);
            }
        }
    }
}
