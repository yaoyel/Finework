using System.Data.SqlClient;
using System.Transactions;
using AppBoot.Data;
using NUnit.Framework;

namespace AppBoot.Transactions
{
    /// <summary> Tests for .NET framework <see cref="System.Transactions"/> classes. </summary>
    [TestFixture]
    public class TransactionTests
    {
        /// <summary>
        /// Executes data operations in the outer <see cref="TransactionScope"/> 
        /// after the rollbacked inner <see cref="TransactionScope"/>
        /// causes <see cref="TransactionException"/>.
        /// </summary>
        [Test]
        public void DataOperations_SHOULD_throw_WHEN_inner_TransactionScope_rollbacks()
        {
            using (var txOuter = new TransactionScope())
            {
                //Creates an inner TransactionScope and leaves without calling Complete()
                using (var txInner = new TransactionScope())
                {
                }

                //Executes data operation after the inner TransactionScope rollbacked
                //will throw TransactionException
                Assert.Throws<TransactionException>(() => TransactionTestHelper.ExecuteSampleCommand());
            }
        }

        /// <summary> <see cref="Transaction.Current"/> is NOT distributed if no connection opened. </summary>
        [Test]
        public void NestedTransactionScope_is_NOT_distributed_when_no_connection_opened()
        {
            using (var txOuter = new TransactionScope())
            {
                using (var txInner = new TransactionScope())
                {
                    Assert.IsTrue(Transaction.Current != null);
                    Assert.IsFalse(Transaction.Current.IsDistributed());
                }
            }
        }

        /// <summary> <see cref="Transaction.Current"/> is NOT distributed if at most one connection opened. </summary>
        [Test]
        public void NestedTransactionScope_is_NOT_distributed_when_only_single_connection_opened()
        {
            var cs = TransactionTestHelper.DefaultConnectionString;
            using (var txOuter = new TransactionScope())
            {
                using (SqlConnection c1 = new SqlConnection(cs))
                {
                    c1.Open();
                    Assert.IsTrue(Transaction.Current != null);
                    Assert.IsFalse(Transaction.Current.IsDistributed());
                }

                using (var txInner = new TransactionScope())
                {
                    using (SqlConnection c2 = new SqlConnection(cs))
                    {
                        c2.Open();
                        Assert.IsTrue(Transaction.Current != null);
                        Assert.IsFalse(Transaction.Current.IsDistributed());
                    }

                    Assert.IsTrue(Transaction.Current != null);
                    Assert.IsFalse(Transaction.Current.IsDistributed());
                }
            }
        }

        /// <summary> Open only one <see cref="SqlConnection"/> at the same time
        /// in a <see cref="TransactionScope"/> does not cause escalation. </summary>
        [Test]
        public void NonNestedSqlConnections_in_TransactionScope_SHOULD_NOT_escalate_WHEN_using_SQLServer2012()
        {
            var cs = TransactionTestHelper.DefaultConnectionString;
            using (var ts = new TransactionScope())
            {
                var tx = Transaction.Current;
                Assert.IsFalse(tx.IsDistributed());

                using (SqlConnection c1 = new SqlConnection(cs))
                {
                    Assert.IsFalse(tx.IsDistributed());

                    c1.Open();
                    c1.Close();
                }

                Assert.AreSame(tx, Transaction.Current);
                Assert.IsFalse(tx.IsDistributed());

                using (SqlConnection c2 = new SqlConnection(cs))
                {
                    Assert.AreSame(tx, Transaction.Current);
                    Assert.IsFalse(tx.IsDistributed());

                    /*  This will NOT cause escalation because c1 is closed. */
                    c2.Open();

                    Assert.AreSame(tx, Transaction.Current);
                    Assert.IsFalse(tx.IsDistributed());

                    c2.Close();
                }

                ts.Complete();
            }
        }

        /// <summary> Open more than one <see cref="SqlConnection"/> at the same time
        /// in a <see cref="TransactionScope"/> causes escalation. </summary>
        /// <remarks> This test requires the MSDTC (Distributed Transaction Coordinator) service running. </remarks>
        [Test]
        public void NestedSqlConnections_in_TransactionScope_SHOULD_escalate_WHEN_using_SQLServer2012()
        {
            Assert.IsTrue(TxManager.IsMsdtcRunning(), "This test requires MSDTC");

            var cs = TransactionTestHelper.DefaultConnectionString;
            using (var ts = new TransactionScope())
            {
                var tx = Transaction.Current;
                Assert.IsFalse(tx.IsDistributed());

                using (SqlConnection c1 = new SqlConnection(cs))
                {
                    Assert.IsFalse(tx.IsDistributed());

                    c1.Open();

                    Assert.AreSame(tx, Transaction.Current);
                    Assert.IsFalse(tx.IsDistributed());

                    using (SqlConnection c2 = new SqlConnection(cs))
                    {
                        Assert.AreSame(tx, Transaction.Current);
                        Assert.IsFalse(tx.IsDistributed());

                        /* Opens the second connection while the first connection is still opening
                         * will cause DTC escalation. */
                        c2.Open();

                        Assert.AreSame(tx, Transaction.Current);
                        Assert.IsTrue(tx.IsDistributed());
                    }
                }

                ts.Complete();
            }
        }
    }
}
