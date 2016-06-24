using System;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Core;
using FineWork.Data.Aef;
using FineWork.Logging;
using FineWork.Repos;
using FineWork.Settings;
using FineWork.Settings.Repos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace FineWork.Settings.Repos.Aef
{
    [TestFixture]
    public class SettingRepositoryTests : FineWorkCoreTestBase
    {
        private static readonly ILogger m_Log = LogManager.GetLogger(typeof (SettingRepositoryTests));

        protected override void ConfigureServices(IServiceCollection serviceCollection)
        {
            base.ConfigureServices(serviceCollection);
            serviceCollection.AddScoped<ISettingRepository>(
                services => new SettingRepository(services.GetRequiredService<ISessionProvider<AefSession>>()));
        }

        [Test]
        public void EntityTypes_are_correct()
        {
            this.Services.CheckRepositoryRegistered<ISettingRepository, ISetting, SettingEntity, String>();
        }

        [Test]
        public void FetchAll_returns_all_IAccount()
        {
            using (var session = this.Services.ResolveSessionProvider().GetSession())
            {
                var repository = this.Services.GetRequiredService<ISettingRepository>();
                var list = repository.FetchAll();
                Assert.IsNotNull(list);
            }
        }

        [Test]
        public void FetchAll_returns_all_IAccount_within_service_scope()
        {
            using (var session = this.Services.ResolveSessionProvider().GetSession())
            {
                var repository = this.Services.GetRequiredService<ISettingRepository>();
                var list = repository.FetchAll();
                Assert.IsNotNull(list);
            }
        }

        [Test]
        public void Repository_supports_CRUD()
        {
            using (var tx = TxManager.Acquire())
            {
                using (var session = this.Services.ResolveSessionProvider().GetSession())
                {
                    var repository = this.Services.GetRequiredService<ISettingRepository>();

                    ISetting item = repository.Create(Guid.NewGuid().ToString());
                    repository.Insert(item);

                    var loaded = repository.Find(item.Id);
                    Assert.IsNotNull(loaded);

                    repository.Delete(item);
                    var reloaded = repository.Find(item.Id);
                    Assert.IsNull(reloaded);

                    session.SaveChanges();
                }
                tx.Complete();
            }
        }
    }
}
