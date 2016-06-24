using System;
using NUnit.Framework;

namespace AppBoot.Diagnostics
{
    /// <summary> Unit tests for <see cref="InstanceCounter"/>. </summary>
    [TestFixture]
    public class InstanceCounterTests
    {
        [Test]
        public void InstanceCreated_increases_LivingCount()
        {
            var c = new InstanceCounter();
            Assert.AreEqual(0, c.LivingCount);
            
            c.InstanceCreated();
            Assert.AreEqual(1, c.CreatedCount);
            Assert.AreEqual(0, c.DisposedCount);
            Assert.AreEqual(0, c.FinalizedCount);
            Assert.AreEqual(1, c.LivingCount);
        }

        [Test]
        public void InstanceDisposed_decreases_LivingCount()
        {
            var c = new InstanceCounter();
            c.InstanceCreated();

            c.InstanceDisposed();

            Assert.AreEqual(1, c.CreatedCount);
            Assert.AreEqual(1, c.DisposedCount);
            Assert.AreEqual(0, c.FinalizedCount);
            Assert.AreEqual(0, c.LivingCount);
        }

        [Test]
        public void InstanceFinalized_decreases_LivingCount()
        {
            var c = new InstanceCounter();
            c.InstanceCreated();

            c.InstanceFinalized();

            Assert.AreEqual(1, c.CreatedCount);
            Assert.AreEqual(0, c.DisposedCount);
            Assert.AreEqual(1, c.FinalizedCount);
            Assert.AreEqual(0, c.LivingCount);
        }

        [Test]
        public void Reset_resets_all_counters()
        {
            var c = new InstanceCounter();
            c.InstanceCreated();
            c.InstanceCreated();
            c.InstanceDisposed();
            c.InstanceFinalized();

            c.Reset();
            Assert.AreEqual(0, c.CreatedCount);
            Assert.AreEqual(0, c.DisposedCount);
            Assert.AreEqual(0, c.FinalizedCount);
            Assert.AreEqual(0, c.LivingCount);
        }
    }
}
