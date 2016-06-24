using System;
using System.Linq;
using System.ServiceProcess;
using System.Transactions;

namespace AppBoot.Transactions
{
    /// <summary> Helps creating <see cref="TransactionScope"/>s with reasonable default values. </summary>
    /// <remarks>
    /// see http://blogs.msdn.com/b/dbrowne/archive/2010/06/03/using-new-transactionscope-considered-harmful.aspx
    /// </remarks>
    public static class TxManager
    {
        public static TransactionScope Acquire()
        {
            return Acquire(IsolationLevel.ReadCommitted);
        }

        public static TransactionScope Acquire(IsolationLevel isolationLevel)
        {
            return Acquire(isolationLevel, TransactionManager.DefaultTimeout);
        }

        public static TransactionScope Acquire(TimeSpan timeout)
        {
            return Acquire(IsolationLevel.ReadCommitted, timeout);
        }

        public static TransactionScope Acquire(IsolationLevel isolationLevel, TimeSpan timeout)
        {
            return Acquire(TransactionScopeOption.Required, isolationLevel, timeout);
        }

        public static TransactionScope Acquire(TransactionScopeOption scope,
            IsolationLevel isolationLevel, TimeSpan timeout)
        {
            return Acquire(scope, isolationLevel, timeout, TransactionScopeAsyncFlowOption.Enabled);
        }

        public static TransactionScope Acquire(TransactionScopeOption scope,
            IsolationLevel isolationLevel, TimeSpan timeout,
            TransactionScopeAsyncFlowOption asyncFlowOption)
        {
            TransactionOptions options = new TransactionOptions {IsolationLevel = isolationLevel, Timeout = timeout};
            TransactionScope result = new TransactionScope(scope, options, asyncFlowOption);
            return result;
        }

        /// <summary> Represents the factory of <see cref="TransactionScope"/>. </summary>
        public static bool IsMsdtcRunning()
        {
            ServiceController[] services = ServiceController.GetServices();
            return (from service in services
                where service.ServiceName == "MSDTC"
                select service.Status != ServiceControllerStatus.Stopped).FirstOrDefault();
        }

        /// <summary> Determinates if the <see cref="Transaction.Current"/> is distributed. </summary>
        public static bool IsDistributed(this Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException("transaction");

            var id = transaction.TransactionInformation.DistributedIdentifier;
            return id != Guid.Empty;
        }
    }

    public static class TransactionScopeExtensions
    {
        /// <summary> A nop method used to indicate the current <see cref="TransactionScope"/> should be rollbacked. </summary>
        /// <remarks> This method is typically used in unit-tests. 
        /// An unit-test can simplify the data cleanup code by starting a transaction, performing data operations and rollbacking the transaction. </remarks>
        public static void NoComplete(this TransactionScope transactionScope)
        {
            if (transactionScope == null) throw new ArgumentNullException(nameof(transactionScope));
            //Nothing to do.
        }
    }
}
