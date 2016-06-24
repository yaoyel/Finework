using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AppBoot.Common
{
    /// <summary> Unit tests for <see cref="AsyncHelper"/>. </summary>
    [TestFixture]
    public class AsyncHelperTests
    {
        [SetUp]
        public void SetUp()
        {
            m_Count = 0;
        }

        private int m_Count;

        private Task<int> GenericIncreaseCount()
        {
            Task.Delay(10);
            m_Count ++;
            Task.Delay(10);
            return Task.FromResult(m_Count);
        }

        [Test]
        public void Generic_RunSync_invokes_task()
        {
            Assert.AreEqual(0, m_Count);
            var result = AsyncHelper.RunSync<int>(this.GenericIncreaseCount);
            Assert.AreEqual(1, m_Count);
            Assert.AreEqual(1, result);
        }

        private Task NonGenericIncreaseCount()
        {
            Task.Delay(10);
            m_Count++;
            Task.Delay(10);
            return Task.FromResult(0);
        }

        [Test]
        public void NonGeneric_RunSync_invokes_task()
        {
            Assert.AreEqual(0, m_Count);
            AsyncHelper.RunSync(this.NonGenericIncreaseCount);
            Assert.AreEqual(1, m_Count);
        }
    }
}
