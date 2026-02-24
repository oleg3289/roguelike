using NUnit.Framework;
using Roguelike.Map;
using Roguelike.Run;

namespace Roguelike.Tests.Run
{
    [TestFixture]
    public class RunManagerTests
    {
        [Test]
        public void StartNewRun_InitializesState()
        {
            var manager = new RunManager(totalFloors: 4, seed: 12345);
            manager.StartNewRun();
            
            Assert.AreEqual(4, manager.State.Floors.Count);
        }
        
        [Test]
        public void EnterNode_UpdatesCurrentNode()
        {
            var manager = new RunManager(totalFloors: 4, seed: 12345);
            manager.StartNewRun();
            
            var startNode = manager.State.Floors[0].Nodes[0];
            manager.EnterNode(startNode);
            
            Assert.AreEqual(startNode, manager.CurrentNode);
        }
        
        [Test]
        public void EnterNode_MarksNodeVisited()
        {
            var manager = new RunManager(totalFloors: 4, seed: 12345);
            manager.StartNewRun();
            
            var startNode = manager.State.Floors[0].Nodes[0];
            manager.EnterNode(startNode);
            
            Assert.IsTrue(startNode.IsVisited);
        }
        
        [Test]
        public void GetAvailableNodes_AfterEnteringNode_ReturnsConnections()
        {
            var manager = new RunManager(totalFloors: 4, seed: 12345);
            manager.StartNewRun();
            
            var startNode = manager.State.Floors[0].Nodes[0];
            manager.EnterNode(startNode);
            
            var available = manager.GetAvailableNodes();
            
            Assert.Greater(available.Count, 0);
        }
    }
}
