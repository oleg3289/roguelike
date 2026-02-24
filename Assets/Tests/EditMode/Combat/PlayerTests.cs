using NUnit.Framework;
using Roguelike.Combat.Entities;

namespace Roguelike.Tests.Combat
{
    [TestFixture]
    public class PlayerTests
    {
        [Test]
        public void Player_Initialize_SetsCorrectHealth()
        {
            var player = new Player(80, 3);
            Assert.AreEqual(80, player.CurrentHealth);
            Assert.AreEqual(80, player.MaxHealth);
        }
        
        [Test]
        public void Player_TakeDamage_ReducesHealth()
        {
            var player = new Player(80, 3);
            player.TakeDamage(10);
            Assert.AreEqual(70, player.CurrentHealth);
        }
        
        [Test]
        public void Player_TakeDamage_BlockAbsorbsFirst()
        {
            var player = new Player(80, 3);
            player.AddBlock(5);
            player.TakeDamage(10);
            Assert.AreEqual(75, player.CurrentHealth);
            Assert.AreEqual(0, player.Block);
        }
        
        [Test]
        public void Player_Heal_CappedAtMaxHealth()
        {
            var player = new Player(80, 3);
            player.TakeDamage(10);
            player.Heal(20);
            Assert.AreEqual(80, player.CurrentHealth);
        }
        
        [Test]
        public void Player_TrySpendEnergy_ReturnsTrueWhenEnough()
        {
            var player = new Player(80, 3);
            Assert.IsTrue(player.TrySpendEnergy(2));
            Assert.AreEqual(1, player.CurrentEnergy);
        }
        
        [Test]
        public void Player_TrySpendEnergy_ReturnsFalseWhenNotEnough()
        {
            var player = new Player(80, 3);
            Assert.IsFalse(player.TrySpendEnergy(5));
            Assert.AreEqual(3, player.CurrentEnergy);
        }
    }
}
