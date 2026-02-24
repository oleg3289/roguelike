using NUnit.Framework;
using Roguelike.Combat.Entities;

namespace Roguelike.Tests.Combat
{
    [TestFixture]
    public class EnemyTests
    {
        [Test]
        public void Enemy_Initialize_SetsCorrectHealth()
        {
            var enemy = new Enemy("Slime", 50);
            Assert.AreEqual(50, enemy.CurrentHealth);
            Assert.AreEqual(50, enemy.MaxHealth);
            Assert.AreEqual("Slime", enemy.Name);
        }
        
        [Test]
        public void Enemy_TakeDamage_ReducesHealth()
        {
            var enemy = new Enemy("Slime", 50);
            enemy.TakeDamage(20);
            Assert.AreEqual(30, enemy.CurrentHealth);
        }
        
        [Test]
        public void Enemy_GetKilled_IsDead()
        {
            var enemy = new Enemy("Slime", 50);
            enemy.TakeDamage(50);
            Assert.IsTrue(enemy.IsDead);
            Assert.AreEqual(0, enemy.CurrentHealth);
        }
    }
}
