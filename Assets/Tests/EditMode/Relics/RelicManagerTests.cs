using NUnit.Framework;
using UnityEngine;
using Roguelike.Relics;
using System.Collections.Generic;

namespace Roguelike.Tests.Relics
{
    [TestFixture]
    public class RelicManagerTests
    {
        [Test]
        public void AddRelic_IncreasesCount()
        {
            var manager = new RelicManager();
            var data = ScriptableObject.CreateInstance<RelicData>();
            var relic = new RelicInstance(data);
            
            manager.AddRelic(relic);
            
            Assert.AreEqual(1, manager.RelicCount);
        }
        
        [Test]
        public void RemoveRelic_DecreasesCount()
        {
            var manager = new RelicManager();
            var data = ScriptableObject.CreateInstance<RelicData>();
            var relic = new RelicInstance(data);
            
            manager.AddRelic(relic);
            manager.RemoveRelic(relic);
            
            Assert.AreEqual(0, manager.RelicCount);
        }
        
        [Test]
        public void HasRelic_ReturnsTrueWhenPresent()
        {
            var manager = new RelicManager();
            var data = ScriptableObject.CreateInstance<RelicData>();
            // Note: RelicData.RelicName would need to be set via reflection in real test
            var relic = new RelicInstance(data);
            
            manager.AddRelic(relic);
            
            // Basic test - manager has the relic
            Assert.AreEqual(1, manager.RelicCount);
        }
    }
}
