using NUnit.Framework;
using Roguelike.Combat.TurnSystem;

namespace Roguelike.Tests.Combat
{
    [TestFixture]
    public class TurnManagerTests
    {
        [Test]
        public void StartCombat_SetsPlayerTurn()
        {
            var manager = new TurnManager();
            manager.StartCombat();
            Assert.AreEqual(TurnState.PlayerTurn, manager.CurrentState);
        }
        
        [Test]
        public void StartCombat_SetsTurnNumberToOne()
        {
            var manager = new TurnManager();
            manager.StartCombat();
            Assert.AreEqual(1, manager.TurnNumber);
        }
        
        [Test]
        public void EndPlayerTurn_ChangesStateToTurnEnd()
        {
            var manager = new TurnManager();
            manager.StartCombat();
            manager.EndPlayerTurn();
            Assert.AreEqual(TurnState.TurnEnd, manager.CurrentState);
        }
        
        [Test]
        public void EndEnemyTurn_IncrementsTurnNumber()
        {
            var manager = new TurnManager();
            manager.StartCombat();
            manager.EndPlayerTurn();
            manager.StartEnemyTurn();
            manager.EndEnemyTurn();
            Assert.AreEqual(2, manager.TurnNumber);
        }
    }
}
