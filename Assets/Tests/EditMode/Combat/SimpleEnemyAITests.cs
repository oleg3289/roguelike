using NUnit.Framework;
using Roguelike.Combat.AI;
using Roguelike.Combat.Entities;

namespace Roguelike.Tests.Combat
{
    [TestFixture]
    public class SimpleEnemyAITests
    {
        [Test]
        public void DetermineNextAction_ReturnsAttackOrDefend()
        {
            var ai = new SimpleEnemyAI(attackChance: 0.5f);
            var enemy = new Enemy("Test", 50, ai);
            var player = new Player(80);
            
            var action = ai.DetermineNextAction(enemy, player, 1);
            
            Assert.That(action.ActionType, Is.EqualTo(EnemyActionType.Attack).Or.EqualTo(EnemyActionType.Defend));
        }
        
        [Test]
        public void DetermineNextAction_AttackHasCorrectDamage()
        {
            var ai = new SimpleEnemyAI(attackDamage: 10, attackChance: 1f);
            var enemy = new Enemy("Test", 50, ai);
            var player = new Player(80);
            
            var action = ai.DetermineNextAction(enemy, player, 1);
            
            Assert.AreEqual(EnemyActionType.Attack, action.ActionType);
            Assert.AreEqual(10, action.Value);
        }
    }
}
