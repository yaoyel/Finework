using System;
using System.Data.Entity;
using System.Transactions;

namespace AppBoot.Repos.Aef
{
    public static class AefTestUtil
    {
        public static TransactionScope CreateTransactionScope()
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = IsolationLevel.ReadCommitted;
            var tx = new TransactionScope(
                TransactionScopeOption.RequiresNew, options, TransactionScopeAsyncFlowOption.Enabled);
            return tx;
        }

        public static void InTxContext<TDbContext>(Action<TransactionScope, TDbContext> action)
            where TDbContext : DbContext, new()
        {
            using (var tx = CreateTransactionScope())
            {
                using (var ctx = new TDbContext())
                {
                    action(tx, ctx);
                }
            }
        }

        public static StageExpects StageExpectsNoSession()
        {
            var expects = StageExpects.DefaultNoSession();
            return expects;
        }

        public static StageExpects StageExpectsUseSession()
        {
            var expects = StageExpects.DefaultUseSession();
            expects.AfterDelete_Found = true;
            return expects;
        }

        public static void InitializeContext<TContext>(IDatabaseInitializer<TContext> strategy, bool force)
            where TContext : System.Data.Entity.DbContext, new()
        {
            Database.SetInitializer(strategy);
            using (var ctx = new TContext())
            {
                ctx.Database.Initialize(force);
            }
        }
    }
}
