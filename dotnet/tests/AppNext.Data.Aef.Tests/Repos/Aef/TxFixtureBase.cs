using System.Transactions;
using NUnit.Framework;

namespace AppBoot.Repos.Aef
{
    /// <summary> 
    /// This fixture ensures each test is running inside a <see cref="TransactionScope"/>,
    /// and rollbacks the database changes unless 
    /// <see cref="TransactionScope"/>.<see cref="System.Transactions.TransactionScope.Complete()"/> 
    /// is called inside. 
    /// </summary>
    public abstract class TxFixtureBase
    {
        private TransactionScope m_TransactionScope;

        public TransactionScope TransactionScope
        {
            get { return this.m_TransactionScope; }
        }

        [SetUp]
        public virtual void SetUp()
        {
            m_TransactionScope = AefTestUtil.CreateTransactionScope();
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_TransactionScope.Dispose();
        }
    }
}
