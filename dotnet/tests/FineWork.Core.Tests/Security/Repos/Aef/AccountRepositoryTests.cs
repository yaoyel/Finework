using System;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Logging;
using FineWork.Repos;
using FineWork.Settings.Repos;
using FineWork.Settings.Repos.Aef;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace FineWork.Security.Repos.Aef
{
    [TestFixture]
    public class AccountRepositoryTests : FineWorkCoreTestBase
    {
        private static readonly ILogger Log = LogManager.GetLogger(typeof(AccountRepositoryTests));

        protected override void ConfigureServices(IServiceCollection serviceCollection)
        {
            base.ConfigureServices(serviceCollection);
            serviceCollection.AddScoped<ISettingRepository>(
                services => new SettingRepository(services.GetRequiredService<ISessionProvider<AefSession>>()));
        }

        [Test]
        public void EntityTypes_are_correct()
        {
            this.Services.CheckRepositoryRegistered<IAccountRepository, IAccount, AccountEntity, Guid>();
        }

        [Test]
        public void FetchAll_returns_all_IAccount()
        {
            var repository = this.Services.GetRequiredService<IAccountRepository>();
            var list = repository.FetchAll();
            Assert.IsNotNull(list);
        }

        [Test]
        public void Repository_supports_CRUD()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var repository = this.Services.GetRequiredService<IAccountRepository>();

                    IAccount account = repository.CreateTestAccount(Guid.NewGuid());
                    repository.Insert(account);

                    var loaded = repository.Find(account.Id);
                    Assert.IsNotNull(loaded);

                    repository.Delete(account);
                    var reloaded = repository.Find(account.Id);
                    Assert.IsNull(reloaded);

                    session.SaveChanges();
                }

                tx.Complete();
            }
        }
    }
}
