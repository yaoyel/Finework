using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AppBoot.TestCommon;
using NUnit.Framework;

namespace AppBoot.Transactions
{
    [TestFixture]
    public class TxManagerTests
    {
        [Test]
        public void Aquire_returns_TransactionScope_with_default_values()
        {
            using (var ts = TxManager.Acquire())
            {
                var tx = Transaction.Current;
                Assert.NotNull(tx);
            }
        }

        [Test]
        public void Aquire_returns_TransactionScope_with_IsolationLevel()
        {
            using (var ts = TxManager.Acquire(IsolationLevel.Serializable))
            {
                var tx = Transaction.Current;
                Assert.NotNull(tx);
            }
        }


        [Test]
        public void Aquire_returns_TransactionScope_with_timeout()
        {
            using (var ts = TxManager.Acquire(TransactionManager.MaximumTimeout))
            {
                var tx = Transaction.Current;
                Assert.NotNull(tx);
            }
        }

        [Test]
        public void IsMsdtcRunning_returns_true_when_service_running()
        {
            Assert.IsTrue(TxManager.IsMsdtcRunning());
        }

        [Test]
        public void IsDistributed_throws_when_argument_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => TxManager.IsDistributed(null)).ForArg("transaction");
        }
    }
}
