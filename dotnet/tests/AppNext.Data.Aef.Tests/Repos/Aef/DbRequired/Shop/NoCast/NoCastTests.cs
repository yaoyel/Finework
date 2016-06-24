using System;
using System.Threading;
using System.Threading.Tasks;
using AppBoot.Shop.NoCast;
using NUnit.Framework;

namespace AppBoot.Repos.Aef.DbRequired.Shop.NoCast
{
    [TestFixture]
    public class NoCastTests : TxFixtureBase
    {
        [Test]
        public void ImmediateSync()
        {
            using (var session = new NoCastContext(true))
            {
                var repository = new AefRepository<Customer, int>(session);
                var checker = new SyncStageChecker<Customer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    null,
                    AefTestUtil.StageExpectsNoSession()
                    );
                checker.Execute(1);
            }
        }

        [Test]
        public async Task ImmediateAsync()
        {
            using (var session = new NoCastContext(true))
            {
                var repository = new AefRepository<Customer, int>(session);
                var checker = new AsyncStageChecker<Customer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    null,
                    AefTestUtil.StageExpectsNoSession()
                    );
                await checker.ExecuteAsync(1);
            }
        }

        [Test]
        public void SessionSync()
        {
            using (var session = new NoCastContext(false))
            {
                var repository = new AefRepository<Customer, int>(session);
                var checker = new SyncStageChecker<Customer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    () => session.SaveChanges(),
                    AefTestUtil.StageExpectsUseSession()
                    );
                checker.Execute(1);
            }
        }

        [Test]
        public async Task SessionAsync()
        {
            using (var session = new NoCastContext(false))
            {
                var repository = new AefRepository<Customer, int>(session);
                var checker = new AsyncStageChecker<Customer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    () => session.SaveChangesAsync(CancellationToken.None),
                    AefTestUtil.StageExpectsUseSession()
                    );
                await checker.ExecuteAsync(1);
            }
        }

        private static Customer HandleCreate(int id)
        {
            return new Customer() {Id = id};
        }

        private static void HandleEdit(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            customer.Name = customer.Name + "-Changed";
        }
    }
}