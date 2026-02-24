using NUnit.Framework;
using Roguelike.Combat.Status;

namespace Roguelike.Tests.Combat
{
    [TestFixture]
    public class StatusManagerTests
    {
        [Test]
        public void ApplyStatus_CreatesNewEffect()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Strength, 2);
            
            Assert.IsTrue(manager.HasStatus(StatusType.Strength));
            Assert.AreEqual(2, manager.GetStatusStacks(StatusType.Strength));
        }
        
        [Test]
        public void ApplyStatus_IncreasesExistingStacks()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Weak, 1);
            manager.ApplyStatus(StatusType.Weak, 2);
            
            Assert.AreEqual(3, manager.GetStatusStacks(StatusType.Weak));
        }
        
        [Test]
        public void ModifyOutgoingDamage_StrengthIncreases()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Strength, 3);
            
            int result = manager.ModifyOutgoingDamage(5);
            Assert.AreEqual(8, result);
        }
        
        [Test]
        public void ModifyOutgoingDamage_WeakReduces()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Weak, 1);
            
            int result = manager.ModifyOutgoingDamage(8);
            Assert.AreEqual(6, result); // 8 * 0.75 = 6
        }
        
        [Test]
        public void ModifyIncomingDamage_VulnerableIncreases()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Vulnerable, 1);
            
            int result = manager.ModifyIncomingDamage(10);
            Assert.AreEqual(15, result); // 10 * 1.5 = 15
        }
        
        [Test]
        public void ModifyBlock_DexterityIncreases()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Dexterity, 2);
            
            int result = manager.ModifyBlock(5);
            Assert.AreEqual(7, result);
        }
        
        [Test]
        public void ProcessTurnEnd_DecaysTemporaryStatuses()
        {
            var manager = new StatusManager();
            manager.ApplyStatus(StatusType.Weak, 2);
            manager.ApplyStatus(StatusType.Strength, 2); // Permanent
            
            manager.ProcessTurnEnd(null, null);
            
            Assert.AreEqual(1, manager.GetStatusStacks(StatusType.Weak));
            Assert.AreEqual(2, manager.GetStatusStacks(StatusType.Strength));
        }
    }
}
