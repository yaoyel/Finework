using System;
using System.Threading;
using System.Threading.Tasks;
using AppBoot.Repos.Adapters;
using AppBoot.Shop.UpCast;
using AppBoot.Shop.UpCast.Impls;
using NUnit.Framework;

namespace AppBoot.Repos.Aef.DbRequired.Shop.UpCast
{
    [TestFixture]
    public class UpCastTests : TxFixtureBase
    {
        [Test]
        public void ImmediateSync()
        {
            using (var session = new UpCastContext(true))
            {
                var implRepository = new AefRepository<Customer, int>(session);
                var repository = Adapt.Up<ICustomer, int, Customer>.From(implRepository);
                var checker = new SyncStageChecker<ICustomer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    null,
                    StageExpects.DefaultNoSession()
                    );
                checker.Execute(1);
            }
        }

        [Test]
        public async Task ImmediateAsync()
        {
            using (var session = new UpCastContext(true))
            {
                var implRepository = new AefRepository<Customer, int>(session);
                var repository = Adapt.Up<ICustomer, int, Customer>.From(implRepository);
                var checker = new AsyncStageChecker<ICustomer, int>(
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
            using (var session = new UpCastContext(false))
            {
                var implRepository = new AefRepository<Customer, int>(session);
                var repository = Adapt.Up<ICustomer, int, Customer>.From(implRepository);
                var checker = new SyncStageChecker<ICustomer, int>(
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
            using (var session = new UpCastContext(false))
            {
                var implRepository = new AefRepository<Customer, int>(session);
                var repository = Adapt.Up<ICustomer, int, Customer>.From(implRepository);
                var checker = new AsyncStageChecker<ICustomer, int>(
                    repository,
                    HandleCreate,
                    HandleEdit,
                    () => session.SaveChangesAsync(CancellationToken.None),
                    AefTestUtil.StageExpectsUseSession()
                    );
                await checker.ExecuteAsync(1);
            }
        }

        private static ICustomer HandleCreate(int id)
        {
            return new Customer() {Id = id};
        }

        private static void HandleEdit(ICustomer customer)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            customer.Name = customer.Name + "-Changed";
        }
    }
}